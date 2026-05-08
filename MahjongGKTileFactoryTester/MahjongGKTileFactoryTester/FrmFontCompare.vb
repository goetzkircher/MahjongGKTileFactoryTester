Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On
Imports System.Drawing.Text

Public Class FrmFontCompare

    'Private Sub btnCompareMahjongFonts_Click(sender As Object, e As EventArgs) Handles btnCompareMahjongFonts.Click

    '    Dim bmp As Bitmap = CreateMahjongFontComparisonBitmap(60.0F, 70.0F)

    '    If picCompare.Image IsNot Nothing Then
    '        Dim oldImage As Image = picCompare.Image
    '        picCompare.Image = Nothing
    '        oldImage.Dispose()
    '    End If

    '    picCompare.Image = bmp

    'End Sub

    Private Sub FrmFontCompare_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Dim bmp As Bitmap = CreateMahjongFontComparisonBitmap(60.0F, 70.0F)

        If picCompare.Image IsNot Nothing Then
            Dim oldImage As Image = picCompare.Image
            picCompare.Image = Nothing
            oldImage.Dispose()
        End If

        picCompare.Image = bmp
    End Sub

    Private Function CreateMahjongFontComparisonBitmap(symbolFontSizeSegeo As Single, symbolFontSizeNoto As Single) As Bitmap

        Dim segoeFontName As String = ResolveInstalledFontFamilyName(
            New String() {"Segoe UI Symbol"})

        Dim notoFontName As String = ResolveInstalledFontFamilyName(
            New String() {"Noto Sans Symbols 2", "Noto Sans Symbols2"})

        If segoeFontName = String.Empty Then
            Throw New InvalidOperationException("Font nicht gefunden: Segoe UI Symbol")
        End If

        If notoFontName = String.Empty Then
            Throw New InvalidOperationException("Font nicht gefunden: Noto Sans Symbols 2")
        End If

        Dim firstCodePoint As Integer = &H1F000
        Dim lastCodePoint As Integer = &H1F02B
        Dim rowCount As Integer = lastCodePoint - firstCodePoint + 1

        Dim margin As Integer = 12
        Dim headerHeight As Integer = 34
        Dim rowHeight As Integer = 100

        Dim colCode As Integer = 95
        Dim colSegoe As Integer = 220
        Dim colNoto As Integer = 220
        Dim colInfo As Integer = 220

        Dim bmpWidth As Integer =
            margin + colCode + colSegoe + colNoto + colInfo + margin

        Dim bmpHeight As Integer =
            margin + headerHeight + (rowCount * rowHeight) + margin

        Dim bmp As New Bitmap(bmpWidth, bmpHeight)

        Using g As Graphics = Graphics.FromImage(bmp)

            g.Clear(Color.White)
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit

            Using fontHeader As New Font("Segoe UI", 9.0F, FontStyle.Bold, GraphicsUnit.Point),
                  fontText As New Font("Segoe UI", 9.0F, FontStyle.Regular, GraphicsUnit.Point),
                  fontSegoe As New Font(segoeFontName, symbolFontSizeSegeo, FontStyle.Regular, GraphicsUnit.Point),
                  fontNoto As New Font(notoFontName, symbolFontSizeNoto, FontStyle.Regular, GraphicsUnit.Point),
                  penGrid As New Pen(Color.Silver),
                  brushHeaderBack As New SolidBrush(Color.FromArgb(240, 240, 240)),
                  brushText As New SolidBrush(Color.Black),
                  brushCellBack As New SolidBrush(Color.White),
                  brushAltBack As New SolidBrush(Color.FromArgb(252, 252, 252))

                Dim sfLeft As New StringFormat()
                sfLeft.Alignment = StringAlignment.Near
                sfLeft.LineAlignment = StringAlignment.Center

                Dim sfCenter As New StringFormat()
                sfCenter.Alignment = StringAlignment.Center
                sfCenter.LineAlignment = StringAlignment.Center

                Dim xCode As Integer = margin
                Dim xSegoe As Integer = xCode + colCode
                Dim xNoto As Integer = xSegoe + colSegoe
                Dim xInfo As Integer = xNoto + colNoto

                Dim yHeader As Integer = margin

                g.FillRectangle(brushHeaderBack, xCode, yHeader, colCode, headerHeight)
                g.FillRectangle(brushHeaderBack, xSegoe, yHeader, colSegoe, headerHeight)
                g.FillRectangle(brushHeaderBack, xNoto, yHeader, colNoto, headerHeight)
                g.FillRectangle(brushHeaderBack, xInfo, yHeader, colInfo, headerHeight)

                g.DrawRectangle(penGrid, xCode, yHeader, colCode, headerHeight)
                g.DrawRectangle(penGrid, xSegoe, yHeader, colSegoe, headerHeight)
                g.DrawRectangle(penGrid, xNoto, yHeader, colNoto, headerHeight)
                g.DrawRectangle(penGrid, xInfo, yHeader, colInfo, headerHeight)

                g.DrawString("Codepoint", fontHeader, brushText, New RectangleF(xCode + 6, yHeader, colCode - 12, headerHeight), sfLeft)
                g.DrawString(segoeFontName, fontHeader, brushText, New RectangleF(xSegoe + 6, yHeader, colSegoe - 12, headerHeight), sfLeft)
                g.DrawString(notoFontName, fontHeader, brushText, New RectangleF(xNoto + 6, yHeader, colNoto - 12, headerHeight), sfLeft)
                g.DrawString("Hinweis", fontHeader, brushText, New RectangleF(xInfo + 6, yHeader, colInfo - 12, headerHeight), sfLeft)

                Dim codePoint As Integer
                For codePoint = firstCodePoint To lastCodePoint

                    Dim rowIndex As Integer = codePoint - firstCodePoint
                    Dim yRow As Integer = margin + headerHeight + (rowIndex * rowHeight)

                    Dim rowBrush As Brush =
                        If((rowIndex Mod 2) = 0, brushCellBack, brushAltBack)

                    g.FillRectangle(rowBrush, xCode, yRow, colCode, rowHeight)
                    g.FillRectangle(rowBrush, xSegoe, yRow, colSegoe, rowHeight)
                    g.FillRectangle(rowBrush, xNoto, yRow, colNoto, rowHeight)
                    g.FillRectangle(rowBrush, xInfo, yRow, colInfo, rowHeight)

                    g.DrawRectangle(penGrid, xCode, yRow, colCode, rowHeight)
                    g.DrawRectangle(penGrid, xSegoe, yRow, colSegoe, rowHeight)
                    g.DrawRectangle(penGrid, xNoto, yRow, colNoto, rowHeight)
                    g.DrawRectangle(penGrid, xInfo, yRow, colInfo, rowHeight)

                    Dim s As String = Char.ConvertFromUtf32(codePoint)
                    Dim codeText As String = "U+" & codePoint.ToString("X4")

                    g.DrawString(codeText, fontText, brushText,
                                 New RectangleF(xCode + 6, yRow, colCode - 12, rowHeight), sfLeft)

                    g.DrawString(s, fontSegoe, brushText,
                                 New RectangleF(xSegoe, yRow, colSegoe, rowHeight), sfCenter)

                    g.DrawString(s, fontNoto, brushText,
                                 New RectangleF(xNoto, yRow, colNoto, rowHeight), sfCenter)

                    Dim hintText As String = GetMahjongShortName(codePoint)
                    g.DrawString(hintText, fontText, brushText,
                                 New RectangleF(xInfo + 6, yRow, colInfo - 12, rowHeight), sfLeft)

                Next

            End Using

        End Using

        Return bmp

    End Function

    Private Function ResolveInstalledFontFamilyName(preferredNames As String()) As String

        Dim installed As New InstalledFontCollection()

        Dim fam As FontFamily
        For Each fam In installed.Families

            Dim preferred As String
            For Each preferred In preferredNames
                If NormalizeFontName(fam.Name) = NormalizeFontName(preferred) Then
                    Return fam.Name
                End If
            Next

        Next

        Return String.Empty

    End Function

    Private Function NormalizeFontName(value As String) As String

        If value Is Nothing Then
            Return String.Empty
        End If

        Dim result As String = value.Trim().ToLowerInvariant()
        result = result.Replace(" ", String.Empty)
        result = result.Replace("-", String.Empty)

        Return result

    End Function

    Private Function GetMahjongShortName(codePoint As Integer) As String

        Select Case codePoint
            Case &H1F000 : Return "East Wind"
            Case &H1F001 : Return "South Wind"
            Case &H1F002 : Return "West Wind"
            Case &H1F003 : Return "North Wind"
            Case &H1F004 : Return "Red Dragon"
            Case &H1F005 : Return "Green Dragon"
            Case &H1F006 : Return "White Dragon"

            Case &H1F007 To &H1F00F
                Return "Characters " & (codePoint - &H1F006).ToString()

            Case &H1F010 To &H1F018
                Return "Bamboo " & (codePoint - &H1F00F).ToString()

            Case &H1F019 To &H1F021
                Return "Circles " & (codePoint - &H1F018).ToString()

            Case &H1F022 : Return "Season East"
            Case &H1F023 : Return "Season South"
            Case &H1F024 : Return "Season West"
            Case &H1F025 : Return "Season North"

            Case &H1F026 : Return "Flower Plum"
            Case &H1F027 : Return "Flower Orchid"
            Case &H1F028 : Return "Flower Bamboo"
            Case &H1F029 : Return "Flower Chrys."

            Case &H1F02A : Return "Joker"
            Case &H1F02B : Return "Back"

            Case Else
                Return String.Empty
        End Select

    End Function

End Class