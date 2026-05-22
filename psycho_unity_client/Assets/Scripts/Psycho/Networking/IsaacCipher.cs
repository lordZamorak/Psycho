using System;

namespace Psycho.Networking
{
    public sealed class IsaacCipher
    {
        private readonly int[] memory = new int[256];
        private readonly int[] results = new int[256];
        private int accumulator;
        private int counter;
        private int count;
        private int lastResult;

        public IsaacCipher(int[] seed)
        {
            Array.Copy(seed, results, Math.Min(seed.Length, results.Length));
            InitializeKeySet();
        }

        public int Next()
        {
            if (count-- == 0)
            {
                Isaac();
                count = 255;
            }

            return results[count];
        }

        private void InitializeKeySet()
        {
            int a;
            int b;
            int c;
            int d;
            int e;
            int f;
            int g;
            int h;
            a = b = c = d = e = f = g = h = unchecked((int)0x9e3779b9);

            for (int i = 0; i < 4; i++)
            {
                Mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);
            }

            for (int i = 0; i < 256; i += 8)
            {
                a += results[i];
                b += results[i + 1];
                c += results[i + 2];
                d += results[i + 3];
                e += results[i + 4];
                f += results[i + 5];
                g += results[i + 6];
                h += results[i + 7];
                Mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);
                memory[i] = a;
                memory[i + 1] = b;
                memory[i + 2] = c;
                memory[i + 3] = d;
                memory[i + 4] = e;
                memory[i + 5] = f;
                memory[i + 6] = g;
                memory[i + 7] = h;
            }

            for (int i = 0; i < 256; i += 8)
            {
                a += memory[i];
                b += memory[i + 1];
                c += memory[i + 2];
                d += memory[i + 3];
                e += memory[i + 4];
                f += memory[i + 5];
                g += memory[i + 6];
                h += memory[i + 7];
                Mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);
                memory[i] = a;
                memory[i + 1] = b;
                memory[i + 2] = c;
                memory[i + 3] = d;
                memory[i + 4] = e;
                memory[i + 5] = f;
                memory[i + 6] = g;
                memory[i + 7] = h;
            }

            Isaac();
            count = 256;
        }

        private void Isaac()
        {
            lastResult += ++counter;

            for (int i = 0; i < 256; i++)
            {
                int x = memory[i];

                switch (i & 3)
                {
                    case 0:
                        accumulator ^= accumulator << 13;
                        break;
                    case 1:
                        accumulator ^= (int)((uint)accumulator >> 6);
                        break;
                    case 2:
                        accumulator ^= accumulator << 2;
                        break;
                    default:
                        accumulator ^= (int)((uint)accumulator >> 16);
                        break;
                }

                accumulator += memory[(i + 128) & 0xff];
                int y = memory[(x & 0x3fc) >> 2] + accumulator + lastResult;
                memory[i] = y;
                results[i] = lastResult = memory[((y >> 8) & 0x3fc) >> 2] + x;
            }
        }

        private static void Mix(ref int a, ref int b, ref int c, ref int d, ref int e, ref int f, ref int g, ref int h)
        {
            a ^= b << 11;
            d += a;
            b += c;
            b ^= (int)((uint)c >> 2);
            e += b;
            c += d;
            c ^= d << 8;
            f += c;
            d += e;
            d ^= (int)((uint)e >> 16);
            g += d;
            e += f;
            e ^= f << 10;
            h += e;
            f += g;
            f ^= (int)((uint)g >> 4);
            a += f;
            g += h;
            g ^= h << 8;
            b += g;
            h += a;
            h ^= (int)((uint)a >> 9);
            c += h;
            a += b;
        }
    }
}
