Imports cv = OpenCvSharp

' http://opencvexamples.blogspot.com/
Public Class WarpPerspective_Basics
    Inherits ocvbClass
    Public Sub New(ocvb As AlgorithmData)
        setCaller(ocvb)
        sliders.setupTrackBar1(ocvb, caller, "Warped Width", 0, ocvb.color.cols, ocvb.color.cols - 50)
        sliders.setupTrackBar2("Warped Height", 0, ocvb.color.Rows, ocvb.color.Rows - 50)
        sliders.setupTrackBar3("Warped Angle", 0, 360, 0)
        ocvb.desc = "Use WarpPerspective to transform input images."
    End Sub
    Public Sub Run(ocvb As AlgorithmData)
        Dim srcPt() = {New cv.Point2f(0, 0), New cv.Point2f(0, src.Height), New cv.Point2f(src.Width, 0), New cv.Point2f(src.Width, src.Height)}
        Dim pts() = {New cv.Point2f(0, 0), New cv.Point2f(0, src.Height), New cv.Point2f(src.Width, 0), New cv.Point2f(sliders.TrackBar1.Value, sliders.TrackBar2.Value)}

        Dim perpectiveTranx = cv.Cv2.GetPerspectiveTransform(srcPt, pts)
        cv.Cv2.WarpPerspective(src, dst1, perpectiveTranx, New cv.Size(src.Cols, src.Rows), cv.InterpolationFlags.Cubic, cv.BorderTypes.Constant, cv.Scalar.White)

        Dim center = New cv.Point2f(src.Cols / 2, src.Rows / 2)
        Dim angle = sliders.TrackBar3.Value
        Dim rotationMatrix = cv.Cv2.GetRotationMatrix2D(center, angle, 1.0)
        cv.Cv2.WarpAffine(dst1, dst2, rotationMatrix, src.Size(), cv.InterpolationFlags.Nearest)
    End Sub
End Class

