
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CameraView.Abstractions;


using Camera = Android.Hardware.Camera;
using Android.Graphics;
using Android.Hardware;

namespace CameraView.Droid
{
    public class DroidCameraView : FrameLayout,Abstractions.ICameraView,ISurfaceHolderCallback,Camera.IPreviewCallback,Camera.IShutterCallback,Camera.IPictureCallback
    {
        private SurfaceView surfaceView;
        ISurfaceHolder holder;
        bool isPreviewing;
        bool surfaceAvaileble;
        CameraType cameraType;

        CameraController _camController;

        public DroidCameraView(Context context) :
            base(context)
        {
            Initialize(context);
        }

        public DroidCameraView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public DroidCameraView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        public CameraType Type { get => cameraType; set
            {
                cameraType = value;
                if(isPreviewing)
                {
                    StartAsync();
                }
            } 

            }

        public bool IsRunning => isPreviewing;

       

        public event CameraFrameDelegate OnFrameAvailable;
        TaskCompletionSource<byte[]> tcs;



       // Camera camera;



        public Task<byte[]> SnapAsync()
        {
            if (!isPreviewing) return null;
            tcs = new TaskCompletionSource<byte[]>();

            Android.Hardware.Camera.Parameters p = _camController.Camera.GetParameters();
            p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
            //var size = p.PreviewSize;
            _camController.Camera.SetParameters(p);
            _camController.Camera?.TakePicture(this, this, this);
            return tcs.Task;
        }

        public async Task StartAsync()
        {
            if (isPreviewing) await StopAsync();

            try
            {

                _camController.SetupCamera(ToCameraFacing(Type));
                isPreviewing = true;
                _camController.Camera?.SetPreviewCallback(this);


            }
            catch 
            {
                //ShutdownCamera();
                // MobileBarcodeScanner.LogError("Setup Error: {0}", ex);
            }


        }




        public static CameraFacing ToCameraFacing(CameraType typ)
        {
            switch (typ)
            {
                case CameraType.Back:
                    return CameraFacing.Back;
                case CameraType.Front:
                    return CameraFacing.Front;
                default:
                    return CameraFacing.Back;
            }
        }



        public async Task StopAsync()
        {
            isPreviewing = false;
            _camController.ShutdownCamera();

        }

        void Initialize(Context context)
        {

            PermissionsHandler.CheckCameraPermissions(context);
            surfaceView = new SurfaceView(context);
            AddView(surfaceView);


           
            isPreviewing = false;
            holder = surfaceView.Holder;
            holder.AddCallback(this);
            _camController = new CameraController(surfaceView);

        }




       




        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {

           // _camController?.RefreshCamera();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            surfaceAvaileble = false;
        }

        public async void SurfaceDestroyed(ISurfaceHolder holder)
        {
           await StopAsync();
        }

        public void OnPreviewFrame(byte[] data, Camera camera)
        {

            OnFrameAvailable?.Invoke(data);
        }

        void Camera.IShutterCallback.OnShutter()
        {
           // throw new NotImplementedException();
        }

        void Camera.IPictureCallback.OnPictureTaken(byte[] data, Camera camera)
        {
            tcs?.TrySetResult(data);
        }








    }
}
