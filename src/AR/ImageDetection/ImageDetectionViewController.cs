using ARKit;
using AVFoundation;
using ImageIO;

namespace ARKitDemo.AR.ImageDetection;

public class ImageDetectionViewController : UIViewController
{
    private ARSCNView _sceneView;

    public readonly Dictionary<string, string> Images = new()
    {
        {"test.png", "Skruf"},
    };

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        if (!ARConfiguration.IsSupported)
        {
            ShowUnsupportedAlert();
            return;
        }

        RequestCameraPermission();

        _sceneView = new ARSCNView
        {
            Frame = View!.Bounds,
            AutoenablesDefaultLighting = true,
        };

        _sceneView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

        _sceneView.Delegate = new ImageDetectionArSceneDelegate(this);
        _sceneView.Session = new ARSession();
        _sceneView.Session.Delegate = new ArSessionDelegateImpl();

        View.AddSubview(_sceneView);
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        _sceneView.Frame = View!.Bounds;
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);

        var detectionImages = LoadDetectionImages();
        if (detectionImages.Count == 0)
        {
            Console.WriteLine("No images loaded for detection.");
            return;
        }

        var config = new ARWorldTrackingConfiguration
        {
            DetectionImages = new NSSet<ARReferenceImage>(detectionImages),
            MaximumNumberOfTrackedImages = (nint)detectionImages.Count
        };

        _sceneView.Session.Run(config,
            ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        _sceneView.Session.Pause();
    }

    private NSMutableSet<ARReferenceImage> LoadDetectionImages()
    {
        var detectionImages = new NSMutableSet<ARReferenceImage>();

        foreach (var imageName in Images.Keys)
        {
            var image = UIImage.FromBundle(imageName);
            if (image == null)
            {
                Console.WriteLine($"Failed to load image: {imageName}");
                continue;
            }

            if (image.CGImage == null)
            {
                Console.WriteLine($"CGImage is null: {imageName}");
                continue;
            }

            var referenceImage = new ARReferenceImage(image.CGImage,
                CGImagePropertyOrientation.Up, 0.1f)
            {
                Name = Path.GetFileNameWithoutExtension(imageName)
            };

            detectionImages.Add(referenceImage);
        }

        return detectionImages;
    }
    
    private async void RequestCameraPermission()
    {
        var status = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
        switch (status)
        {
            case AVAuthorizationStatus.NotDetermined:
            {
                var granted = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Video);
                if (!granted)
                {
                    ShowPermissionAlert();
                }
                break;
            }
            case AVAuthorizationStatus.Denied:
            case AVAuthorizationStatus.Restricted:
                ShowPermissionAlert();
                break;
        }
    }
    
    private void ShowPermissionAlert()
    {
        InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create(
                "Camera Permission",
                "Camera access is required for AR functionality. Please update your settings.",
                UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        });
    }
    
    private void ShowUnsupportedAlert()
    {
        InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create(
                "AR Not Supported",
                "This device does not support ARKit.",
                UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        });
    }
}