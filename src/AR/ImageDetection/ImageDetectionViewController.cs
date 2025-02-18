using ARKit;
using ImageIO;

namespace ARKitDemo.AR.ImageDetection;

internal class ImageDetectionViewController : BaseViewController
{
    public readonly Dictionary<string, string> Images = new()
    {
        {"skruf_logo_1.png", "Skruf"},
    };

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        SceneView.Delegate = new ImageDetectionArSceneDelegate(this);
        SceneView.Session = new ARSession();
        SceneView.Session.Delegate = new ArSessionDelegateImpl();

        View!.AddSubview(SceneView);
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        SceneView.Frame = View!.Bounds;
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

        SceneView.Session.Run(config,
            ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
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
}