Imports System.Net
Imports System.Net.Sockets

Namespace MyModbusTCP
    ''' <summary>
    ''' This Modbus Server supports functions :
    ''' 01 (R nCoils)
    ''' 02 (R nDiscrete Inputs)
    ''' 05 (W one Coil)
    ''' 15 (W nCoils)
    ''' 
    ''' 03 (R nHolding Registers)
    ''' 04 (R nInputs Registers)
    ''' 06 (W one Holdin Register)
    ''' 16 (W nHolding Registers)
    ''' </summary>
    Public Class ModbusServer
#Region "DEFINITIONS GLOBALES"
        Private _tcpHandler As TCPHandler
        Public _numOfConnex As UInt16

        Public RTUSlaves() As ModbusSlave = Nothing

        Public holdingRegisters As RegistersWords
        Public inputRegisters As RegistersWords
        Public coils As RegistersBits
        Public discreteInputs As RegistersBits

        Public Event ClientConnected(ByVal client As TcpClient)
        Public Event ClientDisconnected(ByVal client As TcpClient)

        'Events for "gateway" (UnitID = 255)
        Public Event GatewayRegistersChanged(ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        Public Event GatewayCoilsChanged(ByVal firstReg As UInt16, ByVal numOfReg As UInt16)

        'Events for the RTU slaves (UnitID <> 255)
        Public Event SlaveRegistersChanged(ByRef RTUSlave As ModbusSlave, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        Public Event SlaveCoilsChanged(ByRef RTUSlave As ModbusSlave, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)

        Public Event ErrorOccured(ByVal errCode As Byte)

        Private _curMBFrame As DT_TrameModbus

        'Parameters
        Public FC1Disabled As Boolean
        Public FC2Disabled As Boolean
        Public FC3Disabled As Boolean
        Public FC4Disabled As Boolean
        Public FC5Disabled As Boolean
        Public FC6Disabled As Boolean
        Public FC15Disabled As Boolean
        Public FC16Disabled As Boolean

        Private _hasSlaves As Boolean = False 'Does the server simulate RTU slaves ? (in case of a TCP to RTU gateway)
        Private _useBigEndian As Boolean = True 'TODO Ajouter la gestion du Big Endian ou Little Endian

        Private _debug As Boolean = False
#End Region

#Region "STRUCTURES"
        Public Structure DT_TrameModbus 'DT : DataType
            Dim MBAP As DT_MBAPHeader
            Dim functionCode As Byte
            Dim reqPDU As DT_PDU_Requ
            Dim respPDU As DT_PDU_Resp

            Dim HasError As Boolean
            Dim ExceptionCode As Byte
        End Structure

        Public Structure DT_MBAPHeader
            Dim TransactionID As UInt16 '2 bytes : To identify the transaction
            Dim ProtocoleID As UInt16 '2 bytes : To identify the protocol (for Modbus => 0)
            Dim Length As UInt16 '2 bytes : Length of data from UnitID to the end of frame
            Dim UnitID As Byte '1 byte : Identify the slave serial adress
        End Structure

        Public Structure DT_PDU_Requ
            Dim StartAddr As UInt16 '2 bytes : Start address (FC1 ou 3) /OR/ Bit/Word address to write (FC5 ou 6)
            Dim NbReg As UInt16 '2 bytes : Number of registers to read/write (R: FC1 2 3 4 // W: FC15 16)

            Dim BitsToWrite() As Boolean 'Array of bits : Values of bits to write (FC5 / FC15)
            Dim WordsToWrite() As UInt16 'Array of 16bits words : Values of words to write (FC6 / FC16)
        End Structure

        Public Structure DT_PDU_Resp
            Dim nbBytes As UInt16

            Dim BitsState() As Boolean
            Dim WordsValue() As UInt16

        End Structure

#End Region

#Region "CONSTRUCTEURS"
        Public Sub New(port As Integer, bigEndian As Boolean)
            Me._tcpHandler = New TCPHandler(port)
            Me._Construct(0, bigEndian)
        End Sub

        Public Sub New(port As Integer, bigEndian As Boolean, numOfSlaves As UInt16)
            Me._tcpHandler = New TCPHandler(port)
            Me._Construct(numOfSlaves, bigEndian)
        End Sub

        Public Sub New(IP As IPAddress, port As Integer, bigEndian As Boolean)
            Me._tcpHandler = New TCPHandler(IP, port)
            Me._Construct(0, bigEndian)
        End Sub
        Public Sub New(IP As IPAddress, port As Integer, bigEndian As Boolean, numOfSlaves As UInt16)
            Me._tcpHandler = New TCPHandler(IP, port)
            Me._Construct(numOfSlaves, bigEndian)
        End Sub

        Private Sub _Construct(numOfSlaves As UInt16, bigEndian As Boolean)
            Me.coils = New RegistersBits()
            Me.discreteInputs = New RegistersBits()
            Me.holdingRegisters = New RegistersWords()
            Me.inputRegisters = New RegistersWords()

            'Define if this server handle RTU Slaves
            Me._hasSlaves = (numOfSlaves <> 0)

            'Creation of RTU slaves
            If Me._hasSlaves Then
                ReDim Me.RTUSlaves(numOfSlaves - 1)

                For i = 0 To numOfSlaves - 1
                    Me.RTUSlaves(i) = New ModbusSlave(i + 1)
                Next
            End If

            'Define the byte order of the values
            _useBigEndian = bigEndian

            AddHandler Me._tcpHandler.ClientConnected, AddressOf Me._TcpHandler_ClientConnected
            AddHandler Me._tcpHandler.ClientDisconnected, AddressOf Me._TcpHandler_ClientDisconnected
            AddHandler Me._tcpHandler.DataReceived, AddressOf Me._TcpHandler_DataReceived
        End Sub
#End Region

#Region "PUBLIC-FUNCTIONS"
        Public Sub StartListening()
            _tcpHandler.Start()
        End Sub

        Public Sub StopListening()
            _tcpHandler.StopServ()
        End Sub
#End Region

#Region "EVENTS TCPHandler"
        Private Sub _TcpHandler_ClientConnected(ByRef client As TcpClient) 'handles tcpHandler.ClientConnected
            RaiseEvent ClientConnected(client)
        End Sub

        Private Sub _TcpHandler_ClientDisconnected(ByRef client As TcpClient) 'handles tcpHandler.ClientDisconnected
            RaiseEvent ClientDisconnected(client)
        End Sub

        Private Sub _TcpHandler_DataReceived(ByRef client As TcpClient, ByRef buffer() As Byte) 'handles tcpHandler.DataReceived
            If _debug Then
                Dim debugStr As String = ""
                For i = 0 To 5 + buffer(5)
                    debugStr &= CStr(buffer(i))
                Next
                Console.WriteLine(debugStr)
            End If

            'TODO ajouter un paramètre sur le serveur : nb registre (*4 type de reg) + offset ? (voir bloc PxC) + ajouter la gestion du code erreur dans l'extractionFromBuffer
            _curMBFrame = Me._ExtractFromBuffer(buffer)

            Dim respBuffer As Byte()

            'If no error is detected, build the standard response, and update the registers
            If Not _curMBFrame.HasError Then
                Dim regHasChanged As Boolean

                'The request is for the server itself
                If _curMBFrame.MBAP.UnitID = 255 Or Not Me._hasSlaves Then
                    regHasChanged = Me._UpdateRegisters(_curMBFrame, Me.coils, Me.discreteInputs, Me.holdingRegisters, Me.inputRegisters)
                    If regHasChanged Then RaiseEvent GatewayCoilsChanged(_curMBFrame.reqPDU.StartAddr, 1)

                Else 'The request is for a RTU slave
                    'todo ajouter un tableau des UnitID des slaves à l'initialisation de la classe (utiliser une foreach pour trouver le slave)
                    Dim slave As ModbusSlave = RTUSlaves(_curMBFrame.MBAP.UnitID - 1)
                    regHasChanged = Me._UpdateRegisters(_curMBFrame, slave.coils, slave.discreteInputs, slave.holdingRegisters, slave.inputRegisters)
                    If regHasChanged Then RaiseEvent SlaveCoilsChanged(slave, _curMBFrame.reqPDU.StartAddr, 1)
                End If

                respBuffer = BuildResBuffer(_curMBFrame)
            Else
                'If there is an error, build an error response 
                ReDim respBuffer(1)
                respBuffer(0) = _curMBFrame.functionCode Or &H_80
                respBuffer(1) = _curMBFrame.ExceptionCode
                RaiseEvent ErrorOccured(_curMBFrame.ExceptionCode)
            End If

            'Send Buffer to client
            _tcpHandler.SendBuffer(client, respBuffer)

        End Sub
#End Region

#Region "PRIVATE - Managing Bytes Buffers"
        ''' <summary>
        ''' Build of MBAP Header and Request PDU
        ''' </summary>
        ''' <param name="buffer"> Buffer received from a connected client </param>
        Private Function _ExtractFromBuffer(buffer() As Byte) As DT_TrameModbus
            'TODO new name : _BufferToRequest
            'TODO new name : _ResponseToBufer
            Dim MBTrame As New DT_TrameModbus

            With MBTrame.MBAP
                .TransactionID = (CUInt(buffer(0)) << 8) Or buffer(1)
                .ProtocoleID = (CUInt(buffer(2)) << 8) Or buffer(3)
                .Length = (CUInt(buffer(5)) << 8) Or buffer(4)
                .UnitID = buffer(6)
            End With

            MBTrame.functionCode = buffer(7)

            With MBTrame.reqPDU
                Select Case MBTrame.functionCode
                    Case 1, 2 'FC1 : Read nCoils     FC2 : Read nDiscrete Inputs
                        .StartAddr = (CUInt(buffer(8)) << 8) Or buffer(9)
                        .NbReg = (CUInt(buffer(10)) << 8) Or buffer(11)

                        'Check the max amount of requested registers (max : 2000)
                        If .NbReg < 1 OrElse .NbReg > 2000 Then
                            MBTrame.HasError = True
                            'todo : check the exception codes
                            MBTrame.ExceptionCode = 2 'ILLEGAL DATA ADRESS (Adress or Number of Registers not supported)
                        End If

                    Case 3, 4 'FC3 : Read nWords     FC4 : Read nInput Words
                        .StartAddr = (CUInt(buffer(8)) << 8) Or buffer(9)
                        .NbReg = (CUInt(buffer(10)) << 8) Or buffer(11)

                        'Check the max amount of requested registers (max : 123)
                        If .NbReg >= 123 Then
                            MBTrame.HasError = True
                            MBTrame.ExceptionCode = 2 'ILLEGAL DATA ADRESS (Adress or Number of Registers not supported)
                        End If

                    Case 5 'FC5 : Write one bit
                        .StartAddr = (CUInt(buffer(8)) << 8) Or buffer(9)

                        'Check the requested value of register (FF00 or 0000)
                        ReDim .BitsToWrite(0)
                        Select Case buffer(10)
                            Case &H_FF
                                .BitsToWrite(0) = True '0xFF00 => True
                            Case &H_00
                                .BitsToWrite(0) = False '0x0000 => False
                            Case Else
                                MBTrame.HasError = True
                                MBTrame.ExceptionCode = 2 'ILLEGAL DATA ADRESS (Adress or value not supported)
                        End Select

                    Case 6 'FC6 : Write one word
                        ReDim .WordsToWrite(0)
                        .StartAddr = (CUInt(buffer(8)) << 8) Or buffer(9)
                        .WordsToWrite(0) = (CUInt(buffer(10)) << 8) Or buffer(11)

                    Case 15 'FC15 : Write nBits
                        .StartAddr = (CUInt(buffer(8)) << 8) Or buffer(9)
                        .NbReg = (CUInt(buffer(10)) << 8) Or buffer(11)

                        'Check the max amount of requested registers (max : 1968)
                        If .NbReg >= 1968 Then
                            MBTrame.HasError = True
                            MBTrame.ExceptionCode = 2 'ILLEGAL DATA ADRESS (Adress or value not supported)
                        End If

                        ReDim .BitsToWrite(.NbReg - 1)
                        Dim bufIndex, posBit As UInt16
                        For i = 0 To .NbReg - 1
                            bufIndex = 13 + Math.Truncate(i / 8)
                            posBit = i - (8 * Math.Truncate(i / 8))
                            'Extract each bits separatly (bitmask and test if bit is NOT 0)
                            .BitsToWrite(i) = CByte(buffer(bufIndex) And Math.Pow(2, posBit)) <> 0
                        Next

                    Case 16 'FC16 : Write nMots
                        .StartAddr = (CUInt(buffer(8)) << 8) Or buffer(9)
                        .NbReg = (CUInt(buffer(10)) << 8) Or buffer(11)

                        'Check the max amount of requested registers (max : 120)
                        If .NbReg >= 120 Then
                            MBTrame.HasError = True
                            MBTrame.ExceptionCode = 2 'ILLEGAL DATA ADRESS (Adress or value not supported)
                        End If

                        ReDim .WordsToWrite(.NbReg - 1)
                        Dim bufIndex As UInt16
                        For i = 0 To .NbReg - 1
                            bufIndex = 13 + i * 2
                            .WordsToWrite(i) = CUInt(buffer(bufIndex)) << 8 Or buffer(bufIndex + 1)
                        Next

                    Case Else
                        MBTrame.HasError = True
                        MBTrame.ExceptionCode = 1 'ILLEGAL FUNCTION (Function Code not supported)
                        Throw New Exception("Function Code not valid !")
                End Select
            End With

            Return MBTrame
        End Function

        ''' <summary>
        ''' Build the Modbus response buffer
        ''' </summary>
        ''' <param name="MBFrame"> Structure received from a client </param>
        ''' <returns> Buffer containing the Modbus Response </returns>
        Private Function BuildResBuffer(MBFrame As DT_TrameModbus) As Byte()
            Dim buffer(7) As Byte

            'Assign MBAP header to buffer
            With MBFrame.MBAP
                buffer(0) = .TransactionID >> 8
                buffer(1) = .TransactionID And &H_00FF
                buffer(2) = .ProtocoleID >> 8
                buffer(3) = .ProtocoleID And &H_00FF
                buffer(4) = .Length >> 8
                buffer(5) = .Length And &H_00FF
                buffer(6) = .UnitID
            End With

            With MBFrame.respPDU
                Select Case MBFrame.functionCode
                    Case 1, 2 'FC1 : Read nBits      'FC2 : Read nDiscrete Inputs
                        ReDim Preserve buffer(7 + 2 + .nbBytes - 1)
                        buffer(7) = MBFrame.functionCode
                        buffer(8) = .nbBytes

                        Dim bufIndex, posBit As UInt16
                        For i = 0 To .BitsState.Length - 1
                            bufIndex = 9 + Math.Truncate(i / 8)
                            posBit = i - (8 * Math.Truncate(i / 8))
                            buffer(bufIndex) = buffer(bufIndex) Or (IIf(.BitsState(i), 1, 0) * Math.Pow(2, posBit)) 'i doit tourner entre 0 et 7
                        Next

                    Case 3, 4 'FC3 : Read nWords     FC4 : Read nInput Words
                        ReDim Preserve buffer(7 + 2 + .nbBytes * 2 - 1)
                        buffer(7) = MBFrame.functionCode
                        buffer(8) = .nbBytes

                        For i = 0 To .nbBytes - 1
                            'Put bits in order in the buffer
                            buffer(9 + i * 2) = .WordsValue(i) >> 8
                            buffer(10 + i * 2) = .WordsValue(i) And &H_00FF
                        Next

                    Case 5, 6 'FC5 : Write one bit      FC6 : Write one word
                        'todo revoir la partie "PDU_Resp" de la fct "UpdateReg" qui doit contenir l'adresse du début (en plus des datas)
                        ReDim Preserve buffer(7 + 3 + 2) 'MBAP + FC/addrStart + datas
                        buffer(7) = MBFrame.functionCode
                        buffer(8) = MBFrame.reqPDU.StartAddr >> 8
                        buffer(9) = MBFrame.reqPDU.StartAddr And &H_00FF

                        If MBFrame.functionCode = 5 Then
                            'FC5 : Write one bit (FF00: True   0000: False)
                            buffer(10) = IIf(MBFrame.reqPDU.BitsToWrite(0), &H_FF, &H_00)
                            buffer(11) = &H_00
                        Else
                            'FC6 : Write one word
                            buffer(10) = MBFrame.reqPDU.WordsToWrite(0) >> 8
                            buffer(11) = MBFrame.reqPDU.WordsToWrite(0) And &H_00FF
                        End If

                    Case 15, 16 'FC15 : Write nBits     FC16 : Write nMots
                        ReDim Preserve buffer(7 + 3 + 2) 'MBAP + FC/addrStart + datas
                        buffer(7) = MBFrame.functionCode
                        buffer(8) = MBFrame.reqPDU.StartAddr >> 8
                        buffer(9) = MBFrame.reqPDU.StartAddr And &H_00FF

                        'Put number of registers in buffer
                        buffer(10) = MBFrame.reqPDU.NbReg >> 8
                        buffer(11) = MBFrame.reqPDU.NbReg And &H_00FF

                    Case Else
                        Throw New Exception("Function Code not valid !")
                End Select
            End With

            Return buffer
        End Function
#End Region

#Region "PRIVATE - Update of registers of the Modbus Serveur"
        ''' <summary>
        ''' Update registers (FC 5/6/15/16) or put registers data in Response PDU (FC 1/2/3/4)
        ''' </summary>
        ''' <param name="curCoils"> R/W Coils used for the current frame </param>
        ''' <param name="curInputsCoils"> R Coils used for the current frame </param>
        ''' <param name="curHoldingReg"> R/W Registers used for the current frame </param>
        ''' <param name="curInputReg"> R Registers used for the current frame </param>
        ''' <returns> return a bool that indicate if a register has changed </returns>
        Private Function _UpdateRegisters(ByRef MBFrame As DT_TrameModbus,
                                          ByRef curCoils As RegistersBits,
                                          ByRef curInputsCoils As RegistersBits,
                                          ByRef curHoldingReg As RegistersWords,
                                          ByRef curInputReg As RegistersWords) As Boolean
            With MBFrame.respPDU
                Select Case MBFrame.functionCode
                    Case 1, 2 'FC1 : Read nBits     FC2 : Read nDiscrete Inputs
                        .nbBytes = MBFrame.reqPDU.NbReg / 8 'nbBytes = (nb of bits) / 8  ; if the remainder is different than 0, add 1 to nbBytes
                        If MBFrame.reqPDU.NbReg Mod 8 <> 0 Then
                            .nbBytes += 1
                        End If

                        ReDim .BitsState(MBFrame.reqPDU.NbReg - 1)
                        For index = 0 To .BitsState.Length - 1
                            If MBFrame.functionCode = 1 Then
                                'FC1 : Read nBits
                                .BitsState(index) = curCoils(MBFrame.reqPDU.StartAddr + index)
                            Else
                                'FC2 : Read nDiscrete Inputs
                                .BitsState(index) = curInputsCoils(MBFrame.reqPDU.StartAddr + index)
                            End If
                        Next

                    Case 3, 4 'FC3 : Read nWords    FC4 : Read nInput Words
                        .nbBytes = MBFrame.reqPDU.NbReg

                        ReDim .WordsValue(.nbBytes - 1)
                        For index = 0 To .nbBytes - 1
                            If MBFrame.functionCode = 3 Then
                                .WordsValue(index) = curHoldingReg(MBFrame.reqPDU.StartAddr + index)
                            Else
                                .WordsValue(index) = curInputReg(MBFrame.reqPDU.StartAddr + index)
                            End If
                        Next

                    Case 5 'FC5 : Write one bit
                        curCoils(MBFrame.reqPDU.StartAddr) = MBFrame.reqPDU.BitsToWrite(0)

                    Case 6 'FC6 : Write one word
                        curHoldingReg(MBFrame.reqPDU.StartAddr) = MBFrame.reqPDU.WordsToWrite(0)

                    Case 15, 16 'FC15 : Write nBits     FC16 : Write nWords
                        .nbBytes = MBFrame.reqPDU.NbReg
                        For i = 0 To .nbBytes - 1
                            If MBFrame.functionCode = 15 Then
                                curCoils(MBFrame.reqPDU.StartAddr + i) = MBFrame.reqPDU.BitsToWrite(i)
                            Else
                                curHoldingReg(MBFrame.reqPDU.StartAddr + i) = MBFrame.reqPDU.WordsToWrite(i)
                            End If
                        Next

                    Case Else
                        Throw New Exception("Code fonction non pris en compte ! : " & CStr(MBFrame.functionCode))
                End Select
            End With

            'Determine if the current request change values of registers
            Select Case MBFrame.functionCode
                Case 5, 6, 15, 16 'FC5/6 : write one reg    FC15/16 : Write nReg    
                    Return True
                Case Else
                    Return False
            End Select
        End Function

#End Region
    End Class

#Region "REGISTERS-CLASS"
    Public Class RegistersWords
        Public localArray As UInt16() = New UInt16(65534) {}

        Default Public Property Item(ByVal x As Integer) As UInt16
            Get
                Return Me.localArray(x)
            End Get
            Set(ByVal value As UInt16)
                Me.localArray(x) = value
            End Set
        End Property
    End Class

    Public Class RegistersBits
        Public localArray As Boolean() = New Boolean(65534) {}

        Default Public Property Item(ByVal x As Integer) As Boolean
            Get
                Return Me.localArray(x)
            End Get
            Set(ByVal value As Boolean)
                Me.localArray(x) = value
            End Set
        End Property
    End Class

#End Region

End Namespace
