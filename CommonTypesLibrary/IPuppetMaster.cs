using System.Runtime.Remoting;

namespace CommonTypesLibrary
{
    public interface IPuppetMaster
    {
        void ReceiveUrl(string url, ObjRef objRef);
        void Log(string message);
        void ResetWaitHandle();
    }
}
