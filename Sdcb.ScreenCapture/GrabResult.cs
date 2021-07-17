using System;
using D3D11 = SharpDX.Direct3D11;
using Dxgi = SharpDX.DXGI;

namespace Sdcb
{
    internal record GrabResult : IDisposable
    {
        public Dxgi.OutputDuplicateFrameInformation FrameInfo { get; }
        public Dxgi.Resource Resource { get; }

        public D3D11.Texture2D GetTexture2D() { return Resource.QueryInterface<D3D11.Texture2D>(); }

        public void Dispose()
        {
            Resource?.Dispose();
        }

        public GrabResult(Dxgi.OutputDuplicateFrameInformation frameInfo, Dxgi.Resource resource)
        {
            FrameInfo = frameInfo;
            Resource = resource;
        }
    }
}