using ACLib.Archives.Bind;
using ACLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACLib.Archives
{
    public class ArchiveEntry
    {
        public string FileName { get; set; }
        public int BindIndex { get; set; }
        public int BlockOffset { get; set; }
        public int BlockSize { get; set; }
        public int FileSize { get; set; }
        public BindGroup BindGroup { get; set; }

        public Stream? Stream { get; set; }
        private Stream m_BaseStream { get; set; }

        public void Read(BinaryReader reader)
        {
            FileName = reader.ReadString(StringFormat.FixedLength, 0x40);
            BindIndex = reader.ReadInt32BE();
            BlockOffset = reader.ReadInt32BE();
            BlockSize = reader.ReadInt32BE();
            FileSize = reader.ReadInt32BE();
        }

        public void Open()
        {
            Console.WriteLine($"Opening file from bind {BindIndex}");
            byte[] fileDat = new byte[FileSize];
            //m_BaseStream.Seek(BlockOffset << 4, SeekOrigin.Begin);
            BindGroup.Binds[BindIndex].BindEntries[0].Open();
            BindGroup.Binds[BindIndex].BindEntries[0].Stream.Seek(BlockOffset << 4, SeekOrigin.Begin);
            //m_BaseStream.Read(fileDat, 0, FileSize);
            BindGroup.Binds[BindIndex].BindEntries[0].Stream.Read(fileDat, 0, FileSize);
            BindGroup.Binds[BindIndex].BindEntries[0].Close();
            Stream = new MemoryStream(fileDat);
        }

        public void SetBindGroup(BindGroup bindGroup)
        {
            BindGroup = bindGroup;
        }

        public void Close()
        {
            Stream?.Dispose();
        }

        public void Dispoe()
        {
            Stream?.Dispose();
        }

        public ArchiveEntry(Stream baseStream, BindGroup bindGroup)
        {
            FileName = "";
            m_BaseStream = baseStream;
            BindGroup = bindGroup;
        }
    }
    public class Archive
    {
        public List<ArchiveEntry> Files { get; }
        public int BlockOffsetSize { get; set; }
        public int BlockSizeSize { get; set; }
        public short Unk01 { get; set; }
        public short Unk02 { get; set; }
        public BindGroup BindGroup { get; set; }

        public void Load(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int fileCount = reader.ReadInt32BE();
                BlockOffsetSize = reader.ReadInt32BE();
                BlockSizeSize = reader.ReadInt32BE();
                Unk01 = reader.ReadInt16BE();
                Unk02 = reader.ReadInt16BE();

                reader.BaseStream.Seek(0x50, SeekOrigin.Begin);
                for (int i = 0; i < fileCount; i++)
                {
                    ArchiveEntry entry = new ArchiveEntry(stream, BindGroup);
                    entry.Read(reader);
                    Files.Add(entry);
                }
            }
        }

        public void Load(string filePath)
        {
            Load(File.OpenRead(filePath));   
        }

        public Archive(BindGroup bindGroup)
        {
            Files = new List<ArchiveEntry>();
            BindGroup = bindGroup;
        }
    }
}
