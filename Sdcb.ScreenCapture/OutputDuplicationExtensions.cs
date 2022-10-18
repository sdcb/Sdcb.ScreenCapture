using SharpGen.Runtime;
using Vortice.DXGI;

namespace Sdcb
{
    internal static class OutputDuplicationExtensions
    {
        public static GrabResult Grab(this IDXGIOutputDuplication duplication, int timeoutInMilliseconds = int.MaxValue)
        {
            Result result = duplication.AcquireNextFrame(timeoutInMilliseconds,
                out OutduplFrameInfo frameInfo,
                out IDXGIResource desktopResource);
            if (result.Failure) return null;
            return new GrabResult(frameInfo, desktopResource);
        }
    }
}