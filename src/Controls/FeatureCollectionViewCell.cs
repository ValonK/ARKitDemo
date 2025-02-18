using ARKitDemo.Models;

namespace ARKitDemo.Controls;

internal sealed class FeatureCollectionViewCell: UICollectionViewCell
{
    private readonly UIImageView _icon;
    private readonly UILabel _nameLabel;
    private readonly UILabel _descriptionLabel;

    [Export("initWithFrame:")]
    public FeatureCollectionViewCell(CGRect frame) : base(frame)
    {
        ContentView.Layer.CornerRadius = 8;
        ContentView.Layer.BorderColor = UIColor.LightGray.CGColor;
        ContentView.Layer.BorderWidth = 1;
        ContentView.BackgroundColor = UIColor.White;

        _icon = new UIImageView
        {
            TranslatesAutoresizingMaskIntoConstraints = false,
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Layer = { MasksToBounds = true },
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

    public void Configure(Feature client)
    {
        _nameLabel.Text = client.Name;
        _descriptionLabel.Text = client.Description;
        _icon.Image = client.Icon;
    }
}