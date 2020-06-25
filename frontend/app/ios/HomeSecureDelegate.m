//
//  HomeSecureDelegate.m
//  HomeSecure
//
//  Created by Phillip Vance on 6/25/20.
//

#import "HomeSecureDelegate.h"
#import <React/RCTViewManager.h>
#import <React/RCTLog.h>
#import "HomeSecure-Swift.h"

@implementation HomeSecureDelegate

RCT_EXPORT_MODULE();


RCT_EXPORT_METHOD(requestNotificationsPermission){
  [[HomeSecureApp current] requestNotificationsPermission];
}


RCTResponseSenderBlock updateCallback=nil;
int currentUpdateId=0;
BOOL sharedUpdateCallbackSet=NO;

- (void) onUpdate
{
  if(![NSThread isMainThread]){
    dispatch_sync(dispatch_get_main_queue(), ^{
      [self onUpdate];
    });
    return;
  }

  currentUpdateId++;
  if(currentUpdateId>1000000){
    currentUpdateId=0;
  }
  RCTResponseSenderBlock cb=updateCallback;
  updateCallback=nil;
  if(cb){
    cb(@[[NSNull null],@(currentUpdateId),[[HomeSecureApp current] getSharedData] ]);
  }
}

RCT_EXPORT_METHOD(getStatus:(RCTResponseSenderBlock)callback){
  dispatch_sync(dispatch_get_main_queue(), ^{
    @try{
      callback(@[ [NSNull null], [[HomeSecureApp current] getSharedData] ]);
    }
    @catch(NSException *exception){
      callback(@[exception.reason, [NSNull null]]);
    }
  });
}

RCT_EXPORT_METHOD(getStatusWithUpdates:(int)updateId :(RCTResponseSenderBlock)callback){
  dispatch_sync(dispatch_get_main_queue(), ^{

    if(!sharedUpdateCallbackSet){
      [HomeSecureApp current].onDataChange = ^{
        [self onUpdate];
      };
      sharedUpdateCallbackSet=YES;
    }

    @try{
      if(updateId!=currentUpdateId){
        updateCallback=nil;
        callback(@[ [NSNull null], @(currentUpdateId), [[HomeSecureApp current] getSharedData] ]);
      }else{
        updateCallback=callback;
      }
    }
    @catch(NSException *exception){
      callback(@[exception.reason, [NSNull null]]);
    }
  });
}

RCT_EXPORT_METHOD(clearStatusUpdates){
  dispatch_sync(dispatch_get_main_queue(), ^{
    updateCallback=nil;
  });
}

@end
