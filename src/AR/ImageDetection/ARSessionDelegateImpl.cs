using ARKit;

namespace ARKitDemo.AR.ImageDetection;

public class ArSessionDelegateImpl : ARSessionDelegate
{
    public override void DidFail(ARSession session, NSError error) => 
        Console.WriteLine($"AR Session Failed: {error.LocalizedDescription}");

    public override void CameraDidChangeTrackingState(ARSession session, ARCamera camera) => 
        Console.WriteLine($"Tracking state changed: {camera.TrackingState}");

    public override void WasInterrupted(ARSession session) => 
        Console.WriteLine("AR Session was interrupted.");

    public override void InterruptionEnded(ARSession session)
    {
        Console.WriteLine("AR Session interruption ended, resetting tracking.");
        var config = session.Configuration;
        if (config != null)
        {
            session.Run(config, ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors);
        }
        else
        {
            Console.WriteLine("No valid configuration found to restart AR Session.");
        }
    }
}