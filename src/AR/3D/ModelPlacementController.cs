using System.Diagnostics.CodeAnalysis;
using ARKit;
using SceneKit;

namespace ARKitDemo.AR._3D;

[SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
internal class ModelPlacementController : BaseViewController
{
    private SCNNode _modelNode;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        
        View!.AddSubview(SceneView);

        var tapGesture = new UITapGestureRecognizer(HandleTap);
        SceneView.AddGestureRecognizer(tapGesture);

        var pinchGesture = new UIPinchGestureRecognizer(HandlePinch);
        SceneView.AddGestureRecognizer(pinchGesture);

        var rotationGesture = new UIRotationGestureRecognizer(HandleRotation);
        SceneView.AddGestureRecognizer(rotationGesture);

        var panGesture = new UIPanGestureRecognizer(HandlePan);
        SceneView.AddGestureRecognizer(panGesture);

        AddTitleBar();
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);

        var configuration = CreateWorldTrackingConfiguration(ARPlaneDetection.Horizontal);
        SceneView.Session.Run(configuration, ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
    }

    private void AddTitleBar()
    {
        nfloat toolbarHeight = 60;
        var toolbarFrame = new CGRect(0, 0, View!.Bounds.Width, toolbarHeight);

        var titleBar = new UIView(toolbarFrame)
        {
            BackgroundColor = UIColor.Black.ColorWithAlpha(0f)
        };
        titleBar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

        View.AddSubview(titleBar);
    }

    private void HandleTap(UITapGestureRecognizer recognizer)
    {
        var tapLocation = recognizer.LocationInView(SceneView);
        if (tapLocation.Y < 60) return;

        var hitResults = SceneView.HitTest(tapLocation, ARHitTestResultType.ExistingPlaneUsingExtent);
        if (hitResults.Length > 0)
        {
            var hitResult = hitResults[0];
            var position = new SCNVector3(
                hitResult.WorldTransform.Column3.X,
                hitResult.WorldTransform.Column3.Y,
                hitResult.WorldTransform.Column3.Z);

            if (_modelNode == null)
            {
                _modelNode = LoadModel();
                if (_modelNode == null) return;
                _modelNode.Position = position;
                _modelNode.Scale = new SCNVector3(0.1f, 0.1f, 0.1f);
                SceneView.Scene.RootNode.AddChildNode(_modelNode);
            }
            else
            {
                _modelNode.Position = position;
            }
        }
    }

    private static SCNNode LoadModel()
    {
        try
        {
            var resourceUrl = NSBundle.MainBundle.GetUrlForResource("BMW_M4_f82", "usdz");
            var scene = SCNScene.FromUrl(resourceUrl, new SCNSceneLoadingOptions(), out _);
            return scene!.RootNode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    private void HandlePinch(UIPinchGestureRecognizer recognizer)
    {
        if (_modelNode == null) return;

        if (recognizer.State != UIGestureRecognizerState.Changed &&
            recognizer.State != UIGestureRecognizerState.Ended) return;
        
        var scale = (float)recognizer.Scale;
        _modelNode.Scale = new SCNVector3(
            _modelNode.Scale.X * scale,
            _modelNode.Scale.Y * scale,
            _modelNode.Scale.Z * scale);

        recognizer.Scale = 1.0f;
    }

    private void HandleRotation(UIRotationGestureRecognizer recognizer)
    {
        if (_modelNode == null) return;

        if (recognizer.State != UIGestureRecognizerState.Changed &&
            recognizer.State != UIGestureRecognizerState.Ended) return;
        
        var rotation = (float)recognizer.Rotation;
        _modelNode.RunAction(SCNAction.RotateBy(0, rotation, 0, 0));
        recognizer.Rotation = 0;
    }

    private void HandlePan(UIPanGestureRecognizer recognizer)
    {
        var panLocation = recognizer.LocationInView(SceneView);
        if (panLocation.Y < 60) return;
        if (_modelNode == null) return;

        var hitResults = SceneView?.HitTest(panLocation, ARHitTestResultType.ExistingPlaneUsingExtent);
        if (hitResults is not { Length: > 0 }) return;
        
        var hitResult = hitResults[0];
        var newPosition = new SCNVector3(
            hitResult.WorldTransform.Column3.X,
            hitResult.WorldTransform.Column3.Y,
            hitResult.WorldTransform.Column3.Z);

        _modelNode.Position = newPosition;
    }
}