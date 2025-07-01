using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Editor.NewTools
{
    public static class L2D
    {
        static readonly byte[] CTRT = Encoding.ASCII.GetBytes("CTRT");
        static long FindNextCTRT(FileStream fs, int start)
        {
            fs.Position = start;
            int b;
            Queue<byte> buffer = new Queue<byte>();

            while ((b = fs.ReadByte()) != -1)
            {
                buffer.Enqueue((byte)b);
                if (buffer.Count > CTRT.Length)
                    buffer.Dequeue();

                if (buffer.SequenceEqual(CTRT))
                    return (int)fs.Position - CTRT.Length;
            }

            return -1; // Not found
        }
        public static void Pack(string archive, List<string> embedded)
        {
            if (!Directory.Exists(Path.GetDirectoryName(archive)))
            {
                return;
            }
            string tempdir = Misc.Paths.temp;
            string temparchive = tempdir + Path.GetFileName(archive);
            List<string> tempembedded = new List<string>();
            Directory.CreateDirectory(tempdir);
            File.Copy(archive, Path.Combine(tempdir, Path.GetFileName(archive)), true);
            foreach (string file in embedded)
            {
                File.Copy(file, Path.Combine(tempdir, Path.GetFileName(file)), true);
                tempembedded.Add(Path.Combine(tempdir, Path.GetFileName(file)));
            }
            File.Delete(archive);
            using (var output = new FileStream(archive, FileMode.Create, FileAccess.Write))
            {
                foreach (string file in tempembedded)
                {
                    using (var fs1 = new FileStream(temparchive, FileMode.Open, FileAccess.Read))
                    {
                        int start = (int)output.Length;
                        long length = FindNextCTRT(fs1, start);
                        fs1.Position = start;

                        long bytesToRead = length - start;
                        int bufferSize = 4096;
                        byte[] buffer = new byte[bufferSize];

                        while (bytesToRead > 0)
                        {
                            int readSize = (int)Math.Min(bufferSize, bytesToRead);
                            int read = fs1.Read(buffer, 0, readSize);
                            if (read == 0) break; // EOF
                            output.Write(buffer, 0, read);
                            bytesToRead -= read;
                        }
                        using (var fs2 = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            start = (int)output.Length;
                            fs2.Position = 0;

                            bytesToRead = fs2.Length;
                            bufferSize = 4096;
                            buffer = new byte[bufferSize];

                            while (bytesToRead > 0)
                            {
                                int readSize = (int)Math.Min(bufferSize, bytesToRead);
                                int read = fs2.Read(buffer, 0, readSize);
                                if (read == 0) break; // EOF
                                output.Write(buffer, 0, read);
                                bytesToRead -= read;
                            }
                        }
                    }
                }
                using (var fs3 = new FileStream(temparchive, FileMode.Open, FileAccess.Read))
                {
                    int start = (int)output.Length;
                    long length = fs3.Length;
                    fs3.Position = start;

                    long bytesToRead = length - start;
                    int bufferSize = 4096;
                    byte[] buffer = new byte[bufferSize];

                    while (bytesToRead > 0)
                    {
                        int readSize = (int)Math.Min(bufferSize, bytesToRead);
                        int read = fs3.Read(buffer, 0, readSize);
                        if (read == 0) break; // EOF
                        output.Write(buffer, 0, read);
                        bytesToRead -= read;
                    }
                }
            }

            File.Delete(tempdir + Path.GetFileName(archive));
            foreach (string file in embedded)
            {
                File.Delete(tempdir + Path.GetFileName(file));
            }
            File.Copy(archive, Path.Combine(Misc.Paths.toolkit, Path.GetFileName(archive)), true);
            Toolkit.ArcUnpack(Path.GetFileName(archive), Path.GetDirectoryName(archive));
        }
        public static void Unpack()
        {

        }
    }
}
