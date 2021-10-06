Public Class SettingsForm
    Dim tempSettings As MainForm.DT_GlobalSettings
    Private Sub SettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UpdateAvailableInv(CB_SelecONDType)
    End Sub

    Private Sub BP_AbortChanges_Click(sender As Object, e As EventArgs) Handles BP_AbortChanges.Click
        'Close this form without applying changes
        Me.Close()
    End Sub

    Private Sub BP_AcceptChanges_Click(sender As Object, e As EventArgs) Handles BP_AcceptChanges.Click
        'Check if settings are corrects

        'Create an instance of the selected inverter
        Dim paramInstance(0) As Byte
        paramInstance(0) = TB_NumInv.Value
        Dim typeSelectedINV As Type = Type.GetType("InvertersSimulation." & CB_SelecONDType.SelectedItem)
        MainForm.InvData = Activator.CreateInstance(typeSelectedINV, paramInstance)

        'Pass these settings to the mainform
        MainForm.globalSettings = tempSettings

        Me.Close()
    End Sub

    Private Sub CB_SelecONDType_Click(sender As Object, e As EventArgs) Handles CB_SelecONDType.Click
        UpdateAvailableInv(CB_SelecONDType)
    End Sub

    Private Sub UpdateAvailableInv(ByRef CB_OndType As ComboBox)
        'Load all INV_*.vb in the folder
        Dim dir As New IO.DirectoryInfo("../../InvertersDefinitions/")
        Dim files As IO.FileInfo() = dir.GetFiles("INV_*.vb")

        'Delete all the previous inverter's definitions
        CB_OndType.Items.Clear()

        Dim nameOfINV As String
        For Each file In files
            nameOfINV = file.Name.Split(".")(0)
            CB_OndType.Items.Add(nameOfINV)
        Next

        CB_OndType.SelectedIndex = 0
    End Sub
End Class