using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Services
{
    public static class ByteFormatChecker
    {
        public static bool IsMp4(this Stream stream)
        {
            byte[] mp4Signature = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 };
            stream.Seek(0, SeekOrigin.Begin);

            byte[] middle4Bytes = new byte[8];
            stream.Read(middle4Bytes, 0, 8);

            for (int i = 0; i < mp4Signature.Length; i++)
            {
                if (middle4Bytes[i] != mp4Signature[i])
                    return false;
            }

            return true;
        }

        public static bool IsPng(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                MagickImage magickImage = new MagickImage(stream);
                string mime = magickImage.FormatInfo.MimeType;

                if (mime == "image/png")
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }        
        }

        public static bool IsJpg(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                MagickImage magickImage = new MagickImage(stream);
                string mime = magickImage.FormatInfo.MimeType;

                if (mime == "image/jpg" || mime == "image/jpeg")
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }     
        }
    }
}
