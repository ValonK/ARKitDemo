using ARKit;
using AVFoundation;

namespace ARKitDemo.Helpers;

public static class PrerequisitesHelper
{
    public static async Task Check(UIViewController controller = null)
    {
        if (!ARConfiguration.IsSupported)
        {
            ShowUnsupportedAlert(controller);
            return;
        }

        await RequestCameraPermission(controller);
    }
    
    private static async Task RequestCameraPermission(UIViewController controller)
    {
        var status = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
        switch (status)
        {
            case AVAuthorizationStatus.NotDetermined:
            {
                var granted = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Video);
                if (!granted)
                {
                    ShowPermissionAlert(controller);
                }
                break;
            }
            case AVAuthorizationStatus.Denied:
            case AVAuthorizationStatus.Restricted:
                ShowPermissionAlert(controller);
                break;
        }
    }
    
    private static void ShowPermissionAlert(UIViewController controller)
    {
        InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create(
                "Camera Permission",
                "Camera access is required for AR functionality. Please update your settings.",
                UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            GetTopViewController(controller)?.PresentViewController(alert, true, null);
        });
    }
    
    private static void ShowUnsupportedAlert(UIViewController controller)
    {
        InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create(
                "AR Not Supported",
                "This device does not support ARKit.",
                UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            GetTopViewController(controller)?.PresentViewController(alert, true, null);
        });
    }

    private static UIViewController GetTopViewController(UIViewController controller)
    {
        if (controller != null)
            return controller;

        var rootViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
        while (rootViewController?.PresentedViewController != null)
        {
            rootViewController = rootViewController.PresentedViewController;
        }
        return rootViewController;
    }

    private static void InvokeOnMainThread(Action action)
    {
        if (NSThread.IsMain)
        {
            action();
        }
        else
        {
            UIApplication.SharedApplication.InvokeOnMainThread(action);
        }
    }
}
