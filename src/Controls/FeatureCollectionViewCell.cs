using CoreAnimation;
using ARKitDemo.Models;

namespace ARKitDemo.Controls;

internal sealed class FeatureCollectionViewCell : UICollectionViewCell
{
    private readonly UIImageView _icon;
    private readonly UILabel _nameLabel;
    private readonly UILabel _descriptionLabel;
    
    private readonly CAGradientLayer _gradientLayer;
    
    private readonly CGColor[] _normalColors =
    [
        UIColor.White.CGColor,
        UIColor.White.CGColor
    ];

    private readonly CGColor[] _highlightColors =
    [
        UIColor.FromRGB(230, 230, 250).CGColor, 
        UIColor.FromRGB(210, 210, 240).CGColor  
    ];

    [Export("initWithFrame:")]
    public FeatureCollectionViewCell(CGRect frame) : base(frame)
    {
        ContentView.BackgroundColor = UIColor.Clear;
        ContentView.Layer.CornerRadius = 8;
        ContentView.Layer.BorderColor = UIColor.LightGray.CGColor;
        ContentView.Layer.BorderWidth = 1;
        
        _gradientLayer = new CAGradientLayer
        {
            Frame = ContentView.Bounds,
            CornerRadius = 8,
            Colors = _normalColors
        };

        ContentView.Layer.InsertSublayer(_gradientLayer, 0);

        _icon = new UIImageView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Layer = { MasksToBounds = true }
        };

        _nameLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Font = UIFont.BoldSystemFontOfSize(16),
            TextColor = UIColor.Black
        };

        _descriptionLabel = new UILabel
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Font = UIFont.SystemFontOfSize(14),
            TextColor = UIColor.Gray
        };

        ContentView.AddSubviews(_icon, _nameLabel, _descriptionLabel);

        NSLayoutConstraint.ActivateConstraints([
            _icon.LeadingAnchor.ConstraintEqualTo(ContentView.LeadingAnchor, 10),
            _icon.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
            _icon.WidthAnchor.ConstraintEqualTo(50), 
            _icon.HeightAnchor.ConstraintEqualTo(50),

            _nameLabel.TopAnchor.ConstraintEqualTo(ContentView.CenterYAnchor, -20),
            _nameLabel.LeadingAnchor.ConstraintEqualTo(_icon.TrailingAnchor, 10),
            _nameLabel.TrailingAnchor.ConstraintEqualTo(ContentView.TrailingAnchor, -10),

            _descriptionLabel.TopAnchor.ConstraintEqualTo(_nameLabel.BottomAnchor, 5),
            _descriptionLabel.LeadingAnchor.ConstraintEqualTo(_icon.TrailingAnchor, 10),
            _descriptionLabel.TrailingAnchor.ConstraintEqualTo(ContentView.TrailingAnchor, -10)
        ]);
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();
        _gradientLayer.Frame = ContentView.Bounds;
    }

    public void Configure(Feature feature)
    {
        _nameLabel.Text = feature.Name;
        _descriptionLabel.Text = feature.Description;
        _icon.Image = feature.Icon;
    }

    public override bool Highlighted
    {
        get => base.Highlighted;
        set
        {
            base.Highlighted = value;
            AnimateGradient(value);
        }
    }

    private void AnimateGradient(bool highlighted)
    {
        var animation = CABasicAnimation.FromKeyPath("colors");
        animation.Duration = 0.2;
        animation.To = NSArray.FromObjects(highlighted ? _highlightColors : _normalColors);
        animation.FillMode = CAFillMode.Forwards;
        animation.RemovedOnCompletion = false;

        _gradientLayer.AddAnimation(animation, "colorsAnimation");
        _gradientLayer.Colors = highlighted ? _highlightColors : _normalColors;
    }
}
