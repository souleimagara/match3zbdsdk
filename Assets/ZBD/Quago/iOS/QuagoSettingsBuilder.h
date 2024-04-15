/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#import <Foundation/Foundation.h>
#import "QuagoSettings.h"

NS_ASSUME_NONNULL_BEGIN

@interface QuagoSettingsBuilder : NSObject

@property (nonatomic) QuagoSettings * settings;

+ (QuagoSettingsBuilder *) create : (NSString*) appToken : (QuagoFlavor) flavor;

- (QuagoSettingsBuilder *) setLogLevel : (LogLevel) logLevel;

- (QuagoSettingsBuilder *) enableManualTouchDispatcher;
- (QuagoSettingsBuilder *) enableManualKeysDispatcher;

- (QuagoSettingsBuilder *) setTrackingMotionAmount : (int) autoMotionAmount;
- (QuagoSettingsBuilder *) setTrackingInterval : (long) intervalMillis;
- (QuagoSettingsBuilder *) setTrackingMaxDuration : (long) durationMillis;

- (QuagoSettingsBuilder*) disableInitSegment;
- (QuagoSettingsBuilder *) setMaxSegments : (int) maxSegments;
- (QuagoSettingsBuilder *) setWrapperInfo : (int) wrapper : (NSString *)version;
- (QuagoSettingsBuilder *)setQueryMaxCount:(QuagoQueryMaxCount)type count:(NSInteger)count;
- (QuagoSettingsBuilder *) setJsonCallback : (void (^)(NSString *headers, NSString *payload)) callback;
- (QuagoSettingsBuilder *) overrideLogger : (QuagoLoggerCallback) callback;
- (QuagoSettings *) build;
@end

NS_ASSUME_NONNULL_END

