Imports cv = OpenCvSharp
Public Class DCT_Basics
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        sliders.setupTrackBar1(ocvb, caller, "Remove Frequencies < x", 0, 100, 1)
        sliders.setupTrackBar2("Run Length Minimum", 1, 100, 15)
        sliders.GroupBox2.Visible = False
        radio.Setup(ocvb, caller, 3)
        radio.check(0).Text = "DCT Flags None"
        radio.check(1).Text = "DCT Flags Row"
        radio.check(2).Text = "DCT Flags Inverse"
        radio.check(0).Checked = True

        ocvb.desc = "Apply OpenCV's Discrete Cosine Transform to a grayscale image and use slider to remove the highest frequencies."
        label2 = "Difference from original"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        If src.Channels = 3 Then src = src.CvtColor(cv.ColorConversionCodes.BGR2GRAY)
        Dim frequencies As New cv.Mat
        Dim src32f As New cv.Mat
        src.ConvertTo(src32f, cv.MatType.CV_32F, 1 / 255)
        Dim dctFlag As cv.DctFlags
        For i = 0 To 2
            If radio.check(i).Checked Then
                dctFlag = Choose(i + 1, cv.DctFlags.None, cv.DctFlags.Rows, cv.DctFlags.Inverse)
            End If
        Next
        cv.Cv2.Dct(src32f, frequencies, dctFlag)

        Dim roi As New cv.Rect(0, 0, sliders.TrackBar1.Value, src32f.Height)
        If roi.Width > 0 Then frequencies(roi).SetTo(0)
        label1 = "Highest " + CStr(sliders.TrackBar1.Value) + " frequencies removed"

        cv.Cv2.Dct(frequencies, src32f, cv.DctFlags.Inverse)
        src32f.ConvertTo(dst1, cv.MatType.CV_8UC1, 255)

        cv.Cv2.Subtract(src, dst1, dst2)
    End Sub
End Class





Public Class DCT_RGB
    Inherits ocvbClass
    Public dct As DCT_Basics
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        dct = New DCT_Basics(ocvb)
        dct.sliders.TrackBar1.Value = 1
        dct.sliders.GroupBox2.Visible = False ' no runlenmin

        label1 = "Reconstituted RGB image"
        label2 = "Difference from original"
        ocvb.desc = "Apply OpenCV's Discrete Cosine Transform to an RGB image and use slider to remove the highest frequencies."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim srcPlanes() As cv.Mat = Nothing
        cv.Cv2.Split(src, srcPlanes)

        Dim dctFlag As cv.DctFlags
        For i = 0 To 2
            If dct.radio.check(i).Checked Then
                dctFlag = Choose(i + 1, cv.DctFlags.None, cv.DctFlags.Rows, cv.DctFlags.Inverse)
            End If
        Next

        Dim freqPlanes(2) As cv.Mat
        For i = 0 To srcPlanes.Count - 1
            Dim src32f As New cv.Mat
            srcPlanes(i).ConvertTo(src32f, cv.MatType.CV_32FC3, 1 / 255)
            freqPlanes(i) = New cv.Mat
            cv.Cv2.Dct(src32f, freqPlanes(i), cv.DctFlags.None)

            Dim roi As New cv.Rect(0, 0, dct.sliders.TrackBar1.Value, src32f.Height)
            If roi.Width > 0 Then freqPlanes(i)(roi).SetTo(0)

            cv.Cv2.Dct(freqPlanes(i), src32f, dctFlag)
            src32f.ConvertTo(srcPlanes(i), cv.MatType.CV_8UC1, 255)
        Next
        label1 = "Highest " + CStr(dct.sliders.TrackBar1.Value) + " frequencies removed"

        cv.Cv2.Merge(srcPlanes, dst1)

        cv.Cv2.Subtract(src, dst1, dst2)
    End Sub
End Class




Public Class DCT_Depth
    Inherits ocvbClass
    Public dct As DCT_Basics
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        dct = New DCT_Basics(ocvb)
        dct.sliders.TrackBar1.Value = 1
        dct.sliders.GroupBox2.Visible = False ' no runlenmin
        label2 = "Subtract DCT inverse from Grayscale depth"
        ocvb.desc = "Find featureless surfaces in the depth data - expected to be useful only on the Kinect for Azure camera."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim gray = ocvb.RGBDepth.CvtColor(cv.ColorConversionCodes.BGR2GRAY)
        Dim frequencies As New cv.Mat
        Dim src32f As New cv.Mat
        gray.ConvertTo(src32f, cv.MatType.CV_32F, 1 / 255)
        cv.Cv2.Dct(src32f, frequencies, cv.DctFlags.None)

        Dim roi As New cv.Rect(0, 0, dct.sliders.TrackBar1.Value, src32f.Height)
        If roi.Width > 0 Then frequencies(roi).SetTo(0)
        label1 = "Highest " + CStr(dct.sliders.TrackBar1.Value) + " frequencies removed"

        cv.Cv2.Dct(frequencies, src32f, cv.DctFlags.Inverse)
        src32f.ConvertTo(dst1, cv.MatType.CV_8UC1, 255)

        cv.Cv2.Subtract(gray, dst1, dst2)
    End Sub
End Class





Public Class DCT_FeatureLess_MT
    Inherits ocvbClass
    Public dct As DCT_Basics
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)

        dct = New DCT_Basics(ocvb)
        dct.sliders.TrackBar1.Value = 1
        ocvb.desc = "Find surfaces that lack any texture.  Remove just the highest frequency from the DCT to get horizontal lines through the image."
        label2 = "FeatureLess RGB regions"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        dct.src = src
        dct.Run(ocvb)
        Dim runLenMin = dct.sliders.TrackBar2.Value
        dst1 = dct.dst1
        dst2 = dct.dst2

        ' Result2 contain the RGB image with highest frequency removed.
        Parallel.For(0, dst2.Rows,
        Sub(i)
            Dim runLen As Int32 = 0
            Dim runStart As Int32 = 0
            For j = 1 To dst2.Cols - 1
                If dst2.Get(Of Byte)(i, j) = dst2.Get(Of Byte)(i, j - 1) Then
                    runLen += 1
                Else
                    If runLen > runLenMin Then
                        Dim roi = New cv.Rect(runStart, i, runLen, 1)
                        dst1(roi).SetTo(255)
                    End If
                    runStart = j
                    runLen = 1
                End If
            Next
        End Sub)
        dst2.SetTo(0)
        If dst1.Channels = 3 Then
            dst1 = dst1.CvtColor(cv.ColorConversionCodes.BGR2GRAY).Threshold(1, 255, cv.ThresholdTypes.Binary)
        Else
            dst1 = dst1.Threshold(1, 255, cv.ThresholdTypes.Binary)
        End If
        src.CopyTo(dst2, dst1)
        label1 = "Mask of DCT with highest frequency removed"
    End Sub
End Class





Public Class DCT_Surfaces_debug
    Inherits ocvbClass
    Dim Mats As Mat_4to1
    Dim grid As Thread_Grid
    Dim dct As DCT_FeatureLess_MT
    Dim flow As Font_FlowText
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        flow = New Font_FlowText(ocvb)
        flow.result1or2 = RESULT1

        grid = New Thread_Grid(ocvb)
        grid.sliders.TrackBar1.Value = 100
        grid.sliders.TrackBar2.Value = 150
        dct = New DCT_FeatureLess_MT(ocvb)
        dct.dct.sliders.TrackBar1.Value = 1
        Mats = New Mat_4to1(ocvb)

        label1 = "Largest flat surface segment stats"
        label2 = "Lower right image identifies potential flat surface"
        ocvb.desc = "Find plane equation for a featureless surface - debugging one region for now."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        grid.Run(ocvb)

        Mats.mat(0) = src.Clone
        Mats.mat(0).SetTo(cv.Scalar.White, grid.gridMask)

        dct.src = src
        dct.Run(ocvb)
        Mats.mat(1) = dct.dst1.CvtColor(cv.ColorConversionCodes.GRAY2BGR).Clone()
        Mats.mat(2) = dct.dst2.Clone()

        Dim mask = dct.dst1.Clone() ' result1 contains the DCT mask of featureless surfaces.
        Dim notMask As New cv.Mat
        cv.Cv2.BitwiseNot(mask, notMask)
        Dim depth32f = getDepth32f(ocvb)
        depth32f.SetTo(0, notMask) ' remove non-featureless surface depth data.

        ' find the most featureless roi
        Dim maxIndex As Int32
        Dim roiCounts(grid.roiList.Count - 1)
        For i = 0 To grid.roiList.Count - 1
            roiCounts(i) = mask(grid.roiList(i)).CountNonZero()
            If roiCounts(i) > roiCounts(maxIndex) Then maxIndex = i
        Next

        Mats.mat(3) = New cv.Mat(src.Size(), cv.MatType.CV_8UC3, 0)
        src(grid.roiList(maxIndex)).CopyTo(Mats.mat(3)(grid.roiList(maxIndex)), mask(grid.roiList(maxIndex)))
        Mats.Run(ocvb)
        dst2 = Mats.dst1

        Dim world As New cv.Mat(src.Size(), cv.MatType.CV_32FC3, 0)
        Dim roi = grid.roiList(maxIndex) ' this is where the debug comes in.  We just want to look at one region which hopefully is a single plane.
        If roi.X = grid.roiList(maxIndex).X And roi.Y = grid.roiList(maxIndex).Y Then
            If roiCounts(maxIndex) > roi.Width * roi.Height / 4 Then
                Dim worldPoints As New List(Of cv.Point3f)
                Dim minDepth = Single.MaxValue, maxDepth = Single.MinValue
                For j = 0 To roi.Height - 1
                    For i = 0 To roi.Width - 1
                        Dim nextD = depth32f(roi).Get(Of Single)(j, i)
                        If nextD <> 0 Then
                            If minDepth > nextD Then minDepth = nextD
                            If maxDepth < nextD Then maxDepth = nextD
                            Dim wpt = New cv.Point3f(roi.X + i, roi.Y + j, nextD)
                            worldPoints.Add(getWorldCoordinates(ocvb, wpt))
                        End If
                    Next
                Next
                Dim plane = computePlaneEquation(worldPoints)
                If Single.IsNaN(plane.Item0) = False Then
                    flow.msgs.Add("a=" + Format(plane.Item0, "#0.00") + " b=" + Format(plane.Item1, "#0.00") + " c=" + Format(Math.Abs(plane.Item2), "#0.00") +
                              vbTab + "depth=" + Format(-plane.Item3 / 1000, "#0.00") + "m" + vbTab + "roi.x = " + Format(roi.X, "000") + vbTab + " roi.y = " +
                              Format(roi.Y, "000") + vbTab + "MinDepth = " + Format(minDepth / 1000, "#0.00") + "m" + vbTab + " MaxDepth = " + Format(maxDepth / 1000, "#0.00") + "m")
                End If
            End If
        End If
        flow.Run(ocvb)
    End Sub
End Class




Public Class DCT_CComponents
    Inherits ocvbClass
    Dim dct As DCT_FeatureLess_MT
    Dim cc As CComp_ColorDepth
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        dct = New DCT_FeatureLess_MT(ocvb)
        cc = New CComp_ColorDepth(ocvb)

        label1 = "DCT masks colorized with average depth."
        label2 = "DCT mask"
        ocvb.desc = "Find surfaces that lack texture with DCT (Discrete Cosine Transform) and use connected components to isolate those surfaces."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        dct.src = src
        dct.Run(ocvb)

        cc.src = dct.dst1.Clone()
        cc.Run(ocvb)
        dst1 = cc.dst1
        dst2 = cc.dst2
    End Sub
End Class

