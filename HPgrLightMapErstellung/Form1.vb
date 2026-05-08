Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text

Public Enum Verlauf
    VerlaufLinear
    VerlaufWeicher
    RandKontrast
    VerlaufHarteKante
End Enum

Public Class Form1

    Private _bmpDst As Bitmap = Nothing

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Button2.Enabled = False
        Button2.Refresh()

        If _bmpDst IsNot Nothing Then
            _bmpDst.Dispose()
            _bmpDst = Nothing
        End If

        Dim bmpSrc As Bitmap = CType(
          Bitmap.FromFile(
          "C:\Users\goetz\Documents\Visual Studio\MahjongGKTileFactoryTester\MahjongGKTileFactoryTester\Original der Lichtkarte\Rahmen breit ohne Verlauf.png"), Bitmap)

        Dim v As Verlauf
        If optLinear.Checked Then
            v = Verlauf.VerlaufLinear
        ElseIf optInnenWeicher.Checked Then
            v = Verlauf.VerlaufWeicher
        ElseIf optKontrastStärker.Checked Then
            v = Verlauf.RandKontrast
        ElseIf optHarteKante.Checked Then
            _bmpDst = LightMapGenerator.ResizeBitmapTo400x500HighQuality(bmpSrc)
        Else
            Stop
        End If

        If _bmpDst Is Nothing Then
            _bmpDst = LightMapGenerator.CreateInnerWhiteGradient(bmpSrc, numTiefe.Value, v, numShrink.Value)
        End If

        '   Me.ClientSize = _bmpDst.Size
        PictureBox1.Image = _bmpDst
        Button2.Enabled = True

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim funktionsName As String = "LightMap_"
        Dim usedBitmap As Boolean = False

        If optLinear.Checked Then
            funktionsName &= Verlauf.VerlaufLinear.ToString
        ElseIf optInnenWeicher.Checked Then
            funktionsName &= Verlauf.VerlaufWeicher.ToString
        ElseIf optKontrastStärker.Checked Then
            funktionsName &= Verlauf.RandKontrast.ToString
            usedBitmap = True
        ElseIf optHarteKante.Checked Then
            funktionsName &= Verlauf.VerlaufHarteKante.ToString
        Else
            Stop
        End If

        If usedBitmap Then
            Select Case numTiefe.Value
                Case 10
                    If numShrink.Value = 0 Then
                        funktionsName = "LightMapRahmenRandS_400x500"
                    ElseIf numShrink.Value = -30 Then
                        funktionsName = "LightMapRahmenRandXS_400x500"
                    End If
                Case 30
                    funktionsName = "LightMapRahmenRandM_400x500"
                Case 60
                    funktionsName = "LightMapRahmenRandL_400x500"
                Case 90
                    funktionsName = "LightMapRahmenRandXL_400x500"
                Case Else
                    usedBitmap = False
            End Select
        End If

        If Not usedBitmap Then
            funktionsName &= "_" & numTiefe.Value.ToString & "_400x500"
        End If

        WriteBitmapLoaderCodeToClipboard(funktionsName, _bmpDst)

    End Sub
    Public Sub WriteBitmapLoaderCodeToClipboard(functionName As String, bmp As Bitmap)

        If String.IsNullOrWhiteSpace(functionName) Then
            Throw New ArgumentException("Funktionsname fehlt.", NameOf(functionName))
        End If

        If bmp Is Nothing Then
            Throw New ArgumentNullException(NameOf(bmp))
        End If

        Dim base64 As String

        Using ms As New MemoryStream()
            bmp.Save(ms, ImageFormat.Png)
            base64 = Convert.ToBase64String(ms.ToArray())
        End Using

        Dim sb As New StringBuilder()

        sb.AppendLine("Private Function LoadImageLichtkarte() As Image")
        sb.AppendLine()
        sb.AppendLine("    Return " & functionName & "()")
        sb.AppendLine()
        sb.AppendLine("End Function")
        sb.AppendLine()
        sb.AppendLine("Private Function " & functionName & "() As Image")
        sb.AppendLine()
        sb.AppendLine("    Dim base64 As String = _")

        Const lineLength As Integer = 100

        For i As Integer = 0 To base64.Length - 1 Step lineLength

            Dim part As String = base64.Substring(i, Math.Min(lineLength, base64.Length - i))
            Dim isLast As Boolean = (i + lineLength >= base64.Length)

            If isLast Then
                sb.AppendLine("        """ & part & """")
            Else
                sb.AppendLine("        """ & part & """ & _")
            End If

        Next

        sb.AppendLine()
        sb.AppendLine("    Dim bytes() As Byte = Convert.FromBase64String(base64)")
        sb.AppendLine()
        sb.AppendLine("    Using ms As New MemoryStream(bytes)")
        sb.AppendLine("        Using img As Image = Image.FromStream(ms)")
        sb.AppendLine("            Return New Bitmap(img)")
        sb.AppendLine("        End Using")
        sb.AppendLine("    End Using")
        sb.AppendLine()
        sb.AppendLine("End Function")

        SetPlainTextClipboard(sb.ToString())

    End Sub

    Private Sub SetPlainTextClipboard(text As String)

        If text Is Nothing Then
            Throw New ArgumentNullException(NameOf(text))
        End If

        Clipboard.Clear()

        Dim data As New DataObject()
        data.SetData(DataFormats.UnicodeText, text)
        data.SetData(DataFormats.Text, text)

        Clipboard.SetDataObject(data, True)

    End Sub
End Class
