using ARKitDemo.AR.ImageDetection;
using ARKitDemo.Controls;
using ARKitDemo.Models;

namespace ARKitDemo.ViewControllers;

public class MainViewController : UIViewController, 
    IUICollectionViewDataSource,
    IUICollectionViewDelegateFlowLayout
{
    private HeaderView _headerGifView;
    private UICollectionView _collectionView;

    private readonly List<Feature> _features =
    [
        new(UIImage.FromBundle("ic_image_recognition.png"),
            "Image Detection", 
            "Detect images in the real world.")
    ];

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        EdgesForExtendedLayout = UIRectEdge.All;

        View!.ClipsToBounds = false; 
        View!.BackgroundColor = UIColor.White;

        _headerGifView = new HeaderView();
        _headerGifView.SetupAnimatedGif();
        View.AddSubview(_headerGifView);

        var layout = new UICollectionViewFlowLayout
        {
            ItemSize = new CGSize(View.Bounds.Width - 40, 80),
            MinimumLineSpacing = 10,
            SectionInset = new UIEdgeInsets(10, 20, 10, 20)
        };

        _collectionView = new UICollectionView(CGRect.Empty, layout)
        {
            BackgroundColor = UIColor.White,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _collectionView.RegisterClassForCell(typeof(FeatureCollectionViewCell), "FeatureViewCell");

        var clientsSource = new FeatureCollectionViewSource(_features, OnFeatureSelected);
        _collectionView.DataSource = clientsSource;
        _collectionView.Delegate = clientsSource;
        View.AddSubview(_collectionView);

        SetupConstraints();
    }

    private void SetupConstraints()
    {
        nfloat headerHeight = 250;

        _headerGifView.TopAnchor.ConstraintEqualTo(View!.TopAnchor).Active = true; 
        _headerGifView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
        _headerGifView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        _headerGifView.HeightAnchor.ConstraintEqualTo(headerHeight).Active = true;

        _collectionView.TopAnchor.ConstraintEqualTo(_headerGifView.BottomAnchor, 10).Active = true;
        _collectionView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
        _collectionView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        _collectionView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
    }
    
    public override bool PrefersStatusBarHidden() => false;

    public override UIStatusBarStyle PreferredStatusBarStyle() => UIStatusBarStyle.LightContent;

    private void OnFeatureSelected(Feature feature)
    {
        var imageDetectionViewController = new ImageDetectionViewController
        {
            ModalPresentationStyle = UIModalPresentationStyle.PageSheet
        };
        PresentViewController(imageDetectionViewController, true, null);
    }
}