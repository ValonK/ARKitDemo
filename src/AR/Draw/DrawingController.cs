using System.Diagnostics.CodeAnalysis;
using ARKit;
using ARKitDemo.Extensions;
using SceneKit;

namespace ARKitDemo.AR.Draw;

[SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
internal class DrawingController : BaseViewController
{
    private readonly List<SCNVector3> _drawnPoints = new();

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View!.AddSubview(SceneView);

        var panGesture = new UIPanGestureRecognizer(HandlePan);
        SceneView.AddGestureRecognizer(panGesture);
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        var configuration = CreateWorldTrackingConfiguration(ARPlaneDetection.Horizontal | ARPlaneDetection.Vertical);
        SceneView.Session.Run(configuration, ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
    }

    private void HandlePan(UIPanGestureRecognizer gesture)
    {
        var screenPoint = gesture.LocationInView(SceneView);
        var worldPoint = GetWorldPointAtScreenPoint(screenPoint);
        if (worldPoint == null) return;

        if (gesture.State == UIGestureRecognizerState.Began)
        {
            ClearDrawing();
            _drawnPoints.Clear();
        }

        _drawnPoints.Add(worldPoint.Value);

        var sphere = SCNSphere.Create(0.002f); 
        if (sphere.FirstMaterial != null) sphere.FirstMaterial.Diffuse.Contents = UIColor.Red;
        var dotNode = new SCNNode { Geometry = sphere, Position = worldPoint.Value };
        dotNode.Name = "drawn";
        SceneView.Scene.RootNode.AddChildNode(dotNode);

        if (_drawnPoints.Count < 2) return;
        
        var previous = _drawnPoints[^2];
        var current = _drawnPoints[^1];
        var lineNode = CreateLineNode(previous, current);
        lineNode.Name = "drawn";
        SceneView.Scene.RootNode.AddChildNode(lineNode);
    }

    private SCNVector3? GetWorldPointAtScreenPoint(CGPoint point)
    {
        var hitResults = SceneView.HitTest(point, ARHitTestResultType.FeaturePoint);
        if (hitResults.Length <= 0) return null;
        
        var result = hitResults[0];
        return new SCNVector3(
            result.WorldTransform.Column3.X,
            result.WorldTransform.Column3.Y,
            result.WorldTransform.Column3.Z);
    }

    private SCNNode CreateLineNode(SCNVector3 start, SCNVector3 end)
    {
        var distance = start.DistanceBetweenPoints(end);
        var cylinder = SCNCylinder.Create(0.004f, distance); 
        if (cylinder.FirstMaterial != null) cylinder.FirstMaterial.Diffuse.Contents = UIColor.Blue;

        var lineNode = new SCNNode { Geometry = cylinder };
        lineNode.Position = new SCNVector3(
            (start.X + end.X) / 2,
            (start.Y + end.Y) / 2,
            (start.Z + end.Z) / 2);

        lineNode.Look(end, SceneView.Scene.RootNode.WorldUp, new SCNVector3(0, 1, 0));

        return lineNode;
    }

    private void ClearDrawing()
    {
        foreach (var node in SceneView.Scene.RootNode.ChildNodes)
        {
            if (node.Name == "drawn") node.RemoveFromParentNode();
        }
    }
}