namespace Xam.Plugins.OnDeviceCustomVision
{
    public class CrossImageClassifier
    {        
        protected CrossImageClassifier() { }
        
        private static ImageClassifierImplementation _implementation;
        public static IImageClassifier Current => _implementation ?? (_implementation = new ImageClassifierImplementation());
    }
}