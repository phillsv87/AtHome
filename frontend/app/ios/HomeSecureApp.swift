//
//  HomeSecureApp.swift
//  HomeSecure
//
//  Created by Phillip Vance on 6/25/20.
//

import Foundation
import UIKit
import MobileCoreServices
import UserNotifications

@objc public class HomeSecureApp:NSObject
{
  
  
  @objc public static func onStart()->Void
  {
    _current=HomeSecureApp()
  }
  
  
  public var sharedData:[String:String]
  
  override init()
  {
    sharedData=[:]
    super.init()
  }
  
  @objc public func requestNotificationsPermission()
  {
    DispatchQueue.main.async{
      HomeSecureApp.requestPermissions()
    }
  }
  
  @objc func setDeviceId(_ deviceId:Data)
  {
    HomeSecureApp.setDeviceToken(deviceId)
    sharedData["devicePushId"]=HomeSecureApp.getDeviceId()
    onDataChange?()
    
  }
  
  private static var _current:HomeSecureApp?;
  @objc public static var current:HomeSecureApp{
    get{
      return _current!
    }
  }
  
  @objc public var onDataChange:(()->Void)?
  
  @objc func getSharedData()->[String:String]{
    return self.sharedData
  }
  
  
  
  @objc func setAppAction(_ action:String)
  {
    sharedData["appAction"]=UUID().uuidString+"||||"+action
    onDataChange?()
  }
  
  
  
  // Noficiations
  
  
  public static func requestPermissions()
  {
    // Request user's permission to send notifications.
    UNUserNotificationCenter.current().requestAuthorization(options: [.alert, .badge, .sound]) { (granted, error) in
        if granted {
            print("Notifications permission granted.")
            getNotificationSettings();
        }
        else {
          print("Notifications permission denied because: "+error.debugDescription)
        }
    }
  }
  
  static func getNotificationSettings() {
    UNUserNotificationCenter.current().getNotificationSettings { settings in
      print("Notification settings: \(settings)")
      guard settings.authorizationStatus == .authorized else { return }
      DispatchQueue.main.async {
        UIApplication.shared.registerForRemoteNotifications()
      }
    }
  }
  
  public static func send(_ fromNow:TimeInterval, _ title:String, _ body:String) {
      let center = UNUserNotificationCenter.current()

      let content = UNMutableNotificationContent()
      content.title = title
      content.body = body
      //content.categoryIdentifier = "alarm"
      content.sound = UNNotificationSound.default

      let trigger = UNTimeIntervalNotificationTrigger(timeInterval: fromNow, repeats: false)

      let request = UNNotificationRequest(identifier: UUID().uuidString, content: content, trigger: trigger)
      center.add(request)
  }
  
  private static var deviceToken:String?
  
  public static func getDeviceId()->String
  {
    return HomeSecureApp.deviceToken!
  }
  
  public static func setDeviceToken(_ deviceToken: Data) {
    let tokenParts = deviceToken.map { data in String(format: "%02.2hhx", data) }
    let token = tokenParts.joined()
    print("Device Token: \(token)")
    HomeSecureApp.deviceToken=token
  }
}
