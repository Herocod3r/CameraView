using System;
using System.Threading.Tasks;



namespace CameraView.Abstractions
{

    public delegate void CameraFrameDelegate(byte[] frame);



    public interface ICameraView
    {
        event CameraFrameDelegate OnFrameAvailable;

        Task StartAsync();

        Task StopAsync();

        Task<byte[]> SnapAsync();

        CameraType Type { get; }

        bool IsRunning { get; }

    }
}
