using System;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Sdcb
{
    internal record GrabResult : IDisposable
    {
        public OutduplFrameInfo FrameInfo { get; }
        public IDXGIResource Resource { get; }

        public ID3D11Texture2D AsTexture2D() => Resource.QueryInterface<ID3D11Texture2D>();

        public void Dispose()
        {
            Resource?.Dispose();
        }

        public GrabResult(OutduplFrameInfo frameInfo, IDXGIResource resource)
        {
            FrameInfo = frameInfo;
            Resource = resource;
        }
    }
}