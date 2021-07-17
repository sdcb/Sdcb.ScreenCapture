<Query Kind="Statements">
  <NuGetReference>OpenCvSharp4</NuGetReference>
  <NuGetReference>OpenCvSharp4.runtime.win</NuGetReference>
  <NuGetReference>Sdcb.ScreenCapture</NuGetReference>
  <Namespace>Sdcb</Namespace>
  <Namespace>SharpDX</Namespace>
  <Namespace>OpenCvSharp</Namespace>
</Query>

var dc = new DumpContainer().Dump();
foreach (LockedFrame frame in ScreenCapture.CaptureScreenFrames(screenId: 0))
{
	using Mat mat = new Mat(frame.Height, frame.Width, MatType.CV_8UC4, frame.DataPointer);
	dc.Content = Util.Image(mat.ToBytes(".jpg"));
	Thread.Sleep(200);
}