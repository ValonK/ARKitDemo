using ARKit;

namespace ARKitDemo.AR;

internal abstract class BaseViewController : UIViewController
{
    private ARSCNView _sceneView;
    protected ARSCNView SceneView
    {
        get
        {
            return _sceneView ??= new ARSCNView
            {
                Frame = View!.Bounds,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                AutoenablesDefaultLighting = true,
            };
        }
    }
    
    protected static ARWorldTrackingConfiguration CreateWorldTrackingConfiguration(ARPlaneDetection planeDetection)
    {
        return new ARWorldTrackingConfiguration
        {
            PlaneDetection = planeDetection
        };
    }
}