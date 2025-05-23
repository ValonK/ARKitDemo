using ARKitDemo.AR.Debug;
using ARKitDemo.AR.Face;
using ARKitDemo.AR.ImageDetection;
using ARKitDemo.AR.Measurement;
using ARKitDemo.AR.Model;
using ARKitDemo.AR.Placement;
using ARKitDemo.Controls;
using ARKitDemo.Helpers;
using ARKitDemo.Models;

namespace ARKitDemo;

internal class MainViewController : UIViewController,
    IUICollectionViewDataSource,
    IUICollectionViewDelegateFlowLayout
{
    private const UIModalPresentationStyle ModalStyle = UIModalPresentationStyle.PageSheet;
    private static readonly UIEdgeInsets SectionInsets = new(10, 20, 10, 20);

    private UICollectionViewFlowLayout _flowLayout;
    private UIStackView _stackView;
    private NSLayoutConstraint _headerHeightConstraint;
    private NSLayoutConstraint _headerWidthConstraint;
    private readonly List<Feature> _features;

    private static readonly List<FeatureDefinition> Definitions =
    [
        new(UIImage.FromBundle("ic_draw.png"),
            "Debug",
            "Show Debug Information",
            typeof(DebugController)),

        new(UIImage.FromBundle("ic_image_recognition.png"),
            "Image Detection",
            "Detect images in the real world.",
            typeof(ImageDetectionViewController)),

        new(UIImage.FromBundle("ic_3dmodel.png"),
            "3D Model Placement",
            "Place 3D models in the real world.",
            typeof(ModelPlacementController)),

        new(UIImage.FromBundle("ic_measure.png"),
            "Measurement", "Measure distances in the real world.",
            typeof(MeasurementController)),

        new(UIImage.FromBundle("ic_video.png"),
            "Video Placement",
            "Place a video player in the real world.",
            typeof(VideoPlacementController)),

        new(UIImage.FromBundle("ic_facetracking.png"),
            "Face Tracking", "Can be used for Face filters.",
            typeof(FaceTrackingController))
    ];


    public MainViewController()
    {
        _features = Definitions
            .Select(d => new Feature(d.Icon, d.Name, d.Description))
            .ToList();
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        EdgesForExtendedLayout = UIRectEdge.All;
        View!.BackgroundColor = UIColor.White;

        var headerGifView = new HeaderView();
        headerGifView.SetupAnimatedGif();

        _flowLayout = new UICollectionViewFlowLayout
        {
            MinimumLineSpacing = 10,
            MinimumInteritemSpacing = 10,
            SectionInset = SectionInsets
        };
        _flowLayout.ItemSize = new CGSize(View.Bounds.Width - 40, 80);

        var collectionView = new UICollectionView(CGRect.Empty, _flowLayout)
        {
            BackgroundColor = UIColor.White,
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        collectionView.RegisterClassForCell(typeof(FeatureCollectionViewCell), "FeatureViewCell");
        var source = new FeatureCollectionViewSource(_features, OnFeatureSelected);
        collectionView.DataSource = source;
        collectionView.Delegate = source;

        _stackView = new UIStackView([headerGifView, collectionView])
        {
            Axis = UILayoutConstraintAxis.Vertical,
            Distribution = UIStackViewDistribution.Fill,
            Alignment = UIStackViewAlignment.Fill,
            Spacing = 10,
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        View.AddSubview(_stackView);

        NSLayoutConstraint.ActivateConstraints([
            _stackView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
            _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
            _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
        ]);

        _headerHeightConstraint = headerGifView.HeightAnchor.ConstraintEqualTo(250);
        _headerWidthConstraint = headerGifView.WidthAnchor.ConstraintEqualTo(250);
        _headerHeightConstraint.Active = true;
        _headerWidthConstraint.Active = false;
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        UpdateLayoutForCurrentSizeClass();
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        UpdateLayoutForCurrentSizeClass();
    }

    public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
    {
        base.TraitCollectionDidChange(previousTraitCollection);
        UpdateLayoutForCurrentSizeClass();
    }

    private void UpdateLayoutForCurrentSizeClass()
    {
        var isLandscape = TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact;

        _stackView.Axis = isLandscape ? UILayoutConstraintAxis.Horizontal : UILayoutConstraintAxis.Vertical;

        _headerHeightConstraint.Active = !isLandscape;
        _headerWidthConstraint.Active = isLandscape;

        if (isLandscape)
        {
            var availableWidth = View!.Bounds.Width - 250 - _stackView.Spacing;
            var totalSpacing = SectionInsets.Left + SectionInsets.Right + _flowLayout.MinimumInteritemSpacing;
            var itemWidth = (availableWidth - totalSpacing) / 2;
            _flowLayout.ItemSize = new CGSize(itemWidth, 80);
        }
        else
        {
            _flowLayout.ItemSize = new CGSize(View!.Bounds.Width - 40, 80);
        }

        _flowLayout.InvalidateLayout();
    }

    private async void OnFeatureSelected(Feature feature)
    {
        try
        {
            await PrerequisitesHelper.Check(this);
            var def = Definitions.FirstOrDefault(d => d.Name == feature.Name);
            if (def == null) return;
            if (Activator.CreateInstance(def.ControllerType) is not UIViewController controller) return;

            controller.ModalPresentationStyle = ModalStyle;
            PresentViewController(controller, true, null);
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    public override bool PrefersStatusBarHidden() => false;
    public override UIStatusBarStyle PreferredStatusBarStyle() => UIStatusBarStyle.LightContent;
}