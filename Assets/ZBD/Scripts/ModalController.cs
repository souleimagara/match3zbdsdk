using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using ZBD;
using System.Collections.Concurrent;

public class ModalController : MonoBehaviour
{

#if UNITY_IOS

    [DllImport("__Internal")]
    private static extern void ZBDSwiftOpenWebview(string url);

    [DllImport("__Internal")]
    private static extern void ZBDSwiftCloseWebview();

    [DllImport("__Internal")]
    private static extern void ZBDSwiftInjectJS(string message);

#endif

    private AndroidJavaObject webViewActivity;
    public static ModalController Instance;
    private bool wasPaused;
    private void Awake()
    {
        Debug.Log("awake modal");
        if (!Instance)
        {
            Debug.Log("awake modal cong");
            Instance = this;
        }
    }


    public void ShowWebView()
    {

#if UNITY_ANDROID

        using AndroidJavaClass webViewClass = new AndroidJavaClass("io.zebedee.zbdsdk.ZBDWebView");
        try
        {
            webViewClass.CallStatic("showWebView");
        }
        catch (Exception e)
        {
            Debug.LogError("Exception while calling showWebView: " + e.Message);
        }


#elif UNITY_IOS
        var url = BuildWebviewUrl();
        ZBDSwiftOpenWebview(url);
#endif
    }

    public void LoadWebView()
    {
        var url = BuildWebviewUrl();
#if UNITY_EDITOR
        Debug.LogError("Editor is not supported");
#elif UNITY_ANDROID

        using AndroidJavaClass webViewClass = new AndroidJavaClass("io.zebedee.zbdsdk.ZBDWebView");
        using AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        try
        {
            webViewActivity = activity;

            webViewClass.CallStatic("createWebView", activity, url, new WebViewMessage());



        }
        catch (Exception e)
        {
            Debug.LogError("Exception while calling createWebView: " + e.Message);
        } 

#endif
    }

    private string BuildWebviewUrl()
    {
        var url = ZBDConstants.WEB_APP_URL;
        var savedUrl = PlayerPrefs.GetString("webViewUrl", "");
        if (savedUrl.Length > 5)
        {
            url = savedUrl;
        }

#if UNITY_ANDROID
        url += "?platform=android";
#elif UNITY_IOS
        url += "?platform=ios";
#endif

        url += "&appId=" + UnityWebRequest.EscapeURL(Application.identifier);

        if (Utils.GetUserDetailsId().Length > 0)
        {
            url += "&userDetailsId=" + Utils.GetUserDetailsId();
        }

        if (ZBDConstants.DEMO_MODE)
        {
            url += "&demoUrl=true";
        }

        return url;
    }

    public void CloseWebView()
    {
#if UNITY_ANDROID
        try
        {
            using AndroidJavaClass webViewClass = new AndroidJavaClass("io.zebedee.zbdsdk.ZBDWebView");

            using AndroidJavaObject webViewInstance = webViewClass.CallStatic<AndroidJavaObject>("getInstance");
            if (webViewInstance != null)
            {
                webViewInstance.Call("closeWebView");
            }
            else
            {
                Debug.Log("webview not found");
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Exception " + e.Message);
        }
#elif UNITY_IOS

        ZBDSwiftCloseWebview();

#endif
    }

    public void SendWebViewMessage(string message)
    {
#if UNITY_ANDROID
        try
        {

            using AndroidJavaClass webViewClass = new AndroidJavaClass("io.zebedee.zbdsdk.ZBDWebView");

            using AndroidJavaObject webViewInstance = webViewClass.CallStatic<AndroidJavaObject>("getInstance");

            if (webViewInstance != null)
            {
                Utils.ZBDLog($"sending message to webpage {message}");
                webViewInstance.Call("sendMessageToWebPage", message);
            }
            else
            {
                Utils.ZBDLog("webview not found");
            }
        }
        catch (Exception e)
        {
            Utils.ZBDLogError($"Exception {e.Message}");
        }
#elif UNITY_IOS
        Utils.ZBDLogError($"sending message to webview {message}");
        ZBDSwiftInjectJS(message);
#endif
    }

    public static void HandleWebviewMessage(string message)
    {
        try
        {
            Utils.ZBDLog("got message " + message);
            var messageObj = JsonConvert.DeserializeObject<ZBDWebViewMessage>(message);
            Utils.ZBDLog("message " + messageObj.type);
            switch (messageObj.type)
            {
                case "close":
                    ModalController.Instance.CloseWebView();
                    break;
                case "openApp":
                    ZBDMainThreadDispatcher.Enqueue(() => Application.OpenURL(messageObj.data));
                    break;
                case "signup":
                case "update":
                    var gamerTag = messageObj.data;
                    ZBDMainThreadDispatcher.Enqueue(() => ZBDController.Instance.Fingerprint(gamerTag));
                    //ZBDController.Instance.Fingerprint(gamerTag);
                    break;
                default:
                    Utils.ZBDLogError("Unknown message type");
                    break;
            }
        }
        catch (Exception e)
        {

            Utils.ZBDLogError($"error parsing message {e.Message}");
        }
    }

    public void OnWebviewMessage(string message)
    {
        Utils.ZBDLog("ios on webview message");
        HandleWebviewMessage(message);
    }


    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            wasPaused = true;
#if UNITY_ANDROID
            using (AndroidJavaClass webViewClass = new AndroidJavaClass("io.zebedee.zbdsdk.ZBDWebView"))
            {


                try
                {
                    webViewClass.CallStatic("destroyWebView");

                }
                catch (Exception e)
                {
                    Utils.ZBDLogError($"Exception while calling destroyWebView: {e.Message}");
                }


            }

#endif
        }
        else
        {
            if (wasPaused)
            {
                wasPaused = false;
                LoadWebView();
            }
        }

    }
}
