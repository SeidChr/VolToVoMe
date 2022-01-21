namespace VolToVoMe.Interop;
using System;
using System.Runtime.InteropServices;

public class VoicemeeterClient : IDisposable
{
    bool disposed = false;

    public VoicemeeterClient()
    {
        VoiceMeterRemote.LoginVoicemeeter();
    }

    public void SetParameter(string parameterName, float value) 
    {
        VoiceMeterRemote.SetParameter(parameterName, value);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            try
            {
                VoiceMeterRemote.Logout();
            }
            catch (Exception)
            {
            }

            disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    ~VoicemeeterClient()
    {
        Dispose();
    }

    private static class VoiceMeterRemote
    {
        public static int LoginVoicemeeter()
        {
            return Environment.Is64BitProcess
                ? RemoteWrapper64.LoginVoicemeeter()
                : RemoteWrapper32.LoginVoicemeeter();
        }

        public static int Logout()
        {
            return Environment.Is64BitProcess
                ? RemoteWrapper64.Logout()
                : RemoteWrapper32.Logout();
        }

        public static int SetParameter(string szParamName, float value)
        {
            return Environment.Is64BitProcess
                ? RemoteWrapper64.SetParameter(szParamName, value)
                : RemoteWrapper32.SetParameter(szParamName, value);
        }

        private static class RemoteWrapper32
        {
            [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_Login", CallingConvention = CallingConvention.StdCall)]
            internal static extern int LoginVoicemeeter();

            [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_Logout")]
            internal static extern int Logout();

            // long __stdcall VBVMR_SetParameterFloat(char * szParamName, float Value);
            [DllImport("VoicemeeterRemote.dll", EntryPoint = "VBVMR_SetParameterFloat")]
            internal static extern int SetParameter(string szParamName, float value);
        }

        private static class RemoteWrapper64
        {
            [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_Login", CallingConvention = CallingConvention.StdCall)]
            internal static extern int LoginVoicemeeter();

            [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_Logout")]
            internal static extern int Logout();

            // long __stdcall VBVMR_SetParameterFloat(char * szParamName, float Value);
            [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_SetParameterFloat")]
            internal static extern int SetParameter(string szParamName, float value);
        }
    }
}



