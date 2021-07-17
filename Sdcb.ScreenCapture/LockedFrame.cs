using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Sdcb
{
    public record LockedFrame
    {
        public IntPtr DataPointer;

        public int Length, RowPitch;
        public int Width => RowPitch / BytePerPixel;
        public int Height => Length / RowPitch;

        public const int BytePerPixel = 4;

        public static explicit operator LockedFrame(DataBox box)
        {
            return new LockedFrame
            {
                DataPointer = box.DataPointer, 
                Length = box.SlicePitch, 
                RowPitch = box.RowPitch,  
            };
        }

        public byte[] GetArray()
        {
            byte[] data = new byte[Length];
            GCHandle pin = GCHandle.Alloc(data, GCHandleType.Pinned);
            Utilities.CopyMemory(pin.AddrOfPinnedObject(), DataPointer, Length);
            pin.Free();
            return data;
        }
    }
}
