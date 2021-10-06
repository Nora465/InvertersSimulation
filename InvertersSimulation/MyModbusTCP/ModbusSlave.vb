
Namespace MyModbusTCP
	''' <summary>
	''' Represent a RTU Slave (connected to the Modbus Server by serial)
	''' </summary>
	Public Class ModbusSlave

		Public AddressMB As UInt16 'Serial address (from 1 to 254)

		Public holdingRegisters As RegistersWords
		Public inputRegisters As RegistersWords
		Public coils As RegistersBits
		Public discreteInputs As RegistersBits

		Public Sub New(ByVal addressModbus As UInt16)
			AddressMB = addressModbus
			holdingRegisters = New RegistersWords
			inputRegisters = New RegistersWords
			coils = New RegistersBits
			discreteInputs = New RegistersBits
		End Sub
	End Class
End Namespace