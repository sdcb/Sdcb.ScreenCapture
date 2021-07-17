using SharpDX;
using Dxgi = SharpDX.DXGI;

namespace Sdcb
{
    internal static class OutputDuplicationExtensions
    {
        public static GrabResult Grab(this Dxgi.OutputDuplication duplication, int timeoutInMilliseconds = int.MaxValue)
        {
            Result result = duplication.TryAcquireNextFrame(timeoutInMilliseconds,
                out Dxgi.OutputDuplicateFrameInformation frameInfo,
                out Dxgi.Resource desktopResource);
            if (result.Failure) return null;
            return new GrabResult(frameInfo, desktopResource);
        }
    }
}