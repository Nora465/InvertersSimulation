<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SettingsForm
	Inherits System.Windows.Forms.Form

	'Form remplace la méthode Dispose pour nettoyer la liste des composants.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Requise par le Concepteur Windows Form
	Private components As System.ComponentModel.IContainer

	'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
	'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
	'Ne la modifiez pas à l'aide de l'éditeur de code.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.BP_AbortChanges = New System.Windows.Forms.Button()
		Me.BP_AcceptChanges = New System.Windows.Forms.Button()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.TB_NumInv = New System.Windows.Forms.NumericUpDown()
		Me.CB_SelecONDType = New System.Windows.Forms.ComboBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.TB_TCPPort = New System.Windows.Forms.NumericUpDown()
		Me.Label3 = New System.Windows.Forms.Label()
		CType(Me.TB_NumInv, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GroupBox1.SuspendLayout()
		CType(Me.TB_TCPPort, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'BP_AbortChanges
		'
		Me.BP_AbortChanges.BackColor = System.Drawing.Color.Red
		Me.BP_AbortChanges.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.BP_AbortChanges.Location = New System.Drawing.Point(16, 140)
		Me.BP_AbortChanges.Name = "BP_AbortChanges"
		Me.BP_AbortChanges.Size = New System.Drawing.Size(145, 44)
		Me.BP_AbortChanges.TabIndex = 0
		Me.BP_AbortChanges.Text = "Abort Changes"
		Me.BP_AbortChanges.UseVisualStyleBackColor = False
		'
		'BP_AcceptChanges
		'
		Me.BP_AcceptChanges.BackColor = System.Drawing.Color.Chartreuse
		Me.BP_AcceptChanges.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.BP_AcceptChanges.ForeColor = System.Drawing.SystemColors.ControlText
		Me.BP_AcceptChanges.Location = New System.Drawing.Point(167, 140)
		Me.BP_AcceptChanges.Name = "BP_AcceptChanges"
		Me.BP_AcceptChanges.Size = New System.Drawing.Size(145, 44)
		Me.BP_AcceptChanges.TabIndex = 1
		Me.BP_AcceptChanges.Text = "Accept Changes"
		Me.BP_AcceptChanges.UseVisualStyleBackColor = False
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(116, 26)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(94, 13)
		Me.Label2.TabIndex = 12
		Me.Label2.Text = "Number of inverter"
		'
		'TB_NumInv
		'
		Me.TB_NumInv.Location = New System.Drawing.Point(117, 41)
		Me.TB_NumInv.Name = "TB_NumInv"
		Me.TB_NumInv.Size = New System.Drawing.Size(93, 20)
		Me.TB_NumInv.TabIndex = 11
		Me.TB_NumInv.Value = New Decimal(New Integer() {10, 0, 0, 0})
		'
		'CB_SelecONDType
		'
		Me.CB_SelecONDType.FormattingEnabled = True
		Me.CB_SelecONDType.Location = New System.Drawing.Point(6, 41)
		Me.CB_SelecONDType.Name = "CB_SelecONDType"
		Me.CB_SelecONDType.Size = New System.Drawing.Size(99, 21)
		Me.CB_SelecONDType.TabIndex = 9
		Me.CB_SelecONDType.Text = "Inverter"
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(6, 25)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(98, 13)
		Me.Label1.TabIndex = 13
		Me.Label1.Text = "Select your inverter"
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.CB_SelecONDType)
		Me.GroupBox1.Controls.Add(Me.Label2)
		Me.GroupBox1.Controls.Add(Me.Label1)
		Me.GroupBox1.Controls.Add(Me.TB_NumInv)
		Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(221, 74)
		Me.GroupBox1.TabIndex = 14
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Selection of Inverter"
		'
		'TB_TCPPort
		'
		Me.TB_TCPPort.Location = New System.Drawing.Point(263, 38)
		Me.TB_TCPPort.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
		Me.TB_TCPPort.Name = "TB_TCPPort"
		Me.TB_TCPPort.Size = New System.Drawing.Size(59, 20)
		Me.TB_TCPPort.TabIndex = 14
		Me.TB_TCPPort.Value = New Decimal(New Integer() {502, 0, 0, 0})
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(262, 22)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(60, 13)
		Me.Label3.TabIndex = 14
		Me.Label3.Text = "Server Port"
		'
		'SettingsForm
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(443, 200)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.TB_TCPPort)
		Me.Controls.Add(Me.GroupBox1)
		Me.Controls.Add(Me.BP_AcceptChanges)
		Me.Controls.Add(Me.BP_AbortChanges)
		Me.Name = "SettingsForm"
		Me.Text = "SettingsForm"
		CType(Me.TB_NumInv, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		CType(Me.TB_TCPPort, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents BP_AbortChanges As Button
	Friend WithEvents BP_AcceptChanges As Button
	Friend WithEvents Label2 As Label
	Friend WithEvents TB_NumInv As NumericUpDown
	Friend WithEvents CB_SelecONDType As ComboBox
	Friend WithEvents Label1 As Label
	Friend WithEvents GroupBox1 As GroupBox
	Friend WithEvents TB_TCPPort As NumericUpDown
	Friend WithEvents Label3 As Label
End Class
