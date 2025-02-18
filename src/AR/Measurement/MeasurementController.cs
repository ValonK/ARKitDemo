using ARKit;
using ARKitDemo.Extensions;
using CoreAnimation;
using SceneKit;

namespace ARKitDemo.AR.Measurement;

internal class MeasurementController : BaseViewController
{
    private UILabel _distanceLabel;

    private SCNVector3? _startPoint;
    private bool _isMeasuring;

    private SCNNode _measurementLine;
    private CADisplayLink _displayLink;
    private CGPoint _currentTouchLocation;
    private UILongPressGestureRecognizer _longPressRecognizer;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.AddSubview(SceneView);

        _distanceLabel = new UILabel(new CGRect(20, 40, View.Bounds.Width - 40, 40))
        {
            TextColor = UIColor.White,
            BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f),
            TextAlignment = UITextAlignment.Center,
            Font = UIFont.BoldSystemFontOfSize(24),
            Text = "Long press to start measuring"
        };
        View.AddSubview(_distanceLabel);

        _longPressRecognizer = new UILongPressGestureRecognizer(HandleLongPress)
        {
            MinimumPressDuration = 0.5f
        };
        SceneView.AddGestureRecognizer(_longPressRecognizer);

        _displayLink = CADisplayLink.Create(UpdateMeasurement);
        _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
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

    private void HandleLongPress(UILongPressGestureRecognizer gesture)
    {
        _currentTouchLocation = gesture.LocationInView(SceneView);

        switch (gesture.State)
        {
            case UIGestureRecognizerState.Began:
                RemoveMeasurement();
                _startPoint = GetWorldPointAtScreenPoint(_currentTouchLocation);
                if (_startPoint == null) return;

                _isMeasuring = true;
                _distanceLabel.Text = "Measuring...";
                break;

            case UIGestureRecognizerState.Changed:
                _currentTouchLocation = gesture.LocationInView(SceneView);
                break;

            case UIGestureRecognizerState.Ended:
            case UIGestureRecognizerState.Cancelled:
                _isMeasuring = false;
                break;
        }
    }

    private void UpdateMeasurement()
    {
        if (!_isMeasuring || _startPoint == null) return;

        var currentPoint = GetWorldPointAtScreenPoint(_currentTouchLocation);
        if (currentPoint == null) return;

        _measurementLine?.RemoveFromParentNode();
        _measurementLine = CreateLineNode(_startPoint.Value, currentPoint.Value);
        SceneView.Scene.RootNode.AddChildNode(_measurementLine);

        var distance = _startPoint.Value.DistanceBetweenPoints(currentPoint.Value);
        _distanceLabel.Text = $"{distance * 100:F2} cm";
    }

    private SCNVector3? GetWorldPointAtScreenPoint(CGPoint point)
    {
        var query = SceneView.CreateRaycastQuery(point,
            ARRaycastTarget.ExistingPlaneGeometry,
            ARRaycastTargetAlignment.Horizontal);
        if (query == null) return null;

        var results = SceneView.Session.Raycast(query);
        if (results.Length == 0)
            return null;

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

        var lineNode = new SCNNode { Geometry = cylinder };
        lineNode.Name = "measurementLine";

        lineNode.Position = new SCNVector3(
            (start.X + end.X) / 2,
            (start.Y + end.Y) / 2,
            (start.Z + end.Z) / 2);

        lineNode.Look(end, SceneView.Scene.RootNode.WorldUp, new SCNVector3(0, 1, 0));
        return lineNode;
    }

    private void RemoveMeasurement()
    {
        _startPoint = null;
        _isMeasuring = false;
        _measurementLine?.RemoveFromParentNode();
        _measurementLine = null;
        _distanceLabel.Text = "Long press to start measuring";
    }
}