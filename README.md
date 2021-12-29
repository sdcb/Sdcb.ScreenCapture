# Sdcb.ScreenCapture [![QQ](https://img.shields.io/badge/QQ_Group-579060605-52B6EF?style=social&logo=tencent-qq&logoColor=000&logoWidth=20)](https://jq.qq.com/?_wv=1027&k=K4fBqpyQ)

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
