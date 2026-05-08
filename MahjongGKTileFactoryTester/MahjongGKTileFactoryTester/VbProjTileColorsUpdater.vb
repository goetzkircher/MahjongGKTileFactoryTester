Option Compare Text
Option Explicit On
Option Infer Off
Option Strict On

Imports System.IO

Public Module VbProjTileColorsUpdater

    Private ReadOnly MsBuildNs As XNamespace =
        "http://schemas.microsoft.com/developer/msbuild/2003"

    Public Function EnsureTileColorsFileRegistered(vbprojPath As String,
                                                   tileColorsXmlFullPath As String) As Boolean

        If String.IsNullOrWhiteSpace(vbprojPath) Then
            Throw New ArgumentException("vbprojPath fehlt.", NameOf(vbprojPath))
        End If

        If String.IsNullOrWhiteSpace(tileColorsXmlFullPath) Then
            Throw New ArgumentException("tileColorsXmlFullPath fehlt.", NameOf(tileColorsXmlFullPath))
        End If

        If Not File.Exists(vbprojPath) Then
            Throw New FileNotFoundException("Die vbproj-Datei wurde nicht gefunden.", vbprojPath)
        End If

        If Not File.Exists(tileColorsXmlFullPath) Then
            Throw New FileNotFoundException("Die TileColors.xml-Datei wurde nicht gefunden.", tileColorsXmlFullPath)
        End If

        Dim projectDir As String = Path.GetDirectoryName(vbprojPath)

        If projectDir Is Nothing Then
            Throw New InvalidOperationException("Projektverzeichnis konnte nicht ermittelt werden.")
        End If

        Dim includePath As String = GetRelativePath(projectDir, tileColorsXmlFullPath)

        includePath = includePath.Replace("/"c, "\"c)

        Dim doc As XDocument = XDocument.Load(vbprojPath, LoadOptions.PreserveWhitespace)

        Dim alreadyExists As Boolean =
            doc.Descendants(MsBuildNs + "Content").
                Any(Function(e As XElement)
                        Dim attr As XAttribute = e.Attribute("Include")
                        Return attr IsNot Nothing AndAlso
                               String.Equals(attr.Value, includePath, StringComparison.OrdinalIgnoreCase)
                    End Function)

        If alreadyExists Then
            Return False
        End If

        Dim itemGroup As XElement = FindTileColorsItemGroup(doc)

        If itemGroup Is Nothing Then
            Dim projectElement As XElement = doc.Root

            If projectElement Is Nothing Then
                Throw New InvalidOperationException("Die Projektdatei hat kein Root-Element.")
            End If

            itemGroup = New XElement(MsBuildNs + "ItemGroup")
            projectElement.Add(Environment.NewLine & "  ", itemGroup, Environment.NewLine)
        End If

        Dim newContent As New XElement(MsBuildNs + "Content",
            New XAttribute("Include", includePath),
            New XElement(MsBuildNs + "CopyToOutputDirectory", "PreserveNewest"))

        ' Sauber eingerückt an das ItemGroup-Ende anhängen.
        itemGroup.Add(Environment.NewLine & "    ", newContent, Environment.NewLine & "  ")

        Dim backupPath As String = vbprojPath & "." & DateTime.Now.ToString("yyyyMMdd-HHmmss") & ".bak"
        File.Copy(vbprojPath, backupPath, overwrite:=False)

        doc.Save(vbprojPath)

        Return True

    End Function

    Public Sub EnsureAllTileColorsRegistered(vbprojPath As String,
                                             tileColorsRoot As String)

        If String.IsNullOrWhiteSpace(tileColorsRoot) Then
            Throw New ArgumentException("tileColorsRoot fehlt.", NameOf(tileColorsRoot))
        End If

        If Not Directory.Exists(tileColorsRoot) Then
            Throw New DirectoryNotFoundException(tileColorsRoot)
        End If

        For Each xmlPath As String In Directory.GetFiles(tileColorsRoot, "*.xml", SearchOption.AllDirectories)
            EnsureTileColorsFileRegistered(vbprojPath, xmlPath)
        Next

    End Sub

    Private Function FindTileColorsItemGroup(doc As XDocument) As XElement

        Return doc.Descendants(MsBuildNs + "ItemGroup").
            FirstOrDefault(Function(ig As XElement)
                               Return ig.Elements(MsBuildNs + "Content").
                                   Any(Function(c As XElement)
                                           Dim attr As XAttribute = c.Attribute("Include")
                                           Return attr IsNot Nothing AndAlso
                                                  attr.Value.StartsWith("Spielfeld\TileColors\",
                                                                       StringComparison.OrdinalIgnoreCase)
                                       End Function)
                           End Function)

    End Function

    Private Function GetRelativePath(baseDirectory As String,
                                     fullPath As String) As String

        Dim baseUri As New Uri(AppendDirectorySeparatorChar(Path.GetFullPath(baseDirectory)))
        Dim fullUri As New Uri(Path.GetFullPath(fullPath))

        Dim relativeUri As Uri = baseUri.MakeRelativeUri(fullUri)

        Return Uri.UnescapeDataString(relativeUri.ToString())

    End Function

    Private Function AppendDirectorySeparatorChar(pathValue As String) As String

        If pathValue.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) Then
            Return pathValue
        End If

        Return pathValue & Path.DirectorySeparatorChar

    End Function

    Public Function RemoveOrphanedTileColorsEntries(vbprojPath As String) As Integer

        Dim projectDir As String = Path.GetDirectoryName(vbprojPath)

        If projectDir Is Nothing Then
            Throw New InvalidOperationException("Projektverzeichnis konnte nicht ermittelt werden.")
        End If

        Dim doc As XDocument = XDocument.Load(vbprojPath, LoadOptions.PreserveWhitespace)

        Dim orphaned As List(Of XElement) =
            doc.Descendants(MsBuildNs + "Content").
                Where(Function(e As XElement)
                          Dim attr As XAttribute = e.Attribute("Include")

                          If attr Is Nothing Then
                              Return False
                          End If

                          If Not attr.Value.StartsWith("Spielfeld\TileColors\",
                                                       StringComparison.OrdinalIgnoreCase) Then
                              Return False
                          End If

                          If Not attr.Value.EndsWith(".xml",
                                                     StringComparison.OrdinalIgnoreCase) Then
                              Return False
                          End If

                          Dim fullPath As String = Path.Combine(projectDir, attr.Value)

                          Return Not File.Exists(fullPath)
                      End Function).
                ToList()

        If orphaned.Count = 0 Then
            Return 0
        End If

        Dim backupPath As String = vbprojPath & "." & DateTime.Now.ToString("yyyyMMdd-HHmmss") & ".bak"
        File.Copy(vbprojPath, backupPath, overwrite:=False)

        For Each element As XElement In orphaned
            element.Remove()
        Next

        doc.Save(vbprojPath)

        Return orphaned.Count

    End Function
End Module
