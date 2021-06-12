Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Text

Public Class Form1
    Dim MonServeur As TcpListener 'Le serveur
    Dim LesClients As List(Of TcpClient) 'Les clients TCP
    Dim counteur As Integer = 0


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try 'Au cas d'erreur
            If (Button1.Text = "Lancer") Then
                MonServeur = New TcpListener(IPAddress.Parse("127.0.0.1"), Integer.Parse(TextPort.Text))
                LesClients = New List(Of TcpClient)
                MonServeur.Start()
                Dim Context As TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
                Task.Factory.StartNew(Sub() Accepter(Context), CancellationToken.None, TaskCreationOptions.LongRunning)
                Button1.Text = "Arrêter"
            Else
                For Each client As TcpClient In LesClients
                    client.Close() 'Fermer la connexion avec tous les clients.
                Next
                MonServeur.Stop()
                Button1.Text = "Lancer"
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Erreur") 'Message d'erreur
        End Try

    End Sub

    Private Sub Accepter(ByVal Context As TaskScheduler) 'Accepter les clients
        Try
            While (True)
                Dim NouveauClient As TcpClient = MonServeur.AcceptTcpClient()
                LesClients.Add(NouveauClient)
                counteur = counteur + 1
                Task.Run(Sub() LireLesMessages(NouveauClient.GetStream(), "Client " + counteur.ToString(), Context)) 'Si t'as pas de Task.Run donc utilise Task.Factory.StartNew()
            End While
        Catch ex As Exception
            Exit Sub
        End Try
    End Sub


    Private Sub LireLesMessages(ByVal stream As NetworkStream, ByVal LeNom As String, ByVal context As TaskScheduler) 'Lire les données envoyées par les clients.
        Try
            Task.Factory.StartNew(Sub() MessageReçu(LeNom + " connecté."), CancellationToken.None, TaskCreationOptions.None, context)
            Dim buffer(4096) As Byte
            While (True)
                Dim lu As Integer = stream.Read(buffer, 0, buffer.Length)
                If (lu > 0) Then
                    Dim Message As String = Encoding.UTF8.GetString(buffer, 0, lu)
                    Task.Factory.StartNew(Sub() MessageReçu(LeNom + ": " + Message), CancellationToken.None, TaskCreationOptions.None, context)
                Else
                    Task.Factory.StartNew(Sub() MessageReçu(LeNom + ": déconnecté."), CancellationToken.None, TaskCreationOptions.None, context)
                    Exit Sub
                End If
            End While

        Catch ex As Exception

        End Try
    End Sub

    Private Sub MessageReçu(ByVal leMessage)
        TextBox2.AppendText(leMessage + vbNewLine)
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

    End Sub
End Class