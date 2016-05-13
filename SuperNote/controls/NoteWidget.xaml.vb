Imports SuperNote

Public Class NoteWidget

    Public ParentSlot As NoteSlot
    Public IsMoving As Boolean = False
    Public IsLocked As Boolean = False
    Public ID As Integer = 0

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ID = Core.GetNextAvailableName()
    End Sub

    ''' <summary>
    ''' Copies Foreground Color, Background Color, Frame Color and Lock State to another widget.
    ''' </summary>
    ''' <param name="copyContent">Optionally it is possible to copy the text of the widget to the other widget.</param>
    ''' <returns></returns>
    Public Function CopyPropertiesToWidget(ByVal otherwidget As NoteWidget, Optional ByVal copyContent As Boolean = False)
        otherwidget.SetTextFieldBackColor(Me.GetTextFieldBackColor)
        otherwidget.SetTextFieldForeColor(Me.GetTextFieldForeColor)
        otherwidget.SetFrameColor(Me.GetFrameColor)
        otherwidget.ToggleLockText(Me.IsLocked)
        If copyContent Then


            Me.GetContentAsRichTextBox.SelectAll()
            Dim ms As New IO.MemoryStream()
            Me.GetContentAsRichTextBox.Selection.Save(ms, DataFormats.Rtf)
            otherwidget.GetContentAsRichTextBox.SelectAll()
            otherwidget.GetContentAsRichTextBox.Selection.Load(ms, DataFormats.Rtf)

        End If
        Return True
    End Function



    Public Function SetTextFieldBackColor(ByVal newbrush As Brush)
        nwcontent.Background = newbrush
        Return True
    End Function

    Public Function GetTextFieldBackColor() As Brush
        Return nwcontent.Background
    End Function

    Public Function SetTextFieldForeColor(ByVal newbrush As Brush)
        nwcontent.Foreground = newbrush
        Return True
    End Function

    Public Function GetTextFieldForeColor() As Brush
        Return nwcontent.Foreground
    End Function

    Public Function SetFrameColor(ByVal newbrush As Brush)
        nwframe.Background = newbrush
        Return True
    End Function

    Public Function GetFrameColor() As Brush
        Return nwframe.Background
    End Function

    Public Function GetContent() As Object
        Return nwcontent
    End Function

    Public Function GetContentAsRichTextBox() As RichTextBox
        Return nwcontent
    End Function


    Public Function Move()
        RaiseEvent OnMoveRequested(Me)
        ParentSlot.MoveWidget(Me)
        Return True
    End Function

    Public Sub RequestSplit(splitdir As EDockDirection)
        Me.ParentSlot.Split(Me, splitdir)
    End Sub

    Public Function CancelMove()
        Return True
    End Function

    Public Function RequestDock(ByVal target As NoteWidget, ByVal dockdir As EDockDirection)
        target.ParentSlot.Dock(target, Me, dockdir)
        Return True
    End Function

    Public Sub RequestClose()
        ParentSlot.RemoveWidget(Me)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns>Da usare con cautela e solo quando necessario. e.g. Serializzazione a Disco.</returns>
    Public Function GetContentAsString() As String
        GetContentAsRichTextBox.SelectAll()
        Dim ms As New IO.MemoryStream
        GetContentAsRichTextBox.Selection.Save(ms, DataFormats.Rtf)
        Dim result As String = ""
        ms.Seek(0, IO.SeekOrigin.Begin)
        Using sr As New IO.StreamReader(ms)
            result = sr.ReadToEnd()
        End Using
        ms.Close()
        Return result
    End Function


    Public Function NotifyDockCompleted(ByVal oldparent As NoteSlot, ByVal newparent As NoteSlot)
        RaiseEvent OnDockCompleted(Me, newparent, oldparent)
        Return True
    End Function

    Public Sub NotifyClosed()
        RaiseEvent OnClosed()
    End Sub

    Public Sub ToggleLockText(ByVal newvalue As Boolean)
        'il contenuto di questo widget passa dallo stato read-only\ editable. vice versa.
        'dopo il cambiamento, Widget Notifica direttamente al Root di questo cambiamento. 
        'il root a questo punto dovrebbe notificare tramite Evento qualsiasi classe che sia in ascolto.
        'Successivamente, il widget chiama il proprio evento OnLockToggled (privato). In questo modo 
        'può gestire i cambiamenti all'UI che sono richisti in questo caso

        nwcontent.IsEnabled = Not newvalue
        IsLocked = newvalue
        If ParentSlot IsNot Nothing Then ParentSlot.GetRoot.NotifyLockToggled(Me, newvalue)
        RaiseEvent OnLockToggled(IsLocked)
    End Sub

    Public Sub NotifyMoveAllowed()
        RaiseEvent OnMoveAllowed(Me)
    End Sub

    Public Sub NotifyMoveCancelled()
        RaiseEvent OnMoveCancelled()
    End Sub

    Public Sub NotifySplit(splitdir As EDockDirection)
        RaiseEvent OnSplit(splitdir)
    End Sub

    Private Event OnCanClose(ByVal widget As NoteWidget)
    Private Event OnMoveRequested(ByVal widget As NoteWidget)
    Private Event OnMoveAllowed(ByVal widget As NoteWidget)
    Private Event OnMoveCancelled()
    Private Event OnDockCompleted(ByVal widget As NoteWidget, ByVal newparentslot As NoteSlot, ByVal oldparentSlot As NoteSlot)
    Private Event OnLockToggled(ByVal IsNowLocked As Boolean)
    Private Event OnClosed()
    Private Event OnSplit(splitdirection As EDockDirection)

    Public Overrides Function ToString() As String
        Return "WIDGET " & ID.ToString
    End Function

    Private Sub btn_close_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Me.RequestClose()
    End Sub

    Private Sub cmenu_lock_text_Click(sender As Object, e As RoutedEventArgs)
        ToggleLockText(Not IsLocked)
    End Sub

    Private Sub cmenu_change_color_Click(sender As Object, e As RoutedEventArgs)
        Dim newcolor As Color
        If Core.ShowColorDialog(newcolor) Then
            Dim br = New SolidColorBrush(newcolor)
            Dim oldcolor = CType(GetFrameColor(), SolidColorBrush).Color
            SetFrameColor(br)
            SetTextFieldBackColor(br)
            ParentSlot.GetRoot.NotifyWidgetColorChanged(Me, oldcolor, newcolor)
        End If
    End Sub

    Private Sub cmenu_dock_u_Click(sender As Object, e As RoutedEventArgs)
        Dim moveRequestingWidget As NoteWidget = Core.GetMoveRequestingWidget
        If moveRequestingWidget IsNot Nothing Then
            moveRequestingWidget.RequestDock(Me, EDockDirection.U)
        End If
    End Sub

    Private Sub cmenu_dock_d_Click(sender As Object, e As RoutedEventArgs)
        Dim moveRequestingWidget As NoteWidget = Core.GetMoveRequestingWidget
        If moveRequestingWidget IsNot Nothing Then
            moveRequestingWidget.RequestDock(Me, EDockDirection.D)
        End If
    End Sub

    Private Sub cmenu_dock_l_Click(sender As Object, e As RoutedEventArgs)
        Dim moveRequestingWidget As NoteWidget = Core.GetMoveRequestingWidget
        If moveRequestingWidget IsNot Nothing Then
            moveRequestingWidget.RequestDock(Me, EDockDirection.L)
        End If
    End Sub

    Private Sub cmenu_dock_r_Click(sender As Object, e As RoutedEventArgs)
        Dim moveRequestingWidget As NoteWidget = Core.GetMoveRequestingWidget
        If moveRequestingWidget IsNot Nothing Then
            moveRequestingWidget.RequestDock(Me, EDockDirection.R)
        End If
    End Sub

    Private Sub cmenu_toggle_lock_text_Click(sender As Object, e As RoutedEventArgs)
        ToggleLockText(Not IsLocked)
    End Sub

    Private Sub cemnu_close_Click(sender As Object, e As RoutedEventArgs)
        Me.RequestClose()
    End Sub

    Private Sub cmenu_move_Click(sender As Object, e As RoutedEventArgs)
        Move()
    End Sub

    Private Sub lbl_move_flag_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Move()
    End Sub

    Private Sub lbl_toggle_lock_flag_MouseUp(sender As Object, e As MouseButtonEventArgs)
        ToggleLockText(Not IsLocked)
    End Sub

    Private Sub NoteWidget_OnMoveRequested(widget As NoteWidget) Handles Me.OnMoveRequested

    End Sub

    Private Sub NoteWidget_OnLockToggled(IsNowLocked As Boolean) Handles Me.OnLockToggled
        If IsNowLocked Then
            lbl_toggle_lock_flag.Content = "n"
            lbl_toggle_lock_flag.Tag = "q"
            cmenu_toggle_lock_text.Header = "Unlock"
        Else
            lbl_toggle_lock_flag.Content = "q"
            lbl_toggle_lock_flag.Tag = "n"
            cmenu_toggle_lock_text.Header = "Lock"
        End If

        cmenu_toggle_lock_text.IsChecked = IsLocked

    End Sub

    Private Sub NoteWidget_OnMoveCancelled() Handles Me.OnMoveCancelled
        IsMoving = False
        lbl_move_flag.Foreground = Brushes.White
        lbl_move_flag.Tag = "1"
        lbl_move_flag.Background = Brushes.Transparent
    End Sub

    Private Sub NoteWidget_OnMoveAllowed(widget As NoteWidget) Handles Me.OnMoveAllowed
        widget.IsMoving = True
        widget.lbl_move_flag.Foreground = Brushes.Black
        widget.lbl_move_flag.Tag = "0"
        widget.lbl_move_flag.Background = Brushes.White
    End Sub

    Private Sub NoteWidget_OnDockCompleted(widget As NoteWidget, newparentslot As NoteSlot, oldparentSlot As NoteSlot) Handles Me.OnDockCompleted
        widget.IsMoving = False
        widget.lbl_move_flag.Foreground = Brushes.White
        widget.lbl_move_flag.Tag = "1"
        widget.lbl_move_flag.Background = Brushes.Transparent
    End Sub

    Private Sub cmnu_split_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub cmenu_main_frame_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles cmenu_main_frame.ContextMenuOpening
        If Core.GetMoveRequestingWidget IsNot Nothing Then
            cmnu_dock.Visibility = Visibility.Visible
        Else
            cmnu_dock.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub cmenu_split_l_Click(sender As Object, e As RoutedEventArgs)
        RequestSplit(EDockDirection.L)
    End Sub

    Private Sub cmenu_split_u_Click(sender As Object, e As RoutedEventArgs)
        RequestSplit(EDockDirection.U)
    End Sub

    Private Sub cmenu_split_r_Click(sender As Object, e As RoutedEventArgs)
        RequestSplit(EDockDirection.R)
    End Sub

    Private Sub cmenu_split_d_Click(sender As Object, e As RoutedEventArgs)
        RequestSplit(EDockDirection.D)
    End Sub

    Private Sub NoteWidget_OnClosed() Handles Me.OnClosed
        Core.FreeName(ID) 'Restituisce il nome del widget al pool dei nomi.
    End Sub
End Class
