using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Threading.Tasks;
using Xam.Plugins.OnDeviceCustomVision;
using System.Linq;
using Humanizer;

namespace CurrencyRecogniser
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            TakePhotoCommand = new Command(async () => await TakePhoto());
        }

        private async Task TakePhoto()
        {
            Image = null;
            Tag = " ";

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

            _photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { PhotoSize = PhotoSize.Medium });
            Image = ImageSource.FromStream(() => _photo.GetStream());

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            
            var classifications = await CrossImageClassifier.Current.ClassifyImage(_photo.GetStream());
            Tag = classifications.OrderByDescending(c => c.Probability).First().Tag.Humanize();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tag)));
        }

        private MediaFile _photo;

        public ImageSource Image { get; private set; }
        public string Tag { get; private set; }
        public ICommand TakePhotoCommand { get; }
    }
}
