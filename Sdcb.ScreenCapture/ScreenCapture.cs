using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using D3D11 = SharpDX.Direct3D11;
using Dxgi = SharpDX.DXGI;

namespace Sdcb
{
    public static class ScreenCapture
    {
        private static RawRectangle GetScreenSize(int screenId)
        {
            using var factory = new Dxgi.Factory1();
            using Dxgi.Adapter adapter = factory.GetAdapter1(0);
            using Dxgi.Output output = adapter.GetOutput(screenId);

            return output.Description.DesktopBounds;
        }

        public static IEnumerable<LockedFrame> CaptureScreenFrames(int screenId, CancellationToken cancellationToken = default)
        {
            using var factory = new Dxgi.Factory1();
            using Dxgi.Adapter adapter = factory.GetAdapter1(0);
            using var device = new D3D11.Device(adapter);
            using Dxgi.Output output = adapter.GetOutput(screenId);
            using Dxgi.Output1 output1 = output.QueryInterface<Dxgi.Output1>();

            RawRectangle bounds = output1.Description.DesktopBounds;
            var textureDesc = new D3D11.Texture2DDescription
            {
                CpuAccessFlags = D3D11.CpuAccessFlags.Read,
                BindFlags = D3D11.BindFlags.None,
                Format = Dxgi.Format.B8G8R8A8_UNorm,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
                OptionFlags = D3D11.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = D3D11.ResourceUsage.Staging
            };

            using Dxgi.OutputDuplication duplication = output1.DuplicateOutput(device);
            while (!cancellationToken.IsCancellationRequested)
            {
                using GrabResult frame = duplication.Grab(20);
                if (frame != null && frame.Resource != null && !cancellationToken.IsCancellationRequested)
                {
                    using D3D11.Texture2D currentFrame = new D3D11.Texture2D(device, textureDesc);

                    using (D3D11.Texture2D rawTexture2d = frame.GetTexture2D())
                    {
                        device.ImmediateContext.CopyResource(rawTexture2d, currentFrame);
                    }
                    DataBox dataBox = device.ImmediateContext.MapSubresource(currentFrame, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
                    yield return (LockedFrame)dataBox;
                    duplication.ReleaseFrame();
                }
            }
        }

        public static IEnumerable<LockedFrame> CaptureScreenFrames(int screenId, double fps, CancellationToken cancellationToken = default)
        {
            double frameIntervalMs = 1000.0 / fps;

            using var factory = new Dxgi.Factory1();
            using Dxgi.Adapter adapter = factory.GetAdapter1(0);
            using var device = new D3D11.Device(adapter);
            using Dxgi.Output output = adapter.GetOutput(screenId);
            using Dxgi.Output1 output1 = output.QueryInterface<Dxgi.Output1>();

            RawRectangle bounds = output1.Description.DesktopBounds;
            var textureDesc = new D3D11.Texture2DDescription
            {
                CpuAccessFlags = D3D11.CpuAccessFlags.Read,
                BindFlags = D3D11.BindFlags.None,
                Format = Dxgi.Format.B8G8R8A8_UNorm,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
                OptionFlags = D3D11.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = D3D11.ResourceUsage.Staging
            };

            using Dxgi.OutputDuplication duplication = output1.DuplicateOutput(device);

            D3D11.Texture2D lastFrame = default;
            DataBox lastDataBox = default;
            double sleepFlag = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                using GrabResult frame = duplication.Grab(Math.Max(1, (int)(frameIntervalMs - 10)));

                if (cancellationToken.IsCancellationRequested) break;

                if (frame != null && frame.Resource != null)
                {
                    D3D11.Texture2D currentFrame = new D3D11.Texture2D(device, textureDesc);

                    using (D3D11.Texture2D rawTexture2d = frame.GetTexture2D())
                    {
                        device.ImmediateContext.CopyResource(rawTexture2d, currentFrame);
                    }
                    DataBox dataBox = device.ImmediateContext.MapSubresource(currentFrame, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);
                    yield return (LockedFrame)dataBox;
                    duplication.ReleaseFrame();

                    lastFrame?.Dispose();
                    lastFrame = currentFrame;
                    lastDataBox = dataBox;
                }
                else
                {
                    yield return (LockedFrame)lastDataBox;
                }
                sleepFlag += frameIntervalMs - sw.Elapsed.TotalMilliseconds;
                sw.Restart();
                if (sleepFlag > 15.625)
                {
                    int durationToSleep = (int)(sleepFlag - Math.IEEERemainder(sleepFlag, 15.625));
                    Thread.Sleep(durationToSleep);
                    sleepFlag -= sw.Elapsed.TotalMilliseconds;
                }
            }
            if (lastFrame != null) lastFrame.Dispose();
        }
    }
}