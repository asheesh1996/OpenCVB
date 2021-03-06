﻿Imports cv = OpenCvSharp
Imports System.IO
Public Class OptionsVideoName
    Public captureVideo As New cv.VideoCapture
    Dim fileinfo As FileInfo
    Private Sub OptionsFilename_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetDesktopLocation(appLocation.Left + Me.Width + radioOffset.X, appLocation.Top + appLocation.Height + radioOffset.Y)
    End Sub
    Public Sub NewVideo(ocvb As AlgorithmData, videoFile As String)
        fileinfo = New FileInfo(videoFile)
        If fileinfo.Exists = False Then fileinfo = New FileInfo(ocvb.parms.HomeDir + "Data\CarsDrivingUnderBridge.mp4")

        Filename.Text = fileinfo.FullName
        captureVideo = New cv.VideoCapture(fileinfo.FullName)
        TrackBar1.Minimum = 0
        TrackBar1.Maximum = captureVideo.FrameCount
        If ocvb.parms.ShowOptions Then Me.Show()
    End Sub
    Public Function nextImage() As cv.Mat
        Static image As New cv.Mat
        captureVideo.Read(image)
        If image.Empty() Then
            captureVideo.Dispose()
            captureVideo = New cv.VideoCapture(fileinfo.FullName)
            captureVideo.Read(image)
        End If

        FrameNumber.Text = "Frame = " + CStr(captureVideo.PosFrames)
        TrackBar1.Value = captureVideo.PosFrames
        Return image
    End Function
    Private Sub TrackBar1_ValueChanged(sender As Object, e As EventArgs) Handles TrackBar1.ValueChanged
        captureVideo.PosFrames = TrackBar1.Value
    End Sub
End Class
