Imports cv = OpenCvSharp
' https://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/laplace_operator/laplace_operator.html
Public Class Filter_Laplacian
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        ocvb.desc = "Use a filter to approximate the Laplacian derivative."
        label1 = "Sharpened image using Filter2D output"
        label2 = "Output of Filter2D (approximated Laplacian)"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim kernel = New cv.Mat(3, 3, cv.MatType.CV_32FC1, New Single() {1, 1, 1, 1, -8, 1, 1, 1, 1})
        Dim imgLaplacian = src.Filter2D(cv.MatType.CV_32F, kernel)
        Dim sharp As New cv.Mat
        src.ConvertTo(sharp, cv.MatType.CV_32F)
        Dim imgResult = (sharp - imgLaplacian).ToMat
        imgResult.ConvertTo(imgResult, cv.MatType.CV_8UC3)
        imgResult.ConvertTo(dst1, cv.MatType.CV_8UC3)
        imgLaplacian.ConvertTo(dst2, cv.MatType.CV_8UC3)
    End Sub
End Class


Public Class Filter_NormalizedKernel
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        radio.Setup(ocvb, caller, 4)
        radio.check(0).Text = "INF"
        radio.check(1).Text = "L1"
        radio.check(1).Checked = True
        radio.check(2).Text = "L2"
        radio.check(3).Text = "MinMax"
        sliders.setupTrackBar1(ocvb, caller, "Normalize alpha X10", 1, 100, 10)
        ocvb.desc = "Create a normalized kernel and use it."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim kernel = New cv.Mat(1, 21, cv.MatType.CV_32FC1, New Single() {2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1})
        Dim normType = cv.NormTypes.L1
        For i = 0 To radio.check.Count - 1
            If radio.check(i).Checked Then
                normType = Choose(i + 1, cv.NormTypes.INF, cv.NormTypes.L1, cv.NormTypes.L2, cv.NormTypes.MinMax)
                Exit For
            End If
        Next

        kernel = kernel.Normalize(sliders.TrackBar1.Value / 10, 0, normType)

        Dim sum As Double
        For i = 0 To kernel.Width - 1
            sum += Math.Abs(kernel.Get(Of Single)(0, i))
        Next
        label1 = "kernel sum = " + Format(sum, "#0.000")

        Dim dst32f = src.Filter2D(cv.MatType.CV_32FC1, kernel, anchor:=New cv.Point(0, 0))
        dst32f.ConvertTo(dst1, cv.MatType.CV_8UC3)
    End Sub
End Class


' https://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/filter_2d/filter_2d.html
Public Class Filter_Normalized2D
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        sliders.setupTrackBar1(ocvb, caller, "Filter_Normalized2D kernel size", 1, 21, 3)
        ocvb.desc = "Create and apply a normalized kernel."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim kernelSize = If(standalone, (ocvb.frameCount Mod 20) + 1, sliders.TrackBar1.Value)
        Dim kernel = New cv.Mat(kernelSize, kernelSize, cv.MatType.CV_32F).SetTo(1 / (kernelSize * kernelSize))
        dst1 = src.Filter2D(-1, kernel)
        label1 = "Normalized KernelSize = " + CStr(kernelSize)
    End Sub
End Class




'https://www.cc.gatech.edu/classes/AY2015/cs4475_summer/documents/smoothing_separable.py
Public Class Filter_SepFilter2D
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        check.Setup(ocvb, caller, 1)
        check.Box(0).Text = "Show Difference SepFilter2D and Gaussian"
        check.Box(0).Checked = True

        sliders.setupTrackBar1(ocvb, caller, "Kernel X size", 1, 21, 5)
        sliders.setupTrackBar2("Kernel Y size", 1, 21, 11)
        sliders.setupTrackBar3("SepFilter2D Sigma X10", 0, 100, 17)
        label1 = "Gaussian Blur result"
        ocvb.desc = "Apply kernel X then kernel Y with OpenCV's SepFilter2D and compare to Gaussian blur"
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim xDim = If(sliders.TrackBar1.Value Mod 2, sliders.TrackBar1.Value, sliders.TrackBar1.Value + 1)
        Dim yDim = If(sliders.TrackBar2.Value Mod 2, sliders.TrackBar2.Value, sliders.TrackBar2.Value + 1)
        Dim sigma = sliders.TrackBar3.Value / 10
        Dim kernel = cv.Cv2.GetGaussianKernel(xDim, sigma)
        dst1 = src.GaussianBlur(New cv.Size(xDim, yDim), sigma)
        dst2 = src.SepFilter2D(cv.MatType.CV_8UC3, kernel, kernel)
        If check.Box(0).Checked Then
            Dim graySep = dst2.CvtColor(cv.ColorConversionCodes.BGR2GRAY)
            Dim grayGauss = dst1.CvtColor(cv.ColorConversionCodes.BGR2GRAY)
            dst2 = (graySep - grayGauss).ToMat.Threshold(0, 255, cv.ThresholdTypes.Binary)
            label2 = "Gaussian - SepFilter2D " + CStr(dst2.CountNonZero()) + " pixels different."
        Else
            label2 = "SepFilter2D Result"
        End If
    End Sub
End Class
