using System;

namespace Psycho.Cache
{
    public static class PsychoModelDecoder
    {
        public static PsychoCacheModel Decode(byte[] data, int modelId)
        {
            if (data == null || data.Length < 20)
            {
                throw new ArgumentException("Model data is missing or too short.", nameof(data));
            }

            bool newFormat = data[data.Length - 1] == 0xff && data[data.Length - 2] == 0xff;
            return newFormat ? DecodeNew(data, modelId) : DecodeOld(data, modelId);
        }

        private static PsychoCacheModel DecodeOld(byte[] data, int modelId)
        {
            OldHeader header = BuildOldHeader(data);
            int[] vertexX = new int[header.VertexCount];
            int[] vertexY = new int[header.VertexCount];
            int[] vertexZ = new int[header.VertexCount];
            int[] triangleA = new int[header.FaceCount];
            int[] triangleB = new int[header.FaceCount];
            int[] triangleC = new int[header.FaceCount];
            int[] faceColors = new int[header.FaceCount];
            int[] faceAlpha = header.FaceAlphaOffset >= 0 ? new int[header.FaceCount] : null;

            PsychoCacheBuffer stream = new PsychoCacheBuffer(data);
            stream.Position = header.VertexFlagOffset;
            PsychoCacheBuffer xStream = new PsychoCacheBuffer(data);
            xStream.Position = header.VertexXOffset;
            PsychoCacheBuffer yStream = new PsychoCacheBuffer(data);
            yStream.Position = header.VertexYOffset;
            PsychoCacheBuffer zStream = new PsychoCacheBuffer(data);
            zStream.Position = header.VertexZOffset;
            PsychoCacheBuffer skinStream = new PsychoCacheBuffer(data);
            skinStream.Position = Math.Max(0, header.VertexSkinOffset);

            int baseX = 0;
            int baseY = 0;
            int baseZ = 0;
            for (int vertex = 0; vertex < header.VertexCount; vertex++)
            {
                int flags = stream.ReadUnsignedByte();
                int dx = (flags & 1) == 0 ? 0 : xStream.ReadSignedSmart();
                int dy = (flags & 2) == 0 ? 0 : yStream.ReadSignedSmart();
                int dz = (flags & 4) == 0 ? 0 : zStream.ReadSignedSmart();
                vertexX[vertex] = baseX + dx;
                vertexY[vertex] = baseY + dy;
                vertexZ[vertex] = baseZ + dz;
                baseX = vertexX[vertex];
                baseY = vertexY[vertex];
                baseZ = vertexZ[vertex];
                if (header.VertexSkinOffset >= 0)
                {
                    skinStream.ReadUnsignedByte();
                }
            }

            PsychoCacheBuffer colorStream = new PsychoCacheBuffer(data);
            colorStream.Position = header.FaceColorOffset;
            PsychoCacheBuffer renderStream = new PsychoCacheBuffer(data);
            renderStream.Position = Math.Max(0, header.FaceRenderTypeOffset);
            PsychoCacheBuffer priorityStream = new PsychoCacheBuffer(data);
            priorityStream.Position = Math.Max(0, header.FacePriorityOffset);
            PsychoCacheBuffer alphaStream = new PsychoCacheBuffer(data);
            alphaStream.Position = Math.Max(0, header.FaceAlphaOffset);
            PsychoCacheBuffer faceSkinStream = new PsychoCacheBuffer(data);
            faceSkinStream.Position = Math.Max(0, header.FaceSkinOffset);

            for (int face = 0; face < header.FaceCount; face++)
            {
                faceColors[face] = colorStream.ReadUnsignedShort();
                if (header.FaceRenderTypeOffset >= 0)
                {
                    renderStream.ReadUnsignedByte();
                }

                if (header.FacePriorityOffset >= 0)
                {
                    priorityStream.ReadUnsignedByte();
                }

                if (header.FaceAlphaOffset >= 0)
                {
                    faceAlpha[face] = alphaStream.ReadUnsignedByte();
                }

                if (header.FaceSkinOffset >= 0)
                {
                    faceSkinStream.ReadUnsignedByte();
                }
            }

            DecodeTriangleIndices(data, header.FaceIndexOffset, header.FaceOpcodeOffset, header.FaceCount, triangleA, triangleB, triangleC);

            return new PsychoCacheModel
            {
                ModelId = modelId,
                Format = PsychoCacheModelFormat.Old,
                VertexX = vertexX,
                VertexY = vertexY,
                VertexZ = vertexZ,
                TriangleA = triangleA,
                TriangleB = triangleB,
                TriangleC = triangleC,
                FaceColors = faceColors,
                FaceAlpha = faceAlpha
            };
        }

        private static PsychoCacheModel DecodeNew(byte[] data, int modelId)
        {
            PsychoCacheBuffer trailer = new PsychoCacheBuffer(data);
            trailer.Position = data.Length - 23;
            int vertices = trailer.ReadUnsignedShort();
            int faces = trailer.ReadUnsignedShort();
            int textureTriangles = trailer.ReadUnsignedByte();
            int flags = trailer.ReadUnsignedByte();
            bool hasRenderType = (flags & 1) == 1;
            bool hasExtendedTextureBlock = (flags & 8) == 8;
            if (!hasExtendedTextureBlock)
            {
                return Decode525(data, modelId, vertices, faces, textureTriangles, flags);
            }

            trailer.Position -= 7;
            int newFormat = trailer.ReadUnsignedByte();
            trailer.Position += 6;
            int priorityFlag = trailer.ReadUnsignedByte();
            int alphaFlag = trailer.ReadUnsignedByte();
            int faceSkinFlag = trailer.ReadUnsignedByte();
            int materialFlag = trailer.ReadUnsignedByte();
            int vertexSkinFlag = trailer.ReadUnsignedByte();
            int vertexXDataLength = trailer.ReadUnsignedShort();
            int vertexYDataLength = trailer.ReadUnsignedShort();
            int vertexZDataLength = trailer.ReadUnsignedShort();
            int faceIndexDataLength = trailer.ReadUnsignedShort();
            int textureIndexDataLength = trailer.ReadUnsignedShort();
            TextureCounts textureCounts = CountTextureTypes(data, textureTriangles);

            int offset = textureTriangles;
            int vertexFlagOffset = offset;
            offset += vertices;
            int faceRenderTypeOffset = offset;
            if (hasRenderType)
            {
                offset += faces;
            }

            if (flags == 1)
            {
                offset += faces;
            }

            int faceOpcodeOffset = offset;
            offset += faces;
            int facePriorityOffset = offset;
            if (priorityFlag == 255)
            {
                offset += faces;
            }

            int faceSkinOffset = offset;
            if (faceSkinFlag == 1)
            {
                offset += faces;
            }

            int vertexSkinOffset = offset;
            if (vertexSkinFlag == 1)
            {
                offset += vertices;
            }

            int faceAlphaOffset = offset;
            if (alphaFlag == 1)
            {
                offset += faces;
            }

            int faceIndexOffset = offset;
            offset += faceIndexDataLength;
            int faceMaterialOffset = offset;
            if (materialFlag == 1)
            {
                offset += faces * 2;
            }

            int faceTextureOffset = offset;
            offset += textureIndexDataLength;
            int faceColorOffset = offset;
            offset += faces * 2;
            int vertexXOffset = offset;
            offset += vertexXDataLength;
            int vertexYOffset = offset;
            offset += vertexYDataLength;
            int vertexZOffset = offset;
            offset += vertexZDataLength;
            offset += textureCounts.Simple * 6;
            offset += textureCounts.Complex * 6;

            int complexSize = 6;
            if (newFormat == 14)
            {
                complexSize = 7;
            }
            else if (newFormat >= 15)
            {
                complexSize = 9;
            }

            offset += complexSize * textureCounts.Complex;
            offset += textureCounts.Complex;
            offset += textureCounts.Complex;
            offset += textureCounts.Complex + textureCounts.Translucent * 2;

            PsychoCacheModel model = DecodeNewCore(
                data,
                modelId,
                PsychoCacheModelFormat.New622,
                vertices,
                faces,
                vertexFlagOffset,
                vertexXOffset,
                vertexYOffset,
                vertexZOffset,
                vertexSkinFlag == 1 ? vertexSkinOffset : -1,
                faceColorOffset,
                flags == 1 ? faceRenderTypeOffset : -1,
                priorityFlag == 255 ? facePriorityOffset : -1,
                alphaFlag == 1 ? faceAlphaOffset : -1,
                faceSkinFlag == 1 ? faceSkinOffset : -1,
                materialFlag == 1 ? faceMaterialOffset : -1,
                materialFlag == 1 && textureTriangles > 0 ? faceTextureOffset : -1,
                faceIndexOffset,
                faceOpcodeOffset);
            ScaleAndTranslate(model, 2, 0, 6, 0);
            return model;
        }

        private static PsychoCacheModel Decode525(byte[] data, int modelId, int vertices, int faces, int textureTriangles, int flags)
        {
            PsychoCacheBuffer trailer = new PsychoCacheBuffer(data);
            trailer.Position = data.Length - 23 + 6;
            int priorityFlag = trailer.ReadUnsignedByte();
            int alphaFlag = trailer.ReadUnsignedByte();
            int faceSkinFlag = trailer.ReadUnsignedByte();
            int materialFlag = trailer.ReadUnsignedByte();
            int vertexSkinFlag = trailer.ReadUnsignedByte();
            int vertexXDataLength = trailer.ReadUnsignedShort();
            int vertexYDataLength = trailer.ReadUnsignedShort();
            int vertexZDataLength = trailer.ReadUnsignedShort();
            int faceIndexDataLength = trailer.ReadUnsignedShort();
            int textureIndexDataLength = trailer.ReadUnsignedShort();
            TextureCounts textureCounts = CountTextureTypes(data, textureTriangles);

            int offset = textureTriangles;
            int vertexFlagOffset = offset;
            offset += vertices;
            int faceRenderTypeOffset = offset;
            if (flags == 1)
            {
                offset += faces;
            }

            int faceOpcodeOffset = offset;
            offset += faces;
            int facePriorityOffset = offset;
            if (priorityFlag == 255)
            {
                offset += faces;
            }

            int faceSkinOffset = offset;
            if (faceSkinFlag == 1)
            {
                offset += faces;
            }

            int vertexSkinOffset = offset;
            if (vertexSkinFlag == 1)
            {
                offset += vertices;
            }

            int faceAlphaOffset = offset;
            if (alphaFlag == 1)
            {
                offset += faces;
            }

            int faceIndexOffset = offset;
            offset += faceIndexDataLength;
            int faceMaterialOffset = offset;
            if (materialFlag == 1)
            {
                offset += faces * 2;
            }

            int faceTextureOffset = offset;
            offset += textureIndexDataLength;
            int faceColorOffset = offset;
            offset += faces * 2;
            int vertexXOffset = offset;
            offset += vertexXDataLength;
            int vertexYOffset = offset;
            offset += vertexYDataLength;
            int vertexZOffset = offset;
            offset += vertexZDataLength;
            offset += textureCounts.Simple * 6;
            offset += textureCounts.Complex * 6;
            offset += textureCounts.Complex * 6;
            offset += textureCounts.Complex;
            offset += textureCounts.Complex;
            offset += textureCounts.Complex + textureCounts.Translucent * 2;

            return DecodeNewCore(
                data,
                modelId,
                PsychoCacheModelFormat.New525,
                vertices,
                faces,
                vertexFlagOffset,
                vertexXOffset,
                vertexYOffset,
                vertexZOffset,
                vertexSkinFlag == 1 ? vertexSkinOffset : -1,
                faceColorOffset,
                flags == 1 ? faceRenderTypeOffset : -1,
                priorityFlag == 255 ? facePriorityOffset : -1,
                alphaFlag == 1 ? faceAlphaOffset : -1,
                faceSkinFlag == 1 ? faceSkinOffset : -1,
                materialFlag == 1 ? faceMaterialOffset : -1,
                materialFlag == 1 && textureTriangles > 0 ? faceTextureOffset : -1,
                faceIndexOffset,
                faceOpcodeOffset);
        }

        private static PsychoCacheModel DecodeNewCore(
            byte[] data,
            int modelId,
            PsychoCacheModelFormat format,
            int vertices,
            int faces,
            int vertexFlagOffset,
            int vertexXOffset,
            int vertexYOffset,
            int vertexZOffset,
            int vertexSkinOffset,
            int faceColorOffset,
            int faceRenderTypeOffset,
            int facePriorityOffset,
            int faceAlphaOffset,
            int faceSkinOffset,
            int faceMaterialOffset,
            int faceTextureOffset,
            int faceIndexOffset,
            int faceOpcodeOffset)
        {
            int[] vertexX = new int[vertices];
            int[] vertexY = new int[vertices];
            int[] vertexZ = new int[vertices];
            int[] triangleA = new int[faces];
            int[] triangleB = new int[faces];
            int[] triangleC = new int[faces];
            int[] faceColors = new int[faces];
            int[] faceAlpha = faceAlphaOffset >= 0 ? new int[faces] : null;

            PsychoCacheBuffer flagStream = new PsychoCacheBuffer(data);
            flagStream.Position = vertexFlagOffset;
            PsychoCacheBuffer xStream = new PsychoCacheBuffer(data);
            xStream.Position = vertexXOffset;
            PsychoCacheBuffer yStream = new PsychoCacheBuffer(data);
            yStream.Position = vertexYOffset;
            PsychoCacheBuffer zStream = new PsychoCacheBuffer(data);
            zStream.Position = vertexZOffset;
            PsychoCacheBuffer skinStream = new PsychoCacheBuffer(data);
            skinStream.Position = Math.Max(0, vertexSkinOffset);

            int baseX = 0;
            int baseY = 0;
            int baseZ = 0;
            for (int vertex = 0; vertex < vertices; vertex++)
            {
                int vertexFlags = flagStream.ReadUnsignedByte();
                int dx = (vertexFlags & 1) == 0 ? 0 : xStream.ReadSignedSmart();
                int dy = (vertexFlags & 2) == 0 ? 0 : yStream.ReadSignedSmart();
                int dz = (vertexFlags & 4) == 0 ? 0 : zStream.ReadSignedSmart();
                vertexX[vertex] = baseX + dx;
                vertexY[vertex] = baseY + dy;
                vertexZ[vertex] = baseZ + dz;
                baseX = vertexX[vertex];
                baseY = vertexY[vertex];
                baseZ = vertexZ[vertex];
                if (vertexSkinOffset >= 0)
                {
                    skinStream.ReadUnsignedByte();
                }
            }

            PsychoCacheBuffer colorStream = new PsychoCacheBuffer(data);
            colorStream.Position = faceColorOffset;
            PsychoCacheBuffer renderStream = new PsychoCacheBuffer(data);
            renderStream.Position = Math.Max(0, faceRenderTypeOffset);
            PsychoCacheBuffer priorityStream = new PsychoCacheBuffer(data);
            priorityStream.Position = Math.Max(0, facePriorityOffset);
            PsychoCacheBuffer alphaStream = new PsychoCacheBuffer(data);
            alphaStream.Position = Math.Max(0, faceAlphaOffset);
            PsychoCacheBuffer faceSkinStream = new PsychoCacheBuffer(data);
            faceSkinStream.Position = Math.Max(0, faceSkinOffset);
            PsychoCacheBuffer materialStream = new PsychoCacheBuffer(data);
            materialStream.Position = Math.Max(0, faceMaterialOffset);
            PsychoCacheBuffer textureStream = new PsychoCacheBuffer(data);
            textureStream.Position = Math.Max(0, faceTextureOffset);

            for (int face = 0; face < faces; face++)
            {
                faceColors[face] = colorStream.ReadUnsignedShort();
                if (faceRenderTypeOffset >= 0)
                {
                    int renderType = renderStream.ReadSignedByte();
                    if (renderType == 2)
                    {
                        faceColors[face] = 65535;
                    }
                }

                if (facePriorityOffset >= 0)
                {
                    priorityStream.ReadSignedByte();
                }

                if (faceAlphaOffset >= 0)
                {
                    int alpha = alphaStream.ReadSignedByte();
                    faceAlpha[face] = alpha < 0 ? 256 + alpha : alpha;
                }

                if (faceSkinOffset >= 0)
                {
                    faceSkinStream.ReadUnsignedByte();
                }

                if (faceMaterialOffset >= 0)
                {
                    int material = materialStream.ReadUnsignedShort() - 1;
                    if (faceTextureOffset >= 0)
                    {
                        if (material != -1)
                        {
                            textureStream.ReadUnsignedByte();
                        }
                    }
                }
            }

            DecodeTriangleIndices(data, faceIndexOffset, faceOpcodeOffset, faces, triangleA, triangleB, triangleC);

            return new PsychoCacheModel
            {
                ModelId = modelId,
                Format = format,
                VertexX = vertexX,
                VertexY = vertexY,
                VertexZ = vertexZ,
                TriangleA = triangleA,
                TriangleB = triangleB,
                TriangleC = triangleC,
                FaceColors = faceColors,
                FaceAlpha = faceAlpha
            };
        }

        private static void DecodeTriangleIndices(byte[] data, int indexOffset, int opcodeOffset, int faceCount, int[] triangleA, int[] triangleB, int[] triangleC)
        {
            PsychoCacheBuffer indexStream = new PsychoCacheBuffer(data);
            indexStream.Position = indexOffset;
            PsychoCacheBuffer opcodeStream = new PsychoCacheBuffer(data);
            opcodeStream.Position = opcodeOffset;
            int a = 0;
            int b = 0;
            int c = 0;
            int last = 0;

            for (int face = 0; face < faceCount; face++)
            {
                int opcode = opcodeStream.ReadUnsignedByte();
                if (opcode == 1)
                {
                    a = indexStream.ReadSignedSmart() + last;
                    last = a;
                    b = indexStream.ReadSignedSmart() + last;
                    last = b;
                    c = indexStream.ReadSignedSmart() + last;
                    last = c;
                }
                else if (opcode == 2)
                {
                    b = c;
                    c = indexStream.ReadSignedSmart() + last;
                    last = c;
                }
                else if (opcode == 3)
                {
                    a = c;
                    c = indexStream.ReadSignedSmart() + last;
                    last = c;
                }
                else if (opcode == 4)
                {
                    int oldA = a;
                    a = b;
                    b = oldA;
                    c = indexStream.ReadSignedSmart() + last;
                    last = c;
                }

                triangleA[face] = a;
                triangleB[face] = b;
                triangleC[face] = c;
            }
        }

        private static OldHeader BuildOldHeader(byte[] data)
        {
            PsychoCacheBuffer trailer = new PsychoCacheBuffer(data);
            trailer.Position = data.Length - 18;
            OldHeader header = new OldHeader();
            header.VertexCount = trailer.ReadUnsignedShort();
            header.FaceCount = trailer.ReadUnsignedShort();
            header.TextureFaceCount = trailer.ReadUnsignedByte();
            int renderTypeFlag = trailer.ReadUnsignedByte();
            int priorityFlag = trailer.ReadUnsignedByte();
            int alphaFlag = trailer.ReadUnsignedByte();
            int faceSkinFlag = trailer.ReadUnsignedByte();
            int vertexSkinFlag = trailer.ReadUnsignedByte();
            int vertexXDataLength = trailer.ReadUnsignedShort();
            int vertexYDataLength = trailer.ReadUnsignedShort();
            int vertexZDataLength = trailer.ReadUnsignedShort();
            int faceIndexDataLength = trailer.ReadUnsignedShort();

            int offset = 0;
            header.VertexFlagOffset = offset;
            offset += header.VertexCount;
            header.FaceOpcodeOffset = offset;
            offset += header.FaceCount;
            header.FacePriorityOffset = offset;
            if (priorityFlag == 255)
            {
                offset += header.FaceCount;
            }
            else
            {
                header.FacePriorityOffset = -priorityFlag - 1;
            }

            header.FaceSkinOffset = offset;
            if (faceSkinFlag == 1)
            {
                offset += header.FaceCount;
            }
            else
            {
                header.FaceSkinOffset = -1;
            }

            header.FaceRenderTypeOffset = offset;
            if (renderTypeFlag == 1)
            {
                offset += header.FaceCount;
            }
            else
            {
                header.FaceRenderTypeOffset = -1;
            }

            header.VertexSkinOffset = offset;
            if (vertexSkinFlag == 1)
            {
                offset += header.VertexCount;
            }
            else
            {
                header.VertexSkinOffset = -1;
            }

            header.FaceAlphaOffset = offset;
            if (alphaFlag == 1)
            {
                offset += header.FaceCount;
            }
            else
            {
                header.FaceAlphaOffset = -1;
            }

            header.FaceIndexOffset = offset;
            offset += faceIndexDataLength;
            header.FaceColorOffset = offset;
            offset += header.FaceCount * 2;
            header.TextureTriangleOffset = offset;
            offset += header.TextureFaceCount * 6;
            header.VertexXOffset = offset;
            offset += vertexXDataLength;
            header.VertexYOffset = offset;
            offset += vertexYDataLength;
            header.VertexZOffset = offset;
            offset += vertexZDataLength;
            return header;
        }

        private static TextureCounts CountTextureTypes(byte[] data, int textureTriangles)
        {
            TextureCounts counts = new TextureCounts();
            PsychoCacheBuffer textureStream = new PsychoCacheBuffer(data);
            textureStream.Position = 0;
            for (int i = 0; i < textureTriangles; i++)
            {
                int type = textureStream.ReadSignedByte();
                if (type == 0)
                {
                    counts.Simple++;
                }
                else if (type >= 1 && type <= 3)
                {
                    counts.Complex++;
                }

                if (type == 2)
                {
                    counts.Translucent++;
                }
            }

            return counts;
        }

        private static void ScaleAndTranslate(PsychoCacheModel model, int shift, int dx, int dy, int dz)
        {
            for (int i = 0; i < model.VertexCount; i++)
            {
                model.VertexX[i] = (model.VertexX[i] >> shift) + dx;
                model.VertexY[i] = (model.VertexY[i] >> shift) + dy;
                model.VertexZ[i] = (model.VertexZ[i] >> shift) + dz;
            }
        }

        private sealed class OldHeader
        {
            public int VertexCount;
            public int FaceCount;
            public int TextureFaceCount;
            public int VertexFlagOffset;
            public int FaceOpcodeOffset;
            public int FacePriorityOffset;
            public int FaceSkinOffset;
            public int FaceRenderTypeOffset;
            public int VertexSkinOffset;
            public int FaceAlphaOffset;
            public int FaceIndexOffset;
            public int FaceColorOffset;
            public int TextureTriangleOffset;
            public int VertexXOffset;
            public int VertexYOffset;
            public int VertexZOffset;
        }

        private struct TextureCounts
        {
            public int Simple;
            public int Complex;
            public int Translucent;
        }
    }
}
