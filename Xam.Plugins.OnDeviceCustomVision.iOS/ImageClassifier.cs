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
    public class ImageClassifierImplementation : ImageClassifierBase
    {
        private static readonly CGSize _targetImageSize = new CGSize(227, 227);
        private VNCoreMLModel _model;

        private VNCoreMLModel LoadModel(string modelName)
        {
            var modelPath = NSBundle.MainBundle.GetUrlForResource(modelName, "mlmodelc") ?? CompileModel(modelName);

            if (modelPath == null)
                throw new ImageClassifierException($"Model {modelName} does not exist");

            var mlModel = MLModel.Create(modelPath, out NSError err);

            if (err != null)
                throw new NSErrorException(err);

            var model = VNCoreMLModel.FromMLModel(mlModel, out err);

            if (err != null)
                throw new NSErrorException(err);

            return model;
        }

        private NSUrl CompileModel(string modelName)
        {
            var uncompiled = NSBundle.MainBundle.GetUrlForResource(modelName, "mlmodel");
            var modelPath = MLModel.CompileModel(uncompiled, out NSError err);

            if (err != null)
                throw new NSErrorException(err);

            return modelPath;
        }

        private async Task<IReadOnlyList<ImageClassification>> Classify(UIImage source)
        {
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

            var buffer = source.ToCVPixelBuffer(_targetImageSize);
            var requestHandler = new VNImageRequestHandler(buffer, new NSDictionary());

            requestHandler.Perform(new[] { request }, out NSError error);

            var classifications = await tcs.Task;

            if (error != null)
                throw new NSErrorException(error);
            
            return classifications.OrderByDescending(p => p.Probability)
                                  .ToList()
                                  .AsReadOnly();
        }

        public override async Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream)
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

        public override void Init(string modelName, ModelType modelType)
        {
            base.Init(modelName, modelType);
            
            try
            {
                _model = LoadModel(modelName);
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
            }
        }
    }
}
