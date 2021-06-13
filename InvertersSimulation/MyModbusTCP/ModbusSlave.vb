Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.ComponentModel 'Contient le worker
Imports System.Threading

Namespace MyModbusTCP
	Public Class ModbusSlave

		Public AddressMB As UInt16
		Public holdingRegisters As RegistersWords
		'Public inputRegisters As RegistersWords
		Public coils As RegistersBits
		'Public discreteInputs As RegistersBits

		Public Sub New(ByVal addressModbus As UInt16)
			AddressMB = addressModbus
		End Sub
	End Class
End Namespace