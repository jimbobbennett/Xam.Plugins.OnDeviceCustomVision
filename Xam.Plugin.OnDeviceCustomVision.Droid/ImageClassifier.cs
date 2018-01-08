using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Org.Tensorflow.Contrib.Android;

namespace Xam.Plugin.OnDeviceCustomVision
{
    public class ImageClassifierImplementation : IImageClassifier
    {
        private List<string> _labels;
        private TensorFlowInferenceInterface _inferenceInterface;

        private static readonly int InputSize = 227;
        private static readonly float ImageMeanR = 124.0f;
        private static readonly float ImageMeanG = 117.0f;
        private static readonly float ImageMeanB = 105.0f;
        private static readonly float ImageStd = 1.0f;
        private static readonly string InputName = "Placeholder";
        private static readonly string OutputName = "loss";

        public async Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream)
        {
            if (_labels == null || !_labels.Any() || _inferenceInterface == null)
                throw new ImageClassifierException("You must call Init before classifying images");

            try
            {
                using (var bitmap = await BitmapFactory.DecodeStreamAsync(imageStream))
                {
                    var retVal = await Task.Run(() => RecognizeImage(bitmap).AsReadOnly());
                    bitmap.Recycle();
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to classify image - check the inner exception for more details", ex);
            }
        }

        public void Init(string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentException("modelName must be set", nameof(modelName));

            try
            {
                var assets = Android.App.Application.Context.Assets;
                using (var sr = new StreamReader(assets.Open("labels.txt")))
                {
                    var content = sr.ReadToEnd();
                    _labels = content.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                }

                _inferenceInterface = new TensorFlowInferenceInterface(assets, modelName);
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
            }
        }

        public List<ImageClassification> RecognizeImage(Bitmap bitmap)
        {
            var ggg = new int[bitmap.Width * bitmap.Height];
            bitmap.GetPixels(ggg, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);

            using (var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, InputSize, InputSize, false).Copy(Bitmap.Config.Argb8888, false))
            {
                var outputNames = new[] { OutputName };
                var intValues = new int[InputSize * InputSize];
                var floatValues = new float[InputSize * InputSize * 3];
                var outputs = new float[_labels.Count];

                resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);
                resizedBitmap.Recycle();

                for (int i = 0; i < intValues.Length; ++i)
                {
                    var val = intValues[i];

                    floatValues[i * 3 + 0] = ((val & 0xFF) - ImageMeanB) / ImageStd;
                    floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - ImageMeanG) / ImageStd;
                    floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - ImageMeanR) / ImageStd;
                }

                _inferenceInterface.Feed(InputName, floatValues, 1, InputSize, InputSize, 3);
                _inferenceInterface.Run(outputNames);
                _inferenceInterface.Fetch(OutputName, outputs);

                var results = new List<ImageClassification>();
                for (var i = 0; i < outputs.Length; ++i)
                    results.Add(new ImageClassification(_labels[i], outputs[i]));

                return results;
            }
        }
    }
}
