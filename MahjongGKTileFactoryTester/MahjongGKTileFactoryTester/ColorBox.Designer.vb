<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ColorBox
    Inherits System.Windows.Forms.UserControl

    'UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblColorLabel = New System.Windows.Forms.Label()
        Me.lblColorText = New System.Windows.Forms.Label()
        Me.txtColor = New System.Windows.Forms.TextBox()
        Me.btnColor = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout()
        '
        'lblColorLabel
        '
        Me.lblColorLabel.BackColor = System.Drawing.Color.DimGray
        Me.lblColorLabel.Location = New System.Drawing.Point(355, 2)
        Me.lblColorLabel.Name = "lblColorLabel"
        Me.lblColorLabel.Size = New System.Drawing.Size(26, 20)
        Me.lblColorLabel.TabIndex = 107
        '
        'lblColorText
        '
        Me.lblColorText.Font = New System.Drawing.Font("Cascadia Mono", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblColorText.Location = New System.Drawing.Point(5, 3)
        Me.lblColorText.Name = "lblColorText"
        Me.lblColorText.Size = New System.Drawing.Size(234, 20)
        Me.lblColorText.TabIndex = 106
        Me.lblColorText.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtColor
        '
        Me.txtColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtColor.Font = New System.Drawing.Font("Cascadia Mono", 9.75!)
        Me.txtColor.Location = New System.Drawing.Point(239, 1)
        Me.txtColor.Name = "txtColor"
        Me.txtColor.Size = New System.Drawing.Size(113, 23)
        Me.txtColor.TabIndex = 105
        Me.txtColor.Tag = ""
        '
        'btnColor
        '
        Me.btnColor.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.24!)
        Me.btnColor.Location = New System.Drawing.Point(384, 0)
        Me.btnColor.Name = "btnColor"
        Me.btnColor.Size = New System.Drawing.Size(53, 22)
        Me.btnColor.TabIndex = 104
        Me.btnColor.Text = "BF - Pic"
        Me.btnColor.UseVisualStyleBackColor = True
        '
        'ColorBox
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.lblColorLabel)
        Me.Controls.Add(Me.lblColorText)
        Me.Controls.Add(Me.txtColor)
        Me.Controls.Add(Me.btnColor)
        Me.Font = New System.Drawing.Font("Cascadia Mono", 9.75!)
        Me.Name = "ColorBox"
        Me.Size = New System.Drawing.Size(441, 23)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblColorLabel As Label
    Friend WithEvents lblColorText As Label
    Friend WithEvents txtColor As TextBox
    Friend WithEvents btnColor As Button
    Friend WithEvents ToolTip1 As ToolTip
End Class
