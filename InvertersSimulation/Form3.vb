Imports System.Net
Imports System.Net.Sockets

Public Class Form3
	Private WithEvents s As MyModbusTCP.TCPHandler

	Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load

		s = New MyModbusTCP.TCPHandler(502)
		s.Start()
		AddHandler s.ClientConnected, AddressOf s_ClientConnected
		AddHandler s.ClientDisconnected, AddressOf s_ClientDisconnected
		AddHandler s.DataReceived, AddressOf s_DataReceived

	End Sub

	Private Sub s_ClientConnected(ByRef client As TcpClient) 'Handles s.ClientConnected
		Console.WriteLine("Connected : " & client.Client.RemoteEndPoint.ToString())
	End Sub

	Private Sub s_ClientDisconnected(ByRef client As TcpClient) 'Handles s.ClientConnected
		Console.WriteLine("Disconnected : " & client.Client.RemoteEndPoint.ToString())
	End Sub

	Private Sub s_DataReceived(ByRef buffer() As Byte) 'Handles s.DataReceived
		Dim sss As String = ""
		For i = 0 To 5 + buffer(5)
			sss = sss & CStr(buffer(i))
		Next
		Console.WriteLine(sss)
	End Sub

	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		s.Close()
	End Sub

	Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
		'si le serveur tcp est arrêté, on le lance
		If Not s.isServerOn Then
			s.Start()
		End If
	End Sub
End Class