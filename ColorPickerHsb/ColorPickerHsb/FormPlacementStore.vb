Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On
Imports System.IO
Imports System.Reflection

Public NotInheritable Class FormPlacementStore

    Private Sub New()
    End Sub

    Public Shared Sub Restore(form As Form)
        If form Is Nothing Then Throw New ArgumentNullException(NameOf(form))

        Dim doc As XDocument = LoadDocument()
        Dim formKey As String = GetFormKey(form)

        Dim elemForm As XElement =
            doc.Root.Elements("Form").FirstOrDefault(
                Function(elem As XElement) String.Equals(CStr(elem.Attribute("Key")), formKey, StringComparison.Ordinal))

        If elemForm Is Nothing Then Exit Sub

        Dim posX As Integer
        Dim posY As Integer
        Dim formWidth As Integer
        Dim formHeight As Integer
        Dim stateValue As Integer

        If Not Integer.TryParse(CStr(elemForm.Attribute("X")), posX) Then Exit Sub
        If Not Integer.TryParse(CStr(elemForm.Attribute("Y")), posY) Then Exit Sub
        If Not Integer.TryParse(CStr(elemForm.Attribute("Width")), formWidth) Then Exit Sub
        If Not Integer.TryParse(CStr(elemForm.Attribute("Height")), formHeight) Then Exit Sub
        If Not Integer.TryParse(CStr(elemForm.Attribute("WindowState")), stateValue) Then Exit Sub

        If formWidth <= 0 OrElse formHeight <= 0 Then Exit Sub

        Dim savedBounds As New Rectangle(posX, posY, formWidth, formHeight)

        If Not IsVisibleOnAnyScreen(savedBounds) Then Exit Sub

        form.StartPosition = FormStartPosition.Manual
        form.Bounds = savedBounds

        If [Enum].IsDefined(GetType(FormWindowState), stateValue) Then
            Dim savedState As FormWindowState = CType(stateValue, FormWindowState)

            If savedState = FormWindowState.Maximized Then
                form.WindowState = FormWindowState.Maximized
            Else
                form.WindowState = FormWindowState.Normal
            End If
        End If
    End Sub

    Public Shared Sub Save(form As Form)
        If form Is Nothing Then Throw New ArgumentNullException(NameOf(form))

        Dim boundsToSave As Rectangle
        Dim stateToSave As FormWindowState

        Select Case form.WindowState
            Case FormWindowState.Normal
                boundsToSave = form.Bounds
                stateToSave = FormWindowState.Normal

            Case FormWindowState.Maximized
                boundsToSave = form.RestoreBounds
                stateToSave = FormWindowState.Maximized

            Case FormWindowState.Minimized
                boundsToSave = form.RestoreBounds
                stateToSave = FormWindowState.Normal

            Case Else
                boundsToSave = form.Bounds
                stateToSave = FormWindowState.Normal
        End Select

        If boundsToSave.Width <= 0 OrElse boundsToSave.Height <= 0 Then Exit Sub

        Dim doc As XDocument = LoadDocument()
        Dim formKey As String = GetFormKey(form)

        Dim elemForm As XElement =
        doc.Root.Elements("Form").FirstOrDefault(
            Function(elem As XElement) String.Equals(CStr(elem.Attribute("Key")), formKey, StringComparison.Ordinal))

        If elemForm IsNot Nothing Then
            elemForm.Remove()
        End If

        doc.Root.Add(
        New XElement("Form",
            New XAttribute("Key", formKey),
            New XAttribute("X", boundsToSave.X),
            New XAttribute("Y", boundsToSave.Y),
            New XAttribute("Width", boundsToSave.Width),
            New XAttribute("Height", boundsToSave.Height),
            New XAttribute("WindowState", CInt(stateToSave))))

        SaveDocument(doc)
    End Sub

    Private Shared Function GetFormKey(form As Form) As String
        Dim asmName As String = form.GetType().Assembly.GetName().Name
        Dim typeName As String = form.GetType().FullName

        Return asmName & "|" & typeName
    End Function

    Private Shared Function LoadDocument() As XDocument
        Dim filePath As String = GetStorageFilePath()

        If Not File.Exists(filePath) Then
            Return New XDocument(New XElement("Forms"))
        End If

        Try
            Return XDocument.Load(filePath)
        Catch
            Return New XDocument(New XElement("Forms"))
        End Try
    End Function

    Private Shared Sub SaveDocument(doc As XDocument)
        Dim filePath As String = GetStorageFilePath()
        Dim folderPath As String = Path.GetDirectoryName(filePath)

        If String.IsNullOrWhiteSpace(folderPath) Then
            Throw New InvalidOperationException("Der Speicherpfad für die Formularpositionen ist ungültig.")
        End If

        Directory.CreateDirectory(folderPath)
        doc.Save(filePath)
    End Sub

    Private Shared Function GetStorageFilePath() As String
        Dim rootPath As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)

        Dim companyName As String = Application.CompanyName
        If String.IsNullOrWhiteSpace(companyName) Then
            companyName = GetEntryAssemblyName()
        End If

        Dim productName As String = Application.ProductName
        If String.IsNullOrWhiteSpace(productName) Then
            productName = GetEntryAssemblyName()
        End If

        Dim folderPath As String = Path.Combine(rootPath, companyName, productName)
        Return Path.Combine(folderPath, "FormPlacement.xml")
    End Function

    Private Shared Function GetEntryAssemblyName() As String
        Dim entryAsm As Assembly = Assembly.GetEntryAssembly()

        If entryAsm IsNot Nothing Then
            Return entryAsm.GetName().Name
        End If

        Return "Application"
    End Function

    Private Shared Function IsVisibleOnAnyScreen(rect As Rectangle) As Boolean
        For Each scr As Screen In Screen.AllScreens
            If scr.WorkingArea.IntersectsWith(rect) Then
                Return True
            End If
        Next

        Return False
    End Function

End Class
