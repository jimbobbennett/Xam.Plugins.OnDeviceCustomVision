# Xam.Plugins.OnDeviceCustomVision

The [Azure Custom Vision service](https://customvision.ai/?WT.mc_id=customvision-github-jabenn) is able to create models that can be exported as CoreML, TensorFlow or ONNX models to do image classification on device.

This plugin makes it easy to download and use these models offline from inside your mobile app, using CoreML on iOS, TensorFlow on Android and Windows ML on Windows. These models can then be called from a .NET standard library, using something like Xam.Plugins.Media to take photos for classification.

## Setup

* Available on NuGet: https://www.nuget.org/packages/Xam.Plugins.OnDeviceCustomVision/ [![NuGet](https://img.shields.io/nuget/v/Xam.Plugins.OnDeviceCustomVision.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugins.OnDeviceCustomVision/)
* Install into your .NET Standard project and iOS, Android and UWP client projects.

## Platform Support

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.iOS|iOS 11+|
|Xamarin.Android|API 21+|
|UWP|Windows SDK 17110+|

## Usage

Before you can use this API, you need to initialize it with the model file downloaded from CustomVision. Trying to classify an image without calling `Init` will result in a `ImageClassifierException` being thrown.

> Each platform has it's own platform-specific `Init` method on a platform-specific static class. This is due to differences in the way each platform handles the models.

### iOS

Export, then download the Core ML model from the Custom Vision service.

#### Pre-compiled models

Models can be compiled before being used, or compiled on the device. To use a pre-compiled model, compile the downloaded model using:

```bash
xcrun coremlcompiler compile <model_file_name>.mlmodel <model_name>.mlmodelc
```

This will spit out a folder called `<model_name>.mlmodelc` containing a number of files. Add this entire folder to the `Resources` folder in your iOS app. Once this has been added, add a call to `Init` to your app delegate, passing in the name of your compiled model without the extension (i.e. the name of the model folder __without__ `mlmodelc`):

```cs
using Xam.Plugins.OnDeviceCustomVision;
...
public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
{
   ...
   iOSImageClassifier.Init("<model_name>");
   return base.FinishedLaunching(uiApplication, launchOptions);
}
```

#### Uncompiled models

Add the downloaded model, called `<model_name>.mlmodel`, to the `Resources` folder in your iOS app.Once this has been added, add a call to `Init` to your app delegate, passing in the name of your model without the extension (i.e. the name of the model folder __without__ `mlmodel`):

```cs
using Xam.Plugins.OnDeviceCustomVision;
...
public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
{
   ...
   iOSImageClassifier.Init("<model_name>");
   return base.FinishedLaunching(uiApplication, launchOptions);
}
```

The call to `Init` will attempt to compile the model, throwing a `ImageClassifierException` if the compile fails.

### Android

Export, then download the TensorFlow model from the Custom Vision service. This will be a zip file, and when unzipped this will be a folder containing two files.

* `labels.txt`
* `model.pb`

Add both these files to the `Assets` folder in your Android app. Once these are added, add a call to `Init` to your main activity passing in the name of the model file, name of the labels file and the type of model downloaded from the custom vision service. The model and labels file names have default arguments of `"models.pb"` and `"labels.txt"`, so you only need to set these if you have changed the names of these files.

```cs
using Xam.Plugins.OnDeviceCustomVision;
...
protected override void OnCreate(Bundle savedInstanceState)
{
   ...
   AndroidImageClassifier.Current.Init("model.pb", "labels.txt", ModelType.General);
}
```

> The model type is used to provide adjustments to the image color scheme, and this is only necessary for models [trained before 7th May 2018](https://github.com/Azure-Samples/cognitive-services-android-customvision-sample#compatibility), models trained after this date do not need any adjustments. The library will work out for you if adjustments need to be made, but if the model was generated after 7th May 2018 you can leave the model type as the default value.

### Windows

Export, then download the ONNX model from the Custom Vision service. Add the downloaded model to the `Assets` folder in your UWP app and ensure the **Build Action** is set to **Content**. A `<model_name>.cs` file will be created in the root folder of your app to provide a wrapper around the model, but this wrapper is not needed as this plugin provides its own wrapper.

> Don't delete the wrapper class yet, as you will need it to get the labels for the model.

You need to pass the name of the model, along with a list of the models labels in the order that the tags have been defined in the model. You can get this order by opening the auto-generated `<model_name>.cs` file and copying the order from there. The wrapper will have a class called `<model_name>ModelOutput` and in the constructor for this class will be some code to create a dictionary called `loss`:

```cs
this.loss = new Dictionary<string, float>()
{
    { "<label 1>", float.NaN },
    { "<label 2>", float.NaN },
    ...
};
```

This defines the labels in the correct order. Pass them to the `Init` method along with the model name inside the `MainPage` class in your UWP app, or similar page class. The `Init` method is `async` so will need to be awaited in an appropriate place, such as by overriding `OnNavigatedTo`.

```cs
using Xam.Plugins.OnDeviceCustomVision;
...
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    await WindowsImageClassifier.Init("Currency", new[] { "<label 1>", "<label 2>", ... });
    base.OnNavigatedTo(e);
}
```

### Calling this from your .NET Standard library

To classify an image, call:

```cs
var tags = await CrossImageClassifier.Current.ClassifyImage(stream);
```

Passing in an image as a stream. You can use a library like [Xam.Plugins.Media](https://github.com/jamesmontemagno/MediaPlugin) to get an image as a stream from the camera or image library.

This will return a list of `ImageClassification` instances, one per tag in the model with the probability that the image matches that tag. Probabilities are doubles in the range of 0 - 1, with 1 being 100% probability that the image matches the tag. To find the most likely classification use:

```cs
tags.OrderByDescending(t => t.Probability)
    .First().Tag;
```

### Using with an IoC container

`CrossImageClassifier.Current` returns an instance of the `IImageClassifier` interface, and this can be stored inside your IoC container and injected where required.
