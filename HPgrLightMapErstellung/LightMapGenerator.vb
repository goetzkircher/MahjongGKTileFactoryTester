Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Public Module LightMapGenerator

    Public Function CreateInnerWhiteGradient(
            bmpSrc As Bitmap,
             t As Integer,
             v As Verlauf,
            shrink As Integer
            ) As Bitmap

        t *= 2

        If bmpSrc Is Nothing Then
            Throw New ArgumentNullException(NameOf(bmpSrc))
        End If

        If t < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(t))
        End If

        Dim bmpDst As New Bitmap(bmpSrc.Width, bmpSrc.Height, PixelFormat.Format32bppArgb)

        Using g As Graphics = Graphics.FromImage(bmpDst)
            g.DrawImageUnscaled(bmpSrc, 0, 0)
        End Using

        'Innenfläche FFFFFFFF suchen
        Dim left As Integer = bmpSrc.Width - 1
        Dim top As Integer = bmpSrc.Height - 1
        Dim right As Integer = 0
        Dim bottom As Integer = 0
        Dim found As Boolean = False

        For y As Integer = 0 To bmpSrc.Height - 1
            For x As Integer = 0 To bmpSrc.Width - 1
                Dim c As Color = bmpSrc.GetPixel(x, y)

                If c.ToArgb() = Color.White.ToArgb() Then
                    If x < left Then left = x
                    If x > right Then right = x
                    If y < top Then top = y
                    If y > bottom Then bottom = y
                    found = True
                End If
            Next
        Next

        If Not found Then
            Throw New InvalidOperationException("Keine Innenfläche mit FFFFFFFF gefunden.")
        End If

        If shrink <> 0 Then
            Dim col As Color = Color.White

            For y As Integer = 0 To bmpSrc.Height - 1
                For x As Integer = 0 To bmpSrc.Width - 1

                    'Innenfläche bleibt exakt weiß
                    If x >= left + shrink AndAlso x <= right - shrink AndAlso y >= top + shrink AndAlso y <= bottom - shrink Then
                        bmpDst.SetPixel(x, y, col)
                    End If
                Next
            Next
            left += shrink
            right -= shrink
            top += shrink
            bottom -= shrink
        End If

        Dim delta As Integer = t * 2 \ 3
        left += delta
        right -= delta
        top += delta
        bottom -= delta

        'Verlauf nach außen erzeugen
        For y As Integer = 0 To bmpSrc.Height - 1
            For x As Integer = 0 To bmpSrc.Width - 1

                'Innenfläche bleibt exakt weiß
                If x >= left AndAlso x <= right AndAlso y >= top AndAlso y <= bottom Then
                    Continue For
                End If

                Dim distance As Double
                Dim sampleX As Integer
                Dim sampleY As Integer

                If x < left AndAlso y >= top AndAlso y <= bottom Then
                    'linke Kante
                    distance = CDbl(left - x)
                    sampleX = left - t
                    sampleY = y

                ElseIf x > right AndAlso y >= top AndAlso y <= bottom Then
                    'rechte Kante
                    distance = CDbl(x - right)
                    sampleX = right + t
                    sampleY = y

                ElseIf y < top AndAlso x >= left AndAlso x <= right Then
                    'obere Kante
                    distance = CDbl(top - y)
                    sampleX = x
                    sampleY = top - t

                ElseIf y > bottom AndAlso x >= left AndAlso x <= right Then
                    'untere Kante
                    distance = CDbl(y - bottom)
                    sampleX = x
                    sampleY = bottom + t

                Else
                    'Ecken: Kreisbogen um die jeweilige Ecke
                    Dim cx As Integer
                    Dim cy As Integer

                    If x < left Then
                        cx = left
                    Else
                        cx = right
                    End If

                    If y < top Then
                        cy = top
                    Else
                        cy = bottom
                    End If

                    Dim dx As Double = CDbl(x - cx)
                    Dim dy As Double = CDbl(y - cy)

                    distance = Math.Sqrt(dx * dx + dy * dy)

                    If distance = 0.0R Then
                        Continue For
                    End If

                    Dim f As Double = CDbl(t) / distance

                    sampleX = CInt(Math.Round(cx + dx * f))
                    sampleY = CInt(Math.Round(cy + dy * f))
                End If

                If distance < 0.0R OrElse distance > CDbl(t) Then
                    Continue For
                End If

                sampleX = Math.Max(0, Math.Min(bmpSrc.Width - 1, sampleX))
                sampleY = Math.Max(0, Math.Min(bmpSrc.Height - 1, sampleY))

                Dim srcAtTarget As Color = bmpSrc.GetPixel(sampleX, sampleY)

                'Transparente Bereiche nicht anfassen
                Dim oldColor As Color = bmpDst.GetPixel(x, y)
                If oldColor.A = 0 Then
                    Continue For
                End If

                Dim targetGray As Integer = CInt((CInt(srcAtTarget.R) + CInt(srcAtTarget.G) + CInt(srcAtTarget.B)) / 3)

                Dim q As Double

                Select Case v
                    Case Verlauf.VerlaufLinear
                        q = distance / CDbl(t)
                    Case Verlauf.VerlaufWeicher
                        q = (distance / CDbl(t)) ^ 1.5
                    Case Verlauf.RandKontrast
                        q = Math.Sqrt(distance / CDbl(t))
                End Select

                Dim gray As Integer = CInt(Math.Round(255.0R + (CDbl(targetGray) - 255.0R) * q))
                gray = Math.Max(0, Math.Min(255, gray))

                bmpDst.SetPixel(x, y, Color.FromArgb(oldColor.A, gray, gray, gray))

            Next
        Next

        Return ResizeBitmapTo400x500HighQuality(bmpDst)

    End Function

    Public Function ResizeBitmapTo400x500HighQuality(bmpSrc As Bitmap) As Bitmap

        If bmpSrc Is Nothing Then
            Throw New ArgumentNullException(NameOf(bmpSrc))
        End If

        Dim bmpDst As New Bitmap(400, 500, PixelFormat.Format32bppArgb)

        bmpDst.SetResolution(bmpSrc.HorizontalResolution, bmpSrc.VerticalResolution)

        Using g As Graphics = Graphics.FromImage(bmpDst)

            g.CompositingMode = CompositingMode.SourceCopy
            g.CompositingQuality = CompositingQuality.HighQuality
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.SmoothingMode = SmoothingMode.HighQuality
            g.PixelOffsetMode = PixelOffsetMode.HighQuality

            Using ia As New ImageAttributes()
                ia.SetWrapMode(WrapMode.TileFlipXY)

                g.DrawImage(
                    bmpSrc,
                    New Rectangle(0, 0, 400, 500),
                    0,
                    0,
                    bmpSrc.Width,
                    bmpSrc.Height,
                    GraphicsUnit.Pixel,
                    ia)
            End Using

        End Using

        Return bmpDst

    End Function

End Module