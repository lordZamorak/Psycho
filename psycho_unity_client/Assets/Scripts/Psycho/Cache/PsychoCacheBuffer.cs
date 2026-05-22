using System;

namespace Psycho.Cache
{
    internal sealed class PsychoCacheBuffer
    {
        private readonly byte[] data;

        public PsychoCacheBuffer(byte[] data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public int Position { get; set; }

        public int Length => data.Length;

        public int ReadUnsignedByte()
        {
            EnsureAvailable(1);
            return data[Position++] & 0xff;
        }

        public sbyte ReadSignedByte()
        {
            EnsureAvailable(1);
            return unchecked((sbyte)data[Position++]);
        }

        public int ReadUnsignedShort()
        {
            EnsureAvailable(2);
            return ((data[Position++] & 0xff) << 8) | (data[Position++] & 0xff);
        }

        public short ReadSignedShort()
        {
            int value = ReadUnsignedShort();
            return unchecked((short)value);
        }

        public int ReadTriByte()
        {
            EnsureAvailable(3);
            return ((data[Position++] & 0xff) << 16) | ((data[Position++] & 0xff) << 8) | (data[Position++] & 0xff);
        }

        public int ReadSignedSmart()
        {
            EnsureAvailable(1);
            int peek = data[Position] & 0xff;
            return peek < 128 ? ReadUnsignedByte() - 64 : ReadUnsignedShort() - 49152;
        }

        public int ReadUnsignedSmart()
        {
            EnsureAvailable(1);
            int peek = data[Position] & 0xff;
            return peek < 128 ? ReadUnsignedByte() : ReadUnsignedShort() - 32768;
        }

        private void EnsureAvailable(int byteCount)
        {
            if (Position < 0 || Position + byteCount > data.Length)
            {
                throw new InvalidOperationException($"Cache buffer read overrun at {Position} for {byteCount} bytes in {data.Length} byte payload.");
            }
        }
    }
}
