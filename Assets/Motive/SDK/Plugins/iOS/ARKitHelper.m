#import <Foundation/Foundation.h>
#import <ARKit/ARKit.h>
#import <UIKit/UIKit.h>

/// Defined here to get the layout to line up
@interface UnityARSessionHandle : NSObject
{
@public
    ARSession* _session;
    void * _frameCallback;
    void * _arSessionFailedCallback;
    void * _arSessionInterrupted;
    void * _arSessionInterruptionEnded;
    void * _arSessionShouldRelocalize;
    void * _arSessionTrackingChanged;
    
    NSMutableDictionary* _classToCallbackMap;
    
    id <MTLDevice> _device;
    CVMetalTextureCacheRef _textureCache;
    BOOL _getPointCloudData;
    BOOL _getLightEstimation;
}

@end

@implementation UnityARSessionHandle
{
    
}

@end


BOOL ARKit_IsSupported()
{
    if ([ARConfiguration class]) {
        return [ARConfiguration isSupported];
    }
    
    return NO;
}

NSMutableSet * g_detectionImages;

void ARKit_AddTrackableImage(ARSession * sessionHandle, char * imageFile, char * imageName, double width)
{
    if (!g_detectionImages)
    {
        g_detectionImages = [NSMutableSet new];
    }
    
    if ([ARReferenceImage class])
    {
        UIImage * img = [UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:imageFile]];
        
        //4. Create An ARReference Image (Remembering Physical Width Is In Metres)
        ARReferenceImage * arImage =
        [[ARReferenceImage alloc] initWithCGImage:img.CGImage orientation:kCGImagePropertyOrientationUp physicalWidth:width];
        
        arImage.name = [NSString stringWithUTF8String:imageName];
        
        [g_detectionImages addObject:arImage];
        
        // todo: if running?
    }
}

void ARKit_StartWorldTrackingSession(void * nativeSession)
{
    UnityARSessionHandle* session = (__bridge UnityARSessionHandle*)nativeSession;
    
    ARWorldTrackingConfiguration* config = [ARWorldTrackingConfiguration new];
    //ARSessionRunOptions runOpts = GetARSessionRunOptionsFromUnityARSessionRunOptions(runOptions);
    //GetARSessionConfigurationFromARKitWorldTrackingSessionConfiguration(unityConfig, config);
    
    config.planeDetection = YES;
    config.worldAlignment = ARWorldAlignmentGravity;
    config.lightEstimationEnabled = YES;
    
    if ([ARImageAnchor class])
    {
        config.autoFocusEnabled = YES;
        /*
         if (unityConfig.ptrVideoFormat != NULL)
         {
         config.videoFormat = (__bridge ARVideoFormat*) unityConfig.ptrVideoFormat;
         }*/
        
        config.detectionImages = g_detectionImages;
    }
    
    session->_getPointCloudData = (BOOL) YES;
    session->_getLightEstimation = (BOOL) YES;
    
    [session->_session runWithConfiguration:config];
    
    SEL setup = @selector(setupMetal);
    [session performSelector:setup];
}
