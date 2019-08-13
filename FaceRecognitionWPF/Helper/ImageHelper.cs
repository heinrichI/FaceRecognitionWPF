using FaceRecognitionBusinessLogic.ObjectModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FaceRecognitionWPF.Helper
{
    class ImageHelper
    {
        public static BitmapSource GetImageStream(Bitmap bitmap)
        {
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        internal static void SaveToClass(string className, string trainPath, string imagePath, 
            FaceLocation faceLocation, int addBorder)
        {
            string fileName = Path.GetFileName(imagePath);
            string targetPath = Path.Combine(trainPath, className, fileName);
            string targetPathSimilar = RenameHelper.GetSimilarName(targetPath, fileName);
            if (File.Exists(targetPathSimilar))
                throw new Exception($"File {targetPathSimilar} already exist!");

            var cropped = GetCroppedBitmap(addBorder, imagePath, 
                faceLocation.Left, faceLocation.Top, faceLocation.Right - faceLocation.Left, 
                faceLocation.Bottom - faceLocation.Top);

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID  
            // for the Quality parameter category.  
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.  
            // An EncoderParameters object has an array of EncoderParameter  
            // objects. In this case, there is only one  
            // EncoderParameter object in the array.  
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 95L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            cropped.Save(targetPathSimilar, jpgEncoder, myEncoderParameters);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        internal static System.Drawing.Bitmap GetCroppedBitmap(int addBorder, string imageFile, 
            int left, int top, int width, int height)
        {
            System.Drawing.Bitmap croppedImage;

            using (var image = System.Drawing.Image.FromFile(imageFile))
            using (var src = new System.Drawing.Bitmap(image))
            {
                if (left > src.Width)
                    throw new ArgumentException("left > src.Width");
                if (top > src.Height)
                    throw new ArgumentException("top > src.Height");
                int leftBorderAdded = left - addBorder >= 0 ? left - addBorder : left;
                int topBorderAdded = top - addBorder >= 0 ? top - addBorder : top;
                int widthBorderAdded = leftBorderAdded + width + addBorder * 2 > src.Width ?
                  (int)src.Width - leftBorderAdded : width + addBorder * 2;
                int heightBorderAdded = topBorderAdded + height + addBorder * 2 > src.Height ?
                    (int)src.Height - topBorderAdded : height + addBorder * 2;

                if (leftBorderAdded + widthBorderAdded > (int)src.Width)
                    throw new ArgumentException("leftBorderAdded + widthBorderAdded > (int)src.Width");
                if (topBorderAdded + heightBorderAdded > (int)src.Height)
                    throw new ArgumentException("topBorderAdded + heightBorderAdded > (int)src.Height");
                //CroppedBitmap cropped = new CroppedBitmap(src, new Int32Rect(leftBorderAdded, topBorderAdded,
                //    widthBorderAdded, heightBorderAdded));
                System.Drawing.Rectangle croppedRect = new System.Drawing.Rectangle(leftBorderAdded, topBorderAdded,
                    widthBorderAdded, heightBorderAdded);
                croppedImage = new System.Drawing.Bitmap(widthBorderAdded, heightBorderAdded);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(croppedImage))
                {
                    g.DrawImage(image, new System.Drawing.Rectangle(0, 0, widthBorderAdded, heightBorderAdded),
                        croppedRect,
                        System.Drawing.GraphicsUnit.Pixel);
                }

                //var resized = new Bitmap(size, size);
                //var resizedPalette = resized.Palette;
                //using (var graphics = Graphics.FromImage(resized))
                //{
                //    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                //    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                //    graphics.DrawImage(croppedImage, 0, 0, size, size);
                //    using (var output = System.IO.File.Open(thumbnailPath, FileMode.Create))
                //    {
                //        resized.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
                //    }
                //}
            }

            return croppedImage;
        }
    }
}
