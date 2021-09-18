#import "AppDelegate.h"

#import <React/RCTBridge.h>
#import <React/RCTBundleURLProvider.h>
#import <React/RCTRootView.h>
#import <AVFoundation/AVFoundation.h>
#import "HomeSecure-Swift.h"

//#if DEBUG
//#import <FlipperKit/FlipperClient.h>
//#import <FlipperKitLayoutPlugin/FlipperKitLayoutPlugin.h>
//#import <FlipperKitUserDefaultsPlugin/FKUserDefaultsPlugin.h>
//#import <FlipperKitNetworkPlugin/FlipperKitNetworkPlugin.h>
//#import <SKIOSNetworkPlugin/SKIOSNetworkAdapter.h>
//#import <FlipperKitReactPlugin/FlipperKitReactPlugin.h>
//
//static void InitializeFlipper(UIApplication *application) {
//  FlipperClient *client = [FlipperClient sharedClient];
//  SKDescriptorMapper *layoutDescriptorMapper = [[SKDescriptorMapper alloc] initWithDefaults];
//  [client addPlugin:[[FlipperKitLayoutPlugin alloc] initWithRootNode:application withDescriptorMapper:layoutDescriptorMapper]];
//  [client addPlugin:[[FKUserDefaultsPlugin alloc] initWithSuiteName:nil]];
//  [client addPlugin:[FlipperKitReactPlugin new]];
//  [client addPlugin:[[FlipperKitNetworkPlugin alloc] initWithNetworkAdapter:[SKIOSNetworkAdapter new]]];
//  [client start];
//}
//#endif

@implementation AppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
#if DEBUG
  //InitializeFlipper(application);
#endif

  RCTBridge *bridge = [[RCTBridge alloc] initWithDelegate:self launchOptions:launchOptions];
  RCTRootView *rootView = [[RCTRootView alloc] initWithBridge:bridge
                                                   moduleName:@"HomeSecure"
                                            initialProperties:nil];

  rootView.backgroundColor = [[UIColor alloc] initWithRed:1.0f green:1.0f blue:1.0f alpha:1];

  self.window = [[UIWindow alloc] initWithFrame:[UIScreen mainScreen].bounds];
  UIViewController *rootViewController = [UIViewController new];
  rootViewController.view = rootView;
  self.window.rootViewController = rootViewController;
  
  [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryAmbient error:nil];  // allow
  
  [HomeSecureApp onStart];
  
  [self.window makeKeyAndVisible];
  return YES;
}

- (NSURL *)sourceURLForBridge:(RCTBridge *)bridge
{
#if DEBUG
  return [[RCTBundleURLProvider sharedSettings] jsBundleURLForBundleRoot:@"index" fallbackResource:nil];
#else
  return [[NSBundle mainBundle] URLForResource:@"main" withExtension:@"jsbundle"];
#endif
}

- (void)application:(UIApplication *)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error
{
  NSLog(@"Remote notifications registration failed");
}

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*) data
{
  [[HomeSecureApp current] setDeviceId:data];
}
-(void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult))completionHandler
{
  id action=[userInfo objectForKey:@"appAction"];
  if(action && [action isKindOfClass:[NSString class]]){
    [[HomeSecureApp current] setAppAction:(NSString*)action];
  }
}

@end
