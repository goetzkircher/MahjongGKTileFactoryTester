Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Text
Imports ColorPickerHsb
Imports MahjongGK.Contracts
Imports MahjongGK.Contracts.GlobalEnum
Imports TileFactory

Public Class Form1

    Private _zuweisungAktiv As Integer = 1
    Private _arrPicTiles() As PictureBox

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        FormPlacementStore.Restore(Me)
        'alle anderen Anweisungen nach dem folgenden Block,
        'andernfalls ist die Oberfläche nicht mehr original
        'und man schießt sich von hinten in die Brust.
        If Debugger.IsAttached Then
            Dim msg As String = "Normalantwort JA" & vbCrLf
            msg &= "Sollen das Auslesen der numXxx- und andere Property in die Zwischenablage übersprungen werden?"
            msg &= vbCrLf & vbCrLf
            msg &= "Wird die Frage mit nein beantwortet, ließ das Programm die aktuellen Daten der Oberfläche aus und beendet das Programm. In der Zwischenablage stehen dann die Daten, die in der Klasse TileColors in der #Region ""Integer und Boolean-Werte numXxx, chkkXxx und optXxx (Copy-Paste-Region aus Form1)"" eingehängt werden können."
            If MsgBox(msg, MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton1) = MsgBoxResult.No Then
                CopyNumPropertiesToClipboard(Me)
                End
            End If
        End If

        TileFactoryAPI.Initialisierung()
        'CopyFormToTileColors()
        FillComboBoxWithEnum(Of SteinDesign)(cboSteinDesign)
        FillComboBoxWithEnum(Of LinearGradientMode)(cboSymbolGradientMode)
        FillComboBoxWithEnum(Of LightMap)(cboFaceLightMapNormal)
        FillComboBoxWithEnum(Of LightMap)(cboFaceLightMapSelectable)
        FillComboBoxWithEnum(Of LightMap)(cboFaceLightMapSelected)
        FillComboBoxWithEnum(Of LightMap)(cboFaceLightMapRemovable)

        HookDirtyStateTracking()

        ReDim _arrHöhen(_arrBreiten.GetUpperBound(0))
        ReDim _arrSteinSize(_arrBreiten.GetUpperBound(0))
        ReDim _arrSteinBasisSize(_arrBreiten.GetUpperBound(0))

        radAktSteinSatzMedium.Checked = True
        Dim arrPicTiles() As PictureBox = {PictureBox1, PictureBox2, PictureBox3, PictureBox4, PictureBox5, PictureBox6, PictureBox7, PictureBox8, PictureBox9, PictureBox10, PictureBox11, PictureBox12, PictureBox13, PictureBox14, PictureBox15, PictureBox16, PictureBox17, PictureBox18, PictureBox19, PictureBox20}
        _arrPicTiles = arrPicTiles

        CopyTileColorsToForm()
        DrawAllTiles(tileColorsInvalide:=True)
        numDHueGhost.Enabled = Not chkGhostUseFastMethode.Checked
        numDSatGhost.Enabled = Not chkGhostUseFastMethode.Checked

        TileFactoryINISettings.Tile_TextUseSegoeUISymbol = True

        _zuweisungAktiv = 0
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        Dim msg As String = "Wirklich beenden?"
        If MsgBox(msg, MsgBoxStyle.Question Or MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2) = MsgBoxResult.No Then
            e.Cancel = True
            Exit Sub
        End If

        TileFactoryAPI.DisposeAll()
        FormPlacementStore.Save(Me)

    End Sub

    Private ReadOnly _arrTypen() As SteinTyp = {
        SteinTyp.WindOst,
        SteinTyp.DracheR,
        SteinTyp.Bambus1,
        SteinTyp.Punkt01,
        SteinTyp.Symbol1,
        SteinTyp.BlütePf,
        SteinTyp.JahrFrl
    }

    Private _größe As Integer
    Private ReadOnly _arrBreiten() As Integer = {200, 180, 160, 150, 140, 120, 110, 100, 90, 80, 70, 65, 60, 55, 50, 45, 40, 35, 30, 25, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2}
    Private _arrHöhen() As Integer 'wird aus den _arrBreiten abgeleitet.
    Private _arrSteinSize() As Size
    Private _arrSteinBasisSize() As Size

    Private ReadOnly _arrSteinStatus() As SteinStatus = {SteinStatus.I01Normal, SteinStatus.I02Selected, SteinStatus.I03Selectable, SteinStatus.I04Removable, SteinStatus.I05Locked, SteinStatus.I06WerkstückStein, SteinStatus.I07MissingSecond, SteinStatus.I08WerkstückEinfügeFehler, SteinStatus.I09WerkstückZufallsgrafik}

    Private _rnd As New Random

    Private ReadOnly Property SteinSatz As SteinSatz
        Get
            If radAktSteinSatzDark.Checked Then
                Return SteinSatz.Dark
            ElseIf radAktSteinSatzMedium.Checked Then
                Return SteinSatz.Medium
            ElseIf radAktSteinSatzLight.Checked Then
                Return SteinSatz.Light
            Else
                MsgBox("Programmierfehler: Keine Option für Steinsatz gewählt. Programm stoppt.", MsgBoxStyle.Exclamation)
                Stop
            End If
            Return SteinSatz.Medium
        End Get
    End Property

    Private _AktSteinDesign As SteinDesign
    Private ReadOnly Property SteinDesign As SteinDesign
        Get
            Return ParseSteinDesign(cboSteinDesign.Text)
        End Get
    End Property

    Private ReadOnly Property SteinFont As SteinFont
        Get
            If optUsedFontSegeoUISymbol.Checked Then
                Return SteinFont.Segoe
            ElseIf optUsedFontNotoSansSymbol2.Checked Then
                Return SteinFont.Noto
            Else
                Stop
                optUsedFontSegeoUISymbol.Checked = True
                Return SteinFont.Segoe
            End If
        End Get
    End Property

    Public ReadOnly Property Use As Integer
        Get
            Return If(checkBoxUseDevelopmentPath.Checked, 1, 0)
        End Get
    End Property

    Private _drawAllTilesCounter As Integer
    Private _tileColors(,) As TileColors = {{New TileColors, New TileColors}, {New TileColors, New TileColors}, {New TileColors, New TileColors}}
    Private _tileUndoColors(,) As TileColors = {{New TileColors, New TileColors}, {New TileColors, New TileColors}, {New TileColors, New TileColors}}
    Private _tileColorsHashOnLoad(2, 1) As String

    Private _counter As Integer

    Sub DrawAllTiles(Optional tileColorsInvalide As Boolean = False)

        If tileColorsInvalide Then
            CopyFormToTileColors()
        End If

        Dim baseW As Integer = numTileBasisWidth.Value
        Dim baseH As Integer = numTileBasisHeight.Value

        For idx As Integer = 0 To _arrBreiten.GetUpperBound(0)

            Dim size As Size = GlobalHelper.SteinSizeFromWidthOrHeight(baseW, baseH, aktW:=_arrBreiten(idx))
            _arrBreiten(idx) = size.Width 'Breite könnte geändert worden sein. (nuss gerade Zahl sein)
            _arrHöhen(idx) = size.Height
            _arrSteinSize(idx) = size
            _arrSteinBasisSize(idx) = New Size(baseW, baseH)
        Next

        _drawAllTilesCounter += 1

        If _extended Then
            Label2.Text = SteinStatus.I05Locked.ToString
            Label3.Text = SteinStatus.I06WerkstückStein.ToString
            Label4.Text = SteinStatus.I07MissingSecond.ToString
            Label5.Text = SteinStatus.I08WerkstückEinfügeFehler.ToString
            Label6.Text = SteinStatus.I09WerkstückZufallsgrafik.ToString
        Else
            Label2.Text = " Dunkel - " & SteinStatus.I01Normal.ToString & " - dunkelster/intensivster Farbton"
            Label3.Text = " Hell - " & SteinStatus.I02Selected.ToString & " - hellster/blassester Farbton"
            Label4.Text = " Mitteldunkel - " & SteinStatus.I03Selectable.ToString & " - heller als Normal, dunkler als Removable"
            Label5.Text = " Mittelhell - " & SteinStatus.I04Removable.ToString & " - heller als Selectable, dunkler als Selected"
            Label6.Text = Nothing
        End If

        Label7.Text = $"Steinsatz:  {SteinFont}-{SteinSatz}, Ausgabe {_drawAllTilesCounter}"

        Label1.Text = $"{_arrBreiten(_größe)} x {_arrHöhen(_größe)}"

        Dim fromStatus As Integer
        Dim toStatus As Integer

        If _extended Then
            fromStatus = 4
            toStatus = 8
        Else
            fromStatus = 0
            toStatus = 3
            For idx As Integer = 16 To 19
                If _arrPicTiles(idx).Image IsNot Nothing Then
                    _arrPicTiles(idx).Image = Nothing
                End If
            Next
        End If

        Dim steinTp() As SteinTyp = {SteinTyp.Bambus6, SteinTyp.WindSüd, SteinTyp.BlüteCt, SteinTyp.JahrSom}
        Dim request As TileRequest
        For pass As Integer = 1 To 2
            For status As Integer = fromStatus To toStatus

                For typ As Integer = 0 To 3

                    request = New TileRequest(AktRenderMode.Spiel,
                                              _tileColors(SteinSatz, SteinFont),
                                              steinTp(typ),
                                              _arrSteinStatus(status),
                                              SteinFrameVersion.Standard,
                                              _arrSteinSize(_größe),
                                              _arrSteinBasisSize(_größe),
                                              If(pass = 2, CheckBoxDrawGhost.Checked, False))

                    Dim picIdx As Integer
                    If _extended Then
                        picIdx = (status - 4) * 4 + typ
                    Else
                        picIdx = status * 4 + typ
                    End If

                    'If status = fromStatus AndAlso typ = 0 Then
                    '    Stop
                    'End If
                    _counter += 1

                    Dim bmp As Bitmap = TileFactoryAPI.GetTile(request)

                    If Not request.TileColors.StatusLoadingOK Then
                        Dim gfx As Graphics = Graphics.FromImage(bmp)
                        DebugDrawKreuz(gfx, New Rectangle(0, 0, bmp.Width, bmp.Height))
                        gfx.Dispose()
                        gfx = Nothing
                    End If
                    _arrPicTiles(picIdx).Image = bmp

                    _arrPicTiles(picIdx).Refresh()

                    '      PictureBox1.Image.Save($"C:\Users\goetz\Documents\_tmp2\Mahjongstein_{_arrBreiten(0)}x{_arrHöhen(0)}.png", Imaging.ImageFormat.Png)

                Next
            Next
            If CheckBoxDrawGhost.Checked Then
                If pass = 1 Then
                    ZeichneSpielfeldUntergrund(picSpielfeld, _arrPicTiles)
                Else
                    UnterlegeSpielfeldFuerTiles(picSpielfeld, _arrPicTiles)
                End If
            Else
                Exit For
            End If
        Next

        If Not _extended Then
            Static flag As Boolean
            flag = Not flag
            If flag Then
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.DracheG, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox17.Image = TileFactoryAPI.GetTile(request)
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.BlüteBa, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox18.Image = TileFactoryAPI.GetTile(request)
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.JahrHer, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox19.Image = TileFactoryAPI.GetTile(request)
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.WindOst, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox20.Image = TileFactoryAPI.GetTile(request)
            Else
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.DracheG, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox17.Image = TileFactoryAPI.GetTile(request)
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.DracheR, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox18.Image = TileFactoryAPI.GetTile(request)
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.DracheW, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox19.Image = TileFactoryAPI.GetTile(request)
                request = New TileRequest(AktRenderMode.Spiel, _tileColors(SteinSatz, SteinFont), SteinTyp.Symbol9, SteinStatus.I01Normal, SteinFrameVersion.Standard, _arrSteinSize(_größe), _arrSteinBasisSize(_größe))
                PictureBox20.Image = TileFactoryAPI.GetTile(request)
            End If
        End If
    End Sub

    Private Sub radAktSteinSatzLight_CheckedChanged(sender As Object, e As EventArgs) _
        Handles radAktSteinSatzLight.CheckedChanged,
                radAktSteinSatzMedium.CheckedChanged,
                radAktSteinSatzDark.CheckedChanged,
                optUsedFontSegeoUISymbol.CheckedChanged,
                optUsedFontNotoSansSymbol2.CheckedChanged

        If _zuweisungAktiv <> 0 Then Exit Sub
        Dim rad As RadioButton = DirectCast(sender, RadioButton)
        If Not rad.Checked Then Exit Sub
        ''Dim idx1 As Integer

        CopyTileColorsToForm()
        DrawAllTiles()

    End Sub

    Private Sub btnKleiner_Click(sender As Object, e As EventArgs) Handles btnKleiner.Click
        If _größe < _arrBreiten.GetUpperBound(0) Then
            _größe += 1
            DrawAllTiles()
        End If
    End Sub

    Private Sub btnGrößer_Click(sender As Object, e As EventArgs) Handles btnGrößer.Click
        If _größe > 0 Then
            _größe -= 1
            DrawAllTiles()
        End If
    End Sub

    Private _extended As Boolean = False

    Private Sub btnNormalerSatz_Click(sender As Object, e As EventArgs) Handles btnNormalerSatz.Click
        _extended = False
        DrawAllTiles()
    End Sub

    Private Sub btnErweiterterSatz_Click(sender As Object, e As EventArgs) Handles btnErweiterterSatz.Click
        _extended = True
        DrawAllTiles()
    End Sub
    Private Sub chkDebugShowFaceFrameMouse_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkDebugShowFaceFrameMouse.CheckedChanged
        If _zuweisungAktiv <> 0 Then Exit Sub
        CopyFormToTileColors()
        DrawAllTiles()
    End Sub
    Private Sub tbDeltaHue_ValueChanged(sender As Object, e As EventArgs) Handles _
            tbHueBasisNormal.ValueChanged,
            tbHueBasisJZeiten.ValueChanged,
            tbHueBasisWinde.ValueChanged,
            tbHueBasisBlüten.ValueChanged,
            tbHueSymbolGradientFrom.ValueChanged,
            tbHueSymbolGradientTo.ValueChanged,
            tbHueSymbolOutline.ValueChanged,
            tbHueFaceFrame.ValueChanged

        If _zuweisungAktiv <> 0 Then Exit Sub

        Dim tb As TrackBar = CType(sender, TrackBar)
        Dim hsb As New HsbInteger(tb.Value, 100, 100, normaliszeTo361:=True)
        lblTbColor.BackColor = hsb.ToColorB360(Hue360Mode.Black360White361)

        Label8.Text = $"Normal={tbHueBasisNormal.Value } Winde={tbHueBasisWinde.Value } Blüten={tbHueBasisBlüten.Value } Jahr={tbHueBasisJZeiten.Value } - From={tbHueSymbolGradientFrom.Value } To={tbHueSymbolGradientTo.Value } Out={tbHueSymbolOutline.Value } Frame={tbHueFaceFrame.Value }"

        If chkGhostUseFastMethode.Checked Then
            If tb.Name = tbHueSymbolGradientFrom.Name Then
                tbHueSymbolGradientTo.Value = tbHueSymbolGradientFrom.Value
            End If
        End If
    End Sub
    Private Sub tbHueWinde_MouseHover(sender As Object, e As EventArgs) Handles _
                tbHueBasisNormal.GotFocus,
                tbHueBasisJZeiten.GotFocus,
                tbHueBasisWinde.GotFocus,
                tbHueBasisBlüten.GotFocus,
                tbHueSymbolGradientFrom.GotFocus,
                tbHueSymbolGradientTo.GotFocus,
                tbHueSymbolOutline.GotFocus,
                tbHueFaceFrame.GotFocus,
                tbHueBasisNormal.MouseEnter,
                tbHueBasisJZeiten.MouseEnter,
                tbHueBasisWinde.MouseEnter,
                tbHueBasisBlüten.MouseEnter,
                tbHueSymbolGradientFrom.MouseEnter,
                tbHueSymbolGradientTo.MouseEnter,
                tbHueSymbolOutline.MouseEnter,
                tbHueFaceFrame.MouseEnter,
                tbHueBasisNormal.Scroll,
                tbHueBasisJZeiten.Scroll,
                tbHueBasisWinde.Scroll,
                tbHueBasisBlüten.Scroll,
                tbHueSymbolGradientFrom.Scroll,
                tbHueSymbolGradientTo.Scroll,
                tbHueSymbolOutline.Scroll,
                tbHueFaceFrame.Scroll

        Dim tb As TrackBar = CType(sender, TrackBar)

        ToolTip1.SetToolTip(tb, tb.Value.ToString)

        Dim hsb As New HsbInteger(tb.Value, 100, 100, normaliszeTo361:=True)

        lblTbColor.BackColor = hsb.ToColorB360(Hue360Mode.Black360White361)

    End Sub
    Private Sub pnlTracbar_MouseLeave(sender As Object, e As EventArgs) Handles _
        pnlTracbar.MouseLeave
        Dim pt As Point = pnlTracbar.PointToClient(MousePosition)
        If Not pnlTracbar.ClientRectangle.Contains(pt) Then
            lblTbColor.BackColor = SystemColors.Control
        End If
    End Sub
    Private Sub chkDrawGhost_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxDrawGhost.CheckedChanged
        DrawAllTiles(tileColorsInvalide:=True)
    End Sub

    Private Sub CopyTileColorsToForm()
        _zuweisungAktiv += 1

        TileColorsFormMapper.CopyTileColorsToForm(_tileColors(SteinSatz, SteinFont), New Control() {Me})
        _zuweisungAktiv -= 1
        _dirtyHash = Now.ToString
    End Sub
    Private Sub CopyFormToTileColors()
        _tileColors(SteinSatz, SteinFont) = New TileColors
        TileColorsFormMapper.CopyFormToTileColors(_tileColors(SteinSatz, SteinFont), New Control() {Me})
    End Sub

#Region "Comboboxen"

    Private Shared Function ParseSteinDesign(value As String) As SteinDesign
        If String.IsNullOrWhiteSpace(value) Then
            If Debugger.IsAttached Then
                Throw New ArgumentException("SteinDesign-String ist leer oder Nothing.", NameOf(value))
            End If

            Return SteinDesign.Default
        End If

        Dim result As SteinDesign

        If [Enum].TryParse(Of SteinDesign)(value.Trim(), True, result) AndAlso
           [Enum].IsDefined(GetType(SteinDesign), result) Then
            Return result
        End If

        If Debugger.IsAttached Then
            Throw New ArgumentException(
                $"Ungültiger SteinDesign-Wert: '{value}'.",
                NameOf(value))
        End If

        Return SteinDesign.Default

    End Function

    ''' <summary>
    ''' Aufruf FillComboBoxWithEnum(Of SteinDesign)(cmbSteinDesign)
    ''' </summary>
    ''' <typeparam name="TEnum"></typeparam>
    ''' <param name="cbo"></param>
    Private Shared Sub FillComboBoxWithEnum(Of TEnum As Structure)(cbo As ComboBox)
        If cbo Is Nothing Then
            Throw New ArgumentNullException(NameOf(cbo))
        End If

        Dim enumType As Type = GetType(TEnum)

        If Not enumType.IsEnum Then
            Throw New ArgumentException("TEnum muss eine Enumeration sein.")
        End If

        Dim names() As String = [Enum].GetNames(enumType)

        cbo.BeginUpdate()
        Try
            cbo.Items.Clear()

            For Each name As String In names
                cbo.Items.Add(name)
            Next

            If names.Length > 0 Then
                cbo.Text = names(0)
            Else
                cbo.Text = String.Empty
            End If

        Finally
            cbo.EndUpdate()
        End Try
    End Sub

    Private Property Color As Color
    Private Sub tbHueSymbolOutline_MouseUp(sender As Object, e As MouseEventArgs) Handles _
                tbHueBasisNormal.MouseUp,
                tbHueBasisJZeiten.MouseUp,
                tbHueBasisWinde.MouseUp,
                tbHueBasisBlüten.MouseUp,
                tbHueSymbolGradientFrom.MouseUp,
                tbHueSymbolGradientTo.MouseUp,
                tbHueSymbolOutline.MouseUp,
                tbHueFaceFrame.MouseUp

        If e.Button <> MouseButtons.Right Then
            Exit Sub
        End If

        Dim tb As TrackBar = DirectCast(sender, TrackBar)

        If e.X < (tb.ClientSize.Width \ 2) Then
            ' linker ColorPicker
            Using frm As New ColorPickerNamedColors
                frm.SelectedColor = Me.Color
                If frm.ShowDialog() = DialogResult.OK Then
                    _Color = frm.SelectedColor
                End If
            End Using
        Else
            ' rechter ColorPicker
            Using frm As New ColorPickerHsb.ColorPickerHSB
                frm.SelectedColor = Me.Color
                If frm.ShowDialog() = DialogResult.OK Then
                    _Color = frm.SelectedColor
                End If
            End Using
        End If

        Dim txtCol As String = _Color.A.ToString("X2", CultureInfo.InvariantCulture) &
                               _Color.R.ToString("X2", CultureInfo.InvariantCulture) &
                               _Color.G.ToString("X2", CultureInfo.InvariantCulture) &
                               _Color.B.ToString("X2", CultureInfo.InvariantCulture)

        Dim hue As Integer = CInt(Math.Round(HsbColorHelper.GetHue(_Color)))
        Dim sat As Integer = HsbColorHelper.GetSaturation(_Color)
        Dim brg As Integer = HsbColorHelper.GetBrightness(_Color)

        tb.Value = hue

        Select Case tb.Name
            Case tbHueBasisNormal.Name
                numSatI01NormalNormal.Value = sat
                numBrgI01NormalNormal.Value = brg

            Case tbHueBasisBlüten.Name
                numSatI01NormalBlüten.Value = sat
                numBrgI01NormalBlüten.Value = brg

            Case tbHueBasisJZeiten.Name
                numSatI01NormalJZeiten.Value = sat
                numBrgI01NormalJZeiten.Value = brg

            Case tbHueBasisWinde.Name
                numSatI01NormalWinde.Value = sat
                numBrgI01NormalWinde.Value = brg

            Case tbHueSymbolGradientFrom.Name
                numSatSymbolGradientFrom.Value = sat
                numBrgSymbolGradientFrom.Value = brg

            Case tbHueSymbolGradientTo.Name
                numSatSymbolGradientTo.Value = sat
                numBrgSymbolGradientTo.Value = brg

            Case tbHueSymbolOutline.Name
                numSatSymbolOutLine.Value = sat
                numBrgSymbolOutline.Value = brg

            Case tbHueFaceFrame.Name
                numSatFaceFrameNormal.Value = sat
                numBrgFaceFrameNormal.Value = brg

        End Select

    End Sub

    Private Sub tmrBtnSaveEnDisable_Tick(sender As Object, e As EventArgs) _
        Handles tmrBtnSaveEnDisable.Tick
        btnSave.Enabled = _tileColors(SteinSatz, SteinFont).IsDirtySinceLastStorage
    End Sub

    Private Sub btnSteinSatzLoad_Click(sender As Object, e As EventArgs) _
        Handles btnSteinSatzLoad.Click

        Dim tc As New TileColors
        Dim hashFromNewTileColors As String = tc.GetMyHash

        Dim found As Boolean
        Dim sb As New StringBuilder
        sb.AppendLine("Folgende Daten wurden nicht gespeichert:")
        For ss As SteinSatz = SteinSatz.Light To SteinSatz.Dark
            For sf As SteinFont = SteinFont.Segoe To SteinFont.Noto
                If _tileColors(ss, sf).GetMyHash <> hashFromNewTileColors Then
                    If String.IsNullOrEmpty(_tileColorsHashOnLoad(ss, sf)) Then
                        found = True
                        sb.AppendLine($"{ss.ToString}.{sf.ToString }")
                    ElseIf _tileColors(ss, sf).GetMyHash <> _tileColorsHashOnLoad(ss, sf) Then
                        found = True
                        sb.AppendLine($"{ss.ToString}.{sf.ToString }")
                    End If
                End If
            Next
        Next

        If found Then
            sb.AppendLine()
            sb.AppendLine("Soll das Laden abgebrochen werden?")
            If MsgBox(sb.ToString, MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                Exit Sub
            End If
        Else
            If MsgBox($"Soll das komplette Steindesign {SteinDesign.ToString} geladen werden?", MsgBoxStyle.Question Or MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
                Exit Sub
            End If
        End If

        For ss As Integer = SteinSatz.Light To SteinSatz.Dark
            For sf As Integer = SteinFont.Segoe To SteinFont.Noto
                Dim tcLoaded As TileColors = TileColors.Load(SteinDesign, ss, sf, checkBoxUseDevelopmentPath.Checked)
                If tcLoaded.GetMyHash = _tileColors(ss, sf).GetMyHash Then
                    Continue For
                Else
                    _tileColors(ss, sf) = tcLoaded
                    _tileColorsHashOnLoad(ss, sf) = tcLoaded.GetMyHash
                End If
            Next
        Next
        CopyTileColorsToForm()
        DrawAllTiles(tileColorsInvalide:=True)
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) _
        Handles btnSave.Click

        If MsgBox($"Soll das komplette Steindesign {SteinDesign.ToString} gespeichert werden?", MsgBoxStyle.Question Or MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
            Exit Sub
        End If

        CopyFormToTileColors()

        For ss As SteinSatz = SteinSatz.Light To SteinSatz.Dark
            For sf As SteinFont = SteinFont.Segoe To SteinFont.Noto
                _tileColors(ss, sf).DonotInsertSymbol = False
                Dim tcLoaded As TileColors = TileColors.Load(SteinDesign, ss, sf, checkBoxUseDevelopmentPath.Checked)
                If tcLoaded.GetMyHash = _tileColors(ss, sf).GetMyHash Then
                    Continue For
                Else
                    'aktualisieren, entspricht jetzt dem Ladezustand
                    _tileColorsHashOnLoad(ss, sf) = _tileColors(ss, sf).GetMyHash
                    With _tileColors(ss, sf)
                        .SteinDesign = EnumHelper.ParseOrDefault(Of SteinDesign)(cboSteinDesign.Text, SteinDesign.Default)
                        .SteinSatz = ss
                        .SteinFont = sf
                        .Save(checkBoxUseDevelopmentPath.Checked)
                    End With
                    If Not checkBoxUseDevelopmentPath.Checked Then
                        Try
                            Dim pathVbProj As String = TileColors.GetMahjongGKVbProjFullPath
                            Dim fullPath As String = TileColors.GetFullPathOnlyForSaving(SteinDesign, ss, sf, checkBoxUseDevelopmentPath.Checked)
                            'Die Registrierung stellt sicher, daß die TileColors auch mit dem Setup transportiert und installiert werden.
                            VbProjTileColorsUpdater.EnsureTileColorsFileRegistered(pathVbProj, fullPath)

                        Catch ex As Exception
                            MsgBox("Fehler:" & ex.ToString, MsgBoxStyle.Exclamation)
                        End Try
                    End If

                End If

            Next
        Next

        MsgBox($"Steindesign {SteinDesign.ToString} gespeichert.", MsgBoxStyle.Information)

    End Sub

#End Region

#Region "Änderungsüberwachung"

    Private _dirtyHash As String = Nothing
    Private Sub tmrDirtyStateDelay_Tick(sender As Object, e As EventArgs) _
        Handles tmrDirtyStateDelay.Tick

        tmrDirtyStateDelay.Stop()

        DoDirtyStateTracking()

    End Sub

    Private Sub DoDirtyStateTracking()
        Dim dirtyTileColors As TileColors = New TileColors
        TileColorsFormMapper.CopyFormToTileColors(dirtyTileColors, New Control() {Me})
        Dim hash As String = dirtyTileColors.GetMyHash
        If _dirtyHash <> hash Then
            _dirtyHash = hash
            CopyFormToTileColors()
            DrawAllTiles()
            Debug.Print("AutoHash geändert " & hash)
        End If
    End Sub

    Private _dirtyTrackingHooked As Boolean = False

    Private Sub HookDirtyStateTracking()
        If _dirtyTrackingHooked Then Return

        For Each tb As TrackBar In GetAllControls(Of TrackBar)(Me)
            AddHandler tb.ValueChanged, AddressOf DirtyStateControlChanged
        Next

        For Each num As NumericUpDown In GetAllControls(Of NumericUpDown)(Me)
            AddHandler num.ValueChanged, AddressOf DirtyStateControlChanged
        Next

        For Each cbo As ComboBox In GetAllControls(Of ComboBox)(Me)
            AddHandler cbo.SelectedIndexChanged, AddressOf DirtyStateControlChanged
            AddHandler cbo.TextChanged, AddressOf DirtyStateControlChanged
        Next

        For Each opt As RadioButton In GetAllControls(Of RadioButton)(Me)
            AddHandler opt.CheckedChanged, AddressOf RadioButtonDirtyStateChanged
        Next

        For Each chk As CheckBox In GetAllControls(Of CheckBox)(Me)
            AddHandler chk.CheckedChanged, AddressOf DirtyStateControlChanged
        Next

        For Each col As ColorBox In GetAllControls(Of ColorBox)(Me)
            AddHandler col.ColorChanged, AddressOf DirtyStateControlChanged
        Next

        _dirtyTrackingHooked = True
    End Sub

    Private Sub DirtyStateControlChanged(sender As Object, e As EventArgs)
        If _zuweisungAktiv <> 0 Then Return
        tmrDirtyStateDelay.Stop()
        tmrDirtyStateDelay.Start()
    End Sub

    Private Sub RadioButtonDirtyStateChanged(sender As Object, e As EventArgs)
        If _zuweisungAktiv <> 0 Then Return

        Dim opt As RadioButton = DirectCast(sender, RadioButton)

        ' Nur auf das Aktivieren reagieren, nicht auch auf das Abwählen.
        If Not opt.Checked Then Return

        tmrDirtyStateDelay.Stop()
        tmrDirtyStateDelay.Start()
    End Sub

    ''Private Iterator Function GetAllControls(Of T As Control)(
    ''    parent As Control) As IEnumerable(Of T)

    ''    If parent Is Nothing Then
    ''        Throw New ArgumentNullException(NameOf(parent))
    ''    End If

    ''    For Each child As Control In parent.Controls
    ''        If TypeOf child Is T Then
    ''            Yield DirectCast(child, T)
    ''        End If

    ''        For Each descendant As T In GetAllControls(Of T)(child)
    ''            Yield descendant
    ''        Next
    ''    Next
    ''End Function

#End Region

#Region "Entwicklungshelfer"

    Public Sub CopyNumPropertiesToClipboard(form As Form)

        If form Is Nothing Then
            Throw New ArgumentNullException(NameOf(form))
        End If

        Dim lines As New List(Of String)

        Dim copyNonColor As New StringBuilder
        copyNonColor.AppendLine("    Private ReadOnly _arrCopyNonColorValues() As String = {")

        Dim copyColor As New StringBuilder
        copyColor.AppendLine("    Private ReadOnly _arrCopyColorValues() As String = {")

        For Each ctl As NumericUpDown In GetAllControls(Of NumericUpDown)(form)

            Dim controlName As String = ctl.Name
            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If

            Dim controlValue As String
            If controlName.StartsWith("Faktor") Then
                controlValue = Decimal.Round(ctl.Value, ctl.DecimalPlaces).ToString
            Else
                controlValue = ctl.Value.ToString
            End If
            Dim controlTag As String = DirectCast(ctl.Tag, String)
            '

            If controlName.StartsWith("num", StringComparison.OrdinalIgnoreCase) Then
                Dim propertyName As String = controlName.Substring(3)

                If propertyName.Length = 0 Then
                    lines.Add($"Public Property {controlName} As Integer    '<=== (=Fehlerhaft)")

                ElseIf propertyName.Contains("Cloud") OrElse propertyName.Contains("Grain") Then
                    lines.Add($"Public Property {propertyName} As Decimal = {controlValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".")}D")

                ElseIf propertyName.StartsWith("Shift") OrElse propertyName.StartsWith("DShift") Then
                    'Debug.WriteLine($"{ctl.Name} | Text='{ctl.Text}' | Value={ctl.Value} | Min={ctl.Minimum} | Max={ctl.Maximum}")
                    'Warum erhalte ich hier als controlValue 
                    Dim sb As New System.Text.StringBuilder
                    sb.AppendLine($"Private _{propertyName} As Integer = {controlValue}")
                    sb.AppendLine($"Public Property {propertyName} As Integer")
                    sb.AppendLine("    Get")
                    sb.AppendLine($"        Return _{propertyName}")
                    sb.AppendLine("    End Get")
                    sb.AppendLine("    Set(value As Integer)")
                    sb.AppendLine($"        _{propertyName} = value")
                    sb.AppendLine($"        Create{propertyName}(_{propertyName})")
                    sb.AppendLine("    End Set")
                    sb.AppendLine("End Property")
                    lines.Add(sb.ToString)
                ElseIf propertyName.StartsWith("Faktor") Then
                    lines.Add($"Public Property {propertyName} As Decimal = {controlValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".")}D")

                ElseIf propertyName.StartsWith("DBrgSummenÄnderung") Then
                    lines.Add($"Public Property {propertyName} As Decimal = {controlValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".")}D")

                ElseIf propertyName.StartsWith("DSatSummenÄnderung") Then
                    lines.Add($"Public Property {propertyName} As Decimal = {controlValue.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".")}D")

                Else
                    lines.Add($"Public Property {propertyName} As Integer = {controlValue}")
                End If

                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                    copyNonColor.AppendLine($"        ""{propertyName}"",")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyColorValues" Then
                    copyColor.AppendLine($"        ""{propertyName}"",")
                End If
            Else
                lines.Add($"Public Property {controlName} As Integer    '<=== (=Fehlerhaft)")
            End If

        Next

        For Each ctl As RadioButton In GetAllControls(Of RadioButton)(form)

            Dim controlName As String = ctl.Name
            Dim controlValue As String = ctl.Checked.ToString

            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If
            If controlName.StartsWith("rad", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If
            Dim controlTag As String = DirectCast(ctl.Tag, String)

            If controlName.StartsWith("opt", StringComparison.OrdinalIgnoreCase) Then
                Dim propertyName As String = controlName.Substring(3)

                If controlName = "optUsedFontNotoSansSymbol2" Then
                    'nichts machen, ist in UsedFontSegeoUISymbol enthalten.
                    Continue For
                End If
                If controlName = "optUsedFontSegeoUISymbol" Then

                    Dim sb As New StringBuilder
                    sb.AppendLine("Public Property UsedFontNotoSansSymbol2 As Boolean")
                    sb.AppendLine("    Get")
                    sb.AppendLine("        Return _SteinFont = SteinFont.Noto")
                    sb.AppendLine("    End Get")
                    sb.AppendLine("    Set(value As Boolean)")
                    sb.AppendLine("        _SteinFont = If(value, SteinFont.Noto, SteinFont.Segoe)")
                    sb.AppendLine("    End Set")
                    sb.AppendLine("End Property")
                    lines.Add(sb.ToString)
                    sb.Clear()
                    sb.AppendLine("Public Property UsedFontSegeoUISymbol As Boolean")
                    sb.AppendLine("    Get")
                    sb.AppendLine("        Return _SteinFont = SteinFont.Segoe")
                    sb.AppendLine("    End Get")
                    sb.AppendLine("    Set(value As Boolean)")
                    sb.AppendLine("        _SteinFont = If(value, SteinFont.Segoe, SteinFont.Noto)")
                    sb.AppendLine("    End Set")
                    sb.AppendLine("End Property")
                    lines.Add(sb.ToString)
                    sb.Clear()
                    sb.AppendLine("Private _SteinFont As SteinFont = SteinFont.Segoe")
                    sb.AppendLine("<XmlIgnore>")
                    sb.AppendLine("Public Property SteinFont As SteinFont")
                    sb.AppendLine("    Get")
                    sb.AppendLine("        Return _SteinFont")
                    sb.AppendLine("    End Get")
                    sb.AppendLine("    Set(value As SteinFont)")
                    sb.AppendLine("        _SteinFont = value")
                    sb.AppendLine("    End Set")
                    sb.AppendLine("End Property")
                    lines.Add(sb.ToString)

                    Continue For
                End If

                If propertyName.Length > 0 Then
                    lines.Add($"Public Property {propertyName} As Boolean = {controlValue}")
                Else
                    lines.Add($"Public Property {controlName} As Boolean    '<=== (=Fehlerhaft)")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                    copyNonColor.AppendLine($"        ""{propertyName}"",")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyColorValues" Then
                    copyColor.AppendLine($"        ""{propertyName}"",")
                End If
            Else
                lines.Add($"Public Property {controlName} As Boolean    '<=== (=Fehlerhaft)")
            End If
        Next

        For Each ctl As CheckBox In GetAllControls(Of CheckBox)(form)
            Dim controlName As String = ctl.Name

            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If

            If controlName.StartsWith("CheckBox", StringComparison.InvariantCultureIgnoreCase) Then
                Continue For
            End If

            Dim controlValue As String = ctl.Checked.ToString
            Dim controlTag As String = DirectCast(ctl.Tag, String)

            If controlName.StartsWith("chk", StringComparison.OrdinalIgnoreCase) Then
                Dim propertyName As String = controlName.Substring(3)

                If propertyName.Length > 0 Then
                    lines.Add($"Public Property {propertyName} As Boolean = {controlValue}")
                Else
                    lines.Add($"Public Property {controlName} As Boolean    '<=== (=Fehlerhaft)")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                    copyNonColor.AppendLine($"        ""{propertyName}"",")
                End If
            Else
                lines.Add($"Public Property {controlName} As Boolean    '<=== (=Fehlerhaft)")
            End If
        Next

        For Each ctl As ColorBox In GetAllControls(Of ColorBox)(form)
            Dim controlName As String = ctl.Name

            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If
            Dim controlValue As String = ctl.ColorText
            Dim controlTag As String = DirectCast(ctl.Tag, String)

            If controlName.StartsWith("colbox", StringComparison.OrdinalIgnoreCase) Then
                Dim propertyName As String = controlName.Substring(6)

                If propertyName.Length > 0 Then
                    lines.Add($"Public Property {propertyName} As String = ""{controlValue}""")
                Else
                    lines.Add($"Public Property {controlName} As String    '<=== (=Fehlerhaft)")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                    copyNonColor.AppendLine($"        ""{propertyName}"",")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyColorValues" Then
                    copyColor.AppendLine($"        ""{propertyName}"",")
                End If
            Else
                lines.Add($"Public Property {controlName} As String    '<=== (=Fehlerhaft)")
            End If
        Next

        For Each ctl As TextBox In GetAllControls(Of TextBox)(form)
            Dim controlName As String = ctl.Name

            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If

            If controlName = "txtColor" Then
                Continue For
            End If
            Dim controlValue As String = ctl.Text
            Dim controlTag As String = DirectCast(ctl.Tag, String)

            If controlName.StartsWith("txt", StringComparison.OrdinalIgnoreCase) Then
                Dim propertyName As String = controlName.Substring(3)

                If propertyName.Length > 0 Then
                    lines.Add($"Public Property {propertyName} As String = ""{controlValue}""")
                Else
                    lines.Add($"Public Property {controlName} As String    '<=== (=Fehlerhaft)")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                    copyNonColor.AppendLine($"        ""{propertyName}"",")
                End If
                If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyColorValues" Then
                    copyColor.AppendLine($"        ""{propertyName}"",")
                End If
            Else
                lines.Add($"Public Property {controlName} As String    '<=== (=Fehlerhaft)")
            End If
        Next

        For Each ctl As TrackBar In GetAllControls(Of TrackBar)(form)
            Dim controlName As String = ctl.Name

            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If

            Dim controlTag As String = DirectCast(ctl.Tag, String)

            Dim propertyName As String = controlName.Substring(2)

            If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                copyNonColor.AppendLine($"        ""{propertyName}"",")
            End If
            If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyColorValues" Then
                copyColor.AppendLine($"        ""{propertyName}"",")
            End If
        Next

        For Each ctl As ComboBox In GetAllControls(Of ComboBox)(form)
            Dim controlName As String = ctl.Name

            If String.IsNullOrWhiteSpace(controlName) Then
                Continue For
            End If

            Dim controlTag As String = DirectCast(ctl.Tag, String)

            Dim propertyName As String = controlName.Substring(3)

            If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyNonColorValues" Then
                copyNonColor.AppendLine($"        ""{propertyName}"",")
            End If
            If Not String.IsNullOrEmpty(controlTag) AndAlso controlTag = "CopyColorValues" Then
                copyColor.AppendLine($"        ""{propertyName}"",")
            End If
        Next

        lines.Sort(New PrefixIgnoringStringComparer)

        Dim result As String = String.Join(Environment.NewLine, lines)

        copyNonColor.AppendLine("    }")
        SortStringbuilder(copyNonColor, preserveFirstAndLastLine:=True)
        copyNonColor.Remove(copyNonColor.Length - 10, 1) 'das letzte Komma entfernen
        copyNonColor.AppendLine()

        copyColor.AppendLine("    }")
        SortStringbuilder(copyColor, preserveFirstAndLastLine:=True)
        copyColor.Remove(copyColor.Length - 10, 1)
        copyColor.AppendLine()

        copyNonColor.AppendLine(copyColor.ToString)
        copyNonColor.AppendLine()
        copyNonColor.AppendLine(result)

        Clipboard.SetText(copyNonColor.ToString)

    End Sub

    Private Iterator Function GetAllControls(Of T As Control)(parent As Control) As IEnumerable(Of T)
        For Each ctrl As Control In parent.Controls
            If TypeOf ctrl Is T Then
                Yield DirectCast(ctrl, T)
            End If

            For Each child As T In GetAllControls(Of T)(ctrl)
                Yield child
            Next
        Next
    End Function

    Public NotInheritable Class PrefixIgnoringStringComparer
        Implements IComparer(Of String)

        Private Shared ReadOnly _prefixes As String() = {
        "Public Property Alpha",
        "Public Property DAlpha",
        "Public Property Hue",
        "Public Property DHue",
        "Public Property Sat",
        "Public Property DSat",
        "Public Property Brg",
        "Public Property DBrg",
        "Public Property ",
        "Private _"
    }

        Public Function Compare(x As String, y As String) As Integer _
        Implements IComparer(Of String).Compare

            Dim sx As String = StripKnownPrefix(x)
            Dim sy As String = StripKnownPrefix(y)

            Return StringComparer.CurrentCultureIgnoreCase.Compare(sx, sy)

        End Function

        Private Shared Function StripKnownPrefix(value As String) As String

            If value Is Nothing Then
                Return String.Empty
            End If

            For Each prefix As String In _prefixes
                If value.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase) Then
                    Return value.Substring(prefix.Length)
                End If
            Next

            Return value

        End Function

    End Class

    Private Sub SortStringbuilder(ByRef sb As StringBuilder,
                             Optional preserveFirstAndLastLine As Boolean = False)

        If sb Is Nothing Then
            Throw New ArgumentNullException(NameOf(sb))
        End If

        Dim text As String = sb.ToString()

        Dim lines As List(Of String) =
            text.Replace(vbCrLf, vbLf).
                 Replace(vbCr, vbLf).
                 Split(ControlChars.Lf).
                 ToList()

        If preserveFirstAndLastLine AndAlso lines.Count >= 3 Then

            Dim firstLine As String = lines(0)
            Dim lastLine As String = lines(lines.Count - 1)

            Dim middleLines As List(Of String) =
            lines.GetRange(1, lines.Count - 2)

            middleLines.Sort(StringComparer.CurrentCultureIgnoreCase)

            lines.Clear()
            lines.Add(firstLine)
            lines.AddRange(middleLines)
            lines.Add(lastLine)

        Else

            lines.Sort(StringComparer.CurrentCultureIgnoreCase)

        End If

        sb.Clear()
        sb.Append(String.Join(Environment.NewLine, lines))

    End Sub
    Private Sub btnCopyLayerNormalToOtherValues_Click(sender As Object, e As EventArgs) Handles btnCopyLayerNormalToOtherValues.Click

        numDeltaHueLayerWinde.Value = numDeltaHueLayerNormal.Value
        numDeltaHueLayerBlüten.Value = numDeltaHueLayerNormal.Value
        numDeltaHueLayerJZeiten.Value = numDeltaHueLayerNormal.Value
        '
        '
        numSatLayerUpWinde.Value = numSatLayerUpNormal.Value
        numSatLayerUpBlüten.Value = numSatLayerUpNormal.Value
        numSatLayerUpJZeiten.Value = numSatLayerUpNormal.Value

        numBrgLayerUpWinde.Value = numBrgLayerUpNormal.Value
        numBrgLayerUpBlüten.Value = numBrgLayerUpNormal.Value
        numBrgLayerUpJZeiten.Value = numBrgLayerUpNormal.Value
        '
        numSatLayerMidUpWinde.Value = numSatLayerMidUpNormal.Value
        numSatLayerMidUpBlüten.Value = numSatLayerMidUpNormal.Value
        numSatLayerMidUpJZeiten.Value = numSatLayerMidUpNormal.Value

        numBrgLayerMidUpWinde.Value = numBrgLayerMidUpNormal.Value
        numBrgLayerMidUpBlüten.Value = numBrgLayerMidUpNormal.Value
        numBrgLayerMidUpJZeiten.Value = numBrgLayerMidUpNormal.Value

        numSatLayerMidDnWinde.Value = numSatLayerMidDnNormal.Value
        numSatLayerMidDnBlüten.Value = numSatLayerMidDnNormal.Value
        numSatLayerMidDnJZeiten.Value = numSatLayerMidDnNormal.Value

        numBrgLayerMidDnWinde.Value = numBrgLayerMidDnNormal.Value
        numBrgLayerMidDnBlüten.Value = numBrgLayerMidDnNormal.Value
        numBrgLayerMidDnJZeiten.Value = numBrgLayerMidDnNormal.Value
        '
        '
        numSatLayerDnWinde.Value = numSatLayerDnNormal.Value
        numSatLayerDnBlüten.Value = numSatLayerDnNormal.Value
        numSatLayerDnJZeiten.Value = numSatLayerDnNormal.Value

        numBrgLayerDnWinde.Value = numBrgLayerDnNormal.Value
        numBrgLayerDnBlüten.Value = numBrgLayerDnNormal.Value
        numBrgLayerDnJZeiten.Value = numBrgLayerDnNormal.Value

        numSatLayerLineWinde.Value = numSatLayerLineNormal.Value
        numSatLayerLineBlüten.Value = numSatLayerLineNormal.Value
        numSatLayerLineJZeiten.Value = numSatLayerLineNormal.Value
        '
        '
        numBrgLayerLineWinde.Value = numBrgLayerLineNormal.Value
        numBrgLayerLineBlüten.Value = numBrgLayerLineNormal.Value
        numBrgLayerLineJZeiten.Value = numBrgLayerLineNormal.Value

        numShiftLayerSelectedWinde.Value = numShiftLayerSelectedNormal.Value
        numShiftLayerSelectedBlüten.Value = numShiftLayerSelectedNormal.Value
        numShiftLayerSelectedJZeiten.Value = numShiftLayerSelectedNormal.Value

        numShiftLayerSelectableWinde.Value = numShiftLayerSelectableNormal.Value
        numShiftLayerSelectableBlüten.Value = numShiftLayerSelectableNormal.Value
        numShiftLayerSelectableJZeiten.Value = numShiftLayerSelectableNormal.Value

        numShiftLayerRemovableWinde.Value = numShiftLayerRemovableNormal.Value
        numShiftLayerRemovableBlüten.Value = numShiftLayerRemovableNormal.Value
        numShiftLayerRemovableJZeiten.Value = numShiftLayerRemovableNormal.Value

    End Sub

    Private Sub btnLoadEmptyFile_Click(sender As Object, e As EventArgs) Handles btnLoadEmptyFile.Click
        Dim msg As String = $"ACHTUNG: geladene Daten vom Steinsatz.{SteinSatz} {vbCrLf}gehen verloren!     Abbrechen ?"
        If MsgBox(msg, MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = vbYes Then
            Exit Sub
        End If

        _tileColors(SteinSatz, SteinFont) = New TileColors
        CopyTileColorsToForm()

        DrawAllTiles(tileColorsInvalide:=True)

    End Sub

    Private Sub btnOpenExplorer_Click(sender As Object, e As EventArgs) Handles btnOpenExplorer.Click

        Dim folderPath As String
        If checkBoxUseDevelopmentPath.Checked Then
            folderPath = IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "MahjongGKTileFactoryTester")
        Else
            folderPath = IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "MahjongGK")
        End If

        IO.Directory.CreateDirectory(folderPath)

        Dim psi As New ProcessStartInfo()
        psi.FileName = "explorer.exe"
        psi.Arguments = """" & folderPath & """"

        Process.Start(psi)

    End Sub

    Private Sub btnCopy_Click(sender As Object, e As EventArgs) Handles btnCopy.Click

        If radCopyLightSrc.Checked Then
            If radCopyLightDst.Checked Then
                MsgBox("Quelle und Ziel identisch", MsgBoxStyle.Exclamation)
                Exit Sub
            ElseIf radCopyMediumDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Medium, SteinFont) Then Exit Sub
                _tileColors(1, SteinFont) = _tileColors(0, SteinFont).DeepCopy
            ElseIf radCopyDarkDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Dark, SteinFont) Then Exit Sub
                _tileColors(2, SteinFont) = _tileColors(0, SteinFont).DeepCopy
            Else
                MsgBox("Kein Ziel ausgewählt", MsgBoxStyle.Exclamation)
                Exit Sub
            End If

        ElseIf radCopyMediumSrc.Checked Then
            If radCopyLightDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Light, SteinFont) Then Exit Sub
                _tileColors(0, SteinFont) = _tileColors(1, SteinFont).DeepCopy
            ElseIf radCopyMediumDst.Checked Then
                MsgBox("Quelle und Ziel identisch", MsgBoxStyle.Exclamation)
            ElseIf radCopyDarkDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Dark, SteinFont) Then Exit Sub
                _tileColors(2, SteinFont) = _tileColors(1, SteinFont).DeepCopy
            Else
                MsgBox("Kein Ziel ausgewählt", MsgBoxStyle.Exclamation)
                Exit Sub
            End If

        ElseIf radCopyDarkSrc.Checked Then
            If radCopyLightDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Light, SteinFont) Then Exit Sub
                _tileColors(0, SteinFont) = _tileColors(2, SteinFont).DeepCopy
            ElseIf radCopyMediumDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Medium, SteinFont) Then Exit Sub
                _tileColors(1, SteinFont) = _tileColors(2, SteinFont).DeepCopy
            ElseIf radCopyDarkDst.Checked Then
                MsgBox("Quelle und Ziel identisch", MsgBoxStyle.Exclamation)
                Exit Sub
            Else
                MsgBox("Kein Ziel ausgewählt", MsgBoxStyle.Exclamation)
                Exit Sub
            End If
        Else
            MsgBox("Keine Quelle ausgewählt", MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        CopyTileColorsToForm()
        DrawAllTiles()
        MsgBox("Kopiert", MsgBoxStyle.Information)
    End Sub

    Private Sub btnSwap_Click(sender As Object, e As EventArgs) Handles btnSwap.Click

        If radCopyLightSrc.Checked Then
            If radCopyLightDst.Checked Then
                MsgBox("Quelle und Ziel identisch", MsgBoxStyle.Exclamation)
                Exit Sub
            ElseIf radCopyMediumDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Medium, SteinFont) Then Exit Sub
                Swap(1, 0)
            ElseIf radCopyDarkDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Dark, SteinFont) Then Exit Sub
                Swap(2, 0)
            Else
                MsgBox("Kein Ziel ausgewählt", MsgBoxStyle.Exclamation)
                Exit Sub
            End If

        ElseIf radCopyMediumSrc.Checked Then
            If radCopyLightDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Light, SteinFont) Then Exit Sub
                Swap(0, 1)
            ElseIf radCopyMediumDst.Checked Then
                MsgBox("Quelle und Ziel identisch", MsgBoxStyle.Exclamation)
            ElseIf radCopyDarkDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Dark, SteinFont) Then Exit Sub
                Swap(2, 1)
            Else
                MsgBox("Kein Ziel ausgewählt", MsgBoxStyle.Exclamation)
                Exit Sub
            End If

        ElseIf radCopyDarkSrc.Checked Then
            If radCopyLightDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Light, SteinFont) Then Exit Sub
                Swap(0, 2)
            ElseIf radCopyMediumDst.Checked Then
                If SteinsatzMustSaved(SteinSatz.Medium, SteinFont) Then Exit Sub
                Swap(1, 2)
            ElseIf radCopyDarkDst.Checked Then
                MsgBox("Quelle und Ziel identisch", MsgBoxStyle.Exclamation)
                Exit Sub
            Else
                MsgBox("Kein Ziel ausgewählt", MsgBoxStyle.Exclamation)
                Exit Sub
            End If
        Else
            MsgBox("Keine Quelle ausgewählt", MsgBoxStyle.Exclamation)
            Exit Sub
        End If
        CopyTileColorsToForm()
        DrawAllTiles(tileColorsInvalide:=True)
        MsgBox("Vertauscht", MsgBoxStyle.Information)
    End Sub

    Private Sub Swap(idx0 As Integer, idx1 As Integer)

        Dim swap As TileColors

        swap = _tileColors(idx0, SteinFont)
        _tileColors(idx0, SteinFont) = _tileColors(idx1, SteinFont)
        _tileColors(idx1, SteinFont) = swap

    End Sub

    Private Function SteinsatzMustSaved(steinSatz As SteinSatz, steinFont As SteinFont) As Boolean

        Dim tc As TileColors = TileColors.Load(SteinDesign, steinSatz, steinFont, checkBoxUseDevelopmentPath.Checked)

        _tileColors(steinSatz, steinFont).CompareTileColors(tc)

        If tc.GetMyHash = _tileColors(Me.SteinSatz, steinFont).GetMyHash Then
            If Global.Microsoft.VisualBasic.Interaction.MsgBox($"Der aktuell geladene Datensaz SteinDesign.{SteinDesign}, SteinSatz.{Me.SteinSatz} ist nicht gespeichert.{Global.Microsoft.VisualBasic.Constants.vbCrLf} Kopieren abbrechen?", Global.Microsoft.VisualBasic.MsgBoxStyle.Question Or Global.Microsoft.VisualBasic.MsgBoxStyle.YesNo) = Global.Microsoft.VisualBasic.MsgBoxResult.Yes Then
                Return True
            End If
        End If

        Return False

    End Function

    Private Sub btnSymbolComparer_Click(sender As Object, e As EventArgs) Handles btnSymbolComparer.Click
        Using frm As New FrmFontCompare
            frm.ShowDialog()
        End Using
    End Sub

    Private Sub btnNurNeuZeichenen_Click(sender As Object, e As EventArgs) Handles btnNurNeuZeichenen.Click
        _tileColors(SteinSatz, SteinFont).SetNewIdent()
        DrawAllTiles(tileColorsInvalide:=True)
    End Sub

    Private Sub btnTileColorFilesCheck_Click(sender As Object, e As EventArgs) Handles btnTileColorFilesCheck.Click
        Using frm As New frmTileColorFilesCheck()
            frm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub btnLoadFileDialog_Click(sender As Object, e As EventArgs) Handles btnLoadFileDialog.Click
        Using frm As New FrmTileColorsLaden()

            If frm.ShowDialog(Me) = DialogResult.OK Then

                Dim colors As TileColors = frm.GeladeneTileColors
                Dim ladenNach As LadeNach = frm.AusgewaehltLadeNach

                If colors IsNot Nothing Then
                    colors.SetNewIdent()
                    optUsedFontNotoSansSymbol2.Checked = colors.UsedFontNotoSansSymbol2
                    optUsedFontSegeoUISymbol.Checked = colors.UsedFontSegeoUISymbol
                    _tileColors(ladenNach, SteinFont) = colors
                End If
                CopyTileColorsToForm()
                DrawAllTiles(tileColorsInvalide:=True)
            End If

        End Using
    End Sub

    Private Sub btnBlockI01BisI04Syncronisieren_Click(sender As Object, e As EventArgs) Handles btnBlockI01BisI04Syncronisieren.Click

        numSatI01NormalBlüten.Value = numSatI01NormalNormal.Value
        numSatI01NormalJZeiten.Value = numSatI01NormalNormal.Value
        numSatI01NormalWinde.Value = numSatI01NormalNormal.Value
        numBrgI01NormalBlüten.Value = numBrgI01NormalNormal.Value
        numBrgI01NormalJZeiten.Value = numBrgI01NormalNormal.Value
        numBrgI01NormalWinde.Value = numBrgI01NormalNormal.Value
        numDShiftNormalBlüten.Value = numBrgI01NormalNormal.Value
        numDShiftNormalJZeiten.Value = numDShiftNormalNormal.Value
        numDShiftNormalWinde.Value = numDShiftNormalNormal.Value

        numSatI02SelectedBlüten.Value = numSatI02SelectedNormal.Value
        numSatI02SelectedJZeiten.Value = numSatI02SelectedNormal.Value
        numSatI02SelectedWinde.Value = numSatI02SelectedNormal.Value
        numBrgI02SelectedBlüten.Value = numBrgI02SelectedNormal.Value
        numBrgI02SelectedJZeiten.Value = numBrgI02SelectedNormal.Value
        numBrgI02SelectedWinde.Value = numBrgI02SelectedNormal.Value
        numDShiftNormalBlüten.Value = numBrgI02SelectedNormal.Value
        numDShiftNormalJZeiten.Value = numDShiftNormalNormal.Value
        numDShiftNormalWinde.Value = numDShiftNormalNormal.Value

        numSatI03SelectableBlüten.Value = numSatI03SelectableNormal.Value
        numSatI03SelectableJZeiten.Value = numSatI03SelectableNormal.Value
        numSatI03SelectableWinde.Value = numSatI03SelectableNormal.Value
        numBrgI03SelectableBlüten.Value = numBrgI03SelectableNormal.Value
        numBrgI03SelectableJZeiten.Value = numBrgI03SelectableNormal.Value
        numBrgI03SelectableWinde.Value = numBrgI03SelectableNormal.Value
        numDShiftNormalBlüten.Value = numBrgI03SelectableNormal.Value
        numDShiftNormalJZeiten.Value = numDShiftNormalNormal.Value
        numDShiftNormalWinde.Value = numDShiftNormalNormal.Value

        numSatI04RemovableBlüten.Value = numSatI04RemovableNormal.Value
        numSatI04RemovableJZeiten.Value = numSatI04RemovableNormal.Value
        numSatI04RemovableWinde.Value = numSatI04RemovableNormal.Value
        numBrgI04RemovableBlüten.Value = numBrgI04RemovableNormal.Value
        numBrgI04RemovableJZeiten.Value = numBrgI04RemovableNormal.Value
        numBrgI04RemovableWinde.Value = numBrgI04RemovableNormal.Value
        numDShiftNormalBlüten.Value = numBrgI04RemovableNormal.Value
        numDShiftNormalJZeiten.Value = numDShiftNormalNormal.Value
        numDShiftNormalWinde.Value = numDShiftNormalNormal.Value

    End Sub

    Private Sub btnVbProjAufräumen_Click(sender As Object, e As EventArgs) Handles btnVbProjAufräumen.Click

        Dim sb As New StringBuilder
        sb.AppendLine("HINWEIS")
        sb.AppendLine("Die IDE mit MahjongGL darf nicht laufen. IDE komplett schließen.")
        sb.AppendLine()
        sb.AppendLine("Die Funktion entfernt verweiste Einträge von TileColors.xml- Dateien und trägt nicht eingetragene Dateien ein.")
        sb.AppendLine("(z.B. nach umbenennen von Verzeichnissen.)")
        If MsgBox(sb.ToString, MsgBoxStyle.Information Or MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
            Exit Sub
        End If

        Dim pathVbProj As String = TileColors.GetMahjongGKVbProjFullPath

        Dim pathTCRoot As String = TileColors.GetProjectTileColorsRoot
        Try
            VbProjTileColorsUpdater.RemoveOrphanedTileColorsEntries(pathVbProj)
            VbProjTileColorsUpdater.EnsureAllTileColorsRegistered(pathVbProj, pathTCRoot)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub numFaceACloudSteinStatusNormal_ValueChanged(sender As Object, e As EventArgs) _
        Handles numFaceACloudSteinStatusNormal.ValueChanged,
                numFaceAGrainSteinStatusNormal.ValueChanged,
                numFaceICloudSteinStatusNormal.ValueChanged,
                numFaceIGrainSteinStatusNormal.ValueChanged,
                chkFaceUseInnerAreaSteinStatusNormal.CheckedChanged,
                numShiftLightMapSteinStatusNormal.ValueChanged

        If _zuweisungAktiv <> 0 Then Exit Sub
        If Not chkCloudGrainKoppeln.Checked Then
            Exit Sub
        End If

        numFaceACloudSteinStatusSelected.Value = numFaceACloudSteinStatusNormal.Value
        numFaceAGrainSteinStatusSelected.Value = numFaceAGrainSteinStatusNormal.Value
        numFaceICloudSteinStatusSelected.Value = numFaceICloudSteinStatusNormal.Value
        numFaceIGrainSteinStatusSelected.Value = numFaceIGrainSteinStatusNormal.Value
        chkFaceUseInnerAreaSteinStatusSelected.Checked = chkFaceUseInnerAreaSteinStatusNormal.Checked

        numFaceACloudSteinStatusSelectable.Value = numFaceACloudSteinStatusNormal.Value
        numFaceAGrainSteinStatusSelectable.Value = numFaceAGrainSteinStatusNormal.Value
        numFaceICloudSteinStatusSelectable.Value = numFaceICloudSteinStatusNormal.Value
        numFaceIGrainSteinStatusSelectable.Value = numFaceIGrainSteinStatusNormal.Value
        chkFaceUseInnerAreaSteinStatusSelectable.Checked = chkFaceUseInnerAreaSteinStatusNormal.Checked

        numFaceACloudSteinStatusRemovable.Value = numFaceACloudSteinStatusNormal.Value
        numFaceAGrainSteinStatusRemovable.Value = numFaceAGrainSteinStatusNormal.Value
        numFaceICloudSteinStatusRemovable.Value = numFaceICloudSteinStatusNormal.Value
        numFaceIGrainSteinStatusRemovable.Value = numFaceIGrainSteinStatusNormal.Value
        chkFaceUseInnerAreaSteinStatusRemovable.Checked = chkFaceUseInnerAreaSteinStatusNormal.Checked

        numShiftLightMapSteinStatusSelected.Value = numShiftLightMapSteinStatusNormal.Value
        numShiftLightMapSteinStatusSelectable.Value = numShiftLightMapSteinStatusNormal.Value
        numShiftLightMapSteinStatusRemovable.Value = numShiftLightMapSteinStatusNormal.Value
    End Sub

#End Region

#Region "Unterlege Spielfeld für Tiles"

    'Die beiden relevanten Funktionen:
    'ZeichneSpielfeldUntergrund(picSpielfeld, _arrPicTiles)
    'UnterlegeSpielfeldFuerTiles(picSpielfeld, _arrPicTiles)

    Private Sub UnterlegeSpielfeldFuerTiles(picSpielfeld As PictureBox,
                                            arrPicTiles() As PictureBox)

        If picSpielfeld Is Nothing OrElse picSpielfeld.Image Is Nothing Then
            Exit Sub
        End If

        If arrPicTiles Is Nothing Then
            Exit Sub
        End If

        Dim spielfeldImage As Image = picSpielfeld.Image
        Dim displayRect As Rectangle = GetImageDisplayRectangle(picSpielfeld)

        For Each picTile As PictureBox In arrPicTiles

            If picTile Is Nothing Then
                Continue For
            End If

            Dim tileRectOnScreen As Rectangle =
                New Rectangle(picTile.PointToScreen(Point.Empty), picTile.Size)

            Dim spielfeldRectOnScreen As Rectangle =
                New Rectangle(picSpielfeld.PointToScreen(Point.Empty), picSpielfeld.Size)

            Dim tileRectRelativeToSpielfeld As New Rectangle(
                tileRectOnScreen.Left - spielfeldRectOnScreen.Left,
                tileRectOnScreen.Top - spielfeldRectOnScreen.Top,
                tileRectOnScreen.Width,
                tileRectOnScreen.Height)

            Dim srcRect As RectangleF =
                ControlRectToImageRect(tileRectRelativeToSpielfeld,
                                       displayRect,
                                       spielfeldImage.Size)

            If srcRect.Width <= 0.0F OrElse srcRect.Height <= 0.0F Then
                picTile.BackgroundImage = Nothing
                Continue For
            End If

            Dim bg As New Bitmap(picTile.Width, picTile.Height, Imaging.PixelFormat.Format32bppArgb)

            Using g As Graphics = Graphics.FromImage(bg)
                g.Clear(Color.Transparent)

                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

                Dim destRect As New Rectangle(0, 0, bg.Width, bg.Height)

                g.DrawImage(spielfeldImage,
                            destRect,
                            srcRect.X,
                            srcRect.Y,
                            srcRect.Width,
                            srcRect.Height,
                            GraphicsUnit.Pixel)
            End Using

            If picTile.BackgroundImage IsNot Nothing Then
                picTile.BackgroundImage.Dispose()
            End If

            picTile.BackgroundImage = bg
            picTile.BackgroundImageLayout = ImageLayout.Stretch
            picTile.BackColor = Color.Transparent

        Next

    End Sub

    Private Function GetImageDisplayRectangle(pic As PictureBox) As Rectangle

        If pic.Image Is Nothing Then
            Return Rectangle.Empty
        End If

        Dim imgSize As Size = pic.Image.Size
        Dim boxSize As Size = pic.ClientSize

        Select Case pic.SizeMode

            Case PictureBoxSizeMode.StretchImage
                Return New Rectangle(0, 0, boxSize.Width, boxSize.Height)

            Case PictureBoxSizeMode.Zoom

                Dim ratioX As Single = CSng(boxSize.Width) / CSng(imgSize.Width)
                Dim ratioY As Single = CSng(boxSize.Height) / CSng(imgSize.Height)
                Dim ratio As Single = Math.Min(ratioX, ratioY)

                Dim w As Integer = CInt(imgSize.Width * ratio)
                Dim h As Integer = CInt(imgSize.Height * ratio)

                Dim x As Integer = (boxSize.Width - w) \ 2
                Dim y As Integer = (boxSize.Height - h) \ 2

                Return New Rectangle(x, y, w, h)

            Case PictureBoxSizeMode.CenterImage

                Dim x As Integer = (boxSize.Width - imgSize.Width) \ 2
                Dim y As Integer = (boxSize.Height - imgSize.Height) \ 2

                Return New Rectangle(x, y, imgSize.Width, imgSize.Height)

            Case Else
                Return New Rectangle(0, 0, imgSize.Width, imgSize.Height)

        End Select

    End Function

    Private Function ControlRectToImageRect(controlRect As Rectangle,
                                        imageDisplayRect As Rectangle,
                                        imageSize As Size) As RectangleF

        Dim scaleX As Single = CSng(imageSize.Width) / CSng(imageDisplayRect.Width)
        Dim scaleY As Single = CSng(imageSize.Height) / CSng(imageDisplayRect.Height)

        Dim x As Single = CSng(controlRect.Left - imageDisplayRect.Left) * scaleX
        Dim y As Single = CSng(controlRect.Top - imageDisplayRect.Top) * scaleY
        Dim w As Single = CSng(controlRect.Width) * scaleX
        Dim h As Single = CSng(controlRect.Height) * scaleY

        Return New RectangleF(x, y, w, h)

    End Function
    Private Sub ZeichneSpielfeldUntergrund(picSpielfeld As PictureBox,
                                       arrPicTiles() As PictureBox)

        If picSpielfeld Is Nothing Then
            Exit Sub
        End If

        If arrPicTiles Is Nothing OrElse arrPicTiles.Length < 16 Then
            Exit Sub
        End If

        Dim bmpUntergrund As New Bitmap(picSpielfeld.Width,
                                    picSpielfeld.Height,
                                    Imaging.PixelFormat.Format32bppArgb)

        Using g As Graphics = Graphics.FromImage(bmpUntergrund)

            g.Clear(SystemColors.Control)

            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

            For groupStart As Integer = 0 To 12 Step 4

                Dim firstPic As PictureBox = arrPicTiles(groupStart)

                If firstPic Is Nothing OrElse firstPic.Image Is Nothing Then
                    Continue For
                End If

                Dim tileWidth As Integer = firstPic.Width
                Dim tileHeight As Integer = firstPic.Height
                Dim halfOffset As Integer = tileWidth \ 2

                For i As Integer = 0 To 3

                    Dim picTile As PictureBox = arrPicTiles(groupStart + i)

                    If picTile Is Nothing OrElse picTile.Image Is Nothing Then
                        Continue For
                    End If

                    Dim posInSpielfeld As Point =
                    picSpielfeld.PointToClient(picTile.PointToScreen(Point.Empty))

                    Dim destRect As New Rectangle(posInSpielfeld.X - halfOffset,
                                              posInSpielfeld.Y,
                                              tileWidth,
                                              tileHeight)

                    g.DrawImage(picTile.Image, destRect)

                Next

                'Ersten Stein rechts als fünften Stein anhängen
                Dim lastPic As PictureBox = arrPicTiles(groupStart + 3)

                If lastPic IsNot Nothing Then

                    Dim lastPos As Point =
                    picSpielfeld.PointToClient(lastPic.PointToScreen(Point.Empty))

                    Dim destRectAnhang As New Rectangle(lastPos.X - halfOffset + tileWidth,
                                                    lastPos.Y,
                                                    tileWidth,
                                                    tileHeight)

                    g.DrawImage(firstPic.Image, destRectAnhang)

                End If

            Next

        End Using

        If picSpielfeld.Image IsNot Nothing Then
            picSpielfeld.Image.Dispose()
        End If

        picSpielfeld.Image = bmpUntergrund
        picSpielfeld.SizeMode = PictureBoxSizeMode.Normal
        picSpielfeld.BackColor = SystemColors.Control

    End Sub

    Private Sub chkGhostUseFastMethode_CheckedChanged(sender As Object, e As EventArgs) Handles chkGhostUseFastMethode.CheckedChanged
        numDHueGhost.Enabled = Not chkGhostUseFastMethode.Checked
        numDSatGhost.Enabled = Not chkGhostUseFastMethode.Checked
    End Sub

    Private Sub btnSwapSelectableRemovable_Click(sender As Object, e As EventArgs) Handles btnSwapSelectableRemovable.Click

        Dim ns1 As Decimal = numSatI03SelectableNormal.Value
        Dim ws1 As Decimal = numSatI03SelectableWinde.Value
        Dim bs1 As Decimal = numSatI03SelectableBlüten.Value
        Dim js1 As Decimal = numSatI03SelectableJZeiten.Value
        Dim ns2 As Decimal = numBrgI03SelectableNormal.Value
        Dim ws2 As Decimal = numBrgI03SelectableWinde.Value
        Dim bs2 As Decimal = numBrgI03SelectableBlüten.Value
        Dim js2 As Decimal = numBrgI03SelectableJZeiten.Value

        Dim nr1 As Decimal = numSatI04RemovableNormal.Value
        Dim wr1 As Decimal = numSatI04RemovableWinde.Value
        Dim br1 As Decimal = numSatI04RemovableBlüten.Value
        Dim jr1 As Decimal = numSatI04RemovableJZeiten.Value
        Dim nr2 As Decimal = numBrgI04RemovableNormal.Value
        Dim wr2 As Decimal = numBrgI04RemovableWinde.Value
        Dim br2 As Decimal = numBrgI04RemovableBlüten.Value
        Dim jr2 As Decimal = numBrgI04RemovableJZeiten.Value

        numSatI04RemovableNormal.Value = ns1
        numSatI04RemovableWinde.Value = ws1
        numSatI04RemovableBlüten.Value = bs1
        numSatI04RemovableJZeiten.Value = js1
        numBrgI04RemovableNormal.Value = ns2
        numBrgI04RemovableWinde.Value = ws2
        numBrgI04RemovableBlüten.Value = bs2
        numBrgI04RemovableJZeiten.Value = js2

        numSatI03SelectableNormal.Value = nr1
        numSatI03SelectableWinde.Value = wr1
        numSatI03SelectableBlüten.Value = br1
        numSatI03SelectableJZeiten.Value = jr1
        numBrgI03SelectableNormal.Value = nr2
        numBrgI03SelectableWinde.Value = wr2
        numBrgI03SelectableBlüten.Value = br2
        numBrgI03SelectableJZeiten.Value = jr2

        Dim fas As String = cboFaceLightMapSelectable.Text
        Dim shs As Decimal = numShiftLightMapSteinStatusSelectable.Value
        Dim acs As Decimal = numFaceACloudSteinStatusSelectable.Value
        Dim ags As Decimal = numFaceAGrainSteinStatusSelectable.Value
        Dim ics As Decimal = numFaceICloudSteinStatusSelectable.Value
        Dim igs As Decimal = numFaceIGrainSteinStatusSelectable.Value
        Dim fus As Boolean = chkFaceUseInnerAreaSteinStatusSelectable.Checked

        Dim far As String = cboFaceLightMapRemovable.Text
        Dim shr As Decimal = numShiftLightMapSteinStatusRemovable.Value
        Dim acr As Decimal = numFaceACloudSteinStatusRemovable.Value
        Dim agr As Decimal = numFaceAGrainSteinStatusRemovable.Value
        Dim icr As Decimal = numFaceICloudSteinStatusRemovable.Value
        Dim igr As Decimal = numFaceIGrainSteinStatusRemovable.Value
        Dim fur As Boolean = chkFaceUseInnerAreaSteinStatusRemovable.Checked

        cboFaceLightMapSelectable.Text = far
        numShiftLightMapSteinStatusSelectable.Value = shr
        numFaceACloudSteinStatusSelectable.Value = acr
        numFaceAGrainSteinStatusSelectable.Value = agr
        numFaceICloudSteinStatusSelectable.Value = icr
        numFaceIGrainSteinStatusSelectable.Value = igr
        chkFaceUseInnerAreaSteinStatusSelectable.Checked = fur

        cboFaceLightMapRemovable.Text = fas
        numShiftLightMapSteinStatusRemovable.Value = shs
        numFaceACloudSteinStatusRemovable.Value = acs
        numFaceAGrainSteinStatusRemovable.Value = ags
        numFaceICloudSteinStatusRemovable.Value = ics
        numFaceIGrainSteinStatusRemovable.Value = igs
        chkFaceUseInnerAreaSteinStatusRemovable.Checked = fus

        DrawAllTiles(tileColorsInvalide:=True)
    End Sub

    Private Sub DebugDrawKreuz(gfx As Graphics, rect As Rectangle)
        'Debug-Kreuz im Zielrechteck zeichnen
        Using pWhite As New Pen(Color.White, 1.0F),
              pBlack As New Pen(Color.Black, 1.0F)
            rect.Inflate(New Size(-30, -30))
            'weißes Kreuz
            gfx.DrawLine(pWhite, rect.Left, rect.Top, rect.Right - 1, rect.Bottom - 1)
            gfx.DrawLine(pWhite, rect.Right - 1, rect.Top, rect.Left, rect.Bottom - 1)

            'schwarzes Kreuz leicht versetzt
            gfx.DrawLine(pBlack, rect.Left + 1, rect.Top, rect.Right - 1, rect.Bottom - 2)
            gfx.DrawLine(pBlack, rect.Right - 2, rect.Top, rect.Left, rect.Bottom - 1)

        End Using
    End Sub

    Private Sub btnSetStandardSymbole_Click(sender As Object, e As EventArgs) Handles btnSetStandardSymbole.Click
        txtTextSymbole.Text = "NSOW▲✿✱!"
    End Sub

    Private Sub btnMediumWerteNachLoghtUndDarkKopieren_Click(sender As Object, e As EventArgs)

    End Sub

    '_tileColors(SteinSatz, SteinFont)
    Const L As Integer = 0 'Light
    Const M As Integer = 1 'Medium
    Const D As Integer = 2 'Dark
    Const S As Integer = 0 'Segeo
    Const N As Integer = 1 'Noto

    Private Sub btnCopyDoJob_Click(sender As Object, e As EventArgs) Handles btnCopyDoJob.Click

        Dim copyColorValues As Boolean = False
        Dim copyAllValues As Boolean = False

        If optCopyColors.Checked Then
            copyColorValues = True
        ElseIf optCopyAll.Checked Then
            copyAllValues = True
        ElseIf optCopyNonColors.Checked Then
        Else
            MsgBox("Was soll kopiert werden?", MsgBoxStyle.Question)
            Exit Sub
        End If

        If Not optCopySegeoMediumToLightAndDark.Checked Then
            If Not optCopyNotoMediumToLightAndDark.Checked Then
                If Not optCopySegeoMediumToNotoMedium.Checked Then
                    If Not optCopyAllSegeoToAllNoto.Checked Then
                        MsgBox("Von wo nach wo soll kopiert werden?", MsgBoxStyle.Question)
                        Exit Sub
                    End If
                End If
            End If
        End If

        If MsgBox("Sicher?)", MsgBoxStyle.Question Or MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
            Exit Sub
        End If

        CopyAktValuesToUndoValues()

        If optCopySegeoMediumToLightAndDark.Checked Then
            If copyAllValues Then
                _tileColors(L, S) = _tileColors(M, S).DeepCopy
                _tileColors(D, S) = _tileColors(M, S).DeepCopy
            Else
                _tileColors(L, S).ImportPartOfTileColors(_tileColors(M, S), copyColorValues)
                _tileColors(D, S).ImportPartOfTileColors(_tileColors(M, S), copyColorValues)
            End If

        ElseIf optCopyNotoMediumToLightAndDark.Checked Then
            If copyAllValues Then
                _tileColors(L, N) = _tileColors(M, N).DeepCopy
                _tileColors(D, N) = _tileColors(M, N).DeepCopy
            Else
                _tileColors(L, N) = _tileColors(M, N).DeepCopy
                _tileColors(D, N) = _tileColors(M, N).DeepCopy
            End If

        ElseIf optCopySegeoMediumToNotoMedium.Checked Then
            If copyAllValues Then
                _tileColors(M, N) = _tileColors(M, S).DeepCopy
            Else
                _tileColors(M, N).ImportPartOfTileColors(_tileColors(M, S), copyColorValues)
            End If

        ElseIf optCopyAllSegeoToAllNoto.Checked Then
            If copyAllValues Then
                _tileColors(L, N) = _tileColors(L, S).DeepCopy
                _tileColors(M, N) = _tileColors(M, S).DeepCopy
                _tileColors(D, N) = _tileColors(D, S).DeepCopy
            Else
                _tileColors(L, N).ImportPartOfTileColors(_tileColors(L, S), copyColorValues)
                _tileColors(M, N).ImportPartOfTileColors(_tileColors(M, S), copyColorValues)
                _tileColors(D, N).ImportPartOfTileColors(_tileColors(D, S), copyColorValues)
            End If
        End If

        DrawAllTiles(tileColorsInvalide:=True)

        MsgBox("Ausgeführt", MsgBoxStyle.Information)

        optCopyColors.Checked = False
        optCopyNonColors.Checked = False
        optCopyAllSegeoToAllNoto.Checked = False
        optCopyNotoMediumToLightAndDark.Checked = False
        optCopySegeoMediumToLightAndDark.Checked = False
        optCopySegeoMediumToNotoMedium.Checked = False

    End Sub

    Private Sub CopyAktValuesToUndoValues()
        _tileUndoColors(L, S) = _tileColors(L, S).DeepCopy
        _tileUndoColors(M, S) = _tileColors(M, S).DeepCopy
        _tileUndoColors(D, S) = _tileColors(D, S).DeepCopy

        _tileUndoColors(L, N) = _tileColors(L, N).DeepCopy
        _tileUndoColors(M, N) = _tileColors(M, N).DeepCopy
        _tileUndoColors(D, N) = _tileColors(D, N).DeepCopy

    End Sub
    Private Sub CopyUndoValuesToAktValues()
        _tileColors(L, S) = _tileUndoColors(L, S).DeepCopy
        _tileColors(M, S) = _tileUndoColors(M, S).DeepCopy
        _tileColors(D, S) = _tileUndoColors(D, S).DeepCopy

        _tileColors(L, N) = _tileUndoColors(L, N).DeepCopy
        _tileColors(M, N) = _tileUndoColors(M, N).DeepCopy
        _tileColors(D, N) = _tileUndoColors(D, N).DeepCopy

    End Sub

    Private Sub btnCopyUndo_Click(sender As Object, e As EventArgs) Handles btnCopyUndo.Click

        Dim swap(1, 2) As TileColors
        swap(L, S) = _tileColors(L, S)
        swap(M, S) = _tileColors(M, S)
        swap(D, S) = _tileColors(D, S)

        swap(L, N) = _tileColors(L, N)
        swap(M, N) = _tileColors(M, N)
        swap(D, N) = _tileColors(D, N)

        _tileColors(L, S) = _tileUndoColors(L, S)
        _tileColors(M, S) = _tileUndoColors(M, S)
        _tileColors(D, S) = _tileUndoColors(D, S)

        _tileColors(L, N) = _tileUndoColors(L, N)
        _tileColors(M, N) = _tileUndoColors(M, N)
        _tileColors(D, N) = _tileUndoColors(D, N)

        _tileUndoColors(L, S) = swap(L, S)
        _tileUndoColors(M, S) = swap(M, S)
        _tileUndoColors(D, S) = swap(D, S)

        _tileUndoColors(L, N) = swap(L, N)
        _tileUndoColors(M, N) = swap(M, N)
        _tileUndoColors(D, N) = swap(D, N)

    End Sub

    Private Sub chkDonotInsertSymbol_CheckedChanged(sender As Object, e As EventArgs) Handles chkDonotInsertSymbol.CheckedChanged
        DrawAllTiles(tileColorsInvalide:=True)
    End Sub

    Private Sub CheckBoxTile_TextUseSegoeUISymbol_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBoxTile_TextUseSegoeUISymbol.CheckedChanged
        TileFactoryINISettings.Tile_TextUseSegoeUISymbol = CheckBoxTile_TextUseSegoeUISymbol.Checked
        DrawAllTiles(True)
    End Sub

    'Private Sub numTileBasisWidth_ValueChanged(sender As Object, e As EventArgs) Handles numTileBasisWidth.ValueChanged, numTileBasisHeight.ValueChanged
    '    If _zuweisungAktiv <> 0 Then Exit Sub
    '    DrawAllTiles(True)
    'End Sub

#End Region

End Class
