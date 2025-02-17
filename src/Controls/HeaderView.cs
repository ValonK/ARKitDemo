using ImageIO;

namespace ARKitDemo.Controls;

public sealed class HeaderView : UIView
{
    private readonly UIImageView _gifImageView;
    private const double FrameDelay = 0.1;
    private const float BlurAlpha = 0.7f;

    public HeaderView()
    {
        TranslatesAutoresizingMaskIntoConstraints = false;
        BackgroundColor = UIColor.Clear;

        _gifImageView = new UIImageView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            ContentMode = UIViewContentMode.ScaleAspectFill,
            ClipsToBounds = true,
            Opaque = false  
        };
        AddSubview(_gifImageView);

        var blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
        var blurView = new UIVisualEffectView(blurEffect)
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Alpha = BlurAlpha
        };
        AddSubview(blurView);

        var headerLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Text = "ARKit",
            Font = UIFont.BoldSystemFontOfSize(50),
            TextColor = UIColor.White,
            TextAlignment = UITextAlignment.Center,
        };
        AddSubview(headerLabel);

        BringSubviewToFront(blurView);
        BringSubviewToFront(headerLabel);

        AddConstraintsToFillSuperview(_gifImageView);
        AddConstraintsToFillSuperview(blurView);

        headerLabel.CenterXAnchor.ConstraintEqualTo(CenterXAnchor).Active = true;
        headerLabel.CenterYAnchor.ConstraintEqualTo(CenterYAnchor, 20).Active = true;
    }
    
    public void SetupAnimatedGif()
    {
        var gifPath = NSBundle.MainBundle.PathForResource("background", "gif");
        using var gifData = NSData.FromFile(gifPath);
        using var imageSource = CGImageSource.FromData(gifData);
        if (imageSource == null) return;

        var frames = ExtractFrames(imageSource, out var totalDuration);
        if (frames.Count <= 0) return;

        _gifImageView.AnimationImages = frames.ToArray();
        _gifImageView.AnimationDuration = totalDuration * 0.8;
        _gifImageView.AnimationRepeatCount = 0; 
        _gifImageView.StartAnimating();
    }

    private void AddConstraintsToFillSuperview(UIView view)
    {
        view.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
        view.LeadingAnchor.ConstraintEqualTo(LeadingAnchor).Active = true;
        view.TrailingAnchor.ConstraintEqualTo(TrailingAnchor).Active = true;
        view.BottomAnchor.ConstraintEqualTo(BottomAnchor).Active = true;
    }

    private static List<UIImage> ExtractFrames(CGImageSource imageSource, out double totalDuration)
    {
        var frames = new List<UIImage>();
        totalDuration = 0;

        var frameCount = (int)imageSource.ImageCount;
        for (var i = 0; i < frameCount; i++)
        {
            using var cgImage = imageSource.CreateImage(i, null!);
            if (cgImage == null)
                continue;

            frames.Add(UIImage.FromImage(cgImage));
            totalDuration += FrameDelay;
        }

        if (totalDuration == 0 && frames.Count > 0)
            totalDuration = frames.Count * FrameDelay;

        return frames;
    }
}