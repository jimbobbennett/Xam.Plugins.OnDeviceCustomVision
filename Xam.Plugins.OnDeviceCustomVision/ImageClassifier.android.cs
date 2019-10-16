using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Org.Tensorflow;
using Org.Tensorflow.Contrib.Android;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public class AndroidImageClassifier : CrossImageClassifier
    {
        public static void Init(string modelName = "model.pb", string labelsFileName = "labels.txt", ModelType modelType = ModelType.General)
        {
            ((ImageClassifierImplementation)Current).Init(modelName, labelsFileName, modelType);
        }
    }

    public class ImageClassifierImplementation : ImageClassifierBase
    {
        private List<string> _labels;
        private TensorFlowInferenceInterface _inferenceInterface;
        private bool _hasNormalizationLayer;
        private ModelType _modelType;

        private static int InputSize = 227;
        private const string InputName = "Placeholder";
        private const string OutputName = "loss";
        private const string DataNormLayerPrefix = "data_bn";

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

        internal void Init(string modelName, string labelsFileName, ModelType modelType)
        {
            _modelType = modelType;

            try
            {
                var assets = Android.App.Application.Context.Assets;
                using (var sr = new StreamReader( File.Exists(labelsFileName)? File.OpenRead(labelsFileName) : assets.Open(labelsFileName)))
                {
                    var content = sr.ReadToEnd();
                    _labels = content.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                }

                _inferenceInterface = new TensorFlowInferenceInterface(assets, modelName);
                InputSize = Convert.ToInt32(_inferenceInterface.GraphOperation(InputName).Output(0).Shape().Size(1));
                var iter = _inferenceInterface.Graph().Operations();
                while (iter.HasNext && !_hasNormalizationLayer)
                {
                    var op = iter.Next() as Operation;
                    if (op.Name().Contains(DataNormLayerPrefix))
                    {
                        _hasNormalizationLayer = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
            }
        }

        private List<ImageClassification> RecognizeImage(Bitmap bitmap)
        {
            var outputNames = new[] { OutputName };
            var floatValues = bitmap.GetBitmapPixels(InputSize, InputSize, _modelType, _hasNormalizationLayer);
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
