namespace VolToVoMe.Interop;
using System.Runtime.InteropServices;

public sealed class VolumeEvents : IDisposable, VolumeEvents.IAudioEndpointVolumeCallback
{
    internal struct AudioVolumeNotificationDataStruct
    {
        // compiler error CS0649
#pragma warning disable 649

        public Guid guidEventContext;

        public bool muted;

        public float masterVolume;

        // public uint channels;
        // public float channelVolume;

#pragma warning restore 649
    }

    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolumeCallback
    {
        void OnNotify(IntPtr notifyData);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);

        int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);

        int GetChannelCount(out int pnChannelCount);

        int SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);

        int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);

        int GetMasterVolumeLevel(out float pfLevelDB);

        int GetMasterVolumeLevelScalar(out float pfLevel);

        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);

        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);

        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);

        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);

        int SetMute([MarshalAs(UnmanagedType.Bool)] Boolean bMute, Guid pguidEventContext);

        int GetMute(out bool pbMute);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        int Activate(ref Guid id, int clsCtx, int activationParams, out IAudioEndpointVolume aev);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumeratorComObject { }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

    private interface IMMDeviceEnumerator
    {
        int A();
        int GetDefaultAudioEndpoint(/*DataFlow*/ int dataFlow, /*Role*/ int role, out IMMDevice endpoint);
    }

    public event Action<float, bool>? VolumeChanged;

    private readonly IAudioEndpointVolume volumeControl;

    private bool disposed = false;

    public VolumeEvents() 
    {
        var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
        Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(/*eRender*/ 0, /*eMultimedia*/ 1, out var dev));
        var epvid = typeof(IAudioEndpointVolume).GUID;

        Marshal.ThrowExceptionForHR(dev.Activate(ref epvid, /*CLSCTX_ALL*/ 23, 0, out this.volumeControl));
        Marshal.ThrowExceptionForHR(this.volumeControl.RegisterControlChangeNotify(this));
    }

    public float Volume
    {
        get
        {
            Marshal.ThrowExceptionForHR(this.volumeControl.GetMasterVolumeLevelScalar(out var level));
            return level;
        }
    }

    public bool Muted
    {
        get
        {
            Marshal.ThrowExceptionForHR(this.volumeControl.GetMute(out var result));
            return result;
        }
    }

    public void OnNotify(IntPtr notifyData)
    {
        var data = Marshal.PtrToStructure<AudioVolumeNotificationDataStruct>(notifyData);
        this.VolumeChanged?.Invoke(data.masterVolume, data.muted);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Marshal.ThrowExceptionForHR(this.volumeControl.UnregisterControlChangeNotify(this));
            disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    ~VolumeEvents()
    {
        this.Dispose();
    }
}
