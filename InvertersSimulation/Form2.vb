Imports EasyModbus
Public Class Form2
	Private WithEvents servMB As EasyModbus.ModbusServer

	Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		servMB = New EasyModbus.ModbusServer()
		servMB.Port = 504

		servMB.LogFileFilename = "../../../test.txt"

		servMB.UnitIdentifier = 0
		servMB.Listen()


	End Sub

	Private Sub HoldingRegistersChanged(ByVal register As Integer, ByVal numberOfRegisters As Integer) Handles servMB.HoldingRegistersChanged
		MsgBox(register & "  " & numberOfRegisters)
		For Each trame In servMB.ModbusLogData
			If trame IsNot Nothing Then
				Dim ds As Int16
				ds = 5541

			End If
		Next
	End Sub

	Private Sub NumClientChanged() Handles servMB.NumberOfConnectedClientsChanged
		'MsgBox("clients ")
		Console.WriteLine(servMB.NumberOfConnections)
	End Sub
End Class