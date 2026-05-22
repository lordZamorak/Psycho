using System;

namespace Psycho.Cache
{
    public static class PsychoMapDecoder
    {
        public static PsychoMapLandscape DecodeLandscape(byte[] data, int regionX, int regionY)
        {
            if (data == null || data.Length < 2)
            {
                throw new ArgumentException("Landscape data is missing or too short.", nameof(data));
            }

            try
            {
                return DecodeLandscape(data, regionX, regionY, true);
            }
            catch (InvalidOperationException)
            {
                return DecodeLandscape(data, regionX, regionY, false);
            }
        }

        public static PsychoMapObjects DecodeObjects(byte[] data, int regionX, int regionY)
        {
            if (data == null || data.Length < 2)
            {
                throw new ArgumentException("Object map data is missing or too short.", nameof(data));
            }

            bool couldHaveMarker = data[0] == 0 || data[0] == 1;
            if (couldHaveMarker)
            {
                try
                {
                    return DecodeObjects(data, regionX, regionY, true);
                }
                catch (InvalidOperationException)
                {
                    return DecodeObjects(data, regionX, regionY, false);
                }
            }

            return DecodeObjects(data, regionX, regionY, false);
        }

        private static PsychoMapObjects DecodeObjects(byte[] data, int regionX, int regionY, bool consumeFormatMarker)
        {
            PsychoMapObjects objects = new PsychoMapObjects
            {
                RegionX = regionX,
                RegionY = regionY
            };

            PsychoCacheBuffer stream = new PsychoCacheBuffer(data);
            objects.Osrs = consumeFormatMarker && stream.ReadUnsignedByte() == 1;
            int objectId = -1;

            while (true)
            {
                int objectDelta = stream.ReadUnsignedSmart();
                if (objectDelta == 0)
                {
                    return objects;
                }

                objectId += objectDelta;
                int packedLocation = 0;

                while (true)
                {
                    int locationDelta = stream.ReadUnsignedSmart();
                    if (locationDelta == 0)
                    {
                        break;
                    }

                    packedLocation += locationDelta - 1;
                    int localY = packedLocation & 0x3f;
                    int localX = packedLocation >> 6 & 0x3f;
                    int plane = packedLocation >> 12;
                    int attributes = stream.ReadUnsignedByte();

                    objects.Placements.Add(new PsychoMapObjectPlacement
                    {
                        ObjectId = objectId + (objects.Osrs ? 70000 : 0),
                        LocalX = localX,
                        LocalY = localY,
                        WorldX = regionX * 64 + localX,
                        WorldY = regionY * 64 + localY,
                        Plane = plane,
                        Type = attributes >> 2,
                        Orientation = attributes & 3
                    });
                }
            }
        }

        private static PsychoMapLandscape DecodeLandscape(byte[] data, int regionX, int regionY, bool consumeFormatMarker)
        {
            PsychoMapLandscape landscape = new PsychoMapLandscape
            {
                RegionX = regionX,
                RegionY = regionY
            };

            PsychoCacheBuffer stream = new PsychoCacheBuffer(data);
            landscape.Osrs = consumeFormatMarker && stream.ReadUnsignedByte() == 1;
            int baseX = regionX * 64;
            int baseY = regionY * 64;

            for (int plane = 0; plane < 4; plane++)
            {
                for (int localX = 0; localX < 64; localX++)
                {
                    for (int localY = 0; localY < 64; localY++)
                    {
                        DecodeTile(landscape, stream, plane, localX, localY, baseX, baseY);
                    }
                }
            }

            return landscape;
        }

        private static void DecodeTile(PsychoMapLandscape landscape, PsychoCacheBuffer stream, int plane, int x, int y, int baseX, int baseY)
        {
            landscape.RenderFlags[plane, x, y] = 0;

            while (true)
            {
                int opcode = stream.ReadUnsignedByte();
                if (opcode == 0)
                {
                    if (plane == 0)
                    {
                        landscape.Heights[0, x, y] = -CalculateHeight(0xe3b7b + x + baseX, 0x87cce + y + baseY) * 8;
                    }
                    else
                    {
                        landscape.Heights[plane, x, y] = landscape.Heights[plane - 1, x, y] - 240;
                    }

                    return;
                }

                if (opcode == 1)
                {
                    int height = stream.ReadUnsignedByte();
                    if (height == 1)
                    {
                        height = 0;
                    }

                    if (plane == 0)
                    {
                        landscape.Heights[0, x, y] = -height * 8;
                    }
                    else
                    {
                        landscape.Heights[plane, x, y] = landscape.Heights[plane - 1, x, y] - height * 8;
                    }

                    return;
                }

                if (opcode <= 49)
                {
                    landscape.OverlayIds[plane, x, y] = unchecked((byte)stream.ReadSignedByte());
                    landscape.OverlayShapes[plane, x, y] = (byte)((opcode - 2) / 4);
                    landscape.OverlayRotations[plane, x, y] = (byte)((opcode - 2) & 3);
                }
                else if (opcode <= 81)
                {
                    landscape.RenderFlags[plane, x, y] = (byte)(opcode - 49);
                }
                else
                {
                    landscape.UnderlayIds[plane, x, y] = (byte)(opcode - 81);
                }
            }
        }

        private static int CalculateHeight(int x, int y)
        {
            int height = InterpolatedNoise(x + 45365, y + 0x16713, 4) - 128;
            height += (InterpolatedNoise(x + 10294, y + 37821, 2) - 128) >> 1;
            height += (InterpolatedNoise(x, y, 1) - 128) >> 2;
            height = (int)(height * 0.3d) + 35;
            if (height < 10)
            {
                height = 10;
            }
            else if (height > 60)
            {
                height = 60;
            }

            return height;
        }

        private static int InterpolatedNoise(int x, int y, int scale)
        {
            int baseX = x / scale;
            int offsetX = x & (scale - 1);
            int baseY = y / scale;
            int offsetY = y & (scale - 1);
            int southwest = SmoothNoise(baseX, baseY);
            int southeast = SmoothNoise(baseX + 1, baseY);
            int northwest = SmoothNoise(baseX, baseY + 1);
            int northeast = SmoothNoise(baseX + 1, baseY + 1);
            int south = Interpolate(southwest, southeast, offsetX, scale);
            int north = Interpolate(northwest, northeast, offsetX, scale);
            return Interpolate(south, north, offsetY, scale);
        }

        private static int Interpolate(int a, int b, int offset, int scale)
        {
            int curve = 0x10000 - (int)(Math.Cos(offset * Math.PI / scale) * 65536d) >> 1;
            return (a * (0x10000 - curve) >> 16) + (b * curve >> 16);
        }

        private static int SmoothNoise(int x, int y)
        {
            int corners = Noise(x - 1, y - 1) + Noise(x + 1, y - 1) + Noise(x - 1, y + 1) + Noise(x + 1, y + 1);
            int sides = Noise(x - 1, y) + Noise(x + 1, y) + Noise(x, y - 1) + Noise(x, y + 1);
            int center = Noise(x, y);
            return corners / 16 + sides / 8 + center / 4;
        }

        private static int Noise(int x, int y)
        {
            int n = x + y * 57;
            n = n << 13 ^ n;
            int value = n * (n * n * 15731 + 0xc0ae5) + 0x5208dd0d & 0x7fffffff;
            return value >> 19 & 0xff;
        }
    }
}
