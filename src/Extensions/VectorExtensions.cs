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
    
    internal static SCNNode CreateLineNode(this SCNVector3 start, SCNVector3 end, float lineThickness)
    {
        var vector = new SCNVector3(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
        var distance = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

        var cylinder = SCNCylinder.Create(lineThickness, distance);
        cylinder.FirstMaterial!.Diffuse.Contents = UIColor.Red;

        var lineNode = new SCNNode { Geometry = cylinder };

        lineNode.Position = new SCNVector3(
            (start.X + end.X) / 2,
            (start.Y + end.Y) / 2,
            (start.Z + end.Z) / 2);

        var yAxis = new SCNVector3(0, 1, 0);
        var axis = new SCNVector3(
            yAxis.Y * vector.Z - yAxis.Z * vector.Y,
            yAxis.Z * vector.X - yAxis.X * vector.Z,
            yAxis.X * vector.Y - yAxis.Y * vector.X);
        var axisLength = (float)Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);
        if (axisLength != 0)
        {
            axis = new SCNVector3(axis.X / axisLength, axis.Y / axisLength, axis.Z / axisLength);
        }

        var dot = yAxis.X * vector.X + yAxis.Y * vector.Y + yAxis.Z * vector.Z;
        var angle = (float)Math.Acos(dot / distance);
        lineNode.Rotation = new SCNVector4(axis.X, axis.Y, axis.Z, angle);

        return lineNode;
    }
}