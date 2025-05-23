using ARKit;
using ARKitDemo.Extensions;
using ImageIO;
using SceneKit;

namespace ARKitDemo.AR.ImageDetection;

internal class ImageDetectionViewController : BaseViewController, IARSessionDelegate, IARSCNViewDelegate
{
    private readonly Dictionary<string, string> _images = new()
    {
        { "skruf_logo_1.png", "Skruf" },
    };

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        SceneView.Delegate = this;
        SceneView.Session.Delegate = this;
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

        var detectionImages = LoadReferenceImages();
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

        SceneView.Session.Run(config, ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
    }

    private NSMutableSet<ARReferenceImage> LoadReferenceImages()
    {
        var detectionImages = new NSMutableSet<ARReferenceImage>();

        foreach (var imageName in _images.Keys)
        {
            var image = UIImage.FromBundle(imageName);
            if (image?.CGImage == null)
            {
                Console.WriteLine($"Failed to load image: {imageName}");
                continue;
            }

            var referenceImage = new ARReferenceImage(image.CGImage, CGImagePropertyOrientation.Up, 0.1f)
            {
                Name = Path.GetFileNameWithoutExtension(imageName)
            };

            detectionImages.Add(referenceImage);
        }

        return detectionImages;
    }

    public void DidAddNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
    {
        InvokeOnMainThread(() =>
        {
            if (anchor is not ARImageAnchor imageAnchor) return;

            Console.WriteLine($"Detected Image: {imageAnchor.ReferenceImage.Name}");

            if (!_images.TryGetValue($"{imageAnchor.ReferenceImage.Name}.png", out var text)) return;

            var textPlaneNode = CreateBlurredTextPlane(text);
            textPlaneNode.Position = new SCNVector3(0, 0.1f, 0);
            node.AddChildNode(textPlaneNode);

            var lineNode = new SCNVector3(0, 0, 0).CreateLineNode(textPlaneNode.Position, 0.001f);
            node.AddChildNode(lineNode);
        });
    }

    private static SCNNode CreateBlurredTextPlane(string text)
    {
        var containerView = new UIView(new CGRect(0, 0, 300, 100)) { BackgroundColor = UIColor.Clear };

        var blurView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraDark))
        {
            Frame = containerView.Bounds,
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
        };
        containerView.AddSubview(blurView);

        var label = new UILabel(containerView.Bounds)
        {
            Text = text,
            TextAlignment = UITextAlignment.Center,
            TextColor = UIColor.White,
            Font = UIFont.BoldSystemFontOfSize(24),
            AdjustsFontSizeToFitWidth = true,
            BackgroundColor = UIColor.Clear
        };
        containerView.AddSubview(label);

        UIGraphics.BeginImageContextWithOptions(containerView.Bounds.Size, false, 0);
        containerView.Layer.RenderInContext(UIGraphics.GetCurrentContext());
        var renderedImage = UIGraphics.GetImageFromCurrentImageContext();
        UIGraphics.EndImageContext();

        var plane = SCNPlane.Create(0.2f, 0.07f);
        plane.FirstMaterial!.Diffuse.Contents = renderedImage;
        plane.FirstMaterial.DoubleSided = true;

        var node = new SCNNode { Geometry = plane };
        node.Pivot = SCNMatrix4.CreateTranslation(0, -0.035f, 0);

        return node;
    }
    
    public void DidFail(ARSession session, NSError error) =>
        Console.WriteLine($"AR Session Failed: {error.LocalizedDescription}");

    public void WasInterrupted(ARSession session) =>
        Console.WriteLine("AR Session was interrupted.");

    public void InterruptionEnded(ARSession session) =>
        session.Run(session.Configuration!, ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
}