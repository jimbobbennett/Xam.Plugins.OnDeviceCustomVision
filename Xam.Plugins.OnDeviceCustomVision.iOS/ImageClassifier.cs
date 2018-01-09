using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using CoreML;
using Foundation;
using UIKit;
using Vision;
using Xam.Plugins.OnDeviceCustomVision.iOS;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public class ImageClassifierImplementation : IImageClassifier
    {
        private static readonly CGSize _targetImageSize = new CGSize(227, 227);
        private VNCoreMLModel _model;

        private VNCoreMLModel LoadModel(string modelName)
        {
            var assetPath = NSBundle.MainBundle.GetUrlForResource(modelName, "mlmodelc");

            var mlModel = MLModel.Create(assetPath, out NSError err);

            if (err != null)
                throw new NSErrorException(err);

            var model = VNCoreMLModel.FromMLModel(mlModel, out err);

            if (err != null)
                throw new NSErrorException(err);

            return model;
        }

        private async Task<IReadOnlyList<ImageClassification>> Classify(UIImage source)
        {
            var requestHandler = new VNImageRequestHandler(source.ToCVPixelBuffer(_targetImageSize), new NSDictionary());

            var tcs = new TaskCompletionSource<IEnumerable<ImageClassification>>();

            var request = new VNCoreMLRequest(_model, (response, e) => 
            {
                if (e != null)
                    tcs.SetException(new NSErrorException(e));
                else
                {
                    var results = response.GetResults<VNClassificationObservation>();
                    tcs.SetResult(results.Select(r => new ImageClassification(r.Identifier, r.Confidence)).ToList());
                }
            });

            requestHandler.Perform(new[] { request }, out NSError error);
            var classifications = await tcs.Task;

            if (error != null)
                throw new NSErrorException(error);
            
            return classifications.OrderByDescending(p => p.Probability)
                                  .ToList()
                                  .AsReadOnly();
        }

        public async Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream)
        {
            if (_model == null)
                throw new ImageClassifierException("You must call Init before classifying images");

            try
            {
                var image = await imageStream.ToUIImage();
                return await Classify(image);
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
                _model = LoadModel("ToyIdentifier");
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
            }
        }
    }
}
