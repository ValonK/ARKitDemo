using System.Diagnostics.CodeAnalysis;
using ARKit;
using SceneKit;

namespace ARKitDemo.AR.Animations;

[SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
internal class AnimationController : BaseViewController, IARSCNViewDelegate
{
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.AddSubview(SceneView);
        SceneView.Delegate = this;

        var tapGesture = new UITapGestureRecognizer(HandleTap);
        SceneView.AddGestureRecognizer(tapGesture);
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
    }

    [Export("renderer:didAddNode:forAnchor:")]
    public void DidAddNode(ARSCNView renderer, SCNNode node, ARAnchor anchor)
    {
        if (anchor is not ARPlaneAnchor planeAnchor) return;

        var extent = planeAnchor.Extent;
        var plane = SCNPlane.Create(extent.X, extent.Z);
        if (plane.FirstMaterial != null)
        {
            plane.FirstMaterial.Diffuse.Contents = UIColor.FromWhiteAlpha(1.0f, 0.2f);
            plane.FirstMaterial.DoubleSided = true;
        }

        var planeNode = new SCNNode { Geometry = plane };
        planeNode.EulerAngles = new SCNVector3(-(float)Math.PI / 2, 0, 0);
        planeNode.Position = new SCNVector3(planeAnchor.Center.X, 0, planeAnchor.Center.Z);

        node.AddChildNode(planeNode);
    }

    private void HandleTap(UITapGestureRecognizer gesture)
    {
        var tapLocation = gesture.LocationInView(SceneView);
        var planeHitResults = SceneView.HitTest(tapLocation, ARHitTestResultType.ExistingPlaneUsingExtent);
        if (planeHitResults.Length <= 0) return;

        var hitResult = planeHitResults[0];
        var spawnPosition = new SCNVector3(
            hitResult.WorldTransform.Column3.X,
            hitResult.WorldTransform.Column3.Y + 0.05f,
            hitResult.WorldTransform.Column3.Z);

        PlantTree(spawnPosition);
    }

    private void PlantTree(SCNVector3 position)
    {
        var trunk = SCNCylinder.Create(0.005f, 0.1f);
        if (trunk.FirstMaterial == null) return;

        trunk.FirstMaterial.Diffuse.Contents = UIColor.Brown;
        var trunkNode = new SCNNode { Geometry = trunk };
        trunkNode.Position = new SCNVector3(0, 0.05f, 0);

        var foliage = new SCNCone
        {
            TopRadius = 0.0f,
            BottomRadius = 0.02f,
            Height = 0.06f
        };
        if (foliage.FirstMaterial != null) foliage.FirstMaterial.Diffuse.Contents = UIColor.Green;
        var foliageNode = new SCNNode { Geometry = foliage };
        foliageNode.Position = new SCNVector3(0, 0.1f, 0);

        var treeNode = new SCNNode();
        treeNode.Name = "tree";
        treeNode.AddChildNode(trunkNode);
        treeNode.AddChildNode(foliageNode);
        treeNode.Position = position;

        treeNode.Scale = new SCNVector3(0.1f, 0.1f, 0.1f);
        SceneView.Scene.RootNode.AddChildNode(treeNode);

        var growAction = SCNAction.ScaleTo(1.0f, 3.0);
        treeNode.RunAction(growAction);
    }
}