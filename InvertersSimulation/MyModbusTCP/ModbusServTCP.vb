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
        Private tcpHandler As TCPHandler
        Public numOfConnex As UInt16
        Private isGateway As Boolean = False 'Si le serveur simule aussi des esclaves RTU (= passerelle MB TCP -> RTU)
        Public RTUSlaves() As ModbusSlave = Nothing

        Public holdingRegisters As RegistersWords
        'Public inputRegisters As RegistersWords
        Public coils As RegistersBits
        'Public discreteInputs As RegistersBits

        Public Event ClientConnected(ByVal client As TcpClient)
        Public Event ClientDisconnected(ByVal client As TcpClient)
        Public Event HoldingRegistersChanged(ByVal UnitID As UInt16, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        'Public Event InputRegistersChanged(ByVal UnitID As UInt16, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        Public Event CoilsChanged(ByVal UnitID As UInt16, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)
        'Public Event DiscreteInputsChanged(ByVal UnitID As UInt16, ByVal firstReg As UInt16, ByVal numOfReg As UInt16)


        Private TrameRecue As DT_TrameModbus
        Private TrameEnvoi As _DT_PDU_Resp

        Public FC1Disabled As Boolean
        'Public FC2Disabled As Boolean
        Public FC3Disabled As Boolean
        'Public FC4Disabled As Boolean
        Public FC5Disabled As Boolean
        Public FC6Disabled As Boolean
        Public FC15Disabled As Boolean
        Public FC16Disabled As Boolean

        Private debug As Boolean = False
#End Region

#Region "STRUCTURES"
        Public Structure DT_TrameModbus
            Dim FunctionCode As Byte
            Dim MBAPHeader As _DT_MBAPHeader
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
        Public Sub New(ByVal port As Integer)
            tcpHandler = New TCPHandler(port)
            _Construct()
        End Sub

        Public Sub New(ByVal port As Integer, ByVal numOfSlaves As Int16)
            tcpHandler = New TCPHandler(port)
            isGateway = True
            ReDim RTUSlaves(numOfSlaves)
            _Construct()
        End Sub

        Public Sub New(ByVal IP As IPAddress, ByVal port As Integer)
            tcpHandler = New TCPHandler(IP, port)
            _Construct()
        End Sub
        Public Sub New(ByVal IP As IPAddress, ByVal port As Integer, ByVal numOfSlaves As Int16)
            tcpHandler = New TCPHandler(IP, port)
            isGateway = True
            ReDim RTUSlaves(numOfSlaves)
            _Construct()
        End Sub

        Private Sub _Construct()
            coils = New RegistersBits()
            holdingRegisters = New RegistersWords()
            AddHandler tcpHandler.ClientConnected, AddressOf _TcpHandler_ClientConnected
            AddHandler tcpHandler.ClientDisconnected, AddressOf _TcpHandler_ClientDisconnected
            AddHandler tcpHandler.DataReceived, AddressOf _TcpHandler_DataReceived
        End Sub
#End Region

#Region "PUBLIC-FUNCTIONS"
        Public Sub StartListening()
            tcpHandler.Start()
        End Sub

        Public Sub StopListening()
            tcpHandler.StopServ()
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
            Dim sss As String = ""
            For i = 0 To 5 + buffer(5)
                sss &= CStr(buffer(i))
            Next

            TrameRecue = _ExtractFromBuffer(buffer)

            _AnalyzeTrameRecue(TrameRecue)


            _UpdateRegisters(TrameRecue)


            Console.WriteLine(sss)
        End Sub
#End Region

#Region "PRIVATE - Gestion Buffers"
        ''' <summary>
        ''' Construction des MBAPHeader et PDU (le PDU utilisé dépend du code fonction)
        ''' </summary>
        ''' <param name="buffer"> Requête envoyée par un client connecté </param>
        Private Function _ExtractFromBuffer(ByVal buffer() As Byte) As DT_TrameModbus
            Dim MBTrame As New DT_TrameModbus

            With MBTrame.MBAPHeader
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

        'Private Function _ConstructBuffer(ByVal functionCode As Int16, ByVal MBAP As DT_MBAPHeader, ByVal PDU As DT_BasicPDU) As Byte()
        '    Dim buffer(20) As Byte

        '    'FC15 ou FC16 => Ecriture plusieurs mots/bits
        '    If MBAP.FunctionCode = 15 Or MBAP.FunctionCode = 16 Then

        '    End If

        'End Function
#End Region

#Region "PRIVATE - MàJ des registres du serveur MB"
        Private Sub _UpdateRegisters(ByVal trameRecue As DT_TrameModbus)

        End Sub

        ''' <summary>
        ''' Tous ces codes fonctions ont été testé au 13/06/2021 (sauf FC15, car pas implementé)
        ''' </summary>
        ''' <param name="trameRecue"></param>
        ''' <returns> Tableau de 1 + x index : FC, adresses des registres modifiés </returns>
        Private Function _AnalyzeTrameRecue(ByVal trameRecue As DT_TrameModbus) As UInt16()
            TrameEnvoi = New _DT_PDU_Resp
            With TrameEnvoi
                Select Case trameRecue.FunctionCode
                    Case 1 'FC1 : Read nBits
                        'nbBytes = (nb de bits) / 8  ; Si le reste est différent de 0, nbBytes += 1
                        .nbBytes = trameRecue.PDU.NbToRead / 8
                        If trameRecue.PDU.NbToRead Mod 8 <> 0 Then
                            .nbBytes += 1
                        End If

                        ReDim .BitsState(trameRecue.PDU.NbToRead - 1)
                        For index = 0 To trameRecue.PDU.NbToRead - 1
                            .BitsState(index) = coils(trameRecue.PDU.StartAddr + index)
                        Next

                    Case 3 'FC3 : Read nMots
                        'nbBytes = nb de registre demandé
                        .nbBytes = trameRecue.PDU.NbToRead

                        ReDim .WordsValue(trameRecue.PDU.NbToRead - 1)
                        For index = 0 To trameRecue.PDU.NbToRead - 1
                            .WordsValue(index) = holdingRegisters(trameRecue.PDU.StartAddr + index)
                        Next

                    Case 5 'FC5 : Write one bit
                        coils(trameRecue.PDU.StartAddr) = trameRecue.PDU.WOne.BitToWrite
                    Case 6 'FC6 : Write one word
                        holdingRegisters(trameRecue.PDU.StartAddr) = trameRecue.PDU.WOne.WordToWrite
                    Case 15 'FC15 : Write nBits
                        .nbBytes = trameRecue.PDU.NbToWrite
                        For index = trameRecue.PDU.StartAddr To trameRecue.PDU.NbToWrite - 1
                            coils(index) = trameRecue.PDU.WMult.BitsToWrite(index)
                        Next
                    Case 16 'FC16 : Write nMots
                        .nbBytes = trameRecue.PDU.NbToWrite
                        For index = 0 To trameRecue.PDU.NbToWrite - 1
                            holdingRegisters(trameRecue.PDU.StartAddr + index) = trameRecue.PDU.WMult.WordsToWrite(index)
                        Next
                    Case Else
                        Throw New Exception("Code fonction non pris en compte ! : " & CStr(trameRecue.FunctionCode))
                End Select
            End With

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
