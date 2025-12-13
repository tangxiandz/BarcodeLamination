using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationPrint
{
    public static class FileHelper
    {
        public static string GetFileHash(string path)
        {
            using var fs = File.OpenRead(path);
            byte[] hash = MD5.HashData(fs);
            return Convert.ToHexString(hash);
        }

        public static byte[] GetFileContent(string path)
        {
            using var fs = File.OpenRead(path);
            byte[] content = new byte[fs.Length];
            fs.Read(content, 0, content.Length);
            return content;
        }

        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(invalidChar))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
