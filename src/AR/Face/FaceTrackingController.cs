using ARKit;
using SceneKit;

namespace ARKitDemo.AR.Face;

internal class FaceTrackingController : BaseViewController, IARSCNViewDelegate
{
    private ARSCNFaceGeometry _faceGeometry;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        
        View!.AddSubview(SceneView);

        SceneView.Delegate = this;
        var device = SceneView.Device;
        if (device != null) _faceGeometry = ARSCNFaceGeometry.Create(device, false);
        
        if (_faceGeometry is { FirstMaterial: not null })
            _faceGeometry.FirstMaterial.Diffuse.Contents = UIColor.FromRGBA(1, 0, 0, 1f);
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        
        var configuration = new ARFaceTrackingConfiguration();
        SceneView.Session.Run(configuration,
            ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        SceneView.Session.Pause();
    }

    [Export("renderer:nodeForAnchor:")]
    public SCNNode GetNodeForAnchor(ARSCNView renderer, ARAnchor anchor)
    {
        return anchor is not ARFaceAnchor ? new SCNNode() : new SCNNode { Geometry = _faceGeometry };
    }

    [Export("renderer:didUpdateNode:forAnchor:")]
    public void DidUpdateNode(ARSCNView renderer, SCNNode node, ARAnchor anchor)
    {
        if (anchor is ARFaceAnchor faceAnchor) _faceGeometry.Update(faceAnchor.Geometry);
    }
}