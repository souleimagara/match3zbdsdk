using System;
using System.Collections;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Security.Cryptography;

namespace ZBD
{
    public class ZBDController : MonoBehaviour
    {

        [Space(10)]
        [Header("ZBD Settings")]
        [Tooltip("Should the Rewards SDK auto initialize on start?")]
        [SerializeField] private bool autostart;
        [SerializeField] public bool debug;
        [SerializeField] private string appToken;

        public GameObject notSignedInButton;
        public GameObject progressWidget;

        private ZBDRequestIdResponse requestIdResponse;
        private bool antiCheatStarted;
        private ZBDProgressResponse currentProgressResponse;
        public static ZBDController Instance;
        bool started;

        #region Unity Methods

        void Awake()
        {

            if (ZBDController.Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        private void Start()
        {
            if (started)
            {
                return;
            }
            if (autostart)
            {
                started = true;
                Utils.ZBDLog("auto start");
                Init();
            }


        }

        #endregion

        #region Init
        public void Init()
        {

            if (appToken.Length == 0)
            {
                Utilities.Instance.ShowAlert("Error", $"Error app token not set in inspector, if you are not using Quago for anti cheat, remove StartQuago function");
                return;
            }

            notSignedInButton.SetActive(false);
            progressWidget.SetActive(false);

            Utilities.Instance.CheckRegion(regionRes =>
            {

                if (regionRes.success)
                {
                    if (regionRes.data.isSupported == false)
                    {
                        Debug.LogWarning("ZBD SDK is not supported in this region:" + regionRes.data.ipCountry + " " + regionRes.data.ipRegion);
                        Destroy(gameObject);
                        return;
                    }

                    Utils.ZBDLog("Region is supported");


                    ModalController.Instance.LoadWebView();

                    if (Utils.GetUserDetailsId().Length > 0)
                    {
                        Utils.ZBDLog($"has user details id {Utils.GetUserDetailsId()}");
                        StartAntiCheat();
                        SendProgress();

                    }
                    else
                    {
                        notSignedInButton.SetActive(true);
                    }

                    if (Application.isEditor)
                    {
                        // Invoke("Test", 2);
                    }

                }
                else
                {
                    Utilities.Instance.ShowAlert("error", "error checking region");
                }

            });
        }


        void Test()
        {
            progressWidget.SetActive(true);
            notSignedInButton.SetActive(false);

            currentProgressResponse = new ZBDProgressResponse();
            currentProgressResponse.data = new ZBDProgress();
            currentProgressResponse.data.nextRewardId = "1";
            currentProgressResponse.data.timeUntilNextReward = 1;
            currentProgressResponse.data.currentEarnedToday = 1200;
            ProgressWidgetController.Instance.SetData(currentProgressResponse.data);
        }
        #endregion

        public void Init(string userId)
        {
            SetUserId(userId);
            Init();
        }

        #region SetUserId
        public void SetUserId(string userId)
        {
            PlayerPrefs.SetString(ZBDConstants.CUSTOM_USERID_KEY, userId);
        }
        #endregion

        #region AntiCheat

        private void StartAntiCheat()
        {
            string userId = Utils.GetUserDetailsId();

            if (userId.Length > 0)
            {
                notSignedInButton.SetActive(false);
                if (appToken.Length == 0)
                {
                    Utils.ZBDLogError($"Error app token not set in inspector, if you are not using Quago for anti cheat, remove StartQuago function");
                    return;
                }
                progressWidget.SetActive(true);
                if (antiCheatStarted) return;

                Quago.initialize(QuagoSettings.create(appToken, QuagoSettings.QuagoFlavor.PRODUCTION).setLogLevel(QuagoSettings.LogLevel.INFO)
                );


                antiCheatStarted = true;


            }
            else
            {
                notSignedInButton.SetActive(true);
            }
        }

        #endregion


        public void Fingerprint(string username)
        {

            Utils.ZBDLog("check region");
            Utilities.Instance.CheckRegion(regionRes =>
            {
                Utils.ZBDLog("region res " + regionRes.success);
                if (regionRes.success)
                {
                    //If the sign up api returned an error that the user was on a VPN we should have set the check key to true
                    if (PlayerPrefs.GetInt(ZBDConstants.CHECK_VPN_ACTIVE_KEY, 0) == 1)
                    {
                        //If the ip is the same as the last ip it means their vpn is still active
                        if (regionRes.data.ipAddress == PlayerPrefs.GetString(ZBDConstants.CLIENT_IP_KEY, regionRes.data.ipAddress))
                        {
                            Utils.SendSignUpError(403, ZBDConstants.VPN_ERROR_MESSAGE);
                            return;
                        }
                        //If its not we can turn off the check vpn key as the ip has changed so the vpn could have been turned off
                        PlayerPrefs.SetInt(ZBDConstants.CHECK_VPN_ACTIVE_KEY, 0);
                    }

                    PlayerPrefs.SetString(ZBDConstants.CLIENT_IP_KEY, regionRes.data.ipAddress);
                    string customUserId = PlayerPrefs.GetString(ZBDConstants.CUSTOM_USERID_KEY, "");
                    Utils.ZBDLog("Get Request Id");
                    Utilities.Instance.GetRequestId(customUserId, requestIdRes =>
                    {
                        requestIdResponse = requestIdRes;
                        Utils.ZBDLog($"requestId {requestIdRes.requestId}");
                        Utils.ZBDLog($"requestIdError {requestIdRes.requestIdError}");
                        ZBDMainThreadDispatcher.Enqueue(() => SignUp(username));

                    });

                }
                else
                {
                    Utils.ZBDLogError("error");
                    Utilities.Instance.ShowAlert("error", "error checking region");
                }

            });

        }



        #region Signup

        public void SignUp(string username)
        {
            if (requestIdResponse.requestId.Length <= 0)
            {
                Utils.ZBDLogError("request id not found " + requestIdResponse.requestIdError);
                Utils.SendSignUpError(400, "request id not found " + requestIdResponse.requestIdError);
                return; // Fingerprint not set

            }

            PlayerPrefs.SetString(ZBDConstants.FINGERPRINT_REQUEST_ID, requestIdResponse.requestId);
            PlayerPrefs.SetInt(ZBDConstants.FINGERPRINT_COUNT_KEY, PlayerPrefs.GetInt(ZBDConstants.FINGERPRINT_COUNT_KEY, 0) + 1);


            if (username.Length <= 0)
            {
                Utils.ZBDLogError("username not set");
                Utils.SendSignUpError(400, "username not set");
                return; // No gamer tag

            }
            Debug.Log("h5");
            PlayerPrefs.SetString(ZBDConstants.USERNAME_KEY, username);

            var validation = new ZBDValidation
            {
                userId = SystemInfo.deviceUniqueIdentifier,
                appUsageStats = new ZBDAppUsageStats
                {
                    packageName = Application.identifier
                }
            };

            var payload = JsonConvert.SerializeObject(validation);

            var signUp = new ZBDSignUp
            {
                validation = validation,
                userInput = username,
                rewardsAppName = Application.identifier,
                requestId = requestIdResponse.requestId,
            };

            var payloadJSON = JsonConvert.SerializeObject(signUp);
            Utils.ZBDLog($"sign up payload:{payloadJSON}");
            StartCoroutine(StartSignUp(payloadJSON));
        }



        private IEnumerator StartSignUp(string payload)
        {
            string url = Utilities.Instance.GetAPIUrl() + "/api/v1/rewards/signup";
            UnityWebRequest request = Utils.CreatePOSTRequest(url, payload);
            yield return request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string message = Utils.HandleError(request);

                if (Utils.IsVPNError(request))
                {
                    PlayerPrefs.SetInt(ZBDConstants.CHECK_VPN_ACTIVE_KEY, 1);
                }


                ModalController.Instance.SendWebViewMessage(message);
            }
            else
            {

                Utils.ZBDLog($"success:{request.downloadHandler.text}");
                var signUpResponse = JsonConvert.DeserializeObject<ZBDSignUpResponse>(request.downloadHandler.text);

                var userDetailsId = signUpResponse.data.id;
                Utils.SetUserDetailsId(userDetailsId);



                var rep = new ZBDWebViewResponse
                {
                    httpStatusCode = 200,
                    response = signUpResponse
                };
                var message = JsonConvert.SerializeObject(rep);
                ModalController.Instance.SendWebViewMessage(message);

                StartAntiCheat();
                SendProgress();


            }

        }


        #endregion



        #region Progress

        public void SendProgress()
        {

            if (Utils.GetUserDetailsId().Length == 0)
            {
                ScheduleSendProgress();
                return;
            }

            var username = PlayerPrefs.GetString(ZBDConstants.USERNAME_KEY);
            if (username.Length > 0)
            {
                var validation = new ZBDValidation
                {
                    userId = SystemInfo.deviceUniqueIdentifier
                };

                var payload = JsonConvert.SerializeObject(validation);

                validation.appUsageStats = new ZBDAppUsageStats();

                validation.appUsageStats.packageName = Application.identifier;

                ZBDUserProgress userProgress = new ZBDUserProgress();

                userProgress.timePlayedInMinutes = PlaytimeTracker.Instance.GetTotalPlayTimeMins();
                userProgress.validation = validation;

                string payloadJSON = JsonConvert.SerializeObject(userProgress);

                Utils.ZBDLog(payloadJSON);
                StartCoroutine(StartUserProgress(payloadJSON));

            }
            else
            {
                Utils.ZBDLogError($"please enter a username");
                ScheduleSendProgress();
            }

        }


        private IEnumerator StartUserProgress(string payload)
        {


            var url = Utilities.Instance.GetAPIUrl() + "/api/v1/rewards/users/" + Utils.GetUserDetailsId() + "/progress";


            UnityWebRequest request = Utils.CreatePOSTRequest(url, payload);

            yield return request.SendWebRequest();

            try
            {
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {

                    Utils.ZBDLogError($"{request.responseCode} :{request.downloadHandler.text}");

                }
                else
                {
                    Utils.ZBDLog(request.downloadHandler.text);
                    currentProgressResponse = JsonConvert.DeserializeObject<ZBDProgressResponse>(request.downloadHandler.text);

                    if (ProgressWidgetController.Instance)
                    {
                        ProgressWidgetController.Instance.SetData(currentProgressResponse.data);
                    }

                }


                ScheduleSendProgress();

            }
            catch (Exception e)
            {
                Utils.ZBDLogError(e.ToString());
                ScheduleSendProgress();
            }

        }

        public string GetRewardsUserDetails()
        {
            return "";
        }

        private void ScheduleSendProgress()
        {
            StartCoroutine(SendProgressTimed());
        }

        IEnumerator SendProgressTimed()
        {
            yield return new WaitForSeconds(5);
            SendProgress();
        }

        #endregion



        #region Utility

        public ZBDProgress GetProgress()
        {
            if (currentProgressResponse != null)
            {
                return currentProgressResponse.data;
            }
            return null;
        }

        public ZBDUserStatus GetUserStatus()
        {
            ZBDUserStatus status = new ZBDUserStatus();
            status.status = ZBDConstants.UNLINKED_STATUS;

            status.rewardsUserId = Utils.GetUserDetailsId();
            // If the userDetailsId exists it means the user did sign in at least once
            if (status.rewardsUserId.Length > 0)
            {
                status.status = ZBDConstants.LINKED_STATUS;
            }

            return status;
        }

        public void StartTracking()
        {
            PlaytimeTracker.Instance.StartTracking();
            Quago.beginTracking();
        }

        public void PauseTracking()
        {
            PlaytimeTracker.Instance.PauseTracking();
            Quago.endTracking();
        }

        #endregion

        // validate that app token has been populated
        private void OnValidate()
        {
            if (appToken.Length == 0)
            {
                Utils.ZBDLogError("[ZBD] App token not set in inspector");
            }
        }



    }

}
