Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On

Imports System.IO
Imports MahjongGK.Contracts
Imports TileFactory

Public Enum LadeNach
    Light
    Medium
    Dark
End Enum

Public Class FrmTileColorsLaden
    Inherits Form

    Private ReadOnly cboSteinDesign As New ComboBox()
    Private ReadOnly cboSteinSatz As New ComboBox()
    Private ReadOnly cboSteinFont As New ComboBox()
    Private ReadOnly chkUseDevelopmentPath As New CheckBox()

    Private ReadOnly grpLadenNach As New GroupBox()
    Private ReadOnly rdbNachLight As New RadioButton()
    Private ReadOnly rdbNachMedium As New RadioButton()
    Private ReadOnly rdbNachDark As New RadioButton()

    Private ReadOnly btnAbbrechen As New Button()
    Private ReadOnly btnLaden As New Button()

    Public Property GeladeneTileColors As TileColors = Nothing
    Public Property AusgewaehltLadeNach As LadeNach = LadeNach.Medium

    Public Sub New()

        Me.Text = "TileColors laden (Überschreibt ohne Rückfrage!)"
        Me.Font = New Font(Me.Font.FontFamily, 12.0F, FontStyle.Regular)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ClientSize = New Size(430, 330)

        InitControls()
        FillCombos()
        WireEvents()

        UpdateLoadState()
    End Sub

    Private Sub InitControls()

        Dim lblDesign As New Label()
        lblDesign.Text = "SteinDesign:"
        lblDesign.Location = New Point(20, 25)
        lblDesign.AutoSize = True

        cboSteinDesign.DropDownStyle = ComboBoxStyle.DropDownList
        cboSteinDesign.Location = New Point(170, 20)
        cboSteinDesign.Width = 220

        Dim lblSatz As New Label()
        lblSatz.Text = "SteinSatz:"
        lblSatz.Location = New Point(20, 70)
        lblSatz.AutoSize = True

        cboSteinSatz.DropDownStyle = ComboBoxStyle.DropDownList
        cboSteinSatz.Location = New Point(170, 65)
        cboSteinSatz.Width = 220

        Dim lblFont As New Label()
        lblFont.Text = "SteinFont:"
        lblFont.Location = New Point(20, 115)
        lblFont.AutoSize = True

        cboSteinFont.DropDownStyle = ComboBoxStyle.DropDownList
        cboSteinFont.Location = New Point(170, 110)
        cboSteinFont.Width = 220

        chkUseDevelopmentPath.Text = "DevelopmentPath verwenden"
        chkUseDevelopmentPath.Location = New Point(20, 155)
        chkUseDevelopmentPath.AutoSize = True

        grpLadenNach.Text = "Laden nach"
        grpLadenNach.Location = New Point(20, 195)
        grpLadenNach.Size = New Size(370, 75)

        rdbNachLight.Text = "Light"
        rdbNachLight.Location = New Point(20, 32)
        rdbNachLight.AutoSize = True

        rdbNachMedium.Text = "Medium"
        rdbNachMedium.Location = New Point(130, 32)
        rdbNachMedium.AutoSize = True

        rdbNachDark.Text = "Dark"
        rdbNachDark.Location = New Point(260, 32)
        rdbNachDark.AutoSize = True

        grpLadenNach.Controls.Add(rdbNachLight)
        grpLadenNach.Controls.Add(rdbNachMedium)
        grpLadenNach.Controls.Add(rdbNachDark)

        btnAbbrechen.Text = "Abbrechen"
        btnAbbrechen.Location = New Point(170, 285)
        btnAbbrechen.Size = New Size(105, 32)
        btnAbbrechen.DialogResult = DialogResult.Cancel

        btnLaden.Text = "Laden"
        btnLaden.Location = New Point(285, 285)
        btnLaden.Size = New Size(105, 32)
        btnLaden.DialogResult = DialogResult.OK

        Me.Controls.Add(lblDesign)
        Me.Controls.Add(cboSteinDesign)
        Me.Controls.Add(lblSatz)
        Me.Controls.Add(cboSteinSatz)
        Me.Controls.Add(lblFont)
        Me.Controls.Add(cboSteinFont)
        Me.Controls.Add(chkUseDevelopmentPath)
        Me.Controls.Add(grpLadenNach)
        Me.Controls.Add(btnAbbrechen)
        Me.Controls.Add(btnLaden)

        Me.AcceptButton = btnLaden
        Me.CancelButton = btnAbbrechen
    End Sub

    Private Sub FillCombos()

        cboSteinDesign.DataSource = [Enum].GetValues(GetType(SteinDesign))
        cboSteinSatz.DataSource = [Enum].GetValues(GetType(SteinSatz))
        cboSteinFont.DataSource = [Enum].GetValues(GetType(SteinFont))

        cboSteinDesign.SelectedItem = SteinDesign.Default
        cboSteinSatz.SelectedItem = SteinSatz.Medium
        cboSteinFont.SelectedItem = SteinFont.Segoe

        rdbNachLight.Checked = True
    End Sub

    Private Sub WireEvents()

        AddHandler cboSteinDesign.SelectedIndexChanged, AddressOf SelectionChanged
        AddHandler cboSteinSatz.SelectedIndexChanged, AddressOf SelectionChanged
        AddHandler cboSteinFont.SelectedIndexChanged, AddressOf SelectionChanged
        AddHandler chkUseDevelopmentPath.CheckedChanged, AddressOf SelectionChanged

        AddHandler btnLaden.Click, AddressOf BtnLaden_Click
        AddHandler btnAbbrechen.Click, AddressOf BtnAbbrechen_Click
    End Sub

    Private Sub SelectionChanged(sender As Object, e As EventArgs)
        UpdateLoadState()
    End Sub

    Private Sub UpdateLoadState()

        Dim fullPath As String = GetSelectedFullPath()

        If File.Exists(fullPath) Then
            GeladeneTileColors = TileColors.Load(GetSelectedSteinDesign(),
                                                 GetSelectedSteinSatz(),
                                                 GetSelectedSteinFont(),
                                                 chkUseDevelopmentPath.Checked)
            btnLaden.Enabled = True
        Else
            GeladeneTileColors = Nothing
            btnLaden.Enabled = False
        End If
    End Sub

    Private Function GetSelectedFullPath() As String

        Return TileColors.GetFullPathOnlyForSaving(GetSelectedSteinDesign(),
                                                   GetSelectedSteinSatz(),
                                                   GetSelectedSteinFont(),
                                                   chkUseDevelopmentPath.Checked)
    End Function

    Private Function GetSelectedSteinDesign() As SteinDesign
        Return DirectCast(cboSteinDesign.SelectedItem, SteinDesign)
    End Function

    Private Function GetSelectedSteinSatz() As SteinSatz
        Return DirectCast(cboSteinSatz.SelectedItem, SteinSatz)
    End Function

    Private Function GetSelectedSteinFont() As SteinFont
        Return DirectCast(cboSteinFont.SelectedItem, SteinFont)
    End Function

    Private Sub BtnLaden_Click(sender As Object, e As EventArgs)

        UpdateLoadState()

        If GeladeneTileColors Is Nothing Then
            Me.DialogResult = DialogResult.None
            Return
        End If

        AusgewaehltLadeNach = GetLadeNach()
        Me.Close()
    End Sub

    Private Sub BtnAbbrechen_Click(sender As Object, e As EventArgs)

        GeladeneTileColors = Nothing
        Me.Close()
    End Sub

    Private Function GetLadeNach() As LadeNach

        If rdbNachLight.Checked Then
            Return LadeNach.Light
        End If

        If rdbNachMedium.Checked Then
            Return LadeNach.Medium
        End If

        If rdbNachDark.Checked Then
            Return LadeNach.Dark
        End If
        Return LadeNach.Medium

    End Function

End Class