using Android.Graphics;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public static class ImageExtensions
    {
        private static readonly float ImageMeanR = 124.0f;
        private static readonly float ImageMeanG = 117.0f;
        private static readonly float ImageMeanB = 105.0f;
        private static readonly float ImageStd = 1.0f;

        public static float[] GetBitmapPixels(this Bitmap bitmap, int width, int height)
        {
            var floatValues = new float[width * height * 3];

            using (var scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, false))
            {
                using (var resizedBitmap = scaledBitmap.Copy(Bitmap.Config.Argb8888, false))
                {
                    var intValues = new int[width * height];
                    resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        var val = intValues[i];

                        floatValues[i * 3 + 0] = ((val & 0xFF) - ImageMeanB) / ImageStd;
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - ImageMeanG) / ImageStd;
                        floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - ImageMeanR) / ImageStd;
                    }

                    resizedBitmap.Recycle();
                }

                scaledBitmap.Recycle();
            }

            return floatValues;    
        }
    }
}
