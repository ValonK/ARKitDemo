using ARKitDemo.Models;

namespace ARKitDemo.Controls;

public class FeatureCollectionViewSource(List<Feature> features, Action<Feature> onClientSelected)
    : UICollectionViewDataSource, IUICollectionViewDelegate
{
    public override nint GetItemsCount(UICollectionView collectionView, nint section) => features.Count;

    public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
    {
        var cell = collectionView.DequeueReusableCell("FeatureViewCell", indexPath) as FeatureCollectionViewCell;
        var client = features[indexPath.Row];
        cell?.Configure(client);
        return cell;
    }

    [Export("collectionView:didSelectItemAtIndexPath:")]
    public void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
    {
        var client = features[indexPath.Row];
        onClientSelected?.Invoke(client);
    }
}