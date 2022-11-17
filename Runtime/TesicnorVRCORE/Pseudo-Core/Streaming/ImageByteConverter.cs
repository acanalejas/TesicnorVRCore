using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

    public class ImageByteConverter
    {
        public static byte[] ImageToBytes(Image input)
        {
            using(var ms = new MemoryStream())
            {
                input.Save(ms, input.RawFormat);
                return ms.ToArray();
            }
        }

        static Image result;
        public static Image BytesToImage(byte[] input)
        {
            try
            {
                MemoryStream ms = new MemoryStream(input,0, input.Length);
                ms.Write(input,0, input.Length);
                result = Image.FromStream(ms);
            }
            catch { }

            return result;
        }
        public static Image BytesToImage(Stream input)
        {
            try
            {
                result = Image.FromStream(input);
            }
            catch { }
            return result;
        }
    }
