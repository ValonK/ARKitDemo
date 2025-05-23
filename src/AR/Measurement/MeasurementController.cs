using ARKit;
using ARKitDemo.Extensions;
using CoreAnimation;
using SceneKit;

namespace ARKitDemo.AR.Measurement;

internal class MeasurementController : BaseViewController
{
    private UILabel _distanceLabel;
    private SCNNode _startPointNode;
    private SCNNode _endPointNode;
    private SCNNode _measurementLine;
    private CADisplayLink _displayLink;
    private CGPoint _currentTouchLocation;
    private SCNVector3? _startPoint;
    private bool _isMeasuring;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        InitializeUi();

        var longPressRecognizer = new UILongPressGestureRecognizer(HandleLongPress)
        {
            MinimumPressDuration = 0.5f
        };
        var tapRecognizer = new UITapGestureRecognizer(Cleanup);
        SceneView.AddGestureRecognizer(longPressRecognizer);
        SceneView.AddGestureRecognizer(tapRecognizer);

        _displayLink = CADisplayLink.Create(UpdateMeasurement);
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);

        var configuration = CreateWorldTrackingConfiguration(ARPlaneDetection.Horizontal);
        SceneView.Session.Run(configuration,
            ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
        _displayLink.Invalidate();
    }

    private void InitializeUi()
    {
        View!.AddSubview(SceneView);

        _distanceLabel = new UILabel(new CGRect(20, 40, View.Bounds.Width - 40, 50))
        {
            TextColor = UIColor.White,
            BackgroundColor = UIColor.Black.ColorWithAlpha(0.6f),
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(20),
            Text = "Long press to start measuring",
            Layer = { CornerRadius = 8, MasksToBounds = true },
            AdjustsFontSizeToFitWidth = true,
            MinimumScaleFactor = 0.5f
        };
        View.AddSubview(_distanceLabel);
    }

    private void HandleLongPress(UILongPressGestureRecognizer gesture)
    {
        _currentTouchLocation = gesture.LocationInView(SceneView);

        switch (gesture.State)
        {
            case UIGestureRecognizerState.Began:
                Cleanup();
                _startPoint = GetWorldPointAtScreenPoint(_currentTouchLocation);
                if (_startPoint == null)
                {
                    _distanceLabel.Text = "No surface detected";
                    return;
                }

                _isMeasuring = true;
                _distanceLabel.Text = "Measuring...";
                _startPointNode = CreatePointNode(_startPoint.Value);
                SceneView.Scene.RootNode.AddChildNode(_startPointNode);
                break;

            case UIGestureRecognizerState.Changed:
                _currentTouchLocation = gesture.LocationInView(SceneView);
                break;

            case UIGestureRecognizerState.Ended:
            case UIGestureRecognizerState.Cancelled:
                if (_isMeasuring && _startPoint != null)
                {
                    var endPoint = GetWorldPointAtScreenPoint(_currentTouchLocation);
                    if (endPoint != null)
                    {
                        _endPointNode = CreatePointNode(endPoint.Value);
                        SceneView.Scene.RootNode.AddChildNode(_endPointNode);
                    }
                }
                _isMeasuring = false;
                break;
        }
    }

    private void UpdateMeasurement()
    {
        if (!_isMeasuring || _startPoint == null) return;

        var currentPoint = GetWorldPointAtScreenPoint(_currentTouchLocation);
        if (currentPoint == null)
        {
            _distanceLabel.Text = "No surface detected";
            return;
        }

        _measurementLine?.RemoveFromParentNode();
        _measurementLine = CreateLineNode(_startPoint.Value, currentPoint.Value);
        SceneView.Scene.RootNode.AddChildNode(_measurementLine);

        var distance = _startPoint.Value.DistanceBetweenPoints(currentPoint.Value);
        _distanceLabel.Text = $"{distance * 100:F2} cm";

        _endPointNode?.RemoveFromParentNode();
        _endPointNode = CreatePointNode(currentPoint.Value);
        SceneView.Scene.RootNode.AddChildNode(_endPointNode);
    }

    private SCNVector3? GetWorldPointAtScreenPoint(CGPoint point)
    {
        var query = SceneView.CreateRaycastQuery(point,
            ARRaycastTarget.ExistingPlaneGeometry,
            ARRaycastTargetAlignment.Horizontal);
        if (query == null)
        {
            Console.WriteLine("Raycast query failed for point: " + point);
            return null;
        }

        var results = SceneView.Session.Raycast(query);
        if (results.Length == 0)
        {
            Console.WriteLine("No raycast results found for point: " + point);
            return null;
        }

        var result = results[0];
        return new SCNVector3(
            result.WorldTransform.Column3.X,
            result.WorldTransform.Column3.Y,
            result.WorldTransform.Column3.Z);
    }

    private SCNNode CreateLineNode(SCNVector3 start, SCNVector3 end)
    {
        var distance = start.DistanceBetweenPoints(end);
        var cylinder = SCNCylinder.Create(0.001f, distance);
        cylinder.FirstMaterial!.Diffuse.Contents = UIColor.White;

        var lineNode = new SCNNode { Geometry = cylinder, Name = "measurementLine" };
        lineNode.Position = new SCNVector3(
            (start.X + end.X) / 2,
            (start.Y + end.Y) / 2,
            (start.Z + end.Z) / 2);

        lineNode.Look(end, SceneView.Scene.RootNode.WorldUp, new SCNVector3(0, 1, 0));
        return lineNode;
    }

    private static SCNNode CreatePointNode(SCNVector3 position)
    {
        var sphere = SCNSphere.Create(0.002f);
        sphere.FirstMaterial!.Diffuse.Contents = UIColor.Red;

        var pointNode = new SCNNode { Geometry = sphere, Position = position, Name = "pointMarker" };
        return pointNode;
    }
    
    private void Cleanup()
    {
        _startPoint = null;
        _isMeasuring = false;
        _startPointNode?.RemoveFromParentNode();
        _endPointNode?.RemoveFromParentNode();
        _measurementLine?.RemoveFromParentNode();
        _startPointNode = null;
        _endPointNode = null;
        _measurementLine = null;
        _distanceLabel.Text = "Long press to start measuring";
    }
}