using System;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;

namespace Sdcb
{
    public struct LockedFrame
    {
        public IntPtr DataPointer;

        public int Length, RowPitch;
        public int Width => RowPitch / BytePerPixel;
        public int Height => Length / RowPitch;

        public const int BytePerPixel = 4;

        public static explicit operator LockedFrame(MappedSubresource box)
        {
            return new LockedFrame
            {
                DataPointer = box.DataPointer, 
                Length = box.DepthPitch, 
                RowPitch = box.RowPitch,  
            };
        }

        public byte[] ToArray()
        {
            byte[] data = new byte[Length];
            Marshal.Copy(DataPointer, data, 0, Length);
            return data;
        }
    }
}
