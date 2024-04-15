using UnityEngine;
using UnityEngine.Serialization;

namespace ZBD
{
    public class PlaytimeTracker : MonoBehaviourSingleton<PlaytimeTracker>
    {
        [Space(10)]
        [Header("Playtime Tracking")]
        [Space(5)]
        [Header("The amount of inactive playtime that result in tracking to stop (seconds)")]
        [SerializeField] public float inactivityThreshold = 10f; // Set inactivity threshold (e.g., 10 seconds)

        private float _lastActiveTime;
        private float _totalPlayTime;
        private bool _pauseOverride;
        private bool _isActive;


        protected override void Awake()
        {
            base.Awake();

            _totalPlayTime = LoadPlayTime();

        }

        private void Start()
        {
            ForceStart();
        }

        private void OnDestroy()
        {
            SavePlayTime();
        }

        private void OnApplicationQuit()
        {
            SavePlayTime();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SavePlayTime();
            }
        }
        public void ForceStart()
        {
            _isActive = true;
            _lastActiveTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (_pauseOverride) { return; }


            // Check for user interaction
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                _isActive = true;
                _lastActiveTime = Time.unscaledTime;
            }

            // Update total playtime if within active threshold
            if (Time.unscaledTime - _lastActiveTime < inactivityThreshold)
            {
                _totalPlayTime += Time.unscaledDeltaTime;
            }
            else
            {
                _isActive = false;
            }
        }

        private float LoadPlayTime()
        {
            return Utilities.Instance.GetFloat(ZBDConstants.PLAY_TIME_KEY, 0);
        }

        private void SavePlayTime()
        {
            // Save the updated total playtime
            Utilities.Instance.SetFloat(ZBDConstants.PLAY_TIME_KEY, _totalPlayTime);
        }

        public float GetTotalPlayTime()
        {
            return _totalPlayTime;
        }

        public long GetTotalPlayTimeMins()
        {
            return (long)(_totalPlayTime / 60);
        }

        public bool PauseTracking()
        {
            _pauseOverride = true;
            return true;
        }

        public bool ResumeTracking()
        {
            _pauseOverride = false;
            return true;
        }

        public bool StartTracking()
        {
            _pauseOverride = false;
            return true;
        }

        public bool IsActive()
        {
            return _isActive;
        }

        public bool IsPaused()
        {
            return _pauseOverride;
        }
    }
}