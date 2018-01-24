using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Org.Tensorflow.Contrib.Android;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public class ImageClassifierImplementation : ImageClassifierBase
    {
        private List<string> _labels;
        private TensorFlowInferenceInterface _inferenceInterface;

        private static readonly int InputSize = 227;
        private static readonly string InputName = "Placeholder";
        private static readonly string OutputName = "loss";

        public override async Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream)
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

        public override void Init(string modelName, ModelType modelType)
        {
            base.Init(modelName, modelType);

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

        private List<ImageClassification> RecognizeImage(Bitmap bitmap)
        {
            var outputNames = new[] { OutputName };
            var floatValues = bitmap.GetBitmapPixels(InputSize, InputSize,
                                                     ModelType.ImageMeanR(), ModelType.ImageMeanG(), ModelType.ImageMeanB());
            var outputs = new float[_labels.Count];

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
