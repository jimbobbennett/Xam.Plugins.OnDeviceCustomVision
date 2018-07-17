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

    public interface IImageClassifier
    {
        Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream);
    }

    public abstract class ImageClassifierBase : IImageClassifier
    {
        public abstract Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream);
    }

    public class CrossImageClassifier
    {
        protected CrossImageClassifier() { }

#if !NETSTANDARD1_0
        private static ImageClassifierImplementation _implementation;
#endif

        public static IImageClassifier Current
        {
            get
            {
#if NETSTANDARD1_0
                throw new NotImplementedException("Please ensure you have install the Xam.Plugins.OnDeviceCustomVision NuGet package into your iOS, Android and UWP projects");
#else
                return _implementation ?? (_implementation = new ImageClassifierImplementation());
#endif
            }
        }
    }
}
