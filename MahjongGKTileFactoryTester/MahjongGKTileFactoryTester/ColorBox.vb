Imports System.ComponentModel
Imports System.Globalization
Imports ColorPickerHsb

'Public Class ColorBoxEventArgs
'    Sub New()

'    End Sub
'    Sub New(color As Color)
'        Me.Color = color
'    End Sub

'    Public Color As Color
'End Class

Public Class ColorBox

    Public Event ColorChanged(sender As Object, e As EventArgs)

    Private _color As Color

    <Browsable(True),
    Category("Appearance"),
    Description("Die angezeigte Farbe."),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property Color As Color
        Get
            Return _color
        End Get
        Set(value As Color)
            _color = value
            Dim txtCol As String = _color.A.ToString("X2", CultureInfo.InvariantCulture) &
                                   _color.R.ToString("X2", CultureInfo.InvariantCulture) &
                                   _color.G.ToString("X2", CultureInfo.InvariantCulture) &
                                   _color.B.ToString("X2", CultureInfo.InvariantCulture)

            txtColor.Text = txtCol
            Dim name As String = SetTextBoxBackColorIsNamedColor(txtColor)
            ToolTip1.SetToolTip(lblColorLabel, name)
            SetLabelBackColor(_color)
            Me.Invalidate()
            RaiseEvent ColorChanged(Me, New EventArgs)
        End Set
    End Property

    <Browsable(True),
     Category("Appearance"),
     Description("Die Farbe im Format AARRGGBB"),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
     Bindable(True)>
    Public Property ColorText As String
        Get
            Return txtColor.Text
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then
                value = String.Empty
            End If
            Dim col As Color
            If TryParseArgbHexColor(value, col) Then
                _color = col

                Dim txtCol As String = _color.A.ToString("X2", CultureInfo.InvariantCulture) &
                        _color.R.ToString("X2", CultureInfo.InvariantCulture) &
                        _color.G.ToString("X2", CultureInfo.InvariantCulture) &
                        _color.B.ToString("X2", CultureInfo.InvariantCulture)

                'txtColor.BackColor = SystemColors.Control
                SetLabelBackColor(col)
                Dim name As String = SetTextBoxBackColorIsNamedColor(txtColor)
                ToolTip1.SetToolTip(lblColorLabel, name)
                txtColor.Text = txtCol
            Else

                SetLabelBackColor(SystemColors.Control)
                txtColor.BackColor = Color.Red
                ToolTip1.SetToolTip(lblColorLabel, String.Empty)
                txtColor.Text = value
            End If
            Me.Invalidate()
            RaiseEvent ColorChanged(Me, New EventArgs)
        End Set
    End Property

    <Browsable(True),
     Category("Appearance"),
     Description("Die Farbe im Format AARRGGBB"),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
     Bindable(True)>
    Public Overrides Property Text As String
        Get
            Return lblColorText.Text
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then
                value = String.Empty
            End If
            lblColorText.Text = value
            Me.Invalidate()
        End Set
    End Property
    Private Function SetTextBoxBackColorIsNamedColor(txtColor As TextBox) As String

        Dim name As String = String.Empty

        If txtColor.Text.StartsWith("00") Then
            txtColor.BackColor = Color.Orange
        ElseIf IsEquivalentNamedColor(txtColor.Text, name) Then
            txtColor.BackColor = Color.Gray
        Else
            txtColor.BackColor = SystemColors.Control
        End If
        Return name

    End Function

    Private Function IsEquivalentNamedColor(argbHexText As String, ByRef name As String) As Boolean

        Dim c As Color
        name = String.Empty

        If Not TryParseArgbHexColor(argbHexText, c) Then
            Return False
        End If

        For Each kc As KnownColor In [Enum].GetValues(GetType(KnownColor))
            Dim known As Color = Color.FromKnownColor(kc)

            If known.ToArgb() = c.ToArgb() Then
                name = known.Name
                Return True
            End If
        Next

        Return False
    End Function

    Private Function TryParseArgbHexColor(text As String, ByRef result As Color) As Boolean

        result = Color.Empty

        If text Is Nothing Then Return False

        Dim s As String = text.Trim()

        If s.StartsWith("#", StringComparison.Ordinal) Then
            s = s.Substring(1)
        End If

        If s.StartsWith("&H", StringComparison.OrdinalIgnoreCase) Then
            s = s.Substring(2)
        End If

        If s.Length <> 8 Then Return False

        Dim value As UInteger
        If Not UInteger.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, value) Then
            Return False
        End If

        Dim raw As Integer = BitConverter.ToInt32(BitConverter.GetBytes(value), 0)
        result = Color.FromArgb(raw)

        Return True

    End Function

    Private Sub btnColorBen_MouseClick(sender As Object, e As MouseEventArgs) Handles btnColor.MouseClick

        Dim btn As Button = DirectCast(sender, Button)

        If e.X < (btn.ClientSize.Width \ 2) Then
            ' linker ColorPicker
            Using frm As New ColorPickerNamedColors
                frm.SelectedColor = Me.Color
                If frm.ShowDialog() = DialogResult.OK Then
                    _color = frm.SelectedColor
                End If
            End Using
        Else
            ' rechter ColorPicker
            Using frm As New ColorPickerHsb.ColorPickerHSB
                frm.SelectedColor = Me.Color
                If frm.ShowDialog() = DialogResult.OK Then
                    _color = frm.SelectedColor
                End If
            End Using
        End If

        Dim txtCol As String = _color.A.ToString("X2", CultureInfo.InvariantCulture) &
                               _color.R.ToString("X2", CultureInfo.InvariantCulture) &
                               _color.G.ToString("X2", CultureInfo.InvariantCulture) &
                               _color.B.ToString("X2", CultureInfo.InvariantCulture)

        txtColor.Text = txtCol
        Dim name As String = SetTextBoxBackColorIsNamedColor(txtColor)
        ToolTip1.SetToolTip(lblColorLabel, name)
        SetLabelBackColor(_color)

    End Sub

    Private Sub txtColor_TextChanged(sender As Object, e As EventArgs) Handles txtColor.TextChanged
        ColorText = txtColor.Text
    End Sub

    Private Sub SetLabelBackColor(color As Color)
        If color.A = 0 Then
            lblColorLabel.BackColor = SystemColors.Control
            lblColorLabel.Text = "X"
        Else
            lblColorLabel.BackColor = color
            lblColorLabel.Text = ""
        End If
    End Sub

End Class
