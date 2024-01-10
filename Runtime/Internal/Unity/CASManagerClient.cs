//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CAS.Unity
{
    [AddComponentMenu("")]
    internal class CASManagerClient : MonoBehaviour, IInternalManager
    {
        private List<Action> _eventsQueue = new List<Action>();
        private GUIStyle _btnStyle = null;

        [SerializeField]
        private string casId;

        private bool[] enabledTypes;

        [SerializeField]
        private List<CASViewClient> adViews = new List<CASViewClient>();
        [SerializeField]
        private CASFullscreenView _interstitial;
        [SerializeField]
        private CASFullscreenView _rewarded;
        
        private CASInitCompleteEvent _initCompleteEvent;
        private InitCompleteAction _initCompleteAction;
        private LastPageAdContent _lastPageAdContent;

        internal CASSettingsClient _settings;

        public string managerID { get { return casId; } }
        public bool isTestAdMode { get { return true; } }
        public InitialConfiguration initialConfig {
            get { return new InitialConfiguration(null, this, "US", true); }
        }

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                _lastPageAdContent = value;
                if (value == null)
                    Log("CAS Last Page Ad content cleared");
                else
                    Log(new StringBuilder("CAS Last Page Ad apply content:")
                        .Append("\n- Headline:").Append(value.Headline)
                        .Append("\n- DestinationURL:").Append(value.DestinationURL)
                        .Append("\n- ImageURL:").Append(value.ImageURL)
                        .Append("\n- IconURL:").Append(value.IconURL)
                        .Append("\n- AdText:").Append(value.AdText)
                        .ToString());
            }
        }

#pragma warning disable 67
        #region Interstitial ad Events
        public event Action OnInterstitialAdLoaded
        {
            add { _interstitial.OnAdLoaded += value; }
            remove { _interstitial.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnInterstitialAdFailedToLoad
        {
            add { _interstitial.OnAdFailedToLoad += value; }
            remove { _interstitial.OnAdFailedToLoad -= value; }
        }
        public event Action OnInterstitialAdShown
        {
            add { _interstitial.OnAdShown += value; }
            remove { _interstitial.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnInterstitialAdOpening
        {
            add { _interstitial.OnAdOpening += value; }
            remove { _interstitial.OnAdOpening -= value; }
        }
        public event CASEventWithMeta OnInterstitialAdImpression
        {
            add { _interstitial.OnAdImpression += value; }
            remove { _interstitial.OnAdImpression -= value; }
        }
        public event CASEventWithError OnInterstitialAdFailedToShow
        {
            add { _interstitial.OnAdFailedToShow += value; }
            remove { _interstitial.OnAdFailedToShow -= value; }
        }
        public event Action OnInterstitialAdClicked
        {
            add { _interstitial.OnAdClicked += value; }
            remove { _interstitial.OnAdClicked -= value; }
        }
        public event Action OnInterstitialAdClosed
        {
            add { _interstitial.OnAdClosed += value; }
            remove { _interstitial.OnAdClosed -= value; }
        }
        #endregion

        #region Rewarded Ad Events
        public event Action OnRewardedAdLoaded
        {
            add { _rewarded.OnAdLoaded += value; }
            remove { _rewarded.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnRewardedAdFailedToLoad
        {
            add { _rewarded.OnAdFailedToLoad += value; }
            remove { _rewarded.OnAdFailedToLoad -= value; }
        }
        public event Action OnRewardedAdShown
        {
            add { _rewarded.OnAdShown += value; }
            remove { _rewarded.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnRewardedAdOpening
        {
            add { _rewarded.OnAdOpening += value; }
            remove { _rewarded.OnAdOpening -= value; }
        }
        public event CASEventWithMeta OnRewardedAdImpression
        {
            add { _rewarded.OnAdImpression += value; }
            remove { _rewarded.OnAdImpression -= value; }
        }
        public event CASEventWithError OnRewardedAdFailedToShow
        {
            add { _rewarded.OnAdFailedToShow += value; }
            remove { _rewarded.OnAdFailedToShow -= value; }
        }
        public event Action OnRewardedAdClicked
        {
            add { _rewarded.OnAdClicked += value; }
            remove { _rewarded.OnAdClicked -= value; }
        }
        public event Action OnRewardedAdCompleted
        {
            add { _rewarded.OnAdCompleted += value; }
            remove { _rewarded.OnAdCompleted -= value; }
        }
        public event Action OnRewardedAdClosed
        {
            add { _rewarded.OnAdClosed += value; }
            remove { _rewarded.OnAdClosed -= value; }
        }
        #endregion

        #region Return to app not supported for Editor
        public event Action OnAppReturnAdShown;
        public event CASEventWithMeta OnAppReturnAdOpening;
        public event CASEventWithMeta OnAppReturnAdImpression;
        public event CASEventWithError OnAppReturnAdFailedToShow;
        public event Action OnAppReturnAdClicked;
        public event Action OnAppReturnAdClosed;
        #endregion
#pragma warning restore 67

        internal static IInternalManager Create(CASInitSettings initSettings)
        {
            var obj = new GameObject("[CAS] Mediation Manager");
            //obj.hideFlags = HideFlags.HideInHierarchy;
            MonoBehaviour.DontDestroyOnLoad(obj);
            var behaviour = obj.AddComponent<CASManagerClient>();
            behaviour.Init(initSettings);
            return behaviour;
        }

        private void Init(CASInitSettings initSettings)
        {
            // Set Settings before any other calls.
            _settings = CAS.MobileAds.settings as CASSettingsClient;

            Log("Initialized manager with id: " + initSettings.targetId);
            casId = initSettings.targetId;
            enabledTypes = new bool[(int)AdType.None];
            for (int i = 0; i < enabledTypes.Length; i++)
                enabledTypes[i] = ((int)initSettings.defaultAllowedFormats & (1 << i)) != 0;

            _initCompleteEvent = initSettings.initListener;
            _initCompleteAction = initSettings.initListenerDeprecated;
            _interstitial = new CASFullscreenView(this, AdType.Interstitial);
            _rewarded = new CASFullscreenView(this, AdType.Rewarded);
        }

        public void HandleInitEvent(CASInitCompleteEvent initEvent, InitCompleteAction initAction)
        {
            if (initEvent != null)
                initEvent(initialConfig);
            if (initAction != null)
                initAction(true, null);
        }

        #region IMediationManager implementation

        public bool IsEnabledAd(AdType adType)
        {
            return enabledTypes[(int)adType];
        }

        public void SetEnableAd(AdType adType, bool enabled)
        {
            enabledTypes[(int)adType] = enabled;
            if (enabled)
            {
                if (adType == AdType.Banner)
                {
                    for (int i = 0; i < adViews.Count; i++)
                        adViews[i].Load();
                    return;
                }
                LoadAd(adType);
            }
        }

        public bool IsReadyAd(AdType adType)
        {
            if (adType == AdType.Banner)
                return false;
            switch (adType)
            {
                case AdType.Interstitial:
                    return _interstitial.GetReadyError().HasValue == false;
                case AdType.Rewarded:
                    return _rewarded.GetReadyError().HasValue == false;
            }
            return false;
        }

        public void LoadAd(AdType adType)
        {
            switch (adType)
            {
                case AdType.Banner:
                    throw new NotSupportedException("Use GetAdView(AdSize).Load() instead");
                case AdType.Interstitial:
                    _interstitial.Load();
                    break;
                case AdType.Rewarded:
                    _rewarded.Load();
                    break;
            }
        }

        public void ShowAd(AdType adType)
        {
            switch (adType)
            {
                case AdType.Banner:
                    throw new NotSupportedException("Use GetAdView(AdSize).SetActive(true) instead");
                case AdType.Interstitial:
                    Post(_interstitial.Show);
                    break;
                case AdType.Rewarded:
                    Post(_rewarded.Show);
                    break;
            }
        }

        public bool TryOpenDebugger()
        {
            // Not supported for editor
            return false;
        }

        public void SetAppReturnAdsEnabled(bool enable)
        {
            Log("App return ads " + (enable ? "enabled" : "disabled"));
        }

        public void SkipNextAppReturnAds()
        {
            Log("The next time user return to the app, no ads will appear.");
        }

        public IAdView GetAdView(AdSize size)
        {
            if (size < AdSize.Banner)
                throw new ArgumentException("Invalid AdSize " + size.ToString());
            for (int i = 0; i < adViews.Count; i++)
            {
                if (adViews[i].size == size)
                    return adViews[i];
            }
            var view = new CASViewClient(this, size);
            adViews.Add(view);
            return view;
        }

        public void RemoveAdViewFromFactory(CASViewClient view)
        {
            adViews.Remove(view);
        }
        #endregion

        #region MonoBehaviour
        private void Start()
        {
            if (isAutolod)
            {
                LoadAd(AdType.Interstitial);
                LoadAd(AdType.Rewarded);
            }
            Post(CallInitComplete);
        }

        private void CallInitComplete()
        {
            CASFactory.OnManagerInitialized(this);
            HandleInitEvent(_initCompleteEvent, _initCompleteAction);
            _initCompleteEvent = null;
            _initCompleteAction = null;
        }

        public void Update()
        {
            if (_eventsQueue.Count == 0)
                return;
            for (int i = 0; i < _eventsQueue.Count; i++)
            {
                try
                {
                    var action = _eventsQueue[i];
                    if (action != null)
                        action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _eventsQueue.Clear();
        }

        public void OnGUI()
        {
            if (_btnStyle == null)
                _btnStyle = new GUIStyle("Button");
            _btnStyle.fontSize = (int)(Math.Min(Screen.width, Screen.height) * 0.035f);

            for (int i = 0; i < adViews.Count; i++)
            {
                adViews[i].OnGUIAd(_btnStyle);
            }
            _interstitial.OnGUIAd(_btnStyle);
            _rewarded.OnGUIAd(_btnStyle);
        }
        #endregion

        #region Utils
        public void Post(Action action)
        {
            if (action == null)
                return;
            //Log( "Event " + action.Target.GetType().FullName + "." + action.Method.Name );
            _eventsQueue.Add(action);
        }

        public void Post(Action action, float delay)
        {
            if (action == null)
                return;
            //Log( "Event " + action.Target.GetType().FullName + "." + action.Method.Name );
            StartCoroutine(DelayAction(action, delay));
        }

        internal void Log(string message)
        {
            if (_settings.isDebugMode)
                Debug.Log("[CAS.AI] " + message);
        }

        public bool isFullscreenAdVisible
        {
            get { return _rewarded.active || _interstitial.active; }
        }
        public bool isAutolod
        {
            get { return _settings.loadingMode != LoadingManagerMode.Manual; }
        }

        private IEnumerator DelayAction(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action();
        }
        #endregion
    }
}
#endif