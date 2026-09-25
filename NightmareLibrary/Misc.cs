using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NightmareLibrary
{
    public static class Misc
    {
        public static void CombineFiles(string firstFile, string secondFile, string outputFile, int offset)
        {
            File.Delete(outputFile);
            using (var output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                using (var fs1 = new FileStream(firstFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[offset];
                    int bytesRead = fs1.Read(buffer, 0, offset);
                    output.Write(buffer, 0, bytesRead);
                }

                using (var fs2 = new FileStream(secondFile, FileMode.Open, FileAccess.Read))
                {
                    fs2.Seek(offset, SeekOrigin.Begin);
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fs2.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = System.IO.Path.GetFileName(file);
                string destFilePath = System.IO.Path.Combine(destinationDir, fileName);
                System.IO.File.Copy(file, destFilePath, overwrite);
            }
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = System.IO.Path.GetFileName(subDir);
                string destSubDirPath = System.IO.Path.Combine(destinationDir, subDirName);
                CopyDirectory(subDir, destSubDirPath, overwrite);
            }
        }

        public enum FileTypes
        {
            rbin = 0,
            ctt = 1,
            l2d = 2,
            fep = 3,
            pmo = 4,
            pmp = 5,
        }
        
        public enum ErrorCode
        {
            Success = 0,
            MissingRbin = 1,
            Cancelled = 2,
            FailedFileFind = 3,
        }
        
        public static string ReplaceFirst(string str, string term, string replace)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(0, position) + replace + str.Substring(position + term.Length);
            return str;
        }
        
        public static string ReplaceFirst(string str, char term, char replace)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(0, position) + replace + str.Substring(position + 1);
            return str;
        }
        
        public static string RemoveAtFirst(string str, char term)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(position + 1);
            return str;
        }
    }
}
