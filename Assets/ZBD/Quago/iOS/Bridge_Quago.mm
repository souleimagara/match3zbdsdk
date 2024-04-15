#import "Bridge_Quago.h"
#import "Quago.h"

@implementation QuagoSettings_Bridge

- (id)init{
    self = [super init];
    return self;
}

#pragma mark - Publicly available C methods
#ifdef __cplusplus
    extern "C" {
#endif
        
#pragma mark -
#pragma mark Initialisation
                                   
        void initializeWithSettings(const char * appToken,
                                    int flavor, int logLevel, int maxSegments,
                                    int wrapper, const char * version,
                                    bool enableManualMotionDispatcher,
                                    bool enableManualKeysDispatcher,
                                    bool disableInitSegment,
                                    int autoMotionAmount,
                                    long autoMotionIntervalMillis,
                                    long autoMaxDurationMillis,
                                    int maxCountMotion,
                                    int maxCountKeys,
                                    int maxCountAccelerometer,
                                    int maxCountOtherSensors,
                                    int maxCountResolution,
                                    int maxCountLifeCycle,
                                    QuagoUnityOnJsonCallback unityOnJsonCallback,
                                    QuagoUnityOnLogMessage unityOnLogMessage){
            
            QuagoFlavor qFlavor;
            if(flavor == PRODUCTION) qFlavor = PRODUCTION;
            else if(flavor == DEVELOPMENT) qFlavor = DEVELOPMENT;
            else if(flavor == UNAUTHENTIC) qFlavor = UNAUTHENTIC;
            else if(flavor == AUTHENTIC) qFlavor = AUTHENTIC;
            else return;
            
            LogLevel qLogLevel;
            if(logLevel == LOG_DISABLED) qLogLevel = LOG_DISABLED;
            else if(logLevel == LOG_VERBOSE) qLogLevel = LOG_VERBOSE;
            else if(logLevel == LOG_DEBUG) qLogLevel = LOG_DEBUG;
            else if(logLevel == LOG_INFO) qLogLevel = LOG_INFO;
            else if(logLevel == LOG_WARNING) qLogLevel = LOG_WARNING;
            else if(logLevel == LOG_ERROR) qLogLevel = LOG_ERROR;
            else return;
            
            QuagoSettingsBuilder* settings = [QuagoSettingsBuilder create : [NSString stringWithUTF8String : appToken] : qFlavor];
            [settings setLogLevel : qLogLevel];
            [settings setMaxSegments : maxSegments];
            if(version != nil)[settings setWrapperInfo : wrapper : [NSString stringWithUTF8String : version]];
            if(enableManualMotionDispatcher)[settings enableManualTouchDispatcher];
            if(enableManualKeysDispatcher)[settings enableManualKeysDispatcher];
            if(disableInitSegment)[settings disableInitSegment];
            
            if(autoMotionAmount>0) [settings setTrackingMotionAmount : autoMotionAmount];
            if(autoMotionIntervalMillis>0) [settings setTrackingInterval : autoMotionIntervalMillis];
            if(autoMaxDurationMillis>0) [settings setTrackingMaxDuration : autoMaxDurationMillis];
            
            /* QuagoMaxCount */
            [settings setQueryMaxCount : MOTION count : maxCountMotion];
            [settings setQueryMaxCount : KEYS count : maxCountKeys];
            [settings setQueryMaxCount : ACCELEROMETER count : maxCountAccelerometer];
            [settings setQueryMaxCount : OTHER_SENSORS count : maxCountOtherSensors];
            [settings setQueryMaxCount : RESOLUTION count : maxCountResolution];
            [settings setQueryMaxCount : LIFECYCLE count : maxCountLifeCycle];
            
            /* Callbacks */
            if(unityOnJsonCallback != nil)
                [settings setJsonCallback:^(NSString * _Nonnull headers, NSString * _Nonnull payload) {
                    unityOnJsonCallback([headers UTF8String],[payload UTF8String]);
                }];
            
            if(unityOnLogMessage != nil)
                [settings overrideLogger:^(LogLevel logLevel, NSString * _Nonnull msg, NSException * _Nonnull throwable) {
                    if(throwable)
                        unityOnLogMessage((int)logLevel, [msg UTF8String], [[throwable description] UTF8String]);
                    else
                        unityOnLogMessage((int)logLevel, [msg UTF8String], nil);
                }];
            
            [Quago initialize : settings];
        }
        
#pragma mark -
#pragma mark begin/end Segment Methods
        
        void beginSegment(const char * name){
            [Quago beginSegment : [NSString stringWithUTF8String: name]];
        }

        void endSegment(){
            [Quago endSegment];
        }
        
#pragma mark -
#pragma mark begin/end tracking Methods
        
        void beginTracking(const char * userId){
            if(userId == nil) [Quago beginTracking];
            else [Quago beginTracking : [NSString stringWithUTF8String: userId]];
        }
        
        void endTracking(){
            [Quago endTracking];
        }
        
#pragma mark -
#pragma mark Set user identifier
        
        void setUserId(const char * userId){
            [Quago setUserId : [NSString stringWithUTF8String : userId]];
        }

        void setAdditionalId(const char * additionalId){
            [Quago setAdditionalId : [NSString stringWithUTF8String : additionalId]];
        }
        
#pragma mark -
#pragma mark Send KeyValues and others

        /* Set custom metadata key,value for running context */
        void setKeyValues(const char * key, const char* value){
            [Quago setKeyValues : [NSString stringWithUTF8String : value]
                         forKey : [NSString stringWithUTF8String : key]];
        }

#pragma mark -
#pragma mark Getters
        
        const char * getSessionId(){
            return [[Quago getSessionId] UTF8String];
        }
        
#ifdef __cplusplus
    }
#endif
@end
