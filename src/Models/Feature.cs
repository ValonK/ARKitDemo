namespace ARKitDemo.Models;

public class Feature(UIImage icon, string name, string description)
{
    public UIImage Icon { get; } = icon;
    public string Name { get; } = name;
    public string Description { get; } = description;
}