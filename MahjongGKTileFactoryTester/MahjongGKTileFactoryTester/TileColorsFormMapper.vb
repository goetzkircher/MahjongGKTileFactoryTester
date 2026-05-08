Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On

Imports System.Reflection
Imports TileFactory

''' <summary>
''' Kopiert Werte zwischen TileColors und WinForms-Controls.
''' 
''' Namenskonvention:
''' Enum      -> cbo+PropertyName>
''' Color     -> txtcol+PropertyName>
''' String    -> colbox+PropertyName>
''' Integer   -> tb+PropertyName> bei Hue*/Sat*, sonst num+PropertyName>
''' Single    -> num+PropertyName>
''' Double    -> num+PropertyName>
''' Decimal   -> num+PropertyName>
''' 
''' XML-Hilfsproperties mit Suffix "Xml" werden ignoriert.
''' </summary>
Public Module TileColorsFormMapper

#Region "Public API"

    Public Sub CopyFormToTileColors(target As TileColors,
                                           rootControls As IEnumerable(Of Control))

        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If rootControls Is Nothing Then Throw New ArgumentNullException(NameOf(rootControls))

        Dim controlMap As Dictionary(Of String, Control) = BuildControlMap(rootControls)
        Dim props As PropertyInfo() = GetMappableProperties()

        For Each prop As PropertyInfo In props
            Dim controlName As String = GetControlName(prop)
            If controlName.Length = 0 Then
                Continue For
            End If

            Dim ctrl As Control = Nothing
            If Not controlMap.TryGetValue(controlName, ctrl) Then
                Continue For
            End If
            'If controlName.StartsWith("cbo") Then Stop
            SetPropertyFromControl(target, prop, ctrl)
        Next

    End Sub

    Public Sub CopyTileColorsToForm(source As TileColors, rootControls As IEnumerable(Of Control))

        If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
        If rootControls Is Nothing Then Throw New ArgumentNullException(NameOf(rootControls))

        Dim controlMap As Dictionary(Of String, Control) = BuildControlMap(rootControls)
        Dim props As PropertyInfo() = GetMappableProperties()

        For Each prop As PropertyInfo In props
            Dim controlName As String = GetControlName(prop)
            If controlName.Length = 0 Then
                Continue For
            End If

            Dim ctrl As Control = Nothing
            If Not controlMap.TryGetValue(controlName, ctrl) Then
                Continue For
            End If

            SetControlFromProperty(source, prop, ctrl)
        Next

    End Sub

    Public Sub CopyFormToTileColors(target As TileColors,
                                           ParamArray rootControls() As Control)

        CopyFormToTileColors(target, DirectCast(rootControls, IEnumerable(Of Control)))

    End Sub

    Public Sub CopyTileColorsToForm(source As TileColors,
                                           ParamArray rootControls() As Control)

        CopyTileColorsToForm(source, DirectCast(rootControls, IEnumerable(Of Control)))

    End Sub

#End Region

#Region "Properties"

    Private Function GetMappableProperties() As PropertyInfo()

        Dim props As PropertyInfo() =
            GetType(TileColors).GetProperties(BindingFlags.Instance Or BindingFlags.Public)

        Dim result As New List(Of PropertyInfo)()

        For Each prop As PropertyInfo In props
            If Not prop.CanRead Then
                Continue For
            End If
            If Not prop.CanWrite Then
                Continue For
            End If
            If prop.GetIndexParameters().Length <> 0 Then
                Continue For
            End If
            If IsXmlHelperProperty(prop) Then
                Continue For
            End If

            result.Add(prop)
        Next

        Return result.ToArray()

    End Function

    Private Function IsXmlHelperProperty(prop As PropertyInfo) As Boolean
        Return prop.Name.EndsWith("Xml", StringComparison.Ordinal)
    End Function

#End Region

#Region "Control-Mapping"

    Private Function BuildControlMap(rootControls As IEnumerable(Of Control)) As Dictionary(Of String, Control)

        Dim result As New Dictionary(Of String, Control)(StringComparer.OrdinalIgnoreCase)

        For Each rootControl As Control In rootControls
            AddControlRecursive(result, rootControl)
        Next

        Return result

    End Function

    Private Sub AddControlRecursive(controlMap As Dictionary(Of String, Control),
                                           ctrl As Control)

        If ctrl Is Nothing Then Return

        If Not String.IsNullOrWhiteSpace(ctrl.Name) Then
            If Not controlMap.ContainsKey(ctrl.Name) Then
                controlMap.Add(ctrl.Name, ctrl)
            End If
        End If

        For Each child As Control In ctrl.Controls
            AddControlRecursive(controlMap, child)
        Next

    End Sub

    Private Function GetControlName(prop As PropertyInfo) As String

        Dim logicalName As String = prop.Name

        If prop.PropertyType Is GetType(Color) Then
            Return "txtcol" & logicalName
        End If

        If prop.PropertyType Is GetType(String) Then
            If prop.Name.StartsWith("TextSymbole") Then
                Return "txt" & logicalName
            Else
                Return "colbox" & logicalName
            End If
        End If

        If prop.PropertyType.IsEnum Then
            Return "cbo" & logicalName
        End If

        If prop.PropertyType Is GetType(Integer) Then
            If logicalName.StartsWith("Hue", StringComparison.OrdinalIgnoreCase) Then
                Return "tb" & logicalName
            Else
                Return "num" & logicalName
            End If
        End If

        If prop.PropertyType Is GetType(Single) OrElse
           prop.PropertyType Is GetType(Double) OrElse
           prop.PropertyType Is GetType(Decimal) Then
            Return "num" & logicalName
        End If

        If prop.PropertyType Is GetType(Boolean) Then
            If logicalName.StartsWith("DebugShow", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("DonotInsertSymbol", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("FaceUseInnerArea", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("CloudGrainKoppeln", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("SymbolGradientToKoppeln", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("GhostUseFastMethode", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("StatusLoadingOK", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("InsertTextDrachen", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("InsertTextWinde", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("InsertTextBlumen", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("InsertTextJZeiten", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("TextFontStyleBold", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName

            ElseIf logicalName.StartsWith("DBrgSummenÄnderungMultiplikation", StringComparison.OrdinalIgnoreCase) Then
                Return "chk" & logicalName
            ElseIf logicalName.StartsWith("Debug", StringComparison.OrdinalIgnoreCase) Then
                Return "opt" & logicalName
            Else
                Return "opt" & logicalName 'kann geändert werden 
            End If
        End If

        Return String.Empty

    End Function

#End Region

#Region "Form -> TileColors"

    Private Sub SetPropertyFromControl(target As TileColors,
                                              prop As PropertyInfo,
                                              ctrl As Control)

        If prop.PropertyType Is GetType(Color) Then
            Dim colorValue As Color = ReadColorFromControl(ctrl)
            prop.SetValue(target, colorValue, Nothing)
            Return
        End If

        If prop.PropertyType Is GetType(String) Then
            Dim stringValue As String = ReadStringFromControl(ctrl)
            prop.SetValue(target, stringValue, Nothing)
            Return
        End If

        If prop.PropertyType.IsEnum Then
            Dim cbo As ComboBox = TryCast(ctrl, ComboBox)

            If cbo Is Nothing Then
                Throw New InvalidOperationException("Für " & prop.Name & " wird eine ComboBox erwartet.")
            End If

            Dim enumValue As Object = ReadEnumFromComboBox(cbo, prop.PropertyType)
            prop.SetValue(target, enumValue, Nothing)
            Return
        End If

        If prop.PropertyType Is GetType(Integer) Then
            Dim intValue As Integer = ReadIntegerFromControl(ctrl)
            prop.SetValue(target, intValue, Nothing)
            Return
        End If

        If prop.PropertyType Is GetType(Single) Then
            Dim singleValue As Single = ReadSingleFromControl(ctrl)
            prop.SetValue(target, singleValue, Nothing)
            Return
        End If

        If prop.PropertyType Is GetType(Double) Then
            Dim doubleValue As Double = ReadDoubleFromControl(ctrl)
            prop.SetValue(target, doubleValue, Nothing)
            Return
        End If

        If prop.PropertyType Is GetType(Boolean) Then
            Dim bol As Boolean = ReadBooleanFromControl(ctrl)
            prop.SetValue(target, bol, Nothing)
            Return
        End If

        If prop.PropertyType Is GetType(Decimal) Then
            Dim dec As Decimal = ReadDecimalFromControl(ctrl)
            prop.SetValue(target, dec, Nothing)
            Return
        End If
    End Sub

    Private Function ReadStringFromControl(ctrl As Control) As String

        If ctrl.Name = "txtTextSymbole" Then
            Dim txt As TextBox = TryCast(ctrl, TextBox)
            Return txt.Text
        End If

        Dim colbox As ColorBox = TryCast(ctrl, ColorBox)

        If colbox Is Nothing Then
            Throw New InvalidOperationException("Für String wird eine ColorBox erwartet: " & ctrl.Name)
        End If

        Return colbox.ColorText

    End Function

    Private Function ReadColorFromControl(ctrl As Control) As Color

        Dim txt As TextBox = TryCast(ctrl, TextBox)

        If txt Is Nothing Then
            Throw New InvalidOperationException("Für Color wird eine TextBox erwartet: " & ctrl.Name)
        End If

        Return ParseArgbHex(txt.Text)

    End Function

    Private Function ReadIntegerFromControl(ctrl As Control) As Integer

        If TypeOf ctrl Is TrackBar Then
            Return DirectCast(ctrl, TrackBar).Value
        End If

        If TypeOf ctrl Is NumericUpDown Then
            Return Decimal.ToInt32(DirectCast(ctrl, NumericUpDown).Value)
        End If

        Throw New InvalidOperationException("Control-Typ für Integer nicht unterstützt: " & ctrl.GetType().Name)

    End Function

    Private Function ReadSingleFromControl(ctrl As Control) As Single

        If TypeOf ctrl Is NumericUpDown Then
            Return Decimal.ToSingle(DirectCast(ctrl, NumericUpDown).Value)
        End If

        If TypeOf ctrl Is TrackBar Then
            Return CSng(DirectCast(ctrl, TrackBar).Value)
        End If

        Throw New InvalidOperationException("Control-Typ für Single nicht unterstützt: " & ctrl.GetType().Name)

    End Function

    Private Function ReadDoubleFromControl(ctrl As Control) As Double

        If TypeOf ctrl Is NumericUpDown Then
            Return Decimal.ToDouble(DirectCast(ctrl, NumericUpDown).Value)
        End If

        If TypeOf ctrl Is TrackBar Then
            Return CDbl(DirectCast(ctrl, TrackBar).Value)
        End If

        Throw New InvalidOperationException("Control-Typ für Double nicht unterstützt: " & ctrl.GetType().Name)

    End Function

    Private Function ReadDecimalFromControl(ctrl As Control) As Decimal

        If TypeOf ctrl Is NumericUpDown Then
            'Ist das hier dann korrekt?
            Return DirectCast(ctrl, NumericUpDown).Value
        End If

        If TypeOf ctrl Is TrackBar Then
            Return CDec(DirectCast(ctrl, TrackBar).Value)
        End If

        Throw New InvalidOperationException("Control-Typ für Decimal nicht unterstützt: " & ctrl.GetType().Name)

    End Function
    Private Function ReadBooleanFromControl(ctrl As Control) As Boolean

        If TypeOf ctrl Is CheckBox Then
            Return DirectCast(ctrl, CheckBox).Checked
        End If

        If TypeOf ctrl Is RadioButton Then
            Return DirectCast(ctrl, RadioButton).Checked
        End If

        Throw New InvalidOperationException("Control-Typ für Boolean nicht unterstützt: " & ctrl.GetType().Name)

    End Function

    Private Function ReadEnumFromComboBox(cbo As ComboBox,
                                                 enumType As Type) As Object

        If cbo.SelectedItem IsNot Nothing Then
            Dim selectedItem As Object = cbo.SelectedItem

            If selectedItem.GetType() Is enumType Then
                Return selectedItem
            End If

            If TypeOf selectedItem Is String Then
                Return [Enum].Parse(enumType, CStr(selectedItem), True)
            End If

            If TypeOf selectedItem Is Integer OrElse
               TypeOf selectedItem Is Short OrElse
               TypeOf selectedItem Is Byte OrElse
               TypeOf selectedItem Is Long Then
                Return [Enum].ToObject(enumType, selectedItem)
            End If
        End If

        If cbo.SelectedValue IsNot Nothing Then
            Dim selectedValue As Object = cbo.SelectedValue

            If selectedValue.GetType() Is enumType Then
                Return selectedValue
            End If

            If TypeOf selectedValue Is String Then
                Return [Enum].Parse(enumType, CStr(selectedValue), True)
            End If

            If TypeOf selectedValue Is Integer OrElse
               TypeOf selectedValue Is Short OrElse
               TypeOf selectedValue Is Byte OrElse
               TypeOf selectedValue Is Long Then
                Return [Enum].ToObject(enumType, selectedValue)
            End If
        End If

        If Not String.IsNullOrWhiteSpace(cbo.Text) Then
            Return [Enum].Parse(enumType, cbo.Text.Trim(), True)
        End If

        Throw New InvalidOperationException("ComboBox " & cbo.Name & " enthält keinen auswertbaren Enum-Wert.")

    End Function

#End Region

#Region "TileColors -> Form"

    Private Sub SetControlFromProperty(source As TileColors,
                                              prop As PropertyInfo,
                                              ctrl As Control)

        Dim rawValue As Object = prop.GetValue(source, Nothing)

        If prop.PropertyType Is GetType(Color) Then
            SetColorControlValue(ctrl, DirectCast(rawValue, Color))
            Return
        End If

        If prop.PropertyType Is GetType(String) Then
            SetStringControlValue(ctrl, CStr(rawValue))
            Return
        End If

        If prop.PropertyType.IsEnum Then
            Dim cbo As ComboBox = TryCast(ctrl, ComboBox)

            If cbo Is Nothing Then
                Throw New InvalidOperationException("Für " & prop.Name & " wird eine ComboBox erwartet.")
            End If

            SetComboBoxToEnumValue(cbo, rawValue)
            Return
        End If

        If prop.PropertyType Is GetType(Integer) Then
            SetNumericControlValue(ctrl, CDec(CInt(rawValue)))
            Return
        End If

        If prop.PropertyType Is GetType(Single) Then
            SetNumericControlValue(ctrl, CDec(CSng(rawValue)))
            Return
        End If

        If prop.PropertyType Is GetType(Double) Then
            SetNumericControlValue(ctrl, CDec(CDbl(rawValue)))
            Return
        End If

        If prop.PropertyType Is GetType(Decimal) Then
            SetNumericControlValue(ctrl, CDec(rawValue))
            Return
        End If

        If prop.PropertyType Is GetType(Boolean) Then
            SetBooleanControlValue(ctrl, CBool(rawValue))
            Return
        End If
    End Sub

    Private Sub SetStringControlValue(ctrl As Control, value As String)

        If ctrl.Name = "txtTextSymbole" Then
            Dim txt As TextBox = TryCast(ctrl, TextBox)
            txt.Text = value
            Exit Sub
        End If

        Dim colbox As ColorBox = TryCast(ctrl, ColorBox)
        If colbox Is Nothing Then
            Throw New InvalidOperationException("Für colbox-String wird eine ColorBox erwartet: " & ctrl.Name)
        End If
        If value Is Nothing Then
            colbox.ColorText = String.Empty
        Else
            colbox.ColorText = value
        End If

    End Sub

    Private Sub SetColorControlValue(ctrl As Control, value As Color)

        Dim txt As TextBox = TryCast(ctrl, TextBox)

        If txt Is Nothing Then
            Throw New InvalidOperationException("Für Color wird eine TextBox erwartet: " & ctrl.Name)
        End If

        txt.Text = ColorToArgbHex(value)

    End Sub

    Private Sub SetNumericControlValue(ctrl As Control, value As Decimal)

        If TypeOf ctrl Is TrackBar Then
            Dim tb As TrackBar = DirectCast(ctrl, TrackBar)
            Dim intValue As Integer = Decimal.ToInt32(value)

            If intValue < tb.Minimum Then intValue = tb.Minimum
            If intValue > tb.Maximum Then intValue = tb.Maximum

            tb.Value = intValue
            Return
        End If

        If TypeOf ctrl Is NumericUpDown Then
            Dim num As NumericUpDown = DirectCast(ctrl, NumericUpDown)

            If value < num.Minimum Then value = num.Minimum
            If value > num.Maximum Then value = num.Maximum

            num.Value = value
            Return
        End If

        Throw New InvalidOperationException("Control-Typ numerisch nicht unterstützt: " & ctrl.GetType().Name)

    End Sub
    Private Sub SetBooleanControlValue(ctrl As Control, value As Boolean)

        If TypeOf ctrl Is RadioButton Then
            Dim rb As RadioButton = DirectCast(ctrl, RadioButton)
            rb.Checked = value
            Return
        End If
        If TypeOf ctrl Is CheckBox Then
            Dim chk As CheckBox = DirectCast(ctrl, CheckBox)
            chk.Checked = value
            Return
        End If

        Throw New InvalidOperationException("Control-Typ Boolean nicht unterstützt: " & ctrl.GetType().Name)

    End Sub

    Private Sub SetComboBoxToEnumValue(cbo As ComboBox,
                                              value As Object)

        If value Is Nothing Then
            cbo.SelectedIndex = -1
            cbo.Text = String.Empty
            Return
        End If

        Dim enumValue As [Enum] = DirectCast(value, [Enum])

        For i As Integer = 0 To cbo.Items.Count - 1
            Dim item As Object = cbo.Items(i)
            If item Is Nothing Then Continue For

            If item.GetType() Is value.GetType() Then
                Dim itemEnum As [Enum] = DirectCast(item, [Enum])
                If itemEnum.Equals(enumValue) Then
                    cbo.SelectedIndex = i
                    Return
                End If
            End If

            If String.Equals(item.ToString(), enumValue.ToString(), StringComparison.OrdinalIgnoreCase) Then
                cbo.SelectedIndex = i
                Return
            End If
        Next

        cbo.Text = enumValue.ToString()

    End Sub

#End Region

End Module