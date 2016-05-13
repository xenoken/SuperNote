Public Class Core
    Public Const MAX_NUM_WIDGETS_PER_SLOT = 2

    Private Shared Move_Requesting_Widget As NoteWidget = Nothing

    Public Shared colorDialog As System.Windows.Forms.ColorDialog = New System.Windows.Forms.ColorDialog With {.AllowFullOpen = True, .FullOpen = True, .SolidColorOnly = True}

    Public Shared NoteDatabasePath As String = "supernotes.notes.db"

    Public Shared Function GetMoveRequestingWidget() As NoteWidget
        Return Move_Requesting_Widget
    End Function

    Public Shared Sub SetMoveRequestingWidget(ByVal wid As NoteWidget)
        Move_Requesting_Widget = wid
    End Sub

    Public Shared Function ShowColorDialog(ByRef OutColor) As Boolean
        If colorDialog.ShowDialog = Forms.DialogResult.OK Then
            Dim fc = colorDialog.Color
            Dim clr = Color.FromRgb(fc.R, fc.G, fc.B)
            '  colorDialog = New System.Windows.Forms.ColorDialog With {.AllowFullOpen = True, .FullOpen = True, .SolidColorOnly = True} 
            OutColor = clr
            Return True
        End If

        Return False
    End Function

    Private Shared _nextname As Integer = 0
    Private Shared _freenames As New Stack(Of Integer)
    Public Shared Function GetNextAvailableName() As Integer
        If _freenames.Count > 0 Then
            Return _freenames.Pop
        Else
            _nextname += 1
            Return _nextname - 1
        End If
    End Function

    Public Shared Function FreeName(ByVal name As Integer)
        If _freenames.Contains(name) Then
            Throw New Exception("It is not allowed to free the same name multiple times.")
        Else

            _freenames.Push(name)
        End If
        Return True
    End Function
End Class

Public Enum ECloseReasonType
    TEXT_EMPTY
    SLOT_EMPTY
End Enum

Public Enum EDockDirection
    NONE
    L
    U
    R
    D
End Enum

Public Enum ESplitOrientation
    VERTICAL
    HORIZONTAL
End Enum

Public Enum EErrorType
    REMOVAL_OF_LAST_WIDGET
End Enum

Public Class OnErrorArgs
    Public Message As String
    Public Data As Object
    Public ErrorType As EErrorType
End Class