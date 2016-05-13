Imports SuperNote

Public Class NoteSlot : Inherits NoteWidget

    Private widgets As New List(Of NoteWidget)
    Private splitter As New GridSplitter
    Protected SplitOrient As ESplitOrientation = ESplitOrientation.HORIZONTAL
    Public bPreventDeletionWhenisRootAndHasOneWidget As Boolean = False 'if the noteslot will prevent the deletion of the last widget in it.

    Public Function GetCountDescendants(Optional _countSlots As Boolean = False) As Integer
        Dim _count As Integer = 0
        For Each widg In widgets
            If TypeOf (widg) Is NoteSlot Then
                _count += CTypeDynamic(Of NoteSlot)(widg).GetCountDescendants
                _count += IIf(_countSlots, 1, 0)
            Else
                _count += 1
            End If
        Next
        Return _count
    End Function

    Public Function IsRoot() As Boolean
        Return ParentSlot Is Nothing
    End Function

    Private Sub resetlayout()
        nwmainframe.ColumnDefinitions.Clear()
        nwmainframe.RowDefinitions.Clear()
        nwmainframe.Children.Clear()
    End Sub


    Private Sub splitlayout(ByVal _so As ESplitOrientation)
        Select Case _so
            Case ESplitOrientation.HORIZONTAL
                nwmainframe.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
                nwmainframe.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
                nwmainframe.Children.Add(splitter)
                splitter.Margin = New Thickness(0, 0, -5, 0)
                splitter.HorizontalAlignment = HorizontalAlignment.Right
                splitter.VerticalAlignment = VerticalAlignment.Stretch
                splitter.Width = 10
                splitter.Height = Double.NaN
                Grid.SetColumn(splitter, 0)
            Case ESplitOrientation.VERTICAL
                nwmainframe.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Star)})
                nwmainframe.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Star)})
                nwmainframe.Children.Add(splitter)
                splitter.Margin = New Thickness(0, 0, 0, -5)
                splitter.VerticalAlignment = VerticalAlignment.Bottom
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch
                splitter.Height = 10
                splitter.Width = Double.NaN
                Grid.SetRow(splitter, 0)
            Case Else
                'impossibile raggiungere questo posto. Se mai dovesse accadere lo facciamo notare eliminando tutti gli elementi del grid.
                resetlayout()
        End Select
    End Sub

    Public Function Setup()
        resetlayout()

        Select Case widgets.Count
            Case 0
                GetRoot.NotifyCanClose(Me)
                'If IsRoot() = False Then
                ' ParentSlot.RemoveWidget(Me)
                'End If
            Case 1

                nwmainframe.Children.Add(widgets.First)
            Case 2
                'resetlayout() 
                For Each w In widgets
                    nwmainframe.Children.Add(w)
                Next
                splitlayout(SplitOrient)
                Select Case SplitOrient
                    Case ESplitOrientation.HORIZONTAL
                        Grid.SetColumn(widgets.First, 0)
                        Grid.SetColumn(widgets.Last, 1)
                    Case ESplitOrientation.VERTICAL
                        Grid.SetRow(widgets.First, 0)
                        Grid.SetRow(widgets.Last, 1)
                End Select

            Case Else

        End Select
        Return True
    End Function

    Public Sub New()
        MyBase.New
        '  SetAsContainer(True)
        splitter.Background = Brushes.Transparent
    End Sub


    ''' <summary>
    ''' Update coherently the widgets list.
    ''' </summary>
    ''' <param name="newwidget"></param>
    ''' <param name="dockdir"></param>
    ''' <returns></returns>
    Public Function AddWidget(ByVal newwidget As NoteWidget, ByVal dockdir As EDockDirection) As Boolean
        If widgets.Count >= Core.MAX_NUM_WIDGETS_PER_SLOT Then Return False
        Select Case dockdir
            Case EDockDirection.D
                widgets.Add(newwidget)
                SplitOrient = ESplitOrientation.VERTICAL
            Case EDockDirection.L
                widgets.Insert(0, newwidget)
                SplitOrient = ESplitOrientation.HORIZONTAL
            Case EDockDirection.R
                widgets.Add(newwidget)
                SplitOrient = ESplitOrientation.HORIZONTAL
            Case EDockDirection.U
                widgets.Insert(0, newwidget)
                SplitOrient = ESplitOrientation.VERTICAL
            Case Else

        End Select
        newwidget.ParentSlot = Me
        GetRoot().NotifyWidgetAdded(Me, newwidget, dockdir)
        Setup()
        Return True
    End Function

    ''' <summary>
    ''' handles the possibilty to add a widget as a sibling of another.
    ''' </summary>
    ''' <param name="target"></param>
    ''' <param name="requester"></param>
    ''' <param name="dockdir"></param>
    Public Sub Dock(target As NoteWidget, requester As NoteWidget, dockdir As EDockDirection)
        Dim requesterOldSlot = requester.ParentSlot
        If requesterOldSlot IsNot Nothing Then requesterOldSlot.RemoveWidget(requester)

        Dim TargetSlot As NoteSlot = target.ParentSlot
        Select Case TargetSlot.widgets.Count
            Case 0
                'non dovrebbe essere in grado di raggiungere questo punto.
            Case 1
                TargetSlot.AddWidget(requester, dockdir)
                requester.NotifyDockCompleted(requesterOldSlot, TargetSlot)
                GetRoot.NotifyWidgetDocked(TargetSlot, requester, dockdir)
            Case 2
                Dim indexoftarget As Integer = TargetSlot.widgets.IndexOf(target)
                TargetSlot.widgets.Remove(target)

                Dim newcont = New NoteSlot
                Select Case indexoftarget
                    Case 0
                        TargetSlot.widgets.Insert(0, newcont)
                    Case 1
                        TargetSlot.widgets.Add(newcont)
                End Select

                TargetSlot.Setup()

                'newcont.SplitOrient = SplitOrientation.VERTICAL
                newcont.ParentSlot = TargetSlot 'simulate AddWidget Function
                'newcont.AddWidget(target, dockdir)
                newcont.widgets.Add(target)
                target.ParentSlot = newcont
                newcont.AddWidget(requester, dockdir)
                newcont.Setup()


                'requester.NotifyMoveCompleted(requesterOldSlot, newcont)
                requester.NotifyDockCompleted(requesterOldSlot, newcont)
                GetRoot.NotifyWidgetDocked(newcont, requester, dockdir)
            Case Else

        End Select
    End Sub

    Public Sub Split(target As NoteWidget, splitdir As EDockDirection)
        Dim newnote As NoteWidget = New NoteWidget
        'newnote.ID = "splitted"
        Dock(target, newnote, splitdir)
        target.NotifySplit(splitdir)
        GetRoot.NotifyWidgetSplit(target, splitdir)
        target.CopyPropertiesToWidget(newnote, True)
    End Sub

    'Protected Sub onWidgetMoveRequestedCallback(widget As NoteWidget)
    '    Dim mrw = Core.GetMoveRequestingWidget
    '    If mrw IsNot Nothing Then
    '        mrw.CancelMove()
    '    End If
    '    Core.SetMoveRequestingWidget(widget)
    'End Sub

    'Private Sub onWidgetCanCloseCallback(widget As NoteWidget)
    '    RemoveWidget(widget)
    'End Sub

    Public Sub RemoveWidget(ByVal removandawidget As NoteWidget)
        If bPreventDeletionWhenisRootAndHasOneWidget Then
            If IsRoot() And widgets.Count <= 1 Then
                GetRoot.NotifyWidgetRemovalCancelled(Me, removandawidget)
                Return
            End If
        End If

        widgets.Remove(removandawidget)
        GetRoot().NotifyWidgetRemoved(Me, removandawidget)
        Setup()
        removandawidget.NotifyClosed()


        If widgets.Count < 1 Then
            GetRoot.NotifyCanClose(Me)
            Return
        End If
    End Sub

    Public Function GetRoot() As NoteSlot
        Dim _root As NoteSlot = Me
        While _root.ParentSlot IsNot Nothing
            _root = _root.ParentSlot
        End While
        Return _root
    End Function

    'todo INSERT method for responding to dock on a child window.
    Public Sub NotifyWidgetAdded(ByVal noteslot As NoteSlot, ByVal newwidget As NoteWidget, ByVal _dockdir As EDockDirection)
        RaiseEvent OnWidgetAdded(noteslot, newwidget, _dockdir)
    End Sub

    Public Sub NotifyWidgetRemoved(ByVal noteslot As NoteSlot, ByVal removedwidget As NoteWidget)
        RaiseEvent OnWidgetRemoved(noteslot, removedwidget)
    End Sub

    Public Sub NotifyCanClose(ByVal noteslot As NoteSlot)
        RaiseEvent OnCanClose(noteslot)
    End Sub

    Public Sub NotifyWidgetClosed(ByVal noteslot As NoteSlot, ByVal closedWidget As NoteWidget)
        RaiseEvent OnWidgetClosed(noteslot, closedWidget)
    End Sub

    Public Sub NotifyWidgetMoveRequested(ByVal noteslot As NoteSlot, movingwidget As NoteWidget)
        RaiseEvent OnWidgetMoveRequested(noteslot, movingwidget)
    End Sub

    Public Sub NotifyWidgetDocked(ByVal noteslot As NoteSlot, dockedwidget As NoteWidget, ByVal dockdir As EDockDirection)
        RaiseEvent OnWidgetDocked(noteslot, dockedwidget, dockdir)
    End Sub

    Public Sub MoveWidget(ByVal widget As NoteWidget)
        widget.NotifyMoveAllowed()
        GetRoot.NotifyWidgetMoveRequested(Me, widget)
    End Sub

    Public Sub NotifyWidgetSplit(ByVal widget As NoteWidget, ByVal splitdir As EDockDirection)
        RaiseEvent OnWidgetSplitted(widget.ParentSlot, widget, splitdir)
    End Sub

    Public Sub NotifyLockToggled(ByVal widget As NoteWidget, ByVal newvalue As Boolean)
        RaiseEvent onWidgetLockToggled(widget.ParentSlot, widget, newvalue)
    End Sub

    Public Sub NotifyWidgetRemovalCancelled(ByVal noteslot As NoteSlot, ByVal deniedwidget As NoteWidget)
        RaiseEvent OnWidgetRemovalCancelled(noteslot, deniedwidget)
    End Sub

    Public Sub NotifyWidgetColorChanged(ByVal widget As NoteWidget, ByRef oldcolor As Color, ByRef newcolor As Color)
        RaiseEvent OnWidgetColorChanged(widget, oldcolor, newcolor)
    End Sub

    Public Overrides Function ToString() As String
        Return "SLOT " & ID
    End Function

    Private Sub NoteSlot_OnWidgetDocked(noteslot As NoteSlot, dockedwidget As NoteWidget, dockdir As EDockDirection) Handles Me.OnWidgetDocked
        'siccome dockedwidget è stato doccato con successo possiamo azzerare currentMovedWidget
        Core.SetMoveRequestingWidget(Nothing)
    End Sub

    Private Sub NoteSlot_OnWidgetMoveRequested(noteslot As NoteSlot, movrequestingwidget As NoteWidget) Handles Me.OnWidgetMoveRequested
        Dim oldmovingwidget As NoteWidget = Core.GetMoveRequestingWidget
        If oldmovingwidget IsNot Nothing Then
            oldmovingwidget.NotifyMoveCancelled()
        End If
        Core.SetMoveRequestingWidget(movrequestingwidget)
    End Sub

    Protected Shared Function _SerializeWidgetLogic(ByVal widget As NoteWidget, ByVal writer As Xml.XmlWriter)
        If TypeOf (widget) Is NoteSlot Then
            Dim slot As NoteSlot = widget
            writer.WriteStartElement("widgetslot")
            'writer.WriteAttributeString("id", slot.ID)
            writer.WriteAttributeString("orientation", IIf(slot.SplitOrient = ESplitOrientation.HORIZONTAL, "H", "V"))
            writer.WriteAttributeString("count", slot.widgets.Count)
            For Each wid In slot.widgets
                _SerializeWidgetLogic(wid, writer)
            Next
            writer.WriteEndElement() '</widgetslot>
        Else
            Dim cc = New ColorConverter
            Dim forecolorstring As String = cc.ConvertToString(CType(widget.GetTextFieldForeColor, SolidColorBrush).Color)
            Dim backcolorstring = cc.ConvertToString(CType(widget.GetTextFieldBackColor, SolidColorBrush).Color)
            Dim framecolorstring = cc.ConvertToString(CType(widget.GetFrameColor, SolidColorBrush).Color)

            writer.WriteStartElement("widget")
            'writer.WriteAttributeString("id", widget.ID.ToString)
            writer.WriteAttributeString("forecolor", forecolorstring)
            writer.WriteAttributeString("backcolor", backcolorstring)
            writer.WriteAttributeString("framecolor", framecolorstring)
            'Dim contentpath As String = "SN" & widget.ID & ".db"
            ' writer.WriteAttributeString("contentfile", contentpath)
            writer.WriteAttributeString("content", widget.GetContentAsString)
            ' IO.File.WriteAllText(contentpath, widget.GetContentAsString)
            writer.WriteAttributeString("locked", IIf(widget.IsLocked, "true", "false"))
            writer.WriteEndElement() '</widget>
        End If

        Return True

    End Function

    Public Shared Function SerializeSlot(ByVal slot As NoteSlot, ByVal path As String)
        Dim result As Boolean = False
        Try
            Dim setts As New Xml.XmlWriterSettings
            setts.Indent = True
            setts.OmitXmlDeclaration = True
            setts.CheckCharacters = True
            Dim wr As Xml.XmlWriter = Xml.XmlWriter.Create(path, setts)
            wr.WriteStartElement("supernote")
            wr.WriteAttributeString("version", 1.ToString)
            wr.WriteAttributeString("author", "Ken T Ekeoha")
            wr.WriteAttributeString("copyright", "SuperNote by Ken T Ekeoha. All Rights Reserved.")
            _SerializeWidgetLogic(slot, wr)
            wr.WriteEndElement() '</supernote>
            wr.Flush()
            wr.Close()
            result = True
        Catch
            result = False
        End Try

        Return result
    End Function

    Protected Shared Function _DeserializeLogic(ByVal xmldoc As Xml.XmlNode, ByVal xmlreader As Xml.XmlReader)
        'Dim headernode = xmldoc.ReadNode(xmlreader)
        Dim firstslotnode = xmldoc
        Dim widgetcount As Integer = firstslotnode.Attributes.GetNamedItem("count").Value
        Dim slotorientation As ESplitOrientation = IIf(firstslotnode.Attributes.GetNamedItem("orientation").Value = "H", ESplitOrientation.HORIZONTAL, ESplitOrientation.VERTICAL)
        ' Dim slotid As Integer = Integer.Parse(firstslotnode.Attributes.GetNamedItem("id").Value)

        Dim parentslot As New NoteSlot
        parentslot.SplitOrient = slotorientation
        'slot.ID = slotid
        'PROBLEMA DEL PARENTE.
        For Each child In firstslotnode.ChildNodes
            ' For i As Integer = 0 To widgetcount - 1
            Dim widgetnode = child
            If widgetnode.LocalName.ToLower = "widgetslot" Then


                Dim _slot As NoteSlot = _DeserializeLogic(child, xmlreader)
                If _slot IsNot Nothing Then
                    parentslot.widgets.Add(_slot)
                    _slot.ParentSlot = parentslot
                End If
            Else

                    Dim wid = New NoteWidget

                ' Dim cc = New ColorConverter
                ' Dim widid As Integer = Integer.Parse(widgetnode.Attributes.GetNamedItem("id").Value)
                Dim forecolor As Color = ColorConverter.ConvertFromString(widgetnode.Attributes.GetNamedItem("forecolor").Value)
                Dim backcolor As Color = ColorConverter.ConvertFromString(widgetnode.Attributes.GetNamedItem("backcolor").Value)
                Dim framecolor As Color = ColorConverter.ConvertFromString(widgetnode.Attributes.GetNamedItem("framecolor").Value)
                Dim contentstring As String = widgetnode.Attributes.GetNamedItem("content").Value
                'Dim contentstring As String = IO.File.ReadAllText(contentpath)
                Dim widgetislocked As Boolean = Boolean.Parse(widgetnode.Attributes.GetNamedItem("locked").Value)


                wid.SetFrameColor(New SolidColorBrush(framecolor))
                wid.SetTextFieldForeColor(New SolidColorBrush(forecolor))
                wid.SetTextFieldBackColor(New SolidColorBrush(backcolor))
                'wid.ID = widid
                wid.GetContentAsRichTextBox.SelectAll()
                Dim ms = New IO.MemoryStream
                Dim sw = New IO.StreamWriter(ms)
                sw.Write(contentstring)
                sw.Flush()
                ms.Seek(0, IO.SeekOrigin.Begin)
                wid.GetContentAsRichTextBox.Selection.Load(ms, DataFormats.Rtf)
                ms.Close()
                parentslot.widgets.Add(wid)
                wid.ParentSlot = parentslot


                wid.ToggleLockText(widgetislocked)
            End If

            ' Next
        Next
        parentslot.Setup()
        If widgetcount = 0 Then parentslot = Nothing
        Return parentslot
    End Function
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="targetSlot">the root to which attach all the deserialized widgets. Should be the NULL if the We intended to return a Root Objects.</param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Shared Function DeserializeSlot(ByVal targetSlot As NoteSlot, ByVal path As String) As NoteSlot
        Try
            Dim xmldoc = New Xml.XmlDocument
            Dim xmlreadersetts = New Xml.XmlReaderSettings
            xmlreadersetts.IgnoreComments = True
            xmlreadersetts.IgnoreWhitespace = True

            Dim xmlreader As Xml.XmlReader = Xml.XmlReader.Create(path, xmlreadersetts)
            xmldoc.Load(xmlreader)
            Dim nextnode As Xml.XmlNode = xmldoc.FirstChild.FirstChild
            Dim _slot = _DeserializeLogic(nextnode, xmlreader)
            xmlreader.Close()
            Return _slot
        Catch
            Return Nothing
        End Try
    End Function

    Private Sub NoteSlot_OnCanClose(noteslot As NoteSlot) Handles Me.OnCanClose
        If noteslot.ParentSlot IsNot Nothing Then
            noteslot.ParentSlot.RemoveWidget(noteslot)
        End If
    End Sub


    'Public Event OnCloseRequested()
    Public Shadows Event OnCanClose(ByVal noteslot As NoteSlot)
    Public Event OnWidgetClosed(ByVal noteslot As NoteSlot, ByVal closedwidget As NoteWidget)
    Public Event OnWidgetAdded(ByVal noteslot As NoteSlot, ByVal newwidget As NoteWidget, ByVal _dockdir As EDockDirection)
    Public Event OnWidgetRemoved(ByVal noteslot As NoteSlot, ByVal removedwidget As NoteWidget)
    Public Event OnError(ByVal noteslot As NoteSlot, ByVal payload As OnErrorArgs)
    Public Event OnWidgetMoveRequested(ByVal noteslot As NoteSlot, ByVal movrequestingwidget As NoteWidget)
    Public Event OnWidgetMoveCancelled(ByVal noteslot As NoteSlot, ByVal movecancelledwidget As NoteWidget)
    Public Event onWidgetLockToggled(ByVal noteslot As NoteSlot, ByVal locktoggledwidget As NoteWidget, ByVal newvalue As Boolean)
    Public Event onWidgetDockRequested()
    Public Event OnWidgetDocked(ByVal noteslot As NoteSlot, ByVal dockedwidget As NoteWidget, ByVal dockdir As EDockDirection)
    Public Event OnWidgetSplitted(ByVal noteslot As NoteSlot, ByVal splittedwidget As NoteWidget, splitdir As EDockDirection)
    Public Event OnWidgetRemovalCancelled(ByVal noteslot As NoteSlot, ByVal deniedwidget As NoteWidget)
    Public Event OnWidgetColorChanged(ByVal widget As NoteWidget, ByRef oldcolor As Color, ByRef newcolor As Color)


End Class