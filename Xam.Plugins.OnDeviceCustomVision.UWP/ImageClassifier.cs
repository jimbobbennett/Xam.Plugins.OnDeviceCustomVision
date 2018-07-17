using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning.Preview;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;

namespace Xam.Plugins.OnDeviceCustomVision
{
    internal class ModelOutput
    {
        public IList<string> ClassLabel { get; set; }
        public IDictionary<string, float> Loss { get; set; }

        public ModelOutput(IEnumerable<string> labels)
        {
            ClassLabel = new List<string>();
            Loss = new Dictionary<string, float>();

            foreach (var l in labels)
                Loss[l] = float.NaN;
        }
    }
    
    internal class Model
    {
        private LearningModelPreview _learningModel;
        private readonly IEnumerable<string> _labels;

        private Model(IEnumerable<string> labels)
        {
            _labels = labels;
        }
        
        public static async Task<Model> CreateModel(StorageFile file, IEnumerable<string> labels)
        {
            var learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            return new Model(labels)
            {
                _learningModel = learningModel
            };
        }

        public async Task<ModelOutput> EvaluateAsync(VideoFrame videoFrame)
        {
            ModelOutput output = new ModelOutput(_labels);
            LearningModelBindingPreview binding = new LearningModelBindingPreview(_learningModel);
            binding.Bind("data", videoFrame);
            binding.Bind("classLabel", output.ClassLabel);
            binding.Bind("loss", output.Loss);
            LearningModelEvaluationResultPreview evalResult = await _learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }

    public class WindowsImageClassifier : CrossImageClassifier
    {
        public static Task Init(string modelName, IEnumerable<string> labels)
        {
            return ((ImageClassifierImplementation)Current).Init(modelName, labels);
        }
    }

    public class ImageClassifierImplementation : ImageClassifierBase
    {
        private Model _model;

        public override async Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream)
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream.AsRandomAccessStream());
            var sfbmp = await decoder.GetSoftwareBitmapAsync();
            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(sfbmp);

            var output = await _model.EvaluateAsync(videoFrame);

            return output.Loss.Select(l => new ImageClassification(l.Key, l.Value)).ToList().AsReadOnly();
        }

        public async Task Init(string modelName, IEnumerable<string> labels)
        {
            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{modelName}.onnx"));
                _model = await Model.CreateModel(file, labels);
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
            }
        }
    }
}
