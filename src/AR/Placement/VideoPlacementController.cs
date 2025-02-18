using System.Diagnostics.CodeAnalysis;
using ARKit;
using AVFoundation;
using SceneKit;

namespace ARKitDemo.AR.Placement;

[SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
internal class VideoPlacementController : BaseViewController
{
    private AVPlayer _player;
    private SCNNode _videoNode;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.AddSubview(SceneView);

        var tapGesture = new UITapGestureRecognizer(HandleTap);
        SceneView.AddGestureRecognizer(tapGesture);

        var pinchGesture = new UIPinchGestureRecognizer(HandlePinch);
        SceneView.AddGestureRecognizer(pinchGesture);
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);

        var configuration = CreateWorldTrackingConfiguration(ARPlaneDetection.Horizontal | ARPlaneDetection.Vertical);
        SceneView.Session.Run(configuration,
            ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
    }

    private void HandleTap(UITapGestureRecognizer gesture)
    {
        var tapLocation = gesture.LocationInView(SceneView);

        var hitResults = SceneView.HitTest(tapLocation, new NSDictionary());
        if (hitResults.Any(hit => hit.Node.Name == "videoNode"))
        {
            if (_player == null) return;

            switch (_player.Rate)
            {
                case > 0:
                    _player.Pause();
                    break;
                default:
                    _player.Play();
                    break;
            }

            return;
        }

        if (_videoNode != null) return;

        var planeHitTestResults = SceneView.HitTest(tapLocation, ARHitTestResultType.ExistingPlaneUsingExtent);
        if (planeHitTestResults.Length <= 0) return;
            
        var hitResult = planeHitTestResults[0];
        PlaceVideoNode(hitResult);
    }

    private void HandlePinch(UIPinchGestureRecognizer gesture)
    {
        if (_videoNode == null) return;

        if (gesture.State != UIGestureRecognizerState.Changed) return;

        var scale = (float)gesture.Scale;
        _videoNode.Scale = new SCNVector3(
            _videoNode.Scale.X * scale,
            _videoNode.Scale.Y * scale,
            _videoNode.Scale.Z * scale
        );
        gesture.Scale = 1.0f;
    }

    private void PlaceVideoNode(ARHitTestResult hitResult)
    {
        const float videoWidth = 0.5f; 
        const float videoHeight = videoWidth * 9f / 16f; // 16:9 

        var videoPlane = SCNPlane.Create(videoWidth, videoHeight);

        var videoUrl = NSUrl.FromFilename("sample_video.mp4");
        _player = new AVPlayer(videoUrl);
        _player.ActionAtItemEnd = AVPlayerActionAtItemEnd.None;

        NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, _ =>
        {
            _player.Seek(CoreMedia.CMTime.Zero);
            _player.Play();
        });

        videoPlane.FirstMaterial!.Diffuse.Contents = _player;
        videoPlane.FirstMaterial.DoubleSided = true;

        _videoNode = new SCNNode { Geometry = videoPlane };
        _videoNode.Name = "videoNode";

        _videoNode.Position = new SCNVector3(
            hitResult.WorldTransform.Column3.X,
            hitResult.WorldTransform.Column3.Y,
            hitResult.WorldTransform.Column3.Z);

        if (hitResult.Anchor is ARPlaneAnchor planeAnchor)
        {
            _videoNode.EulerAngles = planeAnchor.Alignment switch
            {
                ARPlaneAnchorAlignment.Horizontal => new SCNVector3(-(float)Math.PI / 2, 0, 0),// rotate -90.
                ARPlaneAnchorAlignment.Vertical => new SCNVector3(0, 0, 0),
                _ => _videoNode.EulerAngles
            };
        }
        else
        {
            _videoNode.EulerAngles = new SCNVector3(0, 0, 0);
        }

        SceneView.Scene.RootNode.AddChildNode(_videoNode);
        _player.Play();
    }
}