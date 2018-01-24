using Android.Graphics;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public static class ImageExtensions
    {
        private static readonly float ImageStd = 1.0f;

        public static float ImageMeanR(this ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Retail:
                    return 0.0f;
                case ModelType.General:
                case ModelType.Landscape:
                default:
                    return 123.0f;
            }
        }

        public static float ImageMeanG(this ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Retail:
                    return 0.0f;
                case ModelType.General:
                case ModelType.Landscape:
                default:
                    return 117.0f;
            }
        }

        public static float ImageMeanB(this ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.Retail:
                    return 0.0f;
                case ModelType.General:
                case ModelType.Landscape:
                default:
                    return 104.0f;
            }
        }

        public static float[] GetBitmapPixels(this Bitmap bitmap, int width, int height, 
                                              float imageMeanR, float imageMeanG, float imageMeanB)
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

                        floatValues[i * 3 + 0] = ((val & 0xFF) - imageMeanB) / ImageStd;
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - imageMeanG) / ImageStd;
                        floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - imageMeanR) / ImageStd;
                    }

                    resizedBitmap.Recycle();
                }

                scaledBitmap.Recycle();
            }

            return floatValues;    
        }
    }
}
