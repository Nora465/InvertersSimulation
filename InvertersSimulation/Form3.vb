Imports System.Net
Imports System.Net.Sockets

Public Class Form3
	Private WithEvents mbs As MyModbusTCP.ModbusServer

	Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		mbs = New MyModbusTCP.ModbusServer(503)
		mbs.StartListening()
		AddHandler mbs.ClientConnected, AddressOf mbs_ClientConnected
		AddHandler mbs.ClientDisconnected, AddressOf mbs_ClientDisconnected
	End Sub

	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		mbs.StopListening()
	End Sub

	Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
		mbs.StartListening()
	End Sub

	Private Sub mbs_ClientConnected(ByVal client As TcpClient)
		Console.WriteLine("New client Connected : " & client.Client.RemoteEndPoint.ToString())
	End Sub

	Private Sub mbs_ClientDisconnected(ByVal client As TcpClient)
		Console.WriteLine("A client has disconnected itself : " & client.Client.RemoteEndPoint.ToString())
	End Sub
End Class