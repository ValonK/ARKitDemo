using ARKitDemo.ViewControllers;

namespace ARKitDemo;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        var mainViewController = new MainViewController();
        var navigationController = new UINavigationController(mainViewController)
        {
            NavigationBarHidden = true 
        };
        
        Window = new UIWindow(UIScreen.MainScreen.Bounds)
        {
            RootViewController = navigationController
        };
        
        Window.MakeKeyAndVisible();
        return true;
    }
}