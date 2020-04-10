﻿Imports System.Windows.Controls
Imports System.Runtime.InteropServices
Imports cv = OpenCvSharp
Module Zed2_Interface
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2Open(width As Int32, height As Int32, fps As Int32) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2SerialNumber(cPtr As IntPtr) As Int32
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2intrinsicsLeft(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2intrinsicsRight(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2Acceleration(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2AngularVelocity(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2IMU_Temperature(cPtr As IntPtr) As Single
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2IMU_TimeStamp(cPtr As IntPtr) As Double
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2IMU_Barometer(cPtr As IntPtr) As Single
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2Orientation(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2IMU_Magnetometer(cPtr As IntPtr) As IntPtr
    End Function



    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Sub Zed2WaitForFrame(cPtr As IntPtr)
    End Sub
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Sub Zed2GetData(cPtr As IntPtr)
    End Sub
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2PoseData(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2Color(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2RGBDepth(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2Depth(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2PointCloud(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2GetPoseData(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2RightView(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2LeftView(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2ExtrinsicsRotationMatrix(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2ExtrinsicsTranslation(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2Translation(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2RotationMatrix(cPtr As IntPtr) As IntPtr
    End Function
    <DllImport(("Cam_Zed2.dll"), CallingConvention:=CallingConvention.Cdecl)>
    Public Function Zed2RotationVector(cPtr As IntPtr) As IntPtr
    End Function
End Module
Public Class CameraZED2
    Inherits Camera

    Structure intrinsicsLeftZed
        Dim fx As Single ' Focal length x */
        Dim fy As Single ' Focal length y */
        Dim cx As Single ' Principal point In image, x */
        Dim cy As Single ' Principal point In image, y */
        Dim k1 As Double ' Distortion factor :  [ k1, k2, p1, p2, k3 ]. Radial (k1,k2,k3) And Tangential (p1,p2) distortion
        Dim k2 As Double
        Dim p1 As Double
        Dim p2 As Double
        Dim k3 As Double
        Dim v_fov As Single ' vertical field of view in degrees.
        Dim h_fov As Single ' horizontal field of view in degrees.
        Dim d_fov As Single ' diagonal field of view in degrees.
        Dim width As Int64
        Dim height As Int64
    End Structure

    Public Sub initialize(fps As Int32, width As Int32, height As Int32)
        cPtr = Zed2Open(width, height, 60)
        deviceName = "StereoLabs ZED 2"
        IMU_Present = True
        If cPtr <> 0 Then
            Dim serialNumber = Zed2SerialNumber(cPtr)
            Console.WriteLine("ZED 2 serial number = " + CStr(serialNumber))
            w = width
            h = height

            ReDim Extrinsics_VB.rotation(9 - 1)
            Dim ptr = Zed2ExtrinsicsRotationMatrix(cPtr)
            Marshal.Copy(ptr, Extrinsics_VB.rotation, 0, Extrinsics_VB.rotation.Length)

            ReDim Extrinsics_VB.translation(3 - 1)
            ptr = Zed2ExtrinsicsTranslation(cPtr)
            Marshal.Copy(ptr, Extrinsics_VB.translation, 0, Extrinsics_VB.translation.Length)

            ptr = Zed2intrinsicsLeft(cPtr)
            Dim intrinsics = Marshal.PtrToStructure(Of intrinsicsLeftZed)(ptr)
            intrinsicsLeft_VB.ppx = intrinsics.cx
            intrinsicsLeft_VB.ppy = intrinsics.cy
            intrinsicsLeft_VB.fx = intrinsics.fx
            intrinsicsLeft_VB.fy = intrinsics.fy
            ReDim intrinsicsLeft_VB.FOV(2)
            intrinsicsLeft_VB.FOV(0) = intrinsics.v_fov
            intrinsicsLeft_VB.FOV(1) = intrinsics.h_fov
            intrinsicsLeft_VB.FOV(2) = intrinsics.d_fov
            ReDim intrinsicsLeft_VB.coeffs(5)
            intrinsicsLeft_VB.coeffs(0) = intrinsics.k1
            intrinsicsLeft_VB.coeffs(1) = intrinsics.k2
            intrinsicsLeft_VB.coeffs(2) = intrinsics.p1
            intrinsicsLeft_VB.coeffs(3) = intrinsics.p2
            intrinsicsLeft_VB.coeffs(4) = intrinsics.k3
            intrinsicsLeft_VB.width = intrinsics.width
            intrinsicsLeft_VB.height = intrinsics.height

            ptr = Zed2intrinsicsRight(cPtr)
            intrinsics = Marshal.PtrToStructure(Of intrinsicsLeftZed)(ptr)
            intrinsicsRight_VB.ppx = intrinsics.cx
            intrinsicsRight_VB.ppy = intrinsics.cy
            intrinsicsRight_VB.fx = intrinsics.fx
            intrinsicsRight_VB.fy = intrinsics.fy
            ReDim intrinsicsRight_VB.FOV(2)
            intrinsicsRight_VB.FOV(0) = intrinsics.v_fov
            intrinsicsRight_VB.FOV(1) = intrinsics.h_fov
            intrinsicsRight_VB.FOV(2) = intrinsics.d_fov
            ReDim intrinsicsRight_VB.coeffs(5)
            intrinsicsRight_VB.coeffs(0) = intrinsics.k1
            intrinsicsRight_VB.coeffs(1) = intrinsics.k2
            intrinsicsRight_VB.coeffs(2) = intrinsics.p1
            intrinsicsRight_VB.coeffs(3) = intrinsics.p2
            intrinsicsRight_VB.coeffs(4) = intrinsics.k3
            intrinsicsRight_VB.width = intrinsics.width
            intrinsicsRight_VB.height = intrinsics.height
        End If
    End Sub

    Public Sub GetNextFrame()
        If pipelineClosed Or cPtr = 0 Then Exit Sub
        Zed2WaitForFrame(cPtr)

        SyncLock bufferLock
            Zed2GetData(cPtr)

            color = New cv.Mat(h, w, cv.MatType.CV_8UC3, Zed2Color(cPtr))
            RGBDepth = New cv.Mat(h, w, cv.MatType.CV_8UC3, Zed2RGBDepth(cPtr))
            leftView = New cv.Mat(h, w, cv.MatType.CV_8UC1, Zed2LeftView(cPtr))
            rightView = New cv.Mat(h, w, cv.MatType.CV_8UC1, Zed2RightView(cPtr))
            pointCloud = New cv.Mat(h, w, cv.MatType.CV_32FC3, Zed2PointCloud(cPtr))

            Dim imuFrame = Zed2GetPoseData(cPtr)
            Dim acc = Zed2Acceleration(cPtr)
            IMU_Acceleration = Marshal.PtrToStructure(Of cv.Point3f)(acc)
            IMU_Acceleration.Y *= -1 ' make it consistent with the other cameras.

            Dim ang = Zed2AngularVelocity(cPtr)
            IMU_AngularVelocity = Marshal.PtrToStructure(Of cv.Point3f)(ang)
            IMU_AngularVelocity *= 0.0174533 ' Zed 2 gyro is in degrees/sec
            IMU_AngularVelocity.Z *= -1 ' make it consistent with the other cameras.

            Dim rt = Marshal.PtrToStructure(Of imuDataStruct)(imuFrame)
            Dim t = New cv.Point3f(rt.tx, rt.ty, rt.tz)
            Dim mat() As Single = {-rt.r00, rt.r01, -rt.r02, 0.0,
                                   -rt.r10, rt.r11, rt.r12, 0.0,
                                   -rt.r20, rt.r21, -rt.r22, 0.0,
                                    t.X, t.Y, t.Z, 1.0}
            transformationMatrix = mat

            Dim rot = Zed2RotationMatrix(cPtr)
            Marshal.Copy(rot, IMU_RotationMatrix, 0, IMU_RotationMatrix.Length)

            Dim vec = Zed2RotationVector(cPtr)
            IMU_RotationVector = Marshal.PtrToStructure(Of cv.Point3f)(vec)

            Dim tran = Zed2Translation(cPtr)
            IMU_Translation = Marshal.PtrToStructure(Of cv.Point3f)(tran)

            IMU_Barometer = Zed2IMU_Barometer(cPtr)
            Dim mag = Zed2IMU_Magnetometer(cPtr)
            IMU_Magnetometer = Marshal.PtrToStructure(Of cv.Point3f)(mag)

            IMU_Temperature = Zed2IMU_Temperature(cPtr)

            IMU_TimeStamp = Zed2IMU_TimeStamp(cPtr)
            Static imuStartTime = IMU_TimeStamp
            IMU_TimeStamp -= imuStartTime
            MyBase.GetNextFrameCounts(IMU_FrameTime)
        End SyncLock
    End Sub
End Class
