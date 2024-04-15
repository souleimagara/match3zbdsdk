using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using ZBD;

public class DevTools : MonoBehaviour
{

    public GameObject demoButton;
    public TMP_InputField urlField;

    private void Start()
    {
        urlField.text = PlayerPrefs.GetString("webViewUrl");
    }
    public void SkipTime()
    {
        StartCoroutine(UpdateTimePlayed());
    }

    private IEnumerator UpdateTimePlayed()
    {

        demoButton.SetActive(false);
        var url = Utilities.Instance.GetAPIUrl() + "/api/v1/rewards/users/" + Utils.GetUserDetailsId() + "/update";


        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Set the request header to indicate JSON content type
            request.SetRequestHeader("Content-Type", "application/json");

            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();


            yield return request.SendWebRequest();

            demoButton.SetActive(true);
            // Check for errors
            if (request.isNetworkError || request.isHttpError)
            {

                Debug.LogError(request.responseCode + " " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);

            }

        }



    }

    public void UpdateWebSiteUrl()
    {
        string webUrl = urlField.text;

        PlayerPrefs.SetString("webViewUrl", webUrl);
    }

}
