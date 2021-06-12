Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Net.Sockets
Imports System.Net
Imports System.Threading
Imports System.Net.NetworkInformation
Imports System.IO.Ports

Namespace MyModbusTCP

    'Friend Class TCPHandler

    '    Public Delegate Sub DataChanged(ByVal networkConnectionParameter As Object)
    '    Public Event dataChangedEvent As DataChanged
    '    Public Delegate Sub NumberOfClientsChanged()
    '    Public Event numberOfClientsChangedEvent As NumberOfClientsChanged

    '    Private server As TcpListener = Nothing
    '    Private tcpClientLastRequestList As List(Of TcpClient) = New List(Of TcpClient)()
    '    Public Property NumberOfConnectedClients As Integer
    '    Public ipAddress As String = Nothing

    '    Private localIPAddressField As IPAddress = ipAddress.Any

    '    ''' When making a server TCP listen socket, will listen to this IP address.
    '    Public ReadOnly Property LocalIPAddress As IPAddress
    '        Get
    '            Return localIPAddressField
    '        End Get
    '    End Property

    '    Public Sub New(ByVal port As Integer)
    '        server = New TcpListener(LocalIPAddress, port)
    '        server.Start()
    '        server.BeginAcceptTcpClient(AcceptTcpClientCallback, Nothing)
    '    End Sub
    'End Class

End Namespace
