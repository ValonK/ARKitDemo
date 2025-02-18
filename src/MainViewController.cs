using ARKitDemo.AR._3D;
using ARKitDemo.AR.Animations;
using ARKitDemo.AR.Draw;
using ARKitDemo.AR.Face;
using ARKitDemo.AR.ImageDetection;
using ARKitDemo.AR.Measurement;
using ARKitDemo.AR.Placement;
using ARKitDemo.Controls;
using ARKitDemo.Helpers;
using ARKitDemo.Models;

namespace ARKitDemo;

public class MainViewController : UIViewController, 
    IUICollectionViewDataSource,
    IUICollectionViewDelegateFlowLayout
{
    private const UIModalPresentationStyle ModalStyle = UIModalPresentationStyle.PageSheet;
    private static readonly UIEdgeInsets SectionInsets = new(10, 20, 10, 20);

    private HeaderView _headerGifView;
    private UICollectionView _collectionView;
    private readonly List<Feature> _features = GetFeatures();

    private readonly Dictionary<string, Func<UIViewController>> _featureControllers = new()
    {
        { "Image Detection", () => new ImageDetectionViewController() },
        { "3D Model placement", () => new ModelPlacementController() },
        { "Measurement", () => new MeasurementController() },
        { "Placement", () => new VideoPlacementController() },
        { "Draw", () => new DrawingController() },
        { "Animation", () => new AnimationController() },
        { "Face Tracking", () => new FaceTrackingController() }
    };

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        EdgesForExtendedLayout = UIRectEdge.All;

        View!.ClipsToBounds = false;
        View!.BackgroundColor = UIColor.White;

        SetupHeaderView();
        SetupCollectionView();

        SetupConstraints();
    }

    private static List<Feature> GetFeatures() =>
    [
        new(UIImage.FromBundle("ic_image_recognition.png"), "Image Detection", "Detect images in the real world."),
        new(UIImage.FromBundle("ic_3dmodel.png"), "3D Model placement", "Place 3D models in the real world."),
        new(UIImage.FromBundle("ic_measure.png"), "Measurement", "Measure distances in the real world."),
        new(UIImage.FromBundle("ic_video.png"), "Placement", "Place a video player in the real world."),
        new(UIImage.FromBundle("ic_draw.png"), "Draw", "Draw on the real world."),
        new(UIImage.FromBundle("ic_animation.png"), "Animation", "Place geometry with animations."),
        new(UIImage.FromBundle("ic_facetracking.png"), "Face Tracking", "Can be used for Face filters.")
    ];

    private void SetupHeaderView()
    {
        _headerGifView = new HeaderView();
        _headerGifView.SetupAnimatedGif();
        View!.AddSubview(_headerGifView);
    }

    private void SetupCollectionView()
    {
        var layout = new UICollectionViewFlowLayout
        {
            ItemSize = new CGSize(View!.Bounds.Width - 40, 80),
            MinimumLineSpacing = 10,
            SectionInset = SectionInsets
        };

        _collectionView = new UICollectionView(CGRect.Empty, layout)
        {
            BackgroundColor = UIColor.White,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _collectionView.RegisterClassForCell(typeof(FeatureCollectionViewCell), "FeatureViewCell");
        var featureSource = new FeatureCollectionViewSource(_features, OnFeatureSelected);
        _collectionView.DataSource = featureSource;
        _collectionView.Delegate = featureSource;

        View.AddSubview(_collectionView);
    }

    private void SetupConstraints()
    {
        SetupHeaderViewConstraints();
        SetupCollectionViewConstraints();
    }

    private void SetupHeaderViewConstraints()
    {
        _headerGifView.TopAnchor.ConstraintEqualTo(View!.TopAnchor).Active = true;
        _headerGifView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
        _headerGifView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        _headerGifView.HeightAnchor.ConstraintEqualTo(250).Active = true;
    }

    private void SetupCollectionViewConstraints()
    {
        _collectionView.TopAnchor.ConstraintEqualTo(_headerGifView.BottomAnchor, 10).Active = true;
        _collectionView.LeadingAnchor.ConstraintEqualTo(View!.LeadingAnchor).Active = true;
        _collectionView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        _collectionView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
    }

    private async void OnFeatureSelected(Feature feature)
    {
        await PrerequisitesHelper.Check(this);

        if (!_featureControllers.TryGetValue(feature.Name, out var controllerFactory)) return;
        
        var controller = controllerFactory.Invoke();
        controller.ModalPresentationStyle = ModalStyle;
        PresentViewController(controller, true, null);
    }

    public override bool PrefersStatusBarHidden() => false;

    public override UIStatusBarStyle PreferredStatusBarStyle() => UIStatusBarStyle.LightContent;
}