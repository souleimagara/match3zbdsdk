/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
#endif
namespace com.quago.mobile.sdk
{
#if UNITY_IOS
/* Use [DllImport("__Internal")] to assign native methods that will call the bridge "C" methods */
public class QuagoiOS
{
    /*.......................................Public.Methods.......................................*/

    private static QuagoSettings quagoSettings;
    private delegate void QuagoUnityOnJsonCallback(string headers, string payload);
    private delegate void QuagoUnityOnLogMessage(int priority, string msg, string exception);
    private const string TAG = "QUAGO-SDK";

    public static void initialize(QuagoSettings settings){
        quagoSettings = settings;
        
        QuagoUnityOnJsonCallback unityOnJsonCallback = null;
        QuagoUnityOnLogMessage unityOnLogMessage = null;

        if (settings.getCallback() != null)
            unityOnJsonCallback = ReceiveQuagoJsonCallback;

        if(settings.GetLogger() != null){
            unityOnLogMessage = ReceiveLogMessage;
        }
        
        initializeWithSettings(
            settings.getAppToken(),
            (int)settings.getFlavor(),
            (int)settings.getLogLevel(),
            settings.getMaxSegments(),
            settings.getWrapper(),
            settings.getWrapperVersion(),
            settings.isManualMotionDispatcherEnabled(),
            settings.isManualKeysDispatcherEnabled(),
            settings.isInitSegmentDisabled(),
            settings.getAutoMotionAmount(),
            settings.getAutoMotionIntervalMillis(),
            settings.getAutoMaxDurationMillis(),
            settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.MOTION),
            settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.KEYS),
            settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.ACCELEROMETER),
            settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.OTHER_SENSORS),
            settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.RESOLUTION),
            settings.getQueryMaxCount(QuagoSettings.QuagoQueryMaxCount.LIFECYCLE),
            unityOnJsonCallback,
            unityOnLogMessage
        );
    }

    [AOT.MonoPInvokeCallback(typeof(QuagoUnityOnJsonCallback))]
    protected static void ReceiveQuagoJsonCallback(string headers, string payload)
    {
        if(quagoSettings.getCallback() == null) return;
        quagoSettings.getCallback().onJsonSegment(headers,payload);
    }

    [AOT.MonoPInvokeCallback(typeof(QuagoUnityOnLogMessage))]
    protected static void ReceiveLogMessage(int priority, string msg, string exception)
    {
        if(quagoSettings.GetLogger() == null) return;
        Exception e;
        if(String.IsNullOrEmpty(exception)){
            e = null;
        }else{
            e = new Exception(exception);
        }
        quagoSettings.GetLogger()((QuagoSettings.LogLevel)priority, TAG, msg, e);
    }

    [DllImport("__Internal")]
    private static extern void initializeWithSettings(
        string appToken, int flavor, int logLevel, int maxSegments,
        int wrapper, string version,
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
        QuagoUnityOnLogMessage unityOnLogMessage);

    /*......................................Segments.Methods......................................*/

    [DllImport("__Internal")]
    public static extern void beginSegment(string name);

    [DllImport("__Internal")]
    public static extern void endSegment();

    [DllImport("__Internal")]
    public static extern void beginTracking(string userId);

    [DllImport("__Internal")]
    public static extern void endTracking();

    /*......................................Meta.Data.Methods.....................................*/

    [DllImport("__Internal")]
    public static extern void setKeyValues(string key, string value);

    [DllImport("__Internal")]
    public static extern void setUserId(string userId);

    [DllImport("__Internal")]
    public static extern void setAdditionalId(string additionalId);

    /*.......................................Events.Methods.......................................*/

    // public static void SendCustomEvent(int eventNumber)
    // {
    //     javaQuagoClass.CallStatic("sendCustomEvent", eventNumber);
    // }

    /*.......................................Getter.Methods......................................*/
    
    [DllImport("__Internal")]
    public static extern string getSessionId();
}
#endif
}
