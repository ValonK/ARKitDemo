using SceneKit;

namespace ARKitDemo.Extensions;

internal static class VectorExtensions
{
    internal static float DistanceBetweenPoints(this SCNVector3 a, SCNVector3 b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var dz = b.Z - a.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}