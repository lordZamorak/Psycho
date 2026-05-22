using System;
using System.Numerics;
using System.Text;

namespace Psycho.Networking
{
    public sealed class RsBuffer
    {
        public byte[] Data { get; }
        public int Position { get; set; }

        public RsBuffer(int capacity = 5000)
        {
            Data = new byte[capacity];
        }

        public void PutByte(int value)
        {
            Data[Position++] = (byte)value;
        }

        public void PutBytes(byte[] source, int length, int offset = 0)
        {
            Buffer.BlockCopy(source, offset, Data, Position, length);
            Position += length;
        }

        public void PutUnsignedShort(int value)
        {
            Data[Position++] = (byte)(value >> 8);
            Data[Position++] = (byte)value;
        }

        public void PutInt(int value)
        {
            Data[Position++] = (byte)(value >> 24);
            Data[Position++] = (byte)(value >> 16);
            Data[Position++] = (byte)(value >> 8);
            Data[Position++] = (byte)value;
        }

        public void PutString(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value ?? string.Empty);
            PutBytes(bytes, bytes.Length);
            PutByte(10);
        }

        public static long ReadLongBigEndian(byte[] bytes, int offset)
        {
            uint high = (uint)((bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3]);
            uint low = (uint)((bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7]);
            return ((long)high << 32) | low;
        }

        public void EncryptRsaContent(string modulusText, string exponentText)
        {
            byte[] plain = new byte[Position];
            Buffer.BlockCopy(Data, 0, plain, 0, Position);

            BigInteger decoded = FromBigEndianSigned(plain);
            BigInteger modulus = BigInteger.Parse(modulusText);
            BigInteger exponent = BigInteger.Parse(exponentText);
            BigInteger encoded = BigInteger.ModPow(decoded, exponent, modulus);
            byte[] encodedBytes = ToBigEndianSigned(encoded);

            Position = 0;
            PutByte(encodedBytes.Length);
            PutBytes(encodedBytes, encodedBytes.Length);
        }

        private static BigInteger FromBigEndianSigned(byte[] bytes)
        {
            byte[] littleEndian = new byte[bytes.Length + 1];
            for (int i = 0; i < bytes.Length; i++)
            {
                littleEndian[i] = bytes[bytes.Length - 1 - i];
            }

            return new BigInteger(littleEndian);
        }

        private static byte[] ToBigEndianSigned(BigInteger value)
        {
            byte[] littleEndian = value.ToByteArray();
            Array.Reverse(littleEndian);
            int start = 0;
            while (start < littleEndian.Length - 1 && littleEndian[start] == 0)
            {
                start++;
            }

            if (start == 0)
            {
                return littleEndian;
            }

            byte[] result = new byte[littleEndian.Length - start];
            Buffer.BlockCopy(littleEndian, start, result, 0, result.Length);
            return result;
        }
    }
}
