﻿Imports cv = OpenCvSharp
Public Class ocvbClass : Implements IDisposable
    Public caller As String
    Public check As New OptionsCheckbox
    Public radio As New OptionsRadioButtons
    Public radio1 As New OptionsRadioButtons
    Public sliders As New OptionsSliders
    Public sliders1 As New OptionsSliders
    Public sliders2 As New OptionsSliders
    Public sliders3 As New OptionsSliders
    Public videoOptions As New OptionsVideoName
    Dim algorithm As Object
    Public Sub setCaller(callerRaw As String)
        If callerRaw Is Nothing Then callerRaw = ""
        If callerRaw = "" Or callerRaw.Contains(Me.GetType.Name) Then caller = Me.GetType.Name Else caller = callerRaw + "-->" + Me.GetType.Name
    End Sub
    Public Sub New()
        algorithm = Me
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Dim type = algorithm.GetType
        If type.GetProperty("MyDispose") IsNot Nothing Then algorithm.MyDispose()  ' dispose of any managed and unmanaged classes.
        sliders.Dispose()
        sliders1.Dispose()
        sliders2.Dispose()
        sliders3.Dispose()
        radio1.Dispose()
        videoOptions.Dispose()
    End Sub
End Class
