Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.ComponentModel 'Contient le worker
Imports System.Threading

Namespace MyModbusTCP
	Class TCPHandler
		Private WithEvents _bgWorker As BackgroundWorker
		Private WithEvents _server As TcpListener = Nothing
		Private _localAddrIP As IPAddress = IPAddress.Any

		Public listOfClients(9) As TcpClient '10 clients possible
		Public numOfClients As UInt16

		Public Event DataReceived(ByRef client As TcpClient, ByRef buffer() As Byte)
		Public Event ClientConnected(ByRef client As TcpClient)
		Public Event ClientDisconnected(ByRef client As TcpClient)

		Public isServerOn As Boolean = False

#Region "CONSTRUCTEURS"
		''' <summary>
		''' Ecoute sur toutes les interfaces réseaux
		''' </summary>
		''' <param name="port"> Port d'écoute du serveur TCP </param>
		Public Sub New(ByVal port As Integer)
			_server = New TcpListener(_localAddrIP, port)
		End Sub

		''' <summary>
		''' Ecoute sur une interface réseau spécifique
		''' </summary>
		''' <param name="IP"></param>
		''' <param name="port"></param>
		Public Sub New(ByVal IP As IPAddress, ByVal port As Integer)
			Me._localAddrIP = IP
			_server = New TcpListener(_localAddrIP, port)
		End Sub
#End Region

#Region "PUBLIC"
		''' <summary>
		''' Démarre le serveur
		''' </summary>
		Public Sub Start()
			If Not isServerOn Then
				_server.Start()
				isServerOn = True
				_bgWorker = New BackgroundWorker With {
					.WorkerSupportsCancellation = True
				}
				If Not _bgWorker.IsBusy Then
					_bgWorker.RunWorkerAsync()
				End If
			End If

		End Sub
		''' <summary>
		''' Arrête le serveur
		''' </summary>
		Public Sub StopServ()
			'Fermeture des connexions
			If numOfClients <> 0 Then
				For Each client In listOfClients
					client.Client.Close()
					client.Close()
				Next
			End If
			'Fermeture du serveur
			If isServerOn Then
				_server.Stop()
				isServerOn = False
			End If
		End Sub

		''' <summary>
		''' Envoi une trame au client désigné
		''' </summary>
		''' <param name="client"> Client connecté sur ce serveur </param>
		''' <param name="buffer"> Buffer d'octets à envoyer </param>
		Public Sub SendBuffer(ByRef client As TcpClient, ByRef buffer() As Byte)
			Dim networkStream As NetworkStream = client.GetStream()
			networkStream.Write(buffer, 0, buffer.Length)
		End Sub
#End Region

		Private Sub bgWorker_DoWork() Handles _bgWorker.DoWork
			While isServerOn
				'Si un client tente de se connecter
				If _server.Pending() Then
					For i = 0 To 9
						If listOfClients(i) Is Nothing Then
							Dim client As TcpClient = _server.AcceptTcpClient()

							listOfClients(i) = client
							numOfClients += 1
							client.ReceiveTimeout = 4000
							RaiseEvent ClientConnected(client)
							Exit For
						End If
					Next
				End If

				For Each client In listOfClients
					If client IsNot Nothing AndAlso client.Connected Then

						Try
							'Si on detecte un client déconnecté
							If client.Client.Poll(5000, SelectMode.SelectRead) Then

								' Vérifie si le client est connecté
								Dim buff As Byte() = New Byte(0) {}
								If client.Client.Receive(buff, SocketFlags.Peek) = 0 Then
									RaiseEvent ClientDisconnected(client)
									client.Close()

									'suppression du client déconnecté dans la listOfClients
									Dim indexToDel As Integer = Array.IndexOf(listOfClients, client)
									If indexToDel <> -1 Then
										listOfClients(indexToDel) = Nothing
										numOfClients -= 1
										Continue For 'goto la boucle suivante du for
									End If
								End If
							End If
						Catch ex As Exception
							MsgBox("Erreur TCPHandler/ligne 130 : " & vbCrLf & ex.Message)
						End Try

						'Si un client envoi une trame
						Dim networkStream As NetworkStream = client.GetStream()

						If networkStream.DataAvailable Then
							Dim buffer(256) As Byte
							networkStream.Read(buffer, 0, buffer.Length)
							RaiseEvent DataReceived(client, buffer)
						End If
					End If
				Next

				Thread.Sleep(50)
			End While

			_bgWorker.CancelAsync()
		End Sub
	End Class
End Namespace
