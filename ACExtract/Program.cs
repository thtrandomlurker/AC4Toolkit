using ACLib.Archives;
using ACLib.Archives.Bind;

namespace ACExtract
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BindGroup bindGroup = new BindGroup();

            bindGroup.Load(args[0]);

            Console.WriteLine($"{bindGroup.Binds.Count} Binds found.");

            // ensure all binds are open
            foreach (var bind in bindGroup.Binds)
            {
                bind.BindEntries[0].Open();
            }

            string outPath = Path.GetFileNameWithoutExtension(args[0]);

            if (bindGroup.Binds[0].BindEntries[0].Stream != null)
            {
                Archive arc = new Archive(bindGroup);
                arc.Load(bindGroup.Binds[0].BindEntries[0].Stream);
                foreach (var arcEntry in arc.Files)
                {
                    arcEntry.Open();
                    

                    Console.WriteLine(arcEntry.FileName);

                    Console.WriteLine(outPath);


                    Directory.CreateDirectory(Path.Combine(outPath, Path.GetDirectoryName(arcEntry.FileName)));

                    Stream outFile = File.Create(Path.Combine(outPath, arcEntry.FileName));

                    byte[] datBuf = new byte[arcEntry.FileSize];
                    arcEntry.Stream?.Read(datBuf);
                    outFile.Write(datBuf);
                    outFile.Close();

                    arcEntry.Close();
                }
            }
        }
    }
}