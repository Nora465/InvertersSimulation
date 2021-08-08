Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.ComponentModel 'Contient le worker
Imports System.Threading

Namespace MyModbusTCP
    ''' <summary>
    ''' Ce serveur supporte les fonctions :
    ''' 01 (R nBits)
    ''' 05 (W oneBit)
    ''' 15 (W nBits) - ERR:NOT IMPLEMENTED (flemme)
    ''' 
    ''' 03 (R nMots holding)
    ''' 06 (W oneWord holding)
    ''' 16 (W nMots holding)
    ''' </summary>
    Public Class ModbusServer
#Region "DEFINITIONS GLOBALES"
        Private _tcpHandler As TCPHandler
        Public _numOfConnex As UInt16

        Public RTUSlaves() As ModbusSlave = Nothing

        Public holdingRegisters As RegistersWords
        'Public inputRegisters As RegistersWords
        Public coils As RegistersBits
        'Public discreteInputs As RegistersBits

        Public Event ClientConnected(ByVal client As TcpClient)
        Public Event ClientDisconnected(ByVal client As TcpClient)

        'Events for "gateway" (UnitID = 0)
        Public Event GatewayHoldingRegistersChanged(ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        'Public Event GatewayInputRegistersChanged(ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        Public Event GatewayCoilsChanged(ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        'Public Event GatewayDiscreteInputsChanged(ByVal firstReg As UInt16, ByVal numOfReg As UInt16)

        'Events for the RTU slaves (UnitID <> 0)
        Public Event SlaveHoldingRegChanged(ByRef RTUSlave As ModbusSlave, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        'Public Event SlaveInputRegChanged(ByRef RTUSlave As ModbusSlave)
        Public Event SlaveCoilsChanged(ByRef RTUSlave As ModbusSlave, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        'Public Event SlaveDiscreteInputsChanged(ByRef RTUSlave As ModbusSlave)

        Private _MBRequest As DT_TrameModbus
        Private _MBResponse As _DT_PDU_Resp

        'Parameters
        Public FC1Disabled As Boolean
        Public FC2Disabled As Boolean
        Public FC3Disabled As Boolean
        Public FC4Disabled As Boolean
        Public FC5Disabled As Boolean
        Public FC6Disabled As Boolean
        'Public FC15Disabled As Boolean
        Public FC16Disabled As Boolean

        Private _hasSlaves As Boolean = False 'Si le serveur simule des esclaves RTU (= passerelle MB TCP -> RTU)
        Private _useBigEndian As Boolean = True 'TODO Ajouter la gestion du Big Endian ou Little Endian

        Private _debug As Boolean = False
#End Region

#Region "STRUCTURES"
        Public Structure DT_TrameModbus
            Dim FunctionCode As Byte
            Dim MBAP As _DT_MBAPHeader
            Dim PDU As _DT_PDU_Requ
        End Structure

        Public Structure _DT_PDU_Resp
            Dim IsError As Boolean
            Dim ExceptionCode As Byte

            Dim nbBytes As UInt16

            Dim BitsState() As Boolean
            Dim WordsValue() As UInt16

        End Structure

        Public Structure _DT_MBAPHeader 'DT : DataType
            Dim TransactionID As UInt16 '2 octets : pour identifier la transaction
            Dim ProtocoleID As UInt16 '2 octets : pour identifier le protocole (ici, Modbus => 0)
            Dim Length As UInt16 '2 octets : Taille à partir du UnitID jusqu'à la fin du message
            Dim UnitID As Byte '1 octet : Identifie le numéro de l'esclave
            '(si passerelle MB TCP vers MB RTU, mettre l'adresse esclave (sinon, mettre 0 (pour atteindre la passerelle))
        End Structure

        Public Structure _DT_PDU_Requ
            Dim StartAddr As UInt16 '2 octets : Adresse de Début (FC1 ou 3) /OU/ Adresse du Bit/Mot à écrire (FC5 ou 6)
            Dim NbToRead As UInt16 '2 octets : Nombre de variables à lire (FC1 2 3 4)
            Dim NbToWrite As UInt16 '2 octets : Nombre de variables à écrire (FC15 ou 16)

            Dim WOne As _PDU_W_unique
            Dim WMult As _PDU_W_Multiple
        End Structure
        Public Structure _PDU_W_unique
            'Reservé à l'écriture Unique (W one word/bit)
            Dim BitToWrite As Boolean 'bool : Valeur du bit à écrire (FC5)
            Dim WordToWrite As UInt16 '2 octets : Valeur ou Etat d'un Word (FC6)
        End Structure
        Public Structure _PDU_W_Multiple
            Dim BitsToWrite() As Boolean 'Bools : Valeurs des bits à écrire (FC15)
            Dim WordsToWrite() As UInt16 '2 octets : Valeurs des mots à écrire (FC16)
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
            Me.holdingRegisters = New RegistersWords()

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

        Private Sub _TcpHandler_DataReceived(ByRef buffer() As Byte) 'handles tcpHandler.DataReceived
            If _debug Then
                Dim debugStr As String = ""
                For i = 0 To 5 + buffer(5)
                    debugStr &= CStr(buffer(i))
                Next
                Console.WriteLine(debugStr)
            End If

            _MBRequest = Me._ExtractFromBuffer(buffer)

            Dim regHasChanged As Boolean

            'The request is for the server itself
            If _MBRequest.MBAP.UnitID = 255 Or Not Me._hasSlaves Then
                regHasChanged = Me._UpdateRegisters(Me.holdingRegisters, Me.coils)
                If regHasChanged Then RaiseEvent GatewayCoilsChanged(_MBRequest.PDU.StartAddr, 1)

            Else 'The request is for a RTU slave
                Dim slave As ModbusSlave = RTUSlaves(_MBRequest.MBAP.UnitID - 1)
                regHasChanged = Me._UpdateRegisters(slave.holdingRegisters, slave.coils)

                If regHasChanged Then RaiseEvent SlaveCoilsChanged(slave, _MBRequest.PDU.StartAddr, 1)
            End If

        End Sub
#End Region

#Region "PRIVATE - Gestion Buffers"
        ''' <summary>
        ''' Construction des MBAPHeader et PDU (le PDU utilisé dépend du code fonction)
        ''' </summary>
        ''' <param name="buffer"> Requête envoyée par un client connecté </param>
        Private Function _ExtractFromBuffer(buffer() As Byte) As DT_TrameModbus
            Dim MBTrame As New DT_TrameModbus

            With MBTrame.MBAP
                .TransactionID = (buffer(0) << 8) Or buffer(1)
                .ProtocoleID = (buffer(2) << 8) Or buffer(3)
                .Length = (buffer(5)) << 8 Or buffer(4)
                .UnitID = buffer(6)
            End With

            MBTrame.FunctionCode = buffer(7)

            With MBTrame.PDU
                Select Case MBTrame.FunctionCode
                    Case 1, 3 'FC1 : Read nBits      FC3 : Read nMots
                        'VALIDé 13/06/2021
                        .StartAddr = (buffer(8) << 8) Or buffer(9)
                        .NbToRead = (buffer(10) << 8) Or buffer(11)
                        '.TotalLength = 4 'nb d'octets total dans le PDU             'Vraiment utile ?

                    Case 5 'FC5 : Write one bit
                        'VALIDé 13/06/2021 (manque la vérif pour les 2 cas de la valeur bool)
                        .StartAddr = (buffer(8) << 8) Or buffer(9)
                        .WOne.BitToWrite = (buffer(10) = &H_FF) '0x0000 => False  0xFF00=> True 
                        'TODO faire une vérification de la valeur du buffer(10) au cas où
                    Case 6 'FC6 : Write one word
                        'Validé 13/06/2021
                        .StartAddr = (buffer(8) << 8) Or buffer(9)
                        .WOne.WordToWrite = (buffer(10) << 8) Or buffer(11)

                    Case 15 'FC15 : Write nBits
                        ' ????
                        .StartAddr = (buffer(8) << 8) Or buffer(9)
                        .NbToWrite = (buffer(10) << 8) Or buffer(11)
                        Throw New NotImplementedException()
                        ReDim .WMult.BitsToWrite(.NbToWrite - 1)
                        For index = 0 To .NbToWrite - 1
                            .WMult.BitsToWrite(index) = CUInt(buffer(13 + index * 2)) << 8 Or buffer(14 + index * 2)
                        Next
                    Case 16 'FC16 : Write nMots
                        ' Validé 13/06/2021
                        .StartAddr = (buffer(8) << 8) Or buffer(9)
                        .NbToWrite = (buffer(10) << 8) Or buffer(11)

                        ReDim .WMult.WordsToWrite(.NbToWrite - 1)
                        For index = 0 To .NbToWrite - 1
                            .WMult.WordsToWrite(index) = CUInt(buffer(13 + index * 2)) << 8 Or buffer(14 + index * 2)
                        Next
                    Case Else
                        Throw New Exception("Code fonction non pris en compte !")
                End Select
            End With

            Return MBTrame
        End Function

#End Region

#Region "PRIVATE - MàJ des registres du serveur MB"
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="curRegisters"></param>
        ''' <param name="curCoils"></param>
        ''' <returns> return a bool that indicate if a register has changed </returns>
        Private Function _UpdateRegisters(ByRef curRegisters As RegistersWords, ByRef curCoils As RegistersBits) As Boolean
            Me._MBResponse = New _DT_PDU_Resp
            Dim regHasChanged As Boolean = False

            With Me._MBResponse
                Select Case Me._MBRequest.FunctionCode
                    Case 1 'FC1 : Read nBits
                        'Compute the number of bytes for the Response PDU
                        .nbBytes = _MBRequest.PDU.NbToRead / 8 'nbBytes = (nb de bits) / 8  ; Si le reste est différent de 0, nbBytes += 1
                        If _MBRequest.PDU.NbToRead Mod 8 <> 0 Then
                            .nbBytes += 1
                        End If

                        ReDim .BitsState(Me._MBRequest.PDU.NbToRead - 1)
                        For index = 0 To .BitsState.Length
                            .BitsState(index) = curCoils(_MBRequest.PDU.StartAddr + index)
                        Next

                    Case 3 'FC3 : Read nMots
                        'nbBytes = nb of registers wanted
                        .nbBytes = _MBRequest.PDU.NbToRead

                        ReDim .WordsValue(_MBRequest.PDU.NbToRead - 1)
                        For index = 0 To _MBRequest.PDU.NbToRead - 1
                            .WordsValue(index) = curRegisters(_MBRequest.PDU.StartAddr + index)
                        Next

                    Case 5 'FC5 : Write one bit
                        curCoils(_MBRequest.PDU.StartAddr) = _MBRequest.PDU.WOne.BitToWrite
                        regHasChanged = True

                    Case 6 'FC6 : Write one word
                        curRegisters(_MBRequest.PDU.StartAddr) = _MBRequest.PDU.WOne.WordToWrite
                        regHasChanged = True

                    Case 15 'FC15 : Write nBits
                        .nbBytes = _MBRequest.PDU.NbToWrite
                        For index = _MBRequest.PDU.StartAddr To _MBRequest.PDU.NbToWrite - 1
                            curCoils(index) = _MBRequest.PDU.WMult.BitsToWrite(index)
                        Next
                        regHasChanged = True

                    Case 16 'FC16 : Write nMots
                        .nbBytes = _MBRequest.PDU.NbToWrite
                        For index = 0 To _MBRequest.PDU.NbToWrite - 1
                            curRegisters(_MBRequest.PDU.StartAddr + index) = _MBRequest.PDU.WMult.WordsToWrite(index)
                        Next
                        regHasChanged = True
                    Case Else
                        Throw New Exception("Code fonction non pris en compte ! : " & CStr(_MBRequest.FunctionCode))
                End Select
            End With

            Return regHasChanged
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
