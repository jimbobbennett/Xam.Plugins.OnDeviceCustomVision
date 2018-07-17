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
}
