Imports cv = OpenCvSharp
Public Class Featureless_Basics_MT
    Inherits ocvbClass
    Public edges As Edges_Canny
    Public grid As Thread_Grid
    Public regionCount As Int32
    Public mask As New cv.Mat
    Public objects As New List(Of cv.Mat)
    Public objectSize As New List(Of Int32)
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        sliders.setupTrackBar1(ocvb, caller, "FeatureLess rho", 1, 100, 1)
        sliders.setupTrackBar2("FeatureLess theta", 1, 1000, 1000 * Math.PI / 180)
        sliders.setupTrackBar3("FeatureLess threshold", 1, 100, 3)
        sliders.setupTrackBar4("FeatureLess Flood Threshold", 100, 10000, If(ocvb.color.Width > 1000, 1000, 500))

        edges = New Edges_Canny(ocvb)

        grid = New Thread_Grid(ocvb)
        grid.sliders.TrackBar1.Value = If(ocvb.color.Width > 1000, 16, 8)
        grid.sliders.TrackBar2.Value = If(ocvb.color.Width > 1000, 16, 8)

        ocvb.desc = "Multithread Houghlines to find featureless regions in an image."
        label1 = "Featureless regions with mask in depth color"
    End Sub

    Public Sub Run(ocvb As AlgorithmData)
        grid.Run(ocvb)

        edges.src = src
        edges.Run(ocvb)

        Dim rhoIn = sliders.TrackBar1.Value
        Dim thetaIn = sliders.TrackBar2.Value / 1000
        Dim threshold = sliders.TrackBar3.Value
        Dim floodCountThreshold = sliders.TrackBar4.Value

        src.CopyTo(dst1)
        mask = New cv.Mat(dst2.Size(), cv.MatType.CV_8U, 0)
        Parallel.ForEach(Of cv.Rect)(grid.roiList,
        Sub(roi)
            Dim segments() = cv.Cv2.HoughLines(edges.dst1(roi), rhoIn, thetaIn, threshold)
            If segments.Count = 0 Then mask(roi).SetTo(255)
        End Sub)
        regionCount = 1
        For y = 0 To mask.Rows - 1
            For x = 0 To mask.Cols - 1
                If mask.Get(Of Byte)(y, x) = 255 Then
                    Dim pt As New cv.Point(x, y)
                    Dim floodCount = mask.FloodFill(pt, regionCount)
                    If floodCount < floodCountThreshold Then
                        mask.FloodFill(pt, 0)
                    Else
                        objectSize.Add(floodCount)
                        regionCount += 1
                    End If
                End If
            Next
        Next

        objects.Clear()
        objectSize.Clear()
        dst2.SetTo(0)
        For i = 1 To regionCount - 1
            Dim label = mask.InRange(i, i)
            objects.Add(label.Clone())
            Dim mean = ocvb.RGBDepth.Mean(label)
            dst2.SetTo(mean, label)
        Next
        cv.Cv2.AddWeighted(src, 0.5, dst2, 0.5, 0, dst1)
        label2 = "FeatureLess Regions = " + CStr(regionCount)
    End Sub
End Class




Public Class FeatureLess_Prediction
    Inherits ocvbClass
    Dim fLess As Featureless_Basics_MT
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        sliders.setupTrackBar1(ocvb, caller, "FeatureLess Resize Percent", 1, 100, 1)

        fLess = New Featureless_Basics_MT(ocvb)

        ocvb.desc = "Identify the featureless regions, use color and depth to learn the featureless label, and predict depth over the image. - needs more work"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        fLess.src = src
        fLess.Run(ocvb)
        dst1 = fLess.dst1
        dst2 = fLess.dst2
        Dim labels = fLess.mask.Clone()
        fLess.mask = fLess.mask.Threshold(1, 255, cv.ThresholdTypes.Binary)

        Dim percent = Math.Sqrt(sliders.TrackBar1.Value / 100)
        Dim newSize = New cv.Size(src.Width * percent, src.Height * percent)

        Dim rgb = src.Clone(), depth32f = getDepth32f(ocvb).Resize(newSize), mask = fLess.mask

        rgb = rgb.Resize(newSize)

        ' manually resize the mask to make sure there is no dithering...
        mask = New cv.Mat(depth32f.Size(), cv.MatType.CV_8U, 0)
        Dim labelSmall As New cv.Mat(mask.Size(), cv.MatType.CV_32S, 0)
        Dim xFactor = CInt(fLess.mask.Width / newSize.Width)
        Dim yFactor = CInt(fLess.mask.Height / newSize.Height)
        For y = 0 To mask.Height - 2
            For x = 0 To mask.Width - 2
                If fLess.mask.Get(Of Byte)(y * yFactor, x * xFactor) = 255 Then
                    mask.Set(Of Byte)(y, x, 255)
                    labelSmall.Set(Of Byte)(y, x, labels.Get(Of Byte)(y, x))
                End If
            Next
        Next

        rgb.SetTo(0, mask)
        depth32f.SetTo(0, mask)

        Dim rgb32f As New cv.Mat, response As New cv.Mat
        rgb.ConvertTo(rgb32f, cv.MatType.CV_32FC3)
        labelSmall.ConvertTo(response, cv.MatType.CV_32S)

        Dim saveRGB = rgb32f.Clone()

        Dim learnInput As New cv.Mat
        Dim planes() = rgb32f.Split()
        ReDim Preserve planes(3)
        planes(3) = getDepth32f(ocvb).Resize(newSize)
        cv.Cv2.Merge(planes, learnInput)

        Dim rtree = cv.ML.RTrees.Create()
        learnInput = learnInput.Reshape(1, learnInput.Rows * learnInput.Cols)
        response = response.Reshape(1, response.Rows * response.Cols)
        rtree.Train(learnInput, cv.ML.SampleTypes.RowSample, response)

        cv.Cv2.BitwiseNot(mask, mask)
        rgb32f.SetTo(0)
        depth32f.SetTo(0)
        saveRGB.CopyTo(rgb32f, mask)

        planes = rgb32f.Split()
        ReDim Preserve planes(3)
        planes(3) = depth32f.Clone()
        cv.Cv2.Merge(planes, learnInput)

        learnInput = learnInput.Reshape(1, learnInput.Rows * learnInput.Cols)
        response = response.Reshape(1, response.Rows * response.Cols)
        rtree.Predict(learnInput, response)
        Dim predictedDepth = response.Reshape(1, depth32f.Height)
        predictedDepth.Normalize(0, 255, cv.NormTypes.MinMax)
        predictedDepth.ConvertTo(mask, cv.MatType.CV_8U)
        dst2 = mask.ConvertScaleAbs().Resize(src.Size())
    End Sub
End Class




Public Class Featureless_DCT_MT
    Inherits ocvbClass
    Dim dct As DCT_FeatureLess_MT
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        dct = New DCT_FeatureLess_MT(ocvb)

        ocvb.desc = "Use DCT to find largest featureless region."
        label2 = "Largest FeatureLess Region"
    End Sub

    Public Sub Run(ocvb As AlgorithmData)
        dct.src = src
        dct.Run(ocvb)
        dst1 = dct.dst1
        dst2 = dct.dst2

        Dim mask = dst1.Clone()
        Dim objectSize As New List(Of Int32)
        Dim regionCount = 1
        For y = 0 To mask.Rows - 1
            For x = 0 To mask.Cols - 1
                If mask.Get(Of Byte)(y, x) = 255 Then
                    Dim pt As New cv.Point(x, y)
                    Dim floodCount = mask.FloodFill(pt, regionCount)
                    objectSize.Add(floodCount)
                    regionCount += 1
                End If
            Next
        Next

        Dim maxSize As Int32, maxIndex As Int32
        For i = 0 To objectSize.Count - 1
            If maxSize < objectSize.ElementAt(i) Then
                maxSize = objectSize.ElementAt(i)
                maxIndex = i
            End If
        Next

        Dim label = mask.InRange(maxIndex + 1, maxIndex + 1)
        Dim nonZ = label.CountNonZero()
        label2 = "Largest FeatureLess Region (" + CStr(nonZ) + " " + Format(nonZ / label.Total, "#0.0%") + " pixels)"
        dst2.SetTo(cv.Scalar.White, label)
    End Sub
End Class

