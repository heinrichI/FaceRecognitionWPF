using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognitionDataBase
{
    public class HashHelper
    {
        /// <summary>
        /// Return MD5 Checksum for file.
        /// </summary>
        /// <param name="fn">Filename with full path</param>
        /// <returns>Checksum of file.</returns>
        public static string CreateMD5Checksum(string fn)
        {
            System.Security.Cryptography.MD5 oMD5 = System.Security.Cryptography.MD5.Create();
            StringBuilder sb = new StringBuilder();

            try
            {
                using (System.IO.FileStream fs = System.IO.File.OpenRead(fn))
                {
                    foreach (byte b in oMD5.ComputeHash(fs))
                        sb.Append(b.ToString("x2").ToLower());
                }
            }

            catch (System.UnauthorizedAccessException ex)
            {
                //MessageBox.Show(ex.Message);
                return String.Empty;
            }
            catch (System.IO.FileNotFoundException)
            {
                return String.Empty;
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                //MessageBox.Show(ex.Message);
                return String.Empty;
            }
            catch (System.IO.IOException)
            {
                return String.Empty;
            }

            return sb.ToString();
        }
    }
}
