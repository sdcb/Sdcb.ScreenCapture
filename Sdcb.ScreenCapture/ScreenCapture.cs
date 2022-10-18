using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Sdcb
{
    public static class ScreenCapture
    {
        public static RectI GetScreenSize(int screenId, int adapterId = 0)
        {
            using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            factory.EnumAdapters1(adapterId, out IDXGIAdapter1 _adapter).CheckError();
            using IDXGIAdapter1 adapter = _adapter;
            adapter.EnumOutputs(screenId, out IDXGIOutput _output);
            using IDXGIOutput output = _output;

            return output.Description.DesktopCoordinates;
        }

        public static IEnumerable<LockedFrame> CaptureScreenFrames(int screenId, int adapterId = 0, CancellationToken cancellationToken = default)
        {
            using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            factory.EnumAdapters1(adapterId, out IDXGIAdapter1 _adapter).CheckError();
            using IDXGIAdapter1 adapter = _adapter;
            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.None, new[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 }, out ID3D11Device _device).CheckError();
            using ID3D11Device device = _device;
            adapter.EnumOutputs(screenId, out IDXGIOutput _output);
            using IDXGIOutput output = _output;
            using IDXGIOutput1 output1 = output.QueryInterface<IDXGIOutput1>();

            RawRect bounds = output1.Description.DesktopCoordinates;
            Texture2DDescription textureDesc = new()
            {
                CPUAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
                MiscFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging,
            };
            using IDXGIOutputDuplication duplication = output1.DuplicateOutput(device);
            while (!cancellationToken.IsCancellationRequested)
            {
                using GrabResult frame = duplication.Grab(20);
                if (frame != null && frame.Resource != null && !cancellationToken.IsCancellationRequested)
                {
                    using ID3D11Texture2D currentFrame = device.CreateTexture2D(textureDesc);

                    using (ID3D11Texture2D rawTexture2d = frame.AsTexture2D())
                    {
                        device.ImmediateContext.CopyResource(rawTexture2d, currentFrame);
                    }
                    MappedSubresource dataBox = device.ImmediateContext.Map(currentFrame, 0);
                    yield return (LockedFrame)dataBox;
                    duplication.ReleaseFrame();
                }
            }
        }

        public static IEnumerable<LockedFrame> CaptureScreenFrames(int screenId, double fps, int adapterId = 0, CancellationToken cancellationToken = default)
        {
            double frameIntervalMs = 1000.0 / fps;

            using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            factory.EnumAdapters1(adapterId, out IDXGIAdapter1 _adapter).CheckError();
            using IDXGIAdapter1 adapter = _adapter;
            D3D11.D3D11CreateDevice(adapter, DriverType.Unknown, DeviceCreationFlags.None, new[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 }, out ID3D11Device _device).CheckError();
            using ID3D11Device device = _device;
            adapter.EnumOutputs(screenId, out IDXGIOutput _output);
            using IDXGIOutput output = _output;
            using IDXGIOutput1 output1 = output.QueryInterface<IDXGIOutput1>();

            RawRect bounds = output1.Description.DesktopCoordinates;
            Texture2DDescription textureDesc = new()
            {
                CPUAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = bounds.Right - bounds.Left,
                Height = bounds.Bottom - bounds.Top,
                MiscFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging,
            };

            using IDXGIOutputDuplication duplication = output1.DuplicateOutput(device);

            ID3D11Texture2D lastFrame = default;
            MappedSubresource lastDataBox = default;
            double sleepFlag = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                using GrabResult frame = duplication.Grab(Math.Max(1, (int)(frameIntervalMs - 10)));

                if (cancellationToken.IsCancellationRequested) break;

                if (frame != null && frame.Resource != null)
                {
                    ID3D11Texture2D currentFrame = device.CreateTexture2D(textureDesc);

                    using (ID3D11Texture2D rawTexture2d = frame.AsTexture2D())
                    {
                        device.ImmediateContext.CopyResource(rawTexture2d, currentFrame);
                    }
                    MappedSubresource dataBox = device.ImmediateContext.Map(currentFrame, 0);
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