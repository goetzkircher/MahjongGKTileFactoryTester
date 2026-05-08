<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.numTiefe = New System.Windows.Forms.NumericUpDown()
        Me.optHarteKante = New System.Windows.Forms.RadioButton()
        Me.optKontrastStärker = New System.Windows.Forms.RadioButton()
        Me.optInnenWeicher = New System.Windows.Forms.RadioButton()
        Me.optLinear = New System.Windows.Forms.RadioButton()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.numShrink = New System.Windows.Forms.NumericUpDown()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        CType(Me.numTiefe, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numShrink, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(800, 676)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'Button1
        '
        Me.Button1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Button1.Location = New System.Drawing.Point(417, 607)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "Start"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.numShrink)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.numTiefe)
        Me.GroupBox1.Controls.Add(Me.optHarteKante)
        Me.GroupBox1.Controls.Add(Me.optKontrastStärker)
        Me.GroupBox1.Controls.Add(Me.optInnenWeicher)
        Me.GroupBox1.Controls.Add(Me.optLinear)
        Me.GroupBox1.Location = New System.Drawing.Point(228, 630)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(568, 39)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Verlauf"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(355, 18)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(31, 13)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Tiefe"
        '
        'numTiefe
        '
        Me.numTiefe.Increment = New Decimal(New Integer() {2, 0, 0, 0})
        Me.numTiefe.Location = New System.Drawing.Point(391, 16)
        Me.numTiefe.Maximum = New Decimal(New Integer() {250, 0, 0, 0})
        Me.numTiefe.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.numTiefe.Name = "numTiefe"
        Me.numTiefe.Size = New System.Drawing.Size(53, 20)
        Me.numTiefe.TabIndex = 4
        Me.numTiefe.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'optHarteKante
        '
        Me.optHarteKante.AutoSize = True
        Me.optHarteKante.Location = New System.Drawing.Point(265, 16)
        Me.optHarteKante.Name = "optHarteKante"
        Me.optHarteKante.Size = New System.Drawing.Size(79, 17)
        Me.optHarteKante.TabIndex = 3
        Me.optHarteKante.Text = "HarteKante"
        Me.optHarteKante.UseVisualStyleBackColor = True
        '
        'optKontrastStärker
        '
        Me.optKontrastStärker.AutoSize = True
        Me.optKontrastStärker.Checked = True
        Me.optKontrastStärker.Location = New System.Drawing.Point(160, 16)
        Me.optKontrastStärker.Name = "optKontrastStärker"
        Me.optKontrastStärker.Size = New System.Drawing.Size(99, 17)
        Me.optKontrastStärker.TabIndex = 2
        Me.optKontrastStärker.TabStop = True
        Me.optKontrastStärker.Text = "Kontrast stärker"
        Me.optKontrastStärker.UseVisualStyleBackColor = True
        '
        'optInnenWeicher
        '
        Me.optInnenWeicher.AutoSize = True
        Me.optInnenWeicher.Location = New System.Drawing.Point(63, 16)
        Me.optInnenWeicher.Name = "optInnenWeicher"
        Me.optInnenWeicher.Size = New System.Drawing.Size(91, 17)
        Me.optInnenWeicher.TabIndex = 1
        Me.optInnenWeicher.Text = "innen weicher"
        Me.optInnenWeicher.UseVisualStyleBackColor = True
        '
        'optLinear
        '
        Me.optLinear.AutoSize = True
        Me.optLinear.Location = New System.Drawing.Point(7, 16)
        Me.optLinear.Name = "optLinear"
        Me.optLinear.Size = New System.Drawing.Size(50, 17)
        Me.optLinear.TabIndex = 0
        Me.optLinear.Text = "linear"
        Me.optLinear.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Button2.Enabled = False
        Me.Button2.Location = New System.Drawing.Point(327, 608)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(75, 23)
        Me.Button2.TabIndex = 3
        Me.Button2.Text = "ToClipBoard"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 601)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(166, 65)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Verwendet sind:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "KontrastStärker" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "und die tiefen 10, 30, 60 und 90" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "mit Shrink = " &
    "0" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "sowie Tiefe = 10 und Shrink = -30" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(465, 18)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(37, 13)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Shrink"
        '
        'numShrink
        '
        Me.numShrink.Increment = New Decimal(New Integer() {2, 0, 0, 0})
        Me.numShrink.Location = New System.Drawing.Point(507, 16)
        Me.numShrink.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        Me.numShrink.Minimum = New Decimal(New Integer() {50, 0, 0, -2147483648})
        Me.numShrink.Name = "numShrink"
        Me.numShrink.Size = New System.Drawing.Size(53, 20)
        Me.numShrink.TabIndex = 6
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 676)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.PictureBox1)
        Me.Name = "Form1"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "HPgrLightMapErstellung"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.numTiefe, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numShrink, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Button1 As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents optKontrastStärker As RadioButton
    Friend WithEvents optInnenWeicher As RadioButton
    Friend WithEvents optLinear As RadioButton
    Friend WithEvents Button2 As Button
    Friend WithEvents optHarteKante As RadioButton
    Friend WithEvents numTiefe As NumericUpDown
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents numShrink As NumericUpDown
End Class
