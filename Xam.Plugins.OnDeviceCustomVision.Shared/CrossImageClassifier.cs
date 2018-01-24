using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xam.Plugins.OnDeviceCustomVision
{
    public struct ImageClassification
    {
        public ImageClassification(string tag, double probability)
        {
            Tag = tag;
            Probability = probability;
        }

        public string Tag { get; }
        public double Probability { get; }

        public override string ToString() => $"Tag={Tag}, Probability={Probability:N2}";
    }

    public class ImageClassifierException : Exception
    {
        public ImageClassifierException(string message) : base(message) { }
        public ImageClassifierException(string message, Exception innerException) : base(message, innerException) { }
    }

    public enum ModelType
    {
        General,
        Landscape,
        Retail
    }

    public interface IImageClassifier
    {
        void Init(string modelName, ModelType modelType);
        Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream);
    }

    public abstract class ImageClassifierBase : IImageClassifier
    {
        protected string ModelName { get; private set; }
        protected ModelType ModelType { get; private set; }

        public virtual void Init(string modelName, ModelType modelType)
        {
            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentException("modelName must be set", nameof(modelName));

            ModelName = modelName;
            ModelType = modelType;
        }

        public abstract Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream);
    }

    public static class CrossImageClassifier
    {
#if !NETSTANDARD1_0
        private static ImageClassifierImplementation _implementation;
#endif

        public static IImageClassifier Current
        {
            get
            {
#if NETSTANDARD1_0
                throw new NotImplementedException("Please ensure you have install the Xam.Plugins.OnDeviceCustomVision NuGet package into your iOS and Android projects");
#else
                return _implementation ?? (_implementation = new ImageClassifierImplementation());
#endif
            }
        }
    }
}
