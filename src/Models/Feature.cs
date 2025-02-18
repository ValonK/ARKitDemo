namespace ARKitDemo.Models;

public class Feature(UIImage icon, string name, string description) : IEqualityComparer<Feature>
{
    public UIImage Icon { get; } = icon;
    public string Name { get; } = name;
    public string Description { get; } = description;

    public bool Equals(Feature x, Feature y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return Equals(x.Icon, y.Icon) && x.Name == y.Name && x.Description == y.Description;
    }

    public int GetHashCode(Feature obj)
    {
        return HashCode.Combine(obj.Icon, obj.Name, obj.Description);
    }
}