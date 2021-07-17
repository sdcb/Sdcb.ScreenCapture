# Sdcb.ScreenCapture

DXGI based screen capture, with no dependency of Bitmap, **zero-copy**, raw output only.

Suitable for high performance/real time screen capture/video recording.

## Usage
```csharp
// with LINQPad6 & OpenCVSharp4
var dc = new DumpContainer().Dump();
foreach (LockedFrame frame in ScreenCapture.CaptureScreenFrames(screenId: 0))
{
	using Mat mat = new Mat(frame.Height, frame.Width, MatType.CV_8UC4, frame.DataPointer);
	dc.Content = Util.Image(mat.ToBytes(".jpg"));
	Thread.Sleep(200);
}
```