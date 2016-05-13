Imports System.ComponentModel
Imports SuperNote

Class MainWindow

    Dim RootSlot As NoteSlot

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        SaveNotes(Core.NoteDatabasePath)
    End Sub

    'Private Sub _slot_OnCanClose(noteslot As NoteSlot) Handles _slot.OnCanClose
    '    Debug.WriteLine(noteslot.ToString & " was empty so it was closed - ONCANCLOSE CALLED.")
    '    ' _slot.SetAsContainer(False)
    '    Debug.WriteLine(_slot.GetCountDescendants(True))
    'End Sub


    'Private Sub _slot_OnWidgetAdded(noteslot As NoteSlot, newwidget As NoteWidget, _dockdir As EDockDirection) Handles _slot.OnWidgetAdded
    '    Debug.WriteLine(newwidget.ToString & " - ADDED.")
    '    Debug.WriteLine(_slot.GetCountDescendants(True))
    'End Sub

    'Private Sub _slot_OnWidgetDocked(noteslot As NoteSlot, dockedwidget As NoteWidget, dockdir As EDockDirection) Handles _slot.OnWidgetDocked
    '    Debug.WriteLine(dockedwidget.ToString & " - DOCKED " & "to " & noteslot.ToString)
    '    Debug.WriteLine(_slot.GetCountDescendants(True))
    'End Sub

    'Private Sub _slot_OnWidgetRemovalCancelled(noteslot As NoteSlot, deniedwidget As NoteWidget) Handles _slot.OnWidgetRemovalCancelled
    '    MessageBox.Show("CANNOT Delete the last widget!")
    'End Sub

    'Private Sub _slot_OnWidgetRemoved(noteslot As NoteSlot, removedwidget As NoteWidget) Handles _slot.OnWidgetRemoved
    '    Debug.WriteLine(removedwidget.ToString & " - REMOVED.")
    '    Debug.WriteLine(_slot.GetCountDescendants(True))
    'End Sub

    'Private Sub _slot_OnWidgetSplitted(noteslot As NoteSlot, splittedwidget As NoteWidget, ByVal splitdir As EDockDirection) Handles _slot.OnWidgetSplitted
    '    Debug.WriteLine(splittedwidget.ToString & " was splitted <" & [Enum].GetName(GetType(EDockDirection), splitdir) & ">")
    '    Debug.WriteLine(_slot.GetCountDescendants(True))
    'End Sub

    Private Sub MainWindow_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        'Select Case e.Key
        '    Case Key.Q
        '        _wid2.Move()
        '        _wid2.RequestDock(_wid1, EDockDirection.D)
        '    Case Key.W
        '        _wid2.Move()
        '        _wid2.RequestDock(_wid1, EDockDirection.R)
        '    Case Key.E
        '        _wid3.SetFrameColor(Brushes.Orange)
        '        _wid3.SetTextFieldBackColor(Brushes.Orange)
        '        _wid3.SetTextFieldForeColor(Brushes.White)
        '        _wid3.ID = "ORANGE"
        '        _wid3.RequestDock(_wid2, EDockDirection.U)
        '    Case Key.R
        '        _wid2.RequestSplit(EDockDirection.D)
        'End Select

    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        Dim _slot = LoadNotes(Core.NoteDatabasePath)
        If _slot IsNot Nothing Then
            RootSlot = _slot
            maingrid.Children.Add(RootSlot)
            Return
        End If


        'default layout ( Simple SLot and Widget )
        RootSlot = New NoteSlot
        maingrid.Children.Add(RootSlot)
        RootSlot.HorizontalAlignment = HorizontalAlignment.Stretch
        RootSlot.VerticalAlignment = VerticalAlignment.Stretch
        ' RootSlot.SetFrameColor(Brushes.Green)
        ' RootSlot.SetTextFieldBackColor(Brushes.Green)
        ' RootSlot.SetTextFieldForeColor(Brushes.White)
        'RootSlot.ID = "ROOT"

        Dim defaultWidget = New NoteWidget
        If RootSlot.AddWidget(defaultWidget, EDockDirection.L) Then
            defaultWidget.SetFrameColor(Brushes.Teal)
            defaultWidget.SetTextFieldBackColor(Brushes.Teal)
            defaultWidget.SetTextFieldForeColor(Brushes.White)
        End If

    End Sub

    Public Function LoadNotes(ByVal path As String) As NoteSlot
        If Not IO.File.Exists(path) Then Return Nothing

        Return NoteSlot.DeserializeSlot(Nothing, path)
    End Function

    Public Function SaveNotes(ByVal path As String) As Boolean
        NoteSlot.SerializeSlot(RootSlot, path)
        Return True
    End Function

End Class
