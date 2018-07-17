using Android.Graphics;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public enum ModelType
    {
        General,
        Landscape,
        Retail
    }

    public static class ImageExtensions
    {
        private static readonly float ImageStd = 1.0f;

        private static float ImageMeanR(ModelType modelType)
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

        private static float ImageMeanG(ModelType modelType)
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

        private static float ImageMeanB(ModelType modelType)
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

        public static float[] GetBitmapPixels(this Bitmap bitmap, int width, int height, ModelType modelType, bool hasNormalizationLayer)
        {
            var floatValues = new float[width * height * 3];
            var imageMeanB = hasNormalizationLayer ? 0f : ImageMeanB(modelType);
            var imageMeanG = hasNormalizationLayer ? 0f : ImageMeanG(modelType);
            var imageMeanR = hasNormalizationLayer ? 0f : ImageMeanR(modelType);

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
