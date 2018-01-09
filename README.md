# Xam.Plugin.OnDeviceCustomVision

The [Azure Custom Vision service](https://customvision.ai) is able to create models that can be exported as CoreML or Tensorflow models to do image classification.

This plugin makes it easy to download and use these models offline from inside your mobile app, using CoreML on iOS or Tensorflow on Android. These models can then be called from a .NET standard library or PCL, using something like Xam.Plugin.Media to take photos for classification.

#### Setup

* Available on NuGet: https://www.nuget.org/packages/Xam.Plugins.OnDeviceCustomVision/ [![NuGet](https://img.shields.io/nuget/v/Xam.Plugin.OnDeviceCustomVision.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugin.OnDeviceCustomVision/)
* Install into your PCL/.NET Standard project and iOS and Android client projects.

#### Usage

Before you can use this API, you need to initialise it with the model file downloaded from CustomVision.

##### iOS

Download the Core ML model from Custom Vision. Compile it using:

```bash
xcrun coremlcompiler compile <model file name>.mlmodel <model name>.mlmodelc
```

This will spit out a folder called `<model name>.mlmodelc` containing a number of files. Add this entire folder to the `Resources` folder in your iOS app. Once this has been added, add a call to `Init` to your app delegate, passing in the name of your compiled model (i.e. the name of the model folder __without__ `mlmodelc`):

```cs
public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
{
   ...
   CrossImageClassifier.Current.Init("<model name>");
   return base.FinishedLaunching(uiApplication, launchOptions);
}
```

##### Android

Download the tensorflow model from Custom Vision. This will be a folder containing two files.

* `labels.txt`
* `model.pb`

Add both these files to the `Assets` folder in your Android app. Once this is added, add a call to `Init` to your main activity passing in the name of the model file:

```cs
protected override void OnCreate(Bundle savedInstanceState)
{
   ...
   CrossImageClassifier.Current.Init("model.pb");
}
```

Note - the labels file must be present and called `labels.txt`.

##### Calling this from your PCL/.NET Standard library

To classify an image, call:

```cs
var tags = await CrossImageClassifier.Current.ClassifyImage(stream);
```

Passing in an image as a stream. You can use a library like [Xam.Plugin.Media](https://github.com/jamesmontemagno/MediaPlugin) to get an image as a stream from the camera or image library.

This will return a list of `ImageClassification` instances, one per tag in the model with the probabilty that the image matches that tag. Probabilities are doubles in the range of 0 - 1, with 1 being 100% probability that the image matches the tag. To find the most likely classification use:

```cs
tags..OrderByDescending(t => t.Probability)
     .First().Tag;
```


