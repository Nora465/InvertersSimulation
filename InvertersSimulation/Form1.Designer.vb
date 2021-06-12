<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
		Me.Button1 = New System.Windows.Forms.Button()
		Me.TextPort = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.TextBox2 = New System.Windows.Forms.TextBox()
		Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
		Me.SuspendLayout()
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(291, 12)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(75, 23)
		Me.Button1.TabIndex = 0
		Me.Button1.Text = "Lancer"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'TextPort
		'
		Me.TextPort.Location = New System.Drawing.Point(232, 12)
		Me.TextPort.Name = "TextPort"
		Me.TextPort.Size = New System.Drawing.Size(53, 20)
		Me.TextPort.TabIndex = 1
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(169, 15)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(57, 13)
		Me.Label1.TabIndex = 2
		Me.Label1.Text = "Connexion"
		'
		'TextBox2
		'
		Me.TextBox2.Location = New System.Drawing.Point(12, 59)
		Me.TextBox2.Multiline = True
		Me.TextBox2.Name = "TextBox2"
		Me.TextBox2.Size = New System.Drawing.Size(401, 220)
		Me.TextBox2.TabIndex = 3
		'
		'BackgroundWorker1
		'
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(800, 450)
		Me.Controls.Add(Me.TextBox2)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.TextPort)
		Me.Controls.Add(Me.Button1)
		Me.Name = "Form1"
		Me.Text = "Form1"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents Button1 As Button
	Friend WithEvents TextPort As TextBox
	Friend WithEvents Label1 As Label
	Friend WithEvents TextBox2 As TextBox
	Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
End Class
