using ARKit;
using SceneKit;

namespace ARKitDemo.AR.ImageDetection;

public class ImageDetectionArSceneDelegate(ImageDetectionViewController viewController) : ARSCNViewDelegate
{
    public override void DidAddNode(ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor)
    {
        InvokeOnMainThread(() =>
        {
            try
            {
                if (anchor is ARImageAnchor imageAnchor)
                {
                    Console.WriteLine($"Detected Image: {imageAnchor.ReferenceImage.Name}");
                    viewController.Images.TryGetValue($"{imageAnchor.ReferenceImage.Name}.png", out var text);

                    var textPlaneNode = CreateBlurredTextPlane(text);
                    textPlaneNode.Position = new SCNVector3(0, 0.1f, 0);
                    node.AddChildNode(textPlaneNode);

                    var lineNode = CreateLineNode(new SCNVector3(0, 0, 0), textPlaneNode.Position, 0.001f);
                    node.AddChildNode(lineNode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DidAddNode: {ex}");
            }
        });
    }

    private static SCNNode CreateBlurredTextPlane(string text)
    {
        var viewSize = new CGSize(300, 100);
        var containerView = new UIView(new CGRect(0, 0, viewSize.Width, viewSize.Height))
        {
            BackgroundColor = UIColor.Clear
        };

        var blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraDark);
        var blurView = new UIVisualEffectView(blurEffect)
        {
            Frame = containerView.Bounds,
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
        };
        containerView.AddSubview(blurView);

        var label = new UILabel(new CGRect(0, 0, viewSize.Width, viewSize.Height))
        {
            Text = text,
            TextAlignment = UITextAlignment.Center,
            TextColor = UIColor.White,
            Font = UIFont.BoldSystemFontOfSize(24),
            BackgroundColor = UIColor.Clear,
            AdjustsFontSizeToFitWidth = true
        };
        containerView.AddSubview(label);

        UIGraphics.BeginImageContextWithOptions(viewSize, false, 0);
        containerView.Layer.RenderInContext(UIGraphics.GetCurrentContext());
        var renderedImage = UIGraphics.GetImageFromCurrentImageContext();
        UIGraphics.EndImageContext();

        const float planeWidth = 0.2f;
        const float planeHeight = 0.07f;
        var plane = SCNPlane.Create(planeWidth, planeHeight);
        plane.FirstMaterial.Diffuse.Contents = renderedImage;
        plane.FirstMaterial.DoubleSided = true;

        var textPlaneNode = new SCNNode { Geometry = plane };
        textPlaneNode.EulerAngles = new SCNVector3(0, 0, 0);

        // Set pivot so that the bottom center edge of the plane is at the node's origin.
        textPlaneNode.Pivot = SCNMatrix4.CreateTranslation(0, -planeHeight / 2f, 0);

        return textPlaneNode;
    }

    private SCNNode CreateLineNode(SCNVector3 start, SCNVector3 end, float lineThickness)
    {
        var vector = new SCNVector3(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
        var distance = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

        var cylinder = SCNCylinder.Create(lineThickness, distance);
        cylinder.FirstMaterial!.Diffuse.Contents = UIColor.Red;

        var lineNode = new SCNNode { Geometry = cylinder };

        // Positionthe cylinder at the midpoint.
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