using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZBD
{
    public static class Utils
    {

        public static void ZBDLog(string log)
        {
            if (ZBDController.Instance && ZBDController.Instance.debug)
            {
                UnityEngine.Debug.Log(log);
            }
        }

        public static void ZBDLogError(string log)
        {
            if (ZBDController.Instance && ZBDController.Instance.debug)
            {
                UnityEngine.Debug.LogError(log);
            }
        }

        public static string GetUserDetailsId()
        {
            return PlayerPrefs.GetString(ZBDConstants.USER_DETAILS_KEY, "");
        }

        public static void SetUserDetailsId(string userDetailsId)
        {
            PlayerPrefs.SetString(ZBDConstants.USER_DETAILS_KEY, userDetailsId);
        }


        public static UnityWebRequest CreatePOSTRequest(string url, string payload)
        {

            UnityWebRequest request = UnityWebRequest.PostWwwForm(url, UnityWebRequest.kHttpVerbPOST);

            // Set the request header to indicate JSON content type
            request.SetRequestHeader("Content-Type", "application/json");
            string token = ComputeSha256Hash(payload);
            request.SetRequestHeader("zbd-token", token);
            request.timeout = 10;

            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            // Set the uploaded data
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            return request;

        }
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string SendSignUpError(int statusCode, string errorMessage)
        {


            var rep = new ZBDWebViewResponse
            {
                httpStatusCode = statusCode
            };


            ZBDWebViewError err = new ZBDWebViewError();
            err.success = false;
            err.message = errorMessage;

            rep.response = err;
            var message = JsonConvert.SerializeObject(rep);
            Utils.ZBDLogError($"error: {message}");
            return message;

        }

        public static bool IsVPNError(UnityWebRequest request)
        {
            Utils.ZBDLogError($"error: {request.responseCode}");


            ZBDWebViewError err;
            try
            {
                err = JsonConvert.DeserializeObject<ZBDWebViewError>(request.downloadHandler.text);
            }
            catch (Exception e)
            {
                return false;
            }

            if (err.success == false && err.message == ZBDConstants.VPN_ERROR_MESSAGE)
            {
                return true;
            }
            return false;

        }

        public static string HandleError(UnityWebRequest request)
        {
            Utils.ZBDLogError($"error: {request.responseCode}");

            var rep = new ZBDWebViewResponse
            {
                httpStatusCode = (int)request.responseCode
            };
            ZBDWebViewError err;
            try
            {
                err = JsonConvert.DeserializeObject<ZBDWebViewError>(request.downloadHandler.text);
            }
            catch (Exception e)
            {
                ZBDLogError(e.ToString());
                err = new ZBDWebViewError();
                err.success = false;
                err.message = "unknown error";
            }
            rep.response = err;
            var message = JsonConvert.SerializeObject(rep);
            Utils.ZBDLogError($"error: {message}");
            return message;
        }
    }
    public class Utilities : MonoBehaviourSingleton<Utilities>
    {
#if UNITY_IOS

        [DllImport("__Internal")]
        private static extern void showNativeAlert(string title, string message);

        [DllImport("__Internal")]
        private static extern IntPtr ZBDSwiftGetRequestId(string customUserId);

#endif
        public Action<ZBDFingerprintCallback> fingerPrintCallback;

        public void GetFingerPrintId(string customUserId, Action<ZBDFingerprintCallback> callback)
        {

            fingerPrintCallback = callback;
            if (Application.platform == RuntimePlatform.Android)
            {

                AndroidJavaObject unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaClass pluginClass = new AndroidJavaClass("io.zebedee.zbdsdk.FingerPrintWrapper");


                if (pluginClass != null)
                {

                    pluginClass.CallStatic("getRequestId", currentActivity, customUserId, new AndroidPluginCallback());

                }
            }
            else
            {
                ZBDFingerprintCallback obj = new ZBDFingerprintCallback();
                obj.requestId = "test";
                fingerPrintCallback.Invoke(obj);
            }

        }

        public void GetRequestId(string customUserId, Action<ZBDRequestIdResponse> callback)
        {
            ZBDRequestIdResponse res = new ZBDRequestIdResponse();
            int fingerprintCount = PlayerPrefs.GetInt(ZBDConstants.FINGERPRINT_COUNT_KEY, 0);

            if (fingerprintCount > 4)
            {
                string savedRequestId = PlayerPrefs.GetString(ZBDConstants.FINGERPRINT_REQUEST_ID, "");
                if (savedRequestId.Length > 0)
                {
                    Utils.ZBDLog("using cached request id as fingerprint count met");
                    res.requestId = PlayerPrefs.GetString(ZBDConstants.FINGERPRINT_REQUEST_ID);
                }
                else
                {
                    res.requestId = "";
                    res.requestIdError = "fingerprint count met but cached request id not found";
                }
                callback.Invoke(res);
            }
            else
            {
#if UNITY_ANDROID
                GetFingerPrintId(customUserId, callbackFingerprint =>
                {

                    res.requestId = callbackFingerprint.requestId;
                    res.requestIdError = callbackFingerprint.error;
                    callback.Invoke(res);
                });
#elif UNITY_IOS
                Utils.ZBDLog("Get Request Id iOS");
                IntPtr id = ZBDSwiftGetRequestId(customUserId);
                Utils.ZBDLog("Got RequestId " + res.requestId);
                res.requestId = Marshal.PtrToStringAuto(id);
                callback.Invoke(res);
#endif
            }
        }

        public void CheckRegion(Action<ZBDIPAddressResponse> ipAddressAction)
        {
            StartCoroutine(CheckRegionCont(ipAddressAction));
        }

        IEnumerator CheckRegionCont(Action<ZBDIPAddressResponse> ipAddressAction)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(ZBDConstants.IP_CHECKER_URL))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Utils.ZBDLogError("Error: " + request.error);
                    ZBDIPAddressResponse errorRes = new ZBDIPAddressResponse();
                    errorRes.success = false;
                    ipAddressAction.Invoke(errorRes);
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<ZBDIPAddressResponse>(request.downloadHandler.text);

                    ipAddressAction.Invoke(res);

                }
            }
        }

        public string GetAPIUrl()
        {

            if (ZBDConstants.DEMO_MODE)
            {
                return ZBDConstants.DEMO_URL;
            }

            return ZBDConstants.BASE_URL;
        }


        public void ShowAlert(string title, string message)
        {
#if UNITY_ANDROID

            using AndroidJavaClass utilsClass = new AndroidJavaClass("io.zebedee.zbdsdk.ZBDUtils");
            using AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            try
            {

                utilsClass.CallStatic("showAlert", activity, title, message);

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Exception showing alert: " + e.Message);
            }

#elif UNITY_IOS
            showNativeAlert(title, message);
#endif

        }

        private static readonly byte[] key = StringToByteArray("18467211a11fc4bca0f62c9c64b1a266");
        private static readonly byte[] iv = StringToByteArray("524f2eb9f291b28eb69b35516527c869");

        private static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public void SetFloat(string key, float value)
        {
            // Encrypt the value
            string encryptedValue = Encrypt(value + "");

            // Save the encrypted value instead
            PlayerPrefs.SetString(key, encryptedValue);
        }


        public float GetFloat(string key, float defaultValue = 0f)
        {
            // Get the encrypted value
            string encryptedValue = PlayerPrefs.GetString(key, "");

            if (encryptedValue.Length == 0)
            {
                return 0f;
            }

            string decrypted = Decrypt(encryptedValue);

            float value;
            float.TryParse(decrypted, out value);

            return value;
        }

        private static string Encrypt(string textToEncrypt)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encrypted;

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(textToEncrypt);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                return Convert.ToBase64String(encrypted);
            }
        }

        private static string Decrypt(string textToDecrypt)
        {
            byte[] cipherText = Convert.FromBase64String(textToDecrypt);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new System.IO.MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
