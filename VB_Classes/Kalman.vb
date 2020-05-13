Imports cv = OpenCvSharp
'http://opencvexamples.blogspot.com/2014/01/kalman-filter-implementation-tracking.html
Public Class Kalman_Basics
    Inherits ocvbClass
    Dim kalman() As Kalman_Single
    Public input() As Single
    Public output() As Single
    Public ProcessNoiseCov As Single = 0.00001
    Public MeasurementNoiseCov As Single = 0.1
    Public ErrorCovPost As Single = 1

    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        check.Setup(ocvb, caller, 1)
        check.Box(0).Text = "Turn Kalman filtering on in " + caller
        check.Box(0).Checked = True

        ocvb.desc = "Use Kalman to stabilize a set of value (such as a cv.rect.)"
    End Sub
    Private Sub setValues(ocvb As AlgorithmData)
        label1 = "Rectangle moves smoothly from random locations"
        Static autoRand As New Random()
        ReDim input(4 - 1)
        Dim w = ocvb.color.Width
        Dim h = ocvb.color.Height
        input = {autoRand.Next(50, w - 50), autoRand.Next(50, h - 50), autoRand.Next(5, w / 4), autoRand.Next(5, h / 4)}
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        If input Is Nothing Then setValues(ocvb)
        Static saveDimension As Int32 = -1
        If saveDimension <> input.Length Then
            If kalman IsNot Nothing Then
                If kalman.Count > 0 Then
                    For i = 0 To kalman.Count - 1
                        kalman(i).Dispose()
                    Next
                End If
            End If
            saveDimension = input.Length
            ReDim kalman(input.Length - 1)
            For i = 0 To input.Length - 1
                kalman(i) = New Kalman_Single(ocvb, caller)
                kalman(i).ProcessNoiseCov = ProcessNoiseCov
                kalman(i).MeasurementNoiseCov = MeasurementNoiseCov
                kalman(i).ErrorCovPost = ErrorCovPost
            Next
            ReDim output(input.Count - 1)
        End If

        If check.Box(0).Checked Then
            For i = 0 To kalman.Length - 1
                kalman(i).inputReal = input(i)
                kalman(i).Run(ocvb)
            Next

            For i = 0 To input.Length - 1
                output(i) = kalman(i).stateResult
            Next
        Else
            output = input ' do nothing to the input.
        End If

        If standalone Then
            dst1 = ocvb.color.Clone()
            Static rect As New cv.Rect(CInt(output(0)), CInt(output(1)), CInt(output(2)), CInt(output(3)))
            If rect.X = CInt(output(0)) And rect.Y = CInt(output(1)) And rect.Width = CInt(output(2)) And rect.Height = CInt(output(3)) Then
                setValues(ocvb)
            Else
                rect = New cv.Rect(CInt(output(0)), CInt(output(1)), CInt(output(2)), CInt(output(3)))
            End If
            dst1.Rectangle(rect, cv.Scalar.White, 6)
            dst1.Rectangle(rect, cv.Scalar.Red, 1)
        End If
    End Sub
End Class







' http://opencvexamples.blogspot.com/2014/01/kalman-filter-implementation-tracking.html
Public Class Kalman_Compare
    Inherits ocvbClass
    Dim kalman() As Kalman_Single
    Public plot As Plot_OverTime
    Public kPlot As Plot_OverTime
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        plot = New Plot_OverTime(ocvb, caller)
        plot.dst1 = dst1
        plot.plotCount = 3
        plot.topBottomPad = 20

        kPlot = New Plot_OverTime(ocvb, caller)
        kPlot.dst1 = dst2
        kPlot.plotCount = 3
        kPlot.topBottomPad = 20

        label1 = "Kalman input: mean values for RGB"
        label2 = "Kalman output: smoothed mean values for RGB"
        ocvb.desc = "Use this kalman filter to predict the next value."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        If ocvb.frameCount = 0 Then
            If kalman IsNot Nothing Then
                If kalman.Count > 0 Then
                    For i = 0 To kalman.Count - 1
                        kalman(i).Dispose()
                    Next
                End If
            End If
            ReDim kalman(3 - 1)
            For i = 0 To kalman.Count - 1
                kalman(i) = New Kalman_Single(ocvb, caller)
            Next
        End If

        ' if either one has triggered a reset for the scale, do them both...
        If kPlot.offChartCount >= kPlot.plotTriggerRescale Or plot.offChartCount >= plot.plotTriggerRescale Then
            kPlot.offChartCount = kPlot.plotTriggerRescale + 1
            plot.offChartCount = plot.plotTriggerRescale + 1
        End If

        plot.plotData = ocvb.color.Mean()
        plot.Run(ocvb)

        For i = 0 To kalman.Count - 1
            kalman(i).inputReal = plot.plotData.Item(i)
            kalman(i).Run(ocvb)
        Next

        kPlot.maxScale = plot.maxScale ' keep the scale the same for the side-by-side plots.
        kPlot.minScale = plot.minScale
        kPlot.plotData = New cv.Scalar(kalman(0).stateResult, kalman(1).stateResult, kalman(2).stateResult)
        kPlot.Run(ocvb)
    End Sub
End Class




'https://github.com/opencv/opencv/blob/master/samples/cpp/kalman.cpp
Public Class Kalman_RotatingPoint
    Inherits ocvbClass
    Dim kf As New cv.KalmanFilter(2, 1, 0)
    Dim kState As New cv.Mat(2, 1, cv.MatType.CV_32F)
    Dim processNoise As New cv.Mat(2, 1, cv.MatType.CV_32F)
    Dim measurement As New cv.Mat(1, 1, cv.MatType.CV_32F, 0)
    Dim center As cv.Point2f, statePt As cv.Point2f
    Dim radius As Single
    Private Function calcPoint(center As cv.Point2f, R As Double, angle As Double) As cv.Point
        Return center + New cv.Point2f(Math.Cos(angle), -Math.Sin(angle)) * R
    End Function
    Private Sub drawCross(dst1 As cv.Mat, center As cv.Point, color As cv.Scalar)
        Dim d As Int32 = 3
        cv.Cv2.Line(dst1, New cv.Point(center.X - d, center.Y - d), New cv.Point(center.X + d, center.Y + d), color, 1, cv.LineTypes.AntiAlias)
        cv.Cv2.Line(dst1, New cv.Point(center.X + d, center.Y - d), New cv.Point(center.X - d, center.Y + d), color, 1, cv.LineTypes.AntiAlias)
    End Sub
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        label1 = "Estimate Yellow < Real Red (if working)"

        cv.Cv2.Randn(kState, New cv.Scalar(0), cv.Scalar.All(0.1))
        kf.TransitionMatrix = New cv.Mat(2, 2, cv.MatType.CV_32F, New Single() {1, 1, 0, 1})

        cv.Cv2.SetIdentity(kf.MeasurementMatrix)
        cv.Cv2.SetIdentity(kf.ProcessNoiseCov, cv.Scalar.All(0.00001))
        cv.Cv2.SetIdentity(kf.MeasurementNoiseCov, cv.Scalar.All(0.1))
        cv.Cv2.SetIdentity(kf.ErrorCovPost, cv.Scalar.All(1))
        cv.Cv2.Randn(kf.StatePost, New cv.Scalar(0), cv.Scalar.All(1))
        radius = ocvb.color.Rows / 2.4 ' so we see the entire circle...
        center = New cv.Point2f(ocvb.color.Cols / 2, ocvb.color.Height / 2)
        ocvb.desc = "Track a rotating point using a Kalman filter. Yellow line (estimate) should be shorter than red (real)."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim stateAngle = kState.Get(Of Single)(0)

        Dim prediction = kf.Predict()
        Dim predictAngle = prediction.Get(Of Single)(0)
        Dim predictPt = calcPoint(center, radius, predictAngle)
        statePt = calcPoint(center, radius, stateAngle)

        cv.Cv2.Randn(measurement, New cv.Scalar(0), cv.Scalar.All(kf.MeasurementNoiseCov.Get(Of Single)(0)))

        measurement += kf.MeasurementMatrix * kState
        Dim measAngle = measurement.Get(Of Single)(0)
        Dim measPt = calcPoint(center, radius, measAngle)

        drawCross(dst1, statePt, cv.Scalar.White)
        drawCross(dst1, measPt, cv.Scalar.White)
        drawCross(dst1, predictPt, cv.Scalar.White)
        cv.Cv2.Line(dst1, statePt, measPt, New cv.Scalar(0, 0, 255), 3, cv.LineTypes.AntiAlias)
        cv.Cv2.Line(dst1, statePt, predictPt, New cv.Scalar(0, 255, 255), 3, cv.LineTypes.AntiAlias)

        If ocvb.ms_rng.Next(0, 4) <> 0 Then kf.Correct(measurement)

        cv.Cv2.Randn(processNoise, cv.Scalar.Black, cv.Scalar.All(Math.Sqrt(kf.ProcessNoiseCov.Get(Of Single)(0, 0))))
        kState = kf.TransitionMatrix * kState + processNoise
    End Sub
End Class






'http://opencvexamples.blogspot.com/2014/01/kalman-filter-implementation-tracking.html
Public Class Kalman_MousePredict
    Inherits ocvbClass
    Dim kalman As Kalman_Basics
    Dim locMultiplier = 1
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        kalman = New Kalman_Basics(ocvb, caller)
        ReDim kalman.input(1)
        ReDim kalman.output(1)

        If ocvb.parms.lowResolution = False Then locMultiplier = 2 ' twice the size in both dimensions.
        label1 = "Red is real mouse, white is prediction"
        ocvb.desc = "Use kalman filter to predict the next mouse location."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        If ocvb.frameCount Mod 100 = 0 Then dst1.SetTo(0)

        Static lastRealMouse = ocvb.mousePoint
        kalman.input(0) = ocvb.mousePoint.X
        kalman.input(1) = ocvb.mousePoint.Y
        Dim lastStateResult = New cv.Point(kalman.output(0), kalman.output(1))
        kalman.Run(ocvb)
        cv.Cv2.Line(dst1, New cv.Point(lastStateResult.X, lastStateResult.Y) * locMultiplier,
                                  New cv.Point(kalman.output(0), kalman.output(1)) * locMultiplier,
                                  cv.Scalar.All(255), 1, cv.LineTypes.AntiAlias)
        cv.Cv2.Line(dst1, ocvb.mousePoint * locMultiplier, lastRealMouse * locMultiplier, New cv.Scalar(0, 0, 255), 1, cv.LineTypes.AntiAlias)
        lastRealMouse = ocvb.mousePoint
    End Sub
End Class






Public Class Kalman_CVMat
    Inherits ocvbClass
    Dim kalman() As Kalman_Single
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        label1 = "Rectangle moves smoothly to random locations"
        ocvb.desc = "Use Kalman to stabilize a set of values (such as a cv.rect.)"
    End Sub
    Private Sub setValues(ocvb As AlgorithmData, ByVal callerRaw As String)
        Static autoRand As New Random()
        Dim x = autoRand.Next(50, ocvb.color.Width - 50)
        Dim y = autoRand.Next(50, ocvb.color.Height - 50)
        Dim vals() As Single = {x, y, autoRand.Next(5, ocvb.color.Width - x), autoRand.Next(5, ocvb.color.Height - y)}
        src = New cv.Mat(4, 1, cv.MatType.CV_32F, vals)
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        If src Is Nothing Then setValues(ocvb, "Kalman_CVMat")
        Static saveDimension As Int32 = -1
        If saveDimension <> src.Rows Then
            If kalman IsNot Nothing Then
                If kalman.Count > 0 Then
                    For i = 0 To kalman.Count - 1
                        kalman(i).Dispose()
                    Next
                End If
            End If
            saveDimension = src.Rows
            ReDim kalman(src.Rows - 1)
            For i = 0 To src.Rows - 1
                kalman(i) = New Kalman_Single(ocvb, caller)
            Next
            dst1 = New cv.Mat(src.Rows, 1, cv.MatType.CV_32F, 0)
        End If

        For i = 0 To kalman.Length - 1
            kalman(i).inputReal = src.Get(Of Single)(i, 0)
            kalman(i).Run(ocvb)
        Next

        For i = 0 To src.Rows - 1
            dst1.Set(Of Single)(i, 0, kalman(i).stateResult)
        Next

        If standalone Then
            Dim rx(src.Rows - 1) As Single
            Dim testrect As New cv.Rect
            For i = 0 To src.Rows - 1
                rx(i) = dst1.Get(Of Single)(i, 0)
            Next
            dst1 = ocvb.color
            Static rect As New cv.Rect(CInt(rx(0)), CInt(rx(1)), CInt(rx(2)), CInt(rx(3)))
            If rect.X = CInt(rx(0)) And rect.Y = CInt(rx(1)) And rect.Width = CInt(rx(2)) And rect.Height = CInt(rx(3)) Then
                setValues(ocvb, "Kalman_CVMat")
            Else
                rect = New cv.Rect(CInt(rx(0)), CInt(rx(1)), CInt(rx(2)), CInt(rx(3)))
            End If
            dst1.Rectangle(rect, cv.Scalar.Red, 2)
        End If
    End Sub
End Class







Public Class Kalman_ImageSmall
    Inherits ocvbClass
    Dim kalman As Kalman_CVMat
    Dim resize As Resize_Percentage
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        kalman = New Kalman_CVMat(ocvb, caller)

        resize = New Resize_Percentage(ocvb, caller)

        label1 = "The small image is processed by the Kalman filter"
        label2 = "Mask of the smoothed image minus original"
        ocvb.desc = "Resize the image to allow the Kalman filter to process the whole image."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim gray = ocvb.color.CvtColor(cv.ColorConversionCodes.BGR2GRAY)
        resize.src = gray
        resize.Run(ocvb)

        Dim saveOriginal = resize.dst1.Clone()
        Dim gray32f As New cv.Mat
        resize.dst1.ConvertTo(gray32f, cv.MatType.CV_32F)
        kalman.src = gray32f.Reshape(1, gray32f.Width * gray32f.Height)
        kalman.Run(ocvb)
        Dim dst1 As New cv.Mat
        kalman.dst1.ConvertTo(dst1, cv.MatType.CV_8U)
        dst1 = dst1.Reshape(1, gray32f.Height)
        dst1 = dst1.Resize(dst1.Size())
        cv.Cv2.Subtract(dst1, saveOriginal, dst1)
        dst1 = dst1.Threshold(1, 255, cv.ThresholdTypes.Binary)
        dst2 = dst1.Resize(dst1.Size())
    End Sub
End Class





Public Class Kalman_DepthSmall
    Inherits ocvbClass
    Dim kalman As Kalman_CVMat
    Dim resize As Resize_Percentage
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        kalman = New Kalman_CVMat(ocvb, caller)

        resize = New Resize_Percentage(ocvb, caller)
        resize.sliders.TrackBar1.Value = 4

        label2 = "Brighter: depth is decreasing (object getting closer)"
        label1 = "Mask of non-zero depth after Kalman smoothing"
        ocvb.desc = "Use a resized depth Mat to find where depth is decreasing (something getting closer.)"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim depth32f = getDepth32f(ocvb)
        resize.src = depth32f
        resize.Run(ocvb)

        resize.dst1.ConvertTo(depth32f, cv.MatType.CV_32F)
        kalman.src = depth32f.Reshape(1, depth32f.Width * depth32f.Height)
        Dim saveOriginal = kalman.src.Clone()
        kalman.Run(ocvb)
        Dim dst1 = kalman.dst1.Reshape(1, depth32f.Height)
        dst1 = dst1.Resize(dst1.Size())
        dst1 = dst1.ConvertScaleAbs()
        cv.Cv2.Subtract(kalman.dst1, saveOriginal, depth32f)
        dst1 = depth32f.Threshold(0, 0, cv.ThresholdTypes.Tozero).ConvertScaleAbs()
        dst1 = dst1.Reshape(1, resize.dst1.Height)
        dst2 = dst1.Resize(dst1.Size())
    End Sub
End Class







Public Class Kalman_Single
    Inherits ocvbClass
    Dim plot As Plot_OverTime
    Dim kf As New cv.KalmanFilter(2, 1, 0)
    Dim processNoise As New cv.Mat(2, 1, cv.MatType.CV_32F)
    Public measurement As New cv.Mat(1, 1, cv.MatType.CV_32F, 0)
    Public inputReal As Single
    Public stateResult As Single
    Public ProcessNoiseCov As Single = 0.00001
    Public MeasurementNoiseCov As Single = 0.1
    Public ErrorCovPost As Single = 1
    Public transitionMatrix() As Single = {1, 1, 0, 1} ' Change the transition matrix externally and set newTransmissionMatrix.
    Public newTransmissionMatrix As Boolean = True
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        Dim tMatrix() As Single = {1, 1, 0, 1}
        kf.TransitionMatrix = New cv.Mat(2, 2, cv.MatType.CV_32F, tMatrix)
        kf.MeasurementMatrix.SetIdentity(1)
        kf.ProcessNoiseCov.SetIdentity(0.00001)
        kf.MeasurementNoiseCov.SetIdentity(0.1)
        kf.ErrorCovPost.SetIdentity(1)

        ocvb.desc = "Estimate a single value using a Kalman Filter - in the default case, the value of the mean of the grayscale image."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        If standalone Then
            If ocvb.frameCount = 0 Then
                plot = New Plot_OverTime(ocvb, caller)
                plot.dst1 = dst2
                plot.maxScale = 150
                plot.minScale = 80
                plot.plotCount = 2
            End If

            dst1 = ocvb.color.CvtColor(cv.ColorConversionCodes.BGR2GRAY)
            inputReal = dst1.Mean().Item(0)
        End If

        Dim prediction = kf.Predict()
        measurement.Set(Of Single)(0, 0, inputReal)
        stateResult = kf.Correct(measurement).Get(Of Single)(0, 0)
        If standalone Then
            plot.plotData = New cv.Scalar(inputReal, stateResult, 0, 0)
            plot.Run(ocvb)
            label1 = "Mean of the grayscale image is predicted"
            label2 = "Mean (blue) = " + Format(inputReal, "0.0") + " predicted (green) = " + Format(stateResult, "0.0")
        End If
    End Sub
End Class

