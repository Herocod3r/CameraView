using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CameraView.Abstractions;

using Xamarin.Forms;

namespace CameraView
{
    public class CameraPreviewerView : View
    {
        ICameraView camera;
        public CameraPreviewerView()
        {

        }




        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == nameof(IsCapturing))
            {
                if (IsCapturing) camera.StartAsync();
                else camera.StopAsync();
            }

        }



        public void Init(ICameraView native)
        {
            if (native is null) throw new ArgumentException("value cannot be null");
            if (camera != null) throw new ArgumentException("Camera is already initialized");


            camera = native;
        }

        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
                    "Camera", typeof(CameraType), typeof(CameraPreviewerView), CameraType.Back);


        public CameraType Camera
        {
            get { return (CameraType)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }



        public static readonly BindableProperty IsCapturingProperty = BindableProperty.Create(
                    "IsCapturing", typeof(bool), typeof(CameraPreviewerView), false);


        public bool IsCapturing
        {
            get { return (bool)GetValue(IsCapturingProperty); }
            set { SetValue(IsCapturingProperty, value); }
        }


        public Task<byte[]> CaptureAsync()
        {
            return camera.SnapAsync();
        }



    }
}

