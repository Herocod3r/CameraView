using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Java.Interop;
using Camera = Android.Hardware.Camera;

namespace CameraView.Droid
{
    internal class CameraController
    {
        private readonly SurfaceView surfaceView;
        private readonly Context _context;
        private ISurfaceHolder _holder;
        private int _cameraId;

        public CameraController(SurfaceView surfaceView)
        {
            this.surfaceView = surfaceView;
            _context = surfaceView.Context;
            _holder = surfaceView.Holder;
           
        }



        public Camera Camera { get; private set; }

        public int LastCameraDisplayOrientationDegree { get; private set; }

        //public void RefreshCamera()
        //{
        //    if (_holder == null) return;

        //    ApplyCameraSettings();

        //    try
        //    {
        //        Camera.SetPreviewDisplay(_holder);
        //        Camera.StartPreview();
        //    }
        //    catch (Exception ex)
        //    {
        //        Android.Util.Log.Debug(nameof(CameraController), ex.ToString());
        //    }
        //}




        public void SetupCamera(CameraFacing facing)
        {
            if (Camera != null) return;

            PermissionsHandler.CheckCameraPermissions(_context);


            OpenCamera(facing);


            if (Camera == null) return;


            ApplyCameraSettings();

            try
            {
                Camera.SetPreviewDisplay(_holder);


                //var previewParameters = Camera.GetParameters();
                //var previewSize = previewParameters.PreviewSize;
                //var bitsPerPixel = ImageFormat.GetBitsPerPixel(previewParameters.PreviewFormat);


                //int bufferSize = (previewSize.Width * previewSize.Height * bitsPerPixel) / 8;
                //const int NUM_PREVIEW_BUFFERS = 5;
                //for (uint i = 0; i < NUM_PREVIEW_BUFFERS; ++i)
                //{
                //    using (var buffer = new FastJavaByteArray(bufferSize))
                //        Camera.AddCallbackBuffer(buffer);
                //}



                Camera.StartPreview();

               // Camera.SetNonMarshalingPreviewCallback(_cameraEventListener);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Debug(nameof(CameraController), ex.ToString());
                return;
            }

            // Docs suggest if Auto or Macro modes, we should invoke AutoFocus at least once
            var currentFocusMode = Camera.GetParameters().FocusMode;
            if (currentFocusMode == Camera.Parameters.FocusModeAuto
                || currentFocusMode == Camera.Parameters.FocusModeMacro)
                AutoFocus();
        }




        public void AutoFocus()
        {
            AutoFocus(0, 0, false);
        }

        public void AutoFocus(int x, int y)
        {
            // The bounds for focus areas are actually -1000 to 1000
            // So we need to translate the touch coordinates to this scale
            var focusX = x / surfaceView.Width * 2000 - 1000;
            var focusY = y / surfaceView.Height * 2000 - 1000;

            // Call the autofocus with our coords
            AutoFocus(focusX, focusY, true);
        }


        private void AutoFocus(int x, int y, bool useCoordinates)
        {
            if (Camera == null) return;

           
            var cameraParams = Camera.GetParameters();

            //Android.Util.Log.Debug(MobileBarcodeScanner.TAG, "AutoFocus Requested");

            // Cancel any previous requests
            Camera.CancelAutoFocus();

            try
            {
                // If we want to use coordinates
                // Also only if our camera supports Auto focus mode
                // Since FocusAreas only really work with FocusModeAuto set
                if (useCoordinates
                    && cameraParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeAuto))
                {
                    // Let's give the touched area a 20 x 20 minimum size rect to focus on
                    // So we'll offset -10 from the center of the touch and then 
                    // make a rect of 20 to give an area to focus on based on the center of the touch
                    x = x - 10;
                    y = y - 10;

                    // Ensure we don't go over the -1000 to 1000 limit of focus area
                    if (x >= 1000)
                        x = 980;
                    if (x < -1000)
                        x = -1000;
                    if (y >= 1000)
                        y = 980;
                    if (y < -1000)
                        y = -1000;

                    // Explicitly set FocusModeAuto since Focus areas only work with this setting
                    cameraParams.FocusMode = Camera.Parameters.FocusModeAuto;
                    // Add our focus area
                    cameraParams.FocusAreas = new List<Camera.Area>
                    {
                        new Camera.Area(new Rect(x, y, x + 20, y + 20), 1000)
                    };
                    Camera.SetParameters(cameraParams);
                }

                // Finally autofocus (weather we used focus areas or not)
                //Camera.AutoFocus(_cameraEventListener);
            }
            catch (Exception ex)
            {
                //Android.Util.Log.Debug(MobileBarcodeScanner.TAG, "AutoFocus Failed: {0}", ex);
            }
        }






        public void ShutdownCamera()
        {
            if (Camera == null) return;


            try
            {
                try
                {
                    Camera.StopPreview();
                   
                  
                    //Camera.SetPreviewDisplay(null);
                }
                catch (Exception ex)
                {
                    //Android.Util.Log.Error(MobileBarcodeScanner.TAG, ex.ToString());
                }
                Camera.Release();
                Camera = null;
            }
            catch (Exception e)
            {
                //Android.Util.Log.Error(MobileBarcodeScanner.TAG, e.ToString());
            }

           
        }



        private void OpenCamera(CameraFacing whichCamera)
        {
            try
            {
                var version = Build.VERSION.SdkInt;

                if (version >= BuildVersionCodes.Gingerbread)
                {

                    var numCameras = Camera.NumberOfCameras;
                    var camInfo = new Camera.CameraInfo();
                    var found = false;

                   

                  

                    for (var i = 0; i < numCameras; i++)
                    {
                        Camera.GetCameraInfo(i, camInfo);
                        if (camInfo.Facing == whichCamera)
                        {

                            Camera = Camera.Open(i);
                            _cameraId = i;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {

                        Camera = Camera.Open(0);
                        _cameraId = 0;
                    }
                }
                else
                {
                    Camera = Camera.Open();
                }

                //if (Camera != null)
                //    Camera.SetPreviewCallback(_cameraEventListener);
                //else
                //    MobileBarcodeScanner.LogWarn(MobileBarcodeScanner.TAG, "Camera is null :(");
            }
            catch (Exception ex)
            {
                ShutdownCamera();
               // MobileBarcodeScanner.LogError("Setup Error: {0}", ex);
            }
        }

        private void ApplyCameraSettings()
        {
            if (Camera == null)
            {
                OpenCamera( CameraFacing.Back);
            }

            // do nothing if something wrong with camera
            if (Camera == null) return;

            var parameters = Camera.GetParameters();
            parameters.PreviewFormat = ImageFormatType.Nv21;

            var supportedFocusModes = parameters.SupportedFocusModes;
           
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich &&
                supportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousPicture))
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
            else if (supportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
            else if (supportedFocusModes.Contains(Camera.Parameters.FocusModeAuto))
                parameters.FocusMode = Camera.Parameters.FocusModeAuto;
            else if (supportedFocusModes.Contains(Camera.Parameters.FocusModeFixed))
                parameters.FocusMode = Camera.Parameters.FocusModeFixed;

            var selectedFps = parameters.SupportedPreviewFpsRange.FirstOrDefault();
            if (selectedFps != null)
            {
                // This will make sure we select a range with the lowest minimum FPS
                // and maximum FPS which still has the lowest minimum
                // This should help maximize performance / support for hardware
                foreach (var fpsRange in parameters.SupportedPreviewFpsRange)
                {
                    if (fpsRange[0] <= selectedFps[0] && fpsRange[1] > selectedFps[1])
                        selectedFps = fpsRange;
                }
                parameters.SetPreviewFpsRange(selectedFps[0], selectedFps[1]);
            }



            Camera.SetParameters(parameters);

            SetCameraDisplayOrientation();
        }



        private void SetCameraDisplayOrientation()
        {
            var degrees = GetCameraDisplayOrientation();
            LastCameraDisplayOrientationDegree = degrees;

            //Android.Util.Log.Debug(MobileBarcodeScanner.TAG, "Changing Camera Orientation to: " + degrees);

            try
            {
                Camera.SetDisplayOrientation(degrees);
            }
            catch (Exception ex)
            {
               // Android.Util.Log.Error(MobileBarcodeScanner.TAG, ex.ToString());
            }
        }

        private int GetCameraDisplayOrientation()
        {
            int degrees;
            var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            var display = windowManager.DefaultDisplay;
            var rotation = display.Rotation;

            switch (rotation)
            {
                case SurfaceOrientation.Rotation0:
                    degrees = 0;
                    break;
                case SurfaceOrientation.Rotation90:
                    degrees = 90;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 270;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var info = new Camera.CameraInfo();
            Camera.GetCameraInfo(_cameraId, info);

            int correctedDegrees;
            if (info.Facing == CameraFacing.Front)
            {
                correctedDegrees = (info.Orientation + degrees) % 360;
                correctedDegrees = (360 - correctedDegrees) % 360; // compensate the mirror
            }
            else
            {
                // back-facing
                correctedDegrees = (info.Orientation - degrees + 360) % 360;
            }

            return correctedDegrees;
        }


    }
}
