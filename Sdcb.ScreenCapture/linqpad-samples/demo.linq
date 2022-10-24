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

foreach (LockedBgraFrame frame in ScreenCapture.CaptureScreenFrames(screenId: 0))
{
	using Mat mat = new Mat(frame.Height, frame.Width, MatType.CV_8UC4, frame.DataPointer);
	dc.Content = Util.Image(mat.ToBytes(".png"), Util.ScaleMode.Unscaled);
	Thread.Sleep(200);
}