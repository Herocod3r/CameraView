using System;
using System.Threading.Tasks;
using CameraView.Abstractions;

using Xamarin.Forms;

namespace CameraView
{
    public class CameraPreviewerView : View
    {
        public CameraPreviewerView()
        {

        }



        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
                    "Camera", typeof(CameraType), typeof(CameraPreviewerView), CameraType.Rear);


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

        }



    }
}

