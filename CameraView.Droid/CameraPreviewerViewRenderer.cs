using System;
using Android.App;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CameraView.CameraPreviewerView), typeof(CameraView.Droid.CameraPreviewerViewRenderer))]
namespace CameraView.Droid
{
    public class CameraPreviewerViewRenderer : ViewRenderer<CameraPreviewerView, DroidCameraView>
    {
        private DroidCameraView cameraPreview;

        public CameraPreviewerViewRenderer(Context context) : base(context)
        {
        }


        public static void Init()
        {
            // Keep linker from stripping empty method
            var temp = DateTime.Now;
        }

        protected async override void OnElementChanged(ElementChangedEventArgs<CameraPreviewerView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {


                if (Context is Activity activity)
                    await PermissionsHandler.RequestPermissionsAsync(activity);


                cameraPreview = new DroidCameraView(Context);
                cameraPreview.Type = Element.Camera;
                SetNativeControl(cameraPreview);
                Element.Init(cameraPreview);
            }


        }
    }
}
