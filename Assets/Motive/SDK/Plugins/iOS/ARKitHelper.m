#import <Foundation/Foundation.h>
#import <ARKit/ARKit.h>

BOOL ARKit_IsSupported()
{
    if ([ARConfiguration class]) {
        return [ARConfiguration isSupported];
    }
    
    return NO;
}
