/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
using UnityEngine;
#if !UNITY_IOS && ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

#if UNITY_ANDROID || UNITY_IOS
using com.quago.mobile.sdk;
#endif

public class Quago
{
    public const string VERSION_NAME = "1.7.4";
    public const int VERSION_CODE = 14;
    protected static bool enableWorkInEditor;

    /*......................................Public.Methods.......................................*/

    public static void initialize(QuagoSettings.Builder builder)
    {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
        initialize(builder.build());
#endif
    }

    public static void initialize(QuagoSettings settings)
    {
        if (settings == null)
        {
            Debug.Log("Failed to call Quago.initialize() due to settings = null");
            return;
        }
        enableWorkInEditor = settings.isEnableWorkInEditor();
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor)
        {
            Debug.Log("Quago.initialize() - Editor Mode");
            QuagoStandalone.Initialize(settings);
        }
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        Debug.Log("Quago.initialize()");
        QuagoAndroid.initialize(settings);
#elif UNITY_IOS
        Debug.Log("Quago.initialize()");
        QuagoiOS.initialize(settings);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        Debug.Log("Quago.initialize()");
        QuagoStandalone.Initialize(settings);
#endif
    }

    public static void beginSegment(string name)
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.BeginSegment(name);
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.beginSegment(name);
#elif UNITY_IOS
        QuagoiOS.beginSegment(name);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.BeginSegment(name);
#endif
    }

    public static void endSegment()
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.EndSegment();
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.endSegment();
#elif UNITY_IOS
        QuagoiOS.endSegment();
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.EndSegment();
#endif
    }

    public static void beginTracking()
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.BeginTracking(null);
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.beginTracking();
#elif UNITY_IOS
        QuagoiOS.beginTracking(null);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.BeginTracking(null);
#endif
    }

    public static void beginTracking(string userId)
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.BeginTracking(userId);
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.beginTracking(userId);
#elif UNITY_IOS
        QuagoiOS.beginTracking(userId);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.BeginTracking(userId);
#endif
    }

    public static void endTracking()
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.EndTracking();
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.endTracking();
#elif UNITY_IOS
        QuagoiOS.endTracking();
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.EndTracking();
#endif
    }

    public static void setKeyValues(string key, string value)
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.SetKeyValues(key, value);
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.setKeyValues(key,value);
#elif UNITY_IOS
        QuagoiOS.setKeyValues(key,value);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.SetKeyValues(key,value);
#endif
    }

    public static void setUserId(string userId)
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.SetUserId(userId);
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.setUserId(userId);
#elif UNITY_IOS
        QuagoiOS.setUserId(userId);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.SetUserId(userId);
#endif
    }

    public static void setAdditionalId(string additionalId)
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) QuagoStandalone.SetAdditionalId(additionalId);
#elif UNITY_EDITOR
        return;
#elif UNITY_ANDROID
        QuagoAndroid.setAdditionalId(additionalId);
#elif UNITY_IOS
        QuagoiOS.setAdditionalId(additionalId);
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        QuagoStandalone.SetAdditionalId(additionalId);
#endif
    }

    /*......................................Optional.Methods......................................*/

    public static string getSessionId()
    {
#if UNITY_EDITOR && UNITY_STANDALONE
        if (enableWorkInEditor) return QuagoStandalone.GetSessionId();
        return null;
#elif UNITY_EDITOR
        return null;
#elif UNITY_ANDROID
        return QuagoAndroid.getSessionId();
#elif UNITY_IOS
        return QuagoiOS.getSessionId();
#elif UNITY_STANDALONE //Windows/MacOSx/Linux
        return QuagoStandalone.GetSessionId();
#else
    return null;
#endif
    }

#if !UNITY_IOS
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        public static void keyBind(string keyName, Key key)
        {
#if UNITY_EDITOR && UNITY_STANDALONE
                if (enableWorkInEditor) QuagoStandalone.KeyBind(keyName, key);
#elif UNITY_EDITOR
                return;
#elif UNITY_STANDALONE//Windows/MacOSx/Linux
                QuagoStandalone.KeyBind(keyName, key);
#endif
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        public static void keyBind(string keyName, KeyCode key)
        {
#if UNITY_EDITOR && UNITY_STANDALONE
                if (enableWorkInEditor) QuagoStandalone.KeyBind(keyName, key);
#elif UNITY_EDITOR
                return;
#elif UNITY_STANDALONE//Windows/MacOSx/Linux
                QuagoStandalone.KeyBind(keyName, key);
#endif
        }
#endif
#endif

    /*......................................Private.Classes.......................................*/
}
