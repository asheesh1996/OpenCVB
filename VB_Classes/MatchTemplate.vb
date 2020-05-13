Imports cv = OpenCvSharp
Public Class MatchTemplate_Basics
    Inherits ocvbClass
    Dim flow As Font_FlowText
    Public sample1 As cv.Mat
    Public sample2 As cv.Mat
        Public matchText As String = ""
    Public correlationMat As New cv.Mat
    Public reportFreq = 10 ' report the results every x number of iterations.
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        flow = New Font_FlowText(ocvb, caller)
        flow.result1or2 = RESULT2

        radio.Setup(ocvb, caller, 6)
        radio.check(0).Text = "CCoeff"
        radio.check(1).Text = "CCoeffNormed"
        radio.check(2).Text = "CCorr"
        radio.check(3).Text = "CCorrNormed"
        radio.check(4).Text = "SqDiff"
        radio.check(5).Text = "SqDiffNormed"
        radio.check(1).Checked = True
        sliders.setupTrackBar1(ocvb, caller, "Sample Size", 2, 10000, 100)
        label2 = "Log of correlation results"
        ocvb.desc = "Find correlation coefficient for 2 random series.  Should be near zero except for small sample size."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        if standalone Then
            sample1 = New cv.Mat(New cv.Size(sliders.TrackBar1.Value, 1), cv.MatType.CV_32FC1)
            sample2 = New cv.Mat(New cv.Size(sliders.TrackBar1.Value, 1), cv.MatType.CV_32FC1)
            cv.Cv2.Randn(sample1, 100, 25)
            cv.Cv2.Randn(sample2, 0, 25)
        Else
            sliders.Visible = False
        End If

        Dim matchOption = cv.TemplateMatchModes.CCoeffNormed
        For i = 0 To radio.check.Count - 1
            If radio.check(i).Checked Then
                matchOption = Choose(i + 1, cv.TemplateMatchModes.CCoeff, cv.TemplateMatchModes.CCoeffNormed, cv.TemplateMatchModes.CCorr,
                                            cv.TemplateMatchModes.CCorrNormed, cv.TemplateMatchModes.SqDiff, cv.TemplateMatchModes.SqDiffNormed)
                matchText = Choose(i + 1, "CCoeff", "CCoeffNormed", "CCorr", "CCorrNormed", "SqDiff", "SqDiffNormed")
            End If
        Next
        cv.Cv2.MatchTemplate(sample1, sample2, correlationMat, matchOption)
        If ocvb.frameCount Mod reportFreq = 0 Then
            Dim correlation = correlationMat.Get(Of Single)(0, 0)
            label1 = "Correlation = " + Format(correlation, "#,##0.000")
            if standalone Then
                label1 = matchText + " for " + CStr(sample1.Rows) + " samples each = " + Format(correlation, "#,##0.00")
                flow.msgs.Add(matchText + " = " + Format(correlation, "#,##0.00"))
                flow.Run(ocvb)
            End If
        End If
    End Sub
End Class




Public Class MatchTemplate_RowCorrelation
    Inherits ocvbClass
    Dim corr As MatchTemplate_Basics
    Dim flow As Font_FlowText
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        flow = New Font_FlowText(ocvb, caller)
        flow.result1or2 = RESULT2

        corr = New MatchTemplate_Basics(ocvb, caller)
        corr.sliders.Visible = False

        ocvb.desc = "Find correlation coefficients for 2 random rows in the RGB image to show variability"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim line1 = ocvb.ms_rng.Next(0, ocvb.color.Height - 1)
        Dim line2 = ocvb.ms_rng.Next(0, ocvb.color.Height - 1)

        Dim nextLine = dst1.Row(line1)
        nextLine = ocvb.color.Row(line1)
        nextLine = dst1.Row(line2)
        nextLine = ocvb.color.Row(line2)

        corr.sample1 = ocvb.color.Row(line1).Clone()
        corr.sample2 = ocvb.color.Row(line2 + 1).Clone()
        corr.Run(ocvb)
        Dim correlation = corr.correlationMat.Get(Of Single)(0, 0)
        flow.msgs.Add(corr.matchText + " between lines " + CStr(line1) + " and line " + CStr(line2) + " = " + Format(correlation, "#,##0.00"))
        flow.Run(ocvb)

        Static minCorrelation = Single.PositiveInfinity
        Static maxCorrelation = Single.NegativeInfinity
        If correlation < minCorrelation Then minCorrelation = correlation
        If correlation > maxCorrelation Then maxCorrelation = correlation
        label1 = "Min = " + Format(minCorrelation, "#,##0.00") + " max = " + Format(maxCorrelation, "#,##0.0000")
    End Sub
End Class





Public Class MatchTemplate_DrawRect
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        radio.Setup(ocvb, caller, 6)
        For i = 0 To radio.check.Count - 1
            radio.check(i).Text = Choose(i + 1, "SQDIFF", "SQDIFF NORMED", "TM CCORR", "TM CCORR NORMED", "TM COEFF", "TM COEFF NORMED")
        Next
        radio.check(5).Checked = True

        ocvb.drawRect = New cv.Rect(100, 100, 50, 50) ' arbitrary template to match

        label1 = "Probabilities (draw rectangle to test again)"
        label2 = "White is input, Red circle centers highest probability"
        ocvb.desc = "Find the requested template in an image"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Static saveTemplate As cv.Mat
        Static saveRect As cv.Rect
        If ocvb.drawRect.Width > 0 And ocvb.drawRect.Height > 0 Then
            If ocvb.drawRect.X + ocvb.drawRect.Width >= ocvb.color.Width Then ocvb.drawRect.Width = ocvb.color.Width - ocvb.drawRect.X
            If ocvb.drawRect.Y + ocvb.drawRect.Height >= ocvb.color.Height Then ocvb.drawRect.Height = ocvb.color.Height - ocvb.drawRect.Y
            saveRect = ocvb.drawRect
            saveTemplate = ocvb.color(ocvb.drawRect).Clone()
            ocvb.drawRectClear = True
        End If
        Dim matchMethod As cv.TemplateMatchModes
        For i = 0 To radio.check.Count - 1
            If radio.check(i).Checked Then
                matchMethod = Choose(i + 1, cv.TemplateMatchModes.SqDiff, cv.TemplateMatchModes.SqDiffNormed, cv.TemplateMatchModes.CCorr,
                                          cv.TemplateMatchModes.CCorrNormed, cv.TemplateMatchModes.CCoeff, cv.TemplateMatchModes.CCoeffNormed)
                Exit For
            End If
        Next
        cv.Cv2.MatchTemplate(ocvb.color, saveTemplate, dst1, matchMethod)
        dst2 = ocvb.color
        dst2.Rectangle(saveRect, cv.Scalar.White, 1)
        Dim minVal As Single, maxVal As Single, minLoc As cv.Point, maxLoc As cv.Point
        dst1.MinMaxLoc(minVal, maxVal, minLoc, maxLoc)
        dst2.Circle(maxLoc.X + saveRect.Width / 2, maxLoc.Y + saveRect.Height / 2, 20, cv.Scalar.Red, 3, cv.LineTypes.AntiAlias)
    End Sub
End Class





Public Class MatchTemplate_BestTemplate_MT
    Inherits ocvbClass
    Dim grid As Thread_Grid
    Dim entropies(0) As Entropy_Basics
    Dim match As MatchTemplate_DrawRect
    Public Sub New(ocvb As AlgorithmData, ByVal callerRaw As String)
        setCaller(callerRaw)
        grid = New Thread_Grid(ocvb, caller)
        grid.sliders.TrackBar1.Value = 128
        grid.sliders.TrackBar2.Value = 128

        match = New MatchTemplate_DrawRect(ocvb, caller)

        ocvb.parms.ShowOptions = False ' we won't need the options...

        label1 = "Probabilities - drawing is not used."
        label2 = "White is highest entropy (input). Red is best match."
        ocvb.desc = "Find the best object to track in the image"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Static bestContrast As cv.Rect
        If ocvb.frameCount Mod 30 = 0 Then
            grid.Run(ocvb)
            If entropies.Length <> grid.roiList.Count Then
                ReDim entropies(grid.roiList.Count - 1)
                For i = 0 To entropies.Length - 1
                    entropies(i) = New Entropy_Basics(ocvb, caller)
                Next
            End If

            Parallel.For(0, grid.roiList.Count - 1,
             Sub(i)
                 entropies(i).src = ocvb.color(grid.roiList(i))
                 entropies(i).Run(ocvb)
             End Sub)

            Dim maxEntropy As Single
            Dim maxIndex As Int32
            For i = 0 To entropies.Count - 1
                If entropies(i).entropy > maxEntropy Then
                    maxEntropy = entropies(i).entropy
                    maxIndex = i
                End If
            Next

            dst2 = ocvb.color.Clone()
            bestContrast = grid.roiList(maxIndex)
            ocvb.drawRect = bestContrast
        End If

        match.Run(ocvb)
    End Sub
End Class
