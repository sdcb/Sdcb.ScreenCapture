<Query Kind="Statements">
  <NuGetReference>OpenCvSharp4</NuGetReference>
  <NuGetReference>OpenCvSharp4.runtime.win</NuGetReference>
  <NuGetReference Prerelease="true">Sdcb.ScreenCapture</NuGetReference>
  <Namespace>OpenCvSharp</Namespace>
  <Namespace>Sdcb</Namespace>
  <Namespace>Vortice.DXGI</Namespace>
  <Namespace>Vortice.Direct3D11</Namespace>
  <Namespace>Vortice.Mathematics</Namespace>
  <Namespace>SharpGen.Runtime</Namespace>
  <Namespace>Vortice.Direct3D</Namespace>
</Query>

var dc = new DumpContainer().Dump();
foreach (LockedFrame frame in ScreenCapture.CaptureScreenFrames(screenId: 0))
{
	using Mat mat = new Mat(frame.Height, frame.Width, MatType.CV_8UC4, frame.DataPointer);
	dc.Content = Util.Image(mat.ToBytes(".jpg"));
	Thread.Sleep(200);
}

static IEnumerable<LockedFrame> CaptureScreenFrames2(int screenId, int adapterId = 0, CancellationToken cancellationToken = default)
{
	using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
	factory.EnumAdapters1(adapterId, out IDXGIAdapter1 _adapter).CheckError();
	using IDXGIAdapter1 adapter = _adapter;
	D3D11.D3D11CreateDevice(adapter, Vortice.Direct3D.DriverType.Unknown, DeviceCreationFlags.None, new FeatureLevel[] { FeatureLevel.Level_11_0 }, out ID3D11Device _device).CheckError();
	using ID3D11Device device = _device;
	adapter.EnumOutputs(screenId, out IDXGIOutput _output);
	using IDXGIOutput output = _output;
	using IDXGIOutput1 output1 = output.QueryInterface<IDXGIOutput1>();

	RectI bounds = output1.Description.DesktopCoordinates;
	Texture2DDescription textureDesc = new()
	{
		CPUAccessFlags = CpuAccessFlags.Read,
		BindFlags = BindFlags.None,
		Format = Format.B8G8R8A8_UNorm,
		Width = bounds.Width,
		Height = bounds.Height,
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