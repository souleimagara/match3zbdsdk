/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */

#ifndef QuagoQueryMaxCount_h
#define QuagoQueryMaxCount_h

typedef NS_ENUM(NSInteger,QuagoQueryMaxCount) {
    MOTION = 0,
    KEYS = 1,
    ACCELEROMETER = 2,
    OTHER_SENSORS = 3,
    RESOLUTION = 4,
    LIFECYCLE = 5
};

#endif /* QuagoQueryMaxCount_h */
