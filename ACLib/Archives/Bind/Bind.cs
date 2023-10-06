using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using ACLib.IO;

// AFK - Eating

namespace ACLib.Archives.Bind
{
    // Details:

    // A Bind file appears to act as a means of grouping compressed archives into one single file
    // Depite this, all binds only contain one archive, with archives that appear to be members incrementing their extension by 1
    // for each following member.
    // I.e. app.000 -> app.001 -> app.002
    // These are very clearly related archives, and the bind format at a glance appears to have support for grouping files, but
    // they are completely seperate.

    // Bind files are Big Endian


    public class BindEntry
    {
        private bool m_Initialized = false;
        public int Unk_C0 { get; set; }  // Unknown value. usually always C0
        public int CompressedSize { get; set; }  // Size when compressed.
        public int Offset { get; set; } // offset to the file data

        public int UnkPostOffset { get; set; }
        public string FileName { get; set; }  // name of the file inside the bind, usually the same as the name of the bind itself. stored as a ptr to a string in the file
        public int FileSize { get; set; }  // size of the decompressed data.

        public BindGroup BindGroup { get; set; }

        public Stream? Stream { get; set; }  // Convenience member. Initialized when a BindEntry's "Open" method is called.

        private Stream? m_BaseStream { get; }

        public void Read(BinaryReader reader)
        {
            Unk_C0 = reader.ReadInt32BE();
            CompressedSize = reader.ReadInt32BE();
            Offset = reader.ReadInt32BE();
            UnkPostOffset = reader.ReadInt32BE();
            FileName = reader.ReadStringOffset(StringFormat.NullTerminated);
            FileSize = reader.ReadInt32BE();
        }

        public void Open()
        {
            if (!m_Initialized)
            {
                if (m_BaseStream == null)
                {
                    throw new InvalidDataException("Bind entry has no base stream to open from");
                }
                // Get compressed buffer
                m_BaseStream?.Seek(Offset, SeekOrigin.Begin);

                byte[] cmpData = new byte[CompressedSize];
                m_BaseStream?.Read(cmpData, 0, CompressedSize);

                Stream memStream = new MemoryStream(cmpData);

                ZLibStream decStream = new ZLibStream(memStream, CompressionMode.Decompress);

                Stream = new MemoryStream(FileSize);

                int totalRead = 0;
                byte[] tBuf = new byte[16384];
                while (totalRead < FileSize)
                {
                    int readCount = decStream.Read(tBuf, 0, ((FileSize - totalRead) < 16384 ? (FileSize - totalRead) : 16384));
                    Stream.Write(tBuf, 0, readCount);
                    totalRead += readCount;
                }

                Stream.Seek(0, SeekOrigin.Begin);

                m_Initialized = true;
            }
        }

        public void Close()
        {
            Stream?.Close();
        }

        public void Dispose()
        {
            Stream?.Dispose();
            m_BaseStream?.Dispose();
        }

        public BindEntry(Stream baseStream, BindGroup bindGroup)
        {
            FileName = "";
            m_BaseStream = baseStream;
            BindGroup = bindGroup;
        }
    }
    public class Bind
    {
        char[]? Magic { get; set; }  // 16 bytes, "BND307B17Q42\xE4\x00\x00\x00"
        int EntryCount { get; set; }  // I'm not even sure if this is a count because no binds hold more than one archive

        public BindGroup BindGroup { get; set; }

        public List<BindEntry> BindEntries { get; } // contains all entries in the loaded bind file.

        public void Load(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath), Encoding.UTF8, true))
            {
                Magic = reader.ReadChars(16);
                EntryCount = reader.ReadInt32BE();
                reader.BaseStream.Seek(0x0C, SeekOrigin.Current);
                BindEntries.Capacity = EntryCount;
                for (int i = 0; i < EntryCount; i++)
                {
                    BindEntry entry = new BindEntry(reader.BaseStream, BindGroup);
                    entry.Read(reader);
                    BindEntries.Add(entry);
                }
            }
        }

        ~Bind()
        {
            foreach (var bind in BindEntries)
            {
                bind.Dispose();
            }
        }

        public Bind(BindGroup bindGroup)
        {
            BindEntries = new List<BindEntry>();
            BindGroup = bindGroup;
        }
    }

    public class BindGroup
    {
        public List<Bind> Binds { get; set; }

        public void Load(string filePath)
        {
            string archiveBaseName = Path.GetFileNameWithoutExtension(filePath);
            for (int i = 0 ;; i++)
            {
                if (File.Exists($"{archiveBaseName}.{i:d3}"))
                {
                    Bind bind = new Bind(this);
                    bind.Load($"{archiveBaseName}.{i:d3}");
                    Binds.Add(bind);
                }
                else
                    break;
            }

        }

        public BindGroup()
        {
            Binds = new List<Bind>();
        }
    }
}
