/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#import <Foundation/Foundation.h>
@class QuagoSettingsBuilder;
#import "QuagoFlavor.h"
#import "LogLevel.h"
#import "QuagoLogger.h"
#import "QuagoQueryMaxCount.h"

NS_ASSUME_NONNULL_BEGIN

@interface QuagoSettings : NSObject

@property (copy) void (^callback)(NSString *headers, NSString *payload);

@property (nonatomic,assign) QuagoFlavor flavor;
@property (nonatomic,assign) LogLevel logLevel;
@property (nonatomic,copy) NSString * appToken;
@property (nonatomic,assign) BOOL enableManualMotionDispatcher;
@property (nonatomic,assign) BOOL enableManualKeysDispatcher;
@property (nonatomic,assign) BOOL disableInitSegment;
@property (nonatomic,assign) int wrapper;
@property (nonatomic,copy) NSString * wrapperVersion;
@property (nonatomic,assign) int maxSegments;

@property (nonatomic,assign) int autoMotionAmount;
@property (nonatomic,assign) long autoMotionIntervalMillis;
@property (nonatomic,assign) long autoMaxDurationMillis;

@property (nonatomic, strong) NSMutableDictionary<NSNumber *, NSNumber *> *maxCountMap;
@property (nonatomic,strong) QuagoLoggerCallback loggerCallback;

- (instancetype)initWithSettings:(QuagoSettings *)src;

- (instancetype)initWithAppToken:(NSString *)appToken : (enum QuagoFlavor)flavor;

- (int)getQueryMaxCount:(QuagoQueryMaxCount)type;

- (NSString *)description;

@end


NS_ASSUME_NONNULL_END


