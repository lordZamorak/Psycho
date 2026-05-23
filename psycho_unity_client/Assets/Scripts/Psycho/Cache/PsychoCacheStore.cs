using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Psycho.Cache
{
    public sealed class PsychoCacheStore : IDisposable
    {
        private const int IndexEntrySize = 6;
        private const int SectorSize = 520;
        private const int SectorHeaderSize = 8;
        private const int LargeSectorHeaderSize = 10;

        private readonly string cacheRoot;
        private readonly FileStream dataStream;
        private readonly Dictionary<int, FileStream> indexStreams = new Dictionary<int, FileStream>();
        private readonly int archiveIdOffset;

        public PsychoCacheStore(string cacheRoot)
        {
            if (string.IsNullOrWhiteSpace(cacheRoot))
            {
                throw new ArgumentException("Cache root is required.", nameof(cacheRoot));
            }

            this.cacheRoot = cacheRoot;
            string dataPath = Path.Combine(cacheRoot, "main_file_cache.dat");
            archiveIdOffset = 1;
            if (!File.Exists(dataPath))
            {
                dataPath = Path.Combine(cacheRoot, "main_file_cache.dat2");
                archiveIdOffset = 0;
            }

            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException("Could not find RuneScape cache data file.", dataPath);
            }

            dataStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public static string DefaultClientCachePath
        {
            get
            {
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
                return Path.Combine(projectRoot, "necrotic_client-item_attributes", "cache");
            }
        }

        public static string DefaultClientCache1Path
        {
            get
            {
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
                return Path.Combine(projectRoot, "necrotic_client-item_attributes", "cache1");
            }
        }

        public int GetFileCount(int cacheIndex)
        {
            FileStream index = GetIndexStream(cacheIndex);
            return (int)(index.Length / IndexEntrySize);
        }

        public byte[] ReadFile(int cacheIndex, int fileId)
        {
            if (fileId < 0)
            {
                return null;
            }

            FileStream index = GetIndexStream(cacheIndex);
            long indexOffset = (long)fileId * IndexEntrySize;
            if (indexOffset < 0 || indexOffset + IndexEntrySize > index.Length)
            {
                return null;
            }

            byte[] indexEntry = new byte[IndexEntrySize];
            ReadFully(index, indexOffset, indexEntry, 0, indexEntry.Length);
            int length = ReadMedium(indexEntry, 0);
            int sector = ReadMedium(indexEntry, 3);
            if (length <= 0 || sector <= 0)
            {
                return null;
            }

            byte[] payload = new byte[length];
            byte[] sectorBytes = new byte[SectorSize];
            int bytesRead = 0;
            int chunk = 0;
            bool largeFileId = fileId > 0xffff;
            int sectorHeaderSize = largeFileId ? LargeSectorHeaderSize : SectorHeaderSize;
            int sectorPayloadSize = SectorSize - sectorHeaderSize;
            int expectedArchive = cacheIndex + archiveIdOffset;
            int alternateExpectedArchive = archiveIdOffset == 0 ? cacheIndex + 1 : cacheIndex;
            int maxSectors = Math.Max(1, length / sectorPayloadSize + 2);

            while (bytesRead < length)
            {
                if (sector <= 0 || (long)sector * SectorSize + sectorHeaderSize > dataStream.Length)
                {
                    return null;
                }

                if (chunk > maxSectors)
                {
                    return null;
                }

                ReadFully(dataStream, (long)sector * SectorSize, sectorBytes, 0, sectorBytes.Length);
                int readFileId;
                int readChunk;
                int nextSector;
                int archive;
                if (largeFileId)
                {
                    readFileId = ((sectorBytes[0] & 0xff) << 24) | ((sectorBytes[1] & 0xff) << 16) | ((sectorBytes[2] & 0xff) << 8) | (sectorBytes[3] & 0xff);
                    readChunk = ((sectorBytes[4] & 0xff) << 8) | (sectorBytes[5] & 0xff);
                    nextSector = ReadMedium(sectorBytes, 6);
                    archive = sectorBytes[9] & 0xff;
                }
                else
                {
                    readFileId = ((sectorBytes[0] & 0xff) << 8) | (sectorBytes[1] & 0xff);
                    readChunk = ((sectorBytes[2] & 0xff) << 8) | (sectorBytes[3] & 0xff);
                    nextSector = ReadMedium(sectorBytes, 4);
                    archive = sectorBytes[7] & 0xff;
                }

                if (readFileId != fileId || readChunk != chunk || (archive != expectedArchive && archive != alternateExpectedArchive))
                {
                    return null;
                }

                int copyLength = Math.Min(sectorPayloadSize, length - bytesRead);
                Buffer.BlockCopy(sectorBytes, sectorHeaderSize, payload, bytesRead, copyLength);
                bytesRead += copyLength;
                sector = nextSector;
                chunk++;
            }

            return payload;
        }

        public byte[] ReadGzipFile(int cacheIndex, int fileId)
        {
            byte[] raw = ReadFile(cacheIndex, fileId);
            if (raw == null || raw.Length < 2 || raw[0] != 0x1f || raw[1] != 0x8b)
            {
                return raw;
            }

            using (MemoryStream input = new MemoryStream(raw))
            using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        public byte[] ReadContainerFile(int cacheIndex, int fileId)
        {
            byte[] raw = ReadFile(cacheIndex, fileId);
            if (raw == null || raw.Length < 5)
            {
                return raw;
            }

            int compression = raw[0] & 0xff;
            int compressedLength = ReadInt(raw, 1);
            if (compressedLength < 0 || compressedLength > raw.Length - 5)
            {
                return raw;
            }

            if (compression == 0)
            {
                byte[] output = new byte[compressedLength];
                Buffer.BlockCopy(raw, 5, output, 0, output.Length);
                return output;
            }

            if (raw.Length < 9)
            {
                return null;
            }

            int decompressedLength = ReadInt(raw, 5);
            if (decompressedLength < 0 || compressedLength > raw.Length - 9)
            {
                return null;
            }

            if (compression == 2)
            {
                using (MemoryStream input = new MemoryStream(raw, 9, compressedLength))
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                using (MemoryStream output = new MemoryStream(Math.Max(0, decompressedLength)))
                {
                    gzip.CopyTo(output);
                    return output.ToArray();
                }
            }

            return null;
        }

        public void Dispose()
        {
            dataStream?.Dispose();
            foreach (FileStream stream in indexStreams.Values)
            {
                stream?.Dispose();
            }

            indexStreams.Clear();
        }

        private FileStream GetIndexStream(int cacheIndex)
        {
            if (cacheIndex < 0 || cacheIndex > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(cacheIndex), cacheIndex, "Cache index must be in the main cache idx0-idx255 range.");
            }

            if (indexStreams.TryGetValue(cacheIndex, out FileStream existing))
            {
                return existing;
            }

            string indexPath = Path.Combine(cacheRoot, $"main_file_cache.idx{cacheIndex}");
            if (!File.Exists(indexPath))
            {
                throw new FileNotFoundException("Could not find RuneScape cache index file.", indexPath);
            }

            FileStream created = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            indexStreams.Add(cacheIndex, created);
            return created;
        }

        private static int ReadMedium(byte[] bytes, int offset)
        {
            return ((bytes[offset] & 0xff) << 16) | ((bytes[offset + 1] & 0xff) << 8) | (bytes[offset + 2] & 0xff);
        }

        private static int ReadInt(byte[] bytes, int offset)
        {
            return ((bytes[offset] & 0xff) << 24) | ((bytes[offset + 1] & 0xff) << 16) | ((bytes[offset + 2] & 0xff) << 8) | (bytes[offset + 3] & 0xff);
        }

        private static void ReadFully(FileStream stream, long offset, byte[] buffer, int bufferOffset, int count)
        {
            stream.Position = offset;
            int total = 0;
            while (total < count)
            {
                int read = stream.Read(buffer, bufferOffset + total, count - total);
                if (read <= 0)
                {
                    throw new EndOfStreamException($"Unexpected end of cache file at {stream.Position}.");
                }

                total += read;
            }
        }
    }
}
