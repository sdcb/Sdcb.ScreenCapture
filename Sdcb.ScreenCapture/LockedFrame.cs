using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;

namespace Sdcb
{
    public record struct LockedBgraFrame(IntPtr DataPointer, int Length, int RowPitch)
    {
        public int Width => RowPitch / BytePerPixel;
        public int Height => Length / RowPitch;

        public const int BytePerPixel = 4;

        public static explicit operator LockedBgraFrame(MappedSubresource box) => new LockedBgraFrame(box.DataPointer, box.DepthPitch, box.RowPitch);

        public byte[] ToArray()
        {
            byte[] data = new byte[Length];
            Marshal.Copy(DataPointer, data, 0, Length);
            return data;
        }

        public ManagedBgraFrame ToManaged() => new ManagedBgraFrame(ToArray(), Length, RowPitch);
    }

    public record struct ManagedBgraFrame(byte[] Data, int Length, int RowPitch)
    {
        public int Width => RowPitch / BytePerPixel;
        public int Height => Length / RowPitch;

        public const int BytePerPixel = 4;
    }

    public static class BgraFrameExtensions
    {
        public static IEnumerable<ManagedBgraFrame> ToManaged(this IEnumerable<LockedBgraFrame> frames) => frames.Select(x => x.ToManaged());
    }
}
