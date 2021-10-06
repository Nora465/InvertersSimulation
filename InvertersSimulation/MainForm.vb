Public Class MainForm



#Region "STRUCTURES"
	Public Enum ENUM_LogType 'Type of logging in file
		'todo make a module to handle the file logging ?
		Off = 0
		Basic = 1
		Verbose = 2
	End Enum

	Public Structure DT_GlobalSettings
		Public LogType As ENUM_LogType
		Public InverterUsed As String
	End Structure

#End Region

#Region "GLOBALS VARIABLES"
	Public WithEvents mbs As MyModbusTCP.ModbusServer
	Public globalSettings As DT_GlobalSettings
	Public InvData As BASE_INV

#End Region

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		'Opening Modbus Server
		mbs = New MyModbusTCP.ModbusServer(502, True, 10) 'todo a refaire avec les settings

		mbs.StartListening()
		AddHandler mbs.ClientConnected, AddressOf mbs_ClientConnected
		AddHandler mbs.ClientDisconnected, AddressOf mbs_ClientDisconnected
		AddHandler mbs.GatewayCoilsChanged, AddressOf mbs_GatewayCoilsChanged
		AddHandler mbs.GatewayRegistersChanged, AddressOf mbs_GatewayHoldingRegistersChanged
		AddHandler mbs.SlaveCoilsChanged, AddressOf mbs_SlaveCoilsChanged
		AddHandler mbs.SlaveRegistersChanged, AddressOf mbs_SlaveHoldingRegChanged
		AddHandler mbs.ErrorOccured, AddressOf mbs_ErrorOccured

	End Sub

	Private Sub BP_OpenSettingsForm_Click(sender As Object, e As EventArgs) Handles BP_OpenSettingsForm.Click
		'Open Settings Form (as a modal windows = keep focus)
		SettingsForm.ShowDialog()
	End Sub

#Region "MB Server - Events"
	Private Sub mbs_ClientConnected()

	End Sub
	Private Sub mbs_ClientDisconnected()

	End Sub
	Private Sub mbs_GatewayCoilsChanged()

	End Sub
	Private Sub mbs_GatewayHoldingRegistersChanged()

	End Sub
	Private Sub mbs_SlaveCoilsChanged()

	End Sub
	Private Sub mbs_SlaveHoldingRegChanged()

	End Sub
	Private Sub mbs_ErrorOccured()

	End Sub
#End Region

End Class