using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZBD;

public class ProgressWidgetController : MonoBehaviour
{
    public static ProgressWidgetController Instance;

    #region Public Variables 
    public TextMeshProUGUI nextRewardAmountLabel;
    public GameObject nextRewardCoinIcon;
    public GameObject nextRewardAmountLabelEnd;
    public TMP_Text currentEarnedTodayLabel;
    #endregion

    #region Private Variables
    private float initialCountdownDuration = 0f;
    private float countdownDuration;
    private float countdownStartTime;
    private float animationDuration;
    private string lastProgressId;

    #endregion

    public UILineRenderer lineRenderer;
    List<Vector2> pathPoints = new List<Vector2>();
    public GameObject points;


    public GameObject circle;
    private int currentPointIndex = 0;
    private int lastPointIndex;
    private float totalPathDistance = 0f;
    private float speed;

    private float elapsedTime = 0f; // Tracks the elapsed time since the start
    private List<float> segmentDistances = new List<float>();

    private float pausedAt = 0;// track the time when the user is not playing
    public Canvas canvas;
    private float nextTimeToRun = 0f;
    private float interval = 1f / 20f; // Run 20 times a second
    private int lineCount;
    private float nextTimeToDraw = 0f;
    private float drawInterval = 1f / 2f; // Draw twice a second

    #region MonoBehaviour Callbacks and Loading animation controller

    private void Awake()
    {

        Instance = this;
        if (nextRewardAmountLabelEnd != null)
        {
            nextRewardAmountLabelEnd.SetActive(false);
        }


    }


    public void SetTime(float time)
    {
        initialCountdownDuration = time;
        countdownDuration = time;
        countdownStartTime = Time.time;

        StartAnimation();
    }



    GameObject[] GetChildrenOrderedByIndex(Transform parent)
    {
        GameObject[] children = new GameObject[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i).gameObject;
            children[i].name = "point" + i;
        }
        return children;
    }

    void StartAnimation()
    {
        lineRenderer.ResetPoints();
        GameObject[] pathObjects = GetChildrenOrderedByIndex(points.transform);
        pathPoints = new List<Vector2>();
        foreach (GameObject path in pathObjects)
        {
            pathPoints.Add(path.transform.position);
            path.SetActive(false);
        }

        pathPoints.Add(pathObjects[0].transform.position);
        segmentDistances = new List<float>();
        totalPathDistance = 0;
        currentPointIndex = 0;
        lineCount = 0;

        float segmentDistance;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            segmentDistance = Vector2.Distance(pathPoints[i], pathPoints[i + 1]);
            totalPathDistance += segmentDistance;

            segmentDistances.Add(segmentDistance);
        }

        // Handle the case where the app is closed and reopended, start the progress from where it left off
        LoadSavedProgress();
        speed = totalPathDistance / countdownDuration;

        circle.transform.position = pathPoints[currentPointIndex];

    }


    void Update()
    {
        ZBDProgress progressResponse = ZBDController.Instance.GetProgress();
        if (progressResponse == null)
        {
            return;
        }

        if (PlaytimeTracker.Instance && (PlaytimeTracker.Instance.IsPaused() || !PlaytimeTracker.Instance.IsActive()))
        {
            if (pausedAt == 0)
            {
                pausedAt = Time.time;
            }

            return;
        }
        if (pausedAt != 0)
        {
            countdownStartTime += (Time.time - pausedAt);
            pausedAt = 0;
        }


        SetData(progressResponse);

        float timeRemaining = countdownDuration - (Time.time - countdownStartTime);

        // If countdown reaches or goes below 0, reset the countdown
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
        }



        // Calculate hours, minutes, and seconds
        int hours = Mathf.FloorToInt(timeRemaining / 3600);
        int minutes = Mathf.FloorToInt((timeRemaining % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);



        if (timeRemaining > 0)
        {
            elapsedTime += Time.deltaTime;


            float targetDistance = (elapsedTime / countdownDuration) * totalPathDistance;
            float distanceCovered = 0f;
            int pointIndex = 0;

            // Find current segment based on the distance covered
            while (pointIndex < segmentDistances.Count - 1 && distanceCovered + segmentDistances[pointIndex] < targetDistance)
            {
                distanceCovered += segmentDistances[pointIndex];
                pointIndex++;
            }


            // Calculate how far to move within the current segment
            float remainingDistance = targetDistance - distanceCovered;
            if (pointIndex < pathPoints.Count - 1)
            {
                Vector2 startPosition = pathPoints[pointIndex];
                Vector2 endPosition = pathPoints[pointIndex + 1];
                float segmentRatio = remainingDistance / segmentDistances[pointIndex];
                circle.transform.position = Vector2.Lerp(startPosition, endPosition, segmentRatio);

                if (lineRenderer != null && lineCount < 20000) //we dont want too many lines as it hurts performance
                {
                    if (Time.time >= nextTimeToDraw) // only draw a line after some time not every frame for performance reasons
                    {
                        nextTimeToDraw = Time.time + drawInterval;
                        lineRenderer.AddPoint(circle.GetComponent<RectTransform>().anchoredPosition);
                        lineCount++;

                    }
                }


            }
            lastPointIndex = pointIndex;
        }

    }


    #endregion

    private void LoadSavedProgress()
    {
        elapsedTime = LoadElapsedTime();
        PlayerPrefs.SetFloat(ZBDConstants.ELAPSED_TIME_KEY + lastProgressId, 0);

        //draw until saved point
        int lastPointIndex = PlayerPrefs.GetInt(ZBDConstants.LAST_POINT_INDEX + lastProgressId, 0);

        Utils.ZBDLog("loading saved progress " + elapsedTime + " " + lastPointIndex);
        if (lastPointIndex != 0)
        {

            for (int i = 0; i < lastPointIndex; i++)
            {

                Vector2 endPosition = pathPoints[i];

                circle.transform.position = endPosition;

                lineRenderer.AddPoint(circle.GetComponent<RectTransform>().anchoredPosition);
            }

        }

        PlayerPrefs.SetInt(ZBDConstants.LAST_POINT_INDEX + lastProgressId, 0);
    }
    private void OnApplicationQuit()
    {

        SaveProgress();
    }


    private void OnDestroy()
    {
        SaveProgress();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveProgress();
        }
    }

    void SaveProgress()
    {
        Utils.ZBDLog("save progress");
        //save current progress
        PlayerPrefs.SetInt(ZBDConstants.LAST_POINT_INDEX + lastProgressId, lastPointIndex);
        PlayerPrefs.SetFloat(ZBDConstants.ELAPSED_TIME_KEY + lastProgressId, elapsedTime);


    }

    private float LoadElapsedTime()
    {
        return PlayerPrefs.GetFloat(ZBDConstants.ELAPSED_TIME_KEY + lastProgressId, 0);
    }

    public void SetData(ZBDProgress progressResponse)
    {

        if (lastProgressId != progressResponse.nextRewardId)
        {
            // if the new progress id is not the same as the old then we can delete any aved player prefs using the old id
            string lastCachedProgressId = PlayerPrefs.GetString(ZBDConstants.LAST_PROGRESS_ID, "");
            PlayerPrefs.DeleteKey(ZBDConstants.LAST_POINT_INDEX + lastCachedProgressId);
            PlayerPrefs.DeleteKey(ZBDConstants.ELAPSED_TIME_KEY + lastCachedProgressId);

            lastProgressId = progressResponse.nextRewardId;
            PlayerPrefs.SetString(ZBDConstants.LAST_PROGRESS_ID, lastProgressId);


            currentEarnedTodayLabel.text = (progressResponse.currentEarnedToday / 1000) + "";
            SetTime((float)(progressResponse.timeUntilNextReward * 60));
            long satsAmount = progressResponse.nextRewardAmount / 1000;

            if (nextRewardCoinIcon != null)
            {
                nextRewardCoinIcon.SetActive(true);
            }
            if (nextRewardAmountLabel != null)
            {
                nextRewardAmountLabel.text = satsAmount + "";
                nextRewardAmountLabel.gameObject.SetActive(true);
            }
            circle.SetActive(true);
            if (nextRewardAmountLabelEnd != null)
            {
                nextRewardAmountLabelEnd.SetActive(false);
            }

            if (progressResponse.nextRewardId == "")
            {
                if (nextRewardCoinIcon != null)
                {
                    nextRewardCoinIcon.SetActive(false);
                }
                if (nextRewardAmountLabel != null)
                {
                    nextRewardAmountLabel.gameObject.SetActive(false);
                }
                if (nextRewardAmountLabelEnd != null)
                {
                    nextRewardAmountLabelEnd.SetActive(true);
                }
                circle.SetActive(false);
            }
        }
    }

    public void OpenModal()
    {
        if (ModalController.Instance)
        {
            ModalController.Instance.ShowWebView();
        }
        else
        {
            Debug.LogError("Modal Controller not found");
        }
    }
}
