Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On

Imports System.IO
Imports MahjongGK.Contracts.GlobalEnum
Imports TileFactory

Public Class frmTileColorFilesCheck
    Inherits Form

    Private ReadOnly _lvw As New ListView()
    Private ReadOnly _fontNormal As Font
    Private ReadOnly _fontStrike As Font

    Public Sub New()

        Me.Text = "TileColors-Dateien prüfen"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1300, 750)

        _fontNormal = New Font(Me.Font, FontStyle.Regular)
        _fontStrike = New Font(Me.Font, FontStyle.Strikeout)

        With _lvw
            .Dock = DockStyle.Fill
            .View = View.Details
            .FullRowSelect = True
            .GridLines = True
            .HideSelection = False
            .Font = _fontNormal
        End With

        _lvw.Columns.Add("Pfadtyp", 110)
        _lvw.Columns.Add("Design", 110)
        _lvw.Columns.Add("Satz", 80)
        _lvw.Columns.Add("Font", 80)
        _lvw.Columns.Add("Vorhanden", 90)
        _lvw.Columns.Add("Geändert am", 150)
        _lvw.Columns.Add("FullPath", 700)

        Me.Controls.Add(_lvw)

        AddHandler Me.Load, AddressOf frmTileColorFilesCheck_Load

    End Sub

    Private Sub frmTileColorFilesCheck_Load(sender As Object, e As EventArgs)

        FillList()

    End Sub

    Private Sub FillList()

        _lvw.BeginUpdate()
        _lvw.Items.Clear()

        Dim existsDev As New Dictionary(Of String, Boolean)()
        Dim existsProject As New Dictionary(Of String, Boolean)()

        For Each steinDesign As SteinDesign In DirectCast([Enum].GetValues(GetType(SteinDesign)), SteinDesign())

            For Each steinSatz As SteinSatz In DirectCast([Enum].GetValues(GetType(SteinSatz)), SteinSatz())

                For Each steinFont As SteinFont In DirectCast([Enum].GetValues(GetType(SteinFont)), SteinFont())

                    Dim key As String = GetCompareKey(steinDesign, steinSatz, steinFont)

                    Dim devPath As String =
                   TileColors.GetFullPathOnlyForSaving(steinDesign, steinSatz, steinFont, True)

                    Dim projectPath As String =
                TileColors.GetFullPathOnlyForSaving(steinDesign, steinSatz, steinFont, False)

                    existsDev(key) = File.Exists(devPath)
                    existsProject(key) = File.Exists(projectPath)

                Next

            Next

        Next

        For Each useDevelopmentPath As Boolean In New Boolean() {True, False}

            For Each steinDesign As SteinDesign In DirectCast([Enum].GetValues(GetType(SteinDesign)), SteinDesign())

                For Each steinSatz As SteinSatz In DirectCast([Enum].GetValues(GetType(SteinSatz)), SteinSatz())

                    For Each steinFont As SteinFont In DirectCast([Enum].GetValues(GetType(SteinFont)), SteinFont())

                        Dim key As String = GetCompareKey(steinDesign, steinSatz, steinFont)

                        Dim isInconsistent As Boolean =
                        existsDev(key) <> existsProject(key)

                        AddFileItem(steinDesign, steinSatz, steinFont, useDevelopmentPath, isInconsistent)

                    Next

                Next

            Next

        Next

        _lvw.EndUpdate()

    End Sub

    Private Sub AddFileItem(steinDesign As SteinDesign,
                        steinSatz As SteinSatz,
                        steinFont As SteinFont,
                        useDevelopmentPath As Boolean,
                        isInconsistent As Boolean)

        Dim fullPath As String = TileColors.GetFullPathOnlyForSaving(steinDesign, steinSatz, steinFont, useDevelopmentPath)

        Dim fileExists As Boolean = File.Exists(fullPath)

        Dim pathTyp As String
        If useDevelopmentPath Then
            pathTyp = "Entwicklung"
        Else
            pathTyp = "Projekt"
        End If

        Dim changedText As String = ""

        If fileExists Then
            Dim lastWrite As DateTime = File.GetLastWriteTime(fullPath)
            changedText = lastWrite.ToString("dd.MM.yyyy HH:mm:ss")
        End If

        Dim item As New ListViewItem(pathTyp)

        item.SubItems.Add(steinDesign.ToString())
        item.SubItems.Add(steinSatz.ToString())
        item.SubItems.Add(steinFont.ToString())
        item.SubItems.Add(If(fileExists, "Ja", "Nein"))
        item.SubItems.Add(changedText)
        item.SubItems.Add(fullPath)

        If fileExists Then
            item.Font = _fontNormal
        Else
            item.Font = _fontStrike
        End If

        If isInconsistent Then
            item.BackColor = Color.Moccasin
            item.ForeColor = Color.DarkRed
        ElseIf fileExists Then
            item.ForeColor = Color.Black
        Else
            item.ForeColor = Color.Gray
        End If

        _lvw.Items.Add(item)

    End Sub
    Private Function GetCompareKey(steinDesign As SteinDesign,
                                   steinSatz As SteinSatz,
                                   steinFont As SteinFont) As String

        Return steinDesign.ToString() & "|" &
               steinSatz.ToString() & "|" &
               steinFont.ToString()

    End Function

End Class
