//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CAS.Unity
{

    internal sealed class CASManagerClient : CASManagerBase
    {
        private CASManagerBehaviour _behaviour;

        internal CASManagerClient()
        {
            var obj = new GameObject("[CAS] Mediation Manager");
            //obj.hideFlags = HideFlags.HideInHierarchy;
            MonoBehaviour.DontDestroyOnLoad(obj);
            _behaviour = obj.AddComponent<CASManagerBehaviour>();
            _behaviour.client = this;
        }

        internal override void Init(CASInitSettings initSettings)
        {
            base.Init(initSettings);
            _behaviour.casId = managerID;

            CASFactory.HandleConsentFlow(initSettings.consentFlow, ConsentFlow.Status.Obtained);
        }

        protected override void SetLastPageAdContentNative(string json)
        {
            if (json.Length == 0)
                CASFactory.UnityLog("CAS Last Page Ad content cleared");
            else
                CASFactory.UnityLog("CAS Last Page Ad apply content: " + json);
        }

        public override void EnableAd(AdType adType)
        {
            _behaviour.LoadAd(adType);
        }

        public override void DisposeAd(AdType adType)
        {
            _behaviour.DisposeAd(adType);
        }

        public override bool IsReadyAd(AdType adType)
        {
            return _behaviour.IsReadyAd(adType);
        }

        protected override void LoadAdNetive(AdType adType)
        {
            _behaviour.LoadAd(adType);
        }

        public override void ShowAd(AdType adType)
        {
            _behaviour.ShowAd(adType);
        }

        public bool TryOpenDebugger()
        {
            CASFactory.UnityLog("Ad Debugger in Editor not supported.");
            return false;
        }

        public override void SetAppReturnAdsEnabled(bool enable)
        {
            CASFactory.UnityLog("Set auto show ad on App Return enabled: " + enable +
                ". But Auto Show Ad in Unity Editor not supported");
        }

        public override void SkipNextAppReturnAds()
        {
            CASFactory.UnityLog("The next time user return to the app, no ads will appear.");
        }

        public override AdMetaData WrapImpression(AdType adType, object impression)
        {
            return new CASImpressionClient(adType);
        }

        protected override CASViewBase CreateAdView(AdSize size)
        {
            var view = new CASViewClient(this, _behaviour, size);
            _behaviour._adViews.Add(view);
            return view;
        }

        public override void RemoveAdViewFromFactory(CASViewBase view)
        {
            base.RemoveAdViewFromFactory(view);
            _behaviour._adViews.Remove(view as CASViewClient);
        }
    }

    [AddComponentMenu("")]
    internal sealed class CASManagerBehaviour : MonoBehaviour
    {
        private List<Action> _eventsQueue = new List<Action>();
        private GUIStyle _btnStyle = null;

        public string casId;
        internal CASManagerClient client;
        internal CASSettingsClient _settings;

        public List<CASViewClient> _adViews = new List<CASViewClient>();
        public CASFullscreenView _interstitial;
        public CASFullscreenView _rewarded;
        public CASFullscreenView _appOpen;

        public void Awake()
        {
            // Set Settings before any other calls.
            _settings = CAS.MobileAds.settings as CASSettingsClient;

            _interstitial = new CASFullscreenView(this, AdType.Interstitial);
            _rewarded = new CASFullscreenView(this, AdType.Rewarded);
            _appOpen = new CASFullscreenView(this, AdType.AppOpen);
        }

        public void Start()
        {
            Post(() => client.OnInitialized(null, "US", true, true, (int)ConsentFlow.Status.Obtained));
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

            for (int i = 0; i < _adViews.Count; i++)
            {
                _adViews[i].OnGUIAd(_btnStyle);
            }
            _interstitial.OnGUIAd(_btnStyle);
            _rewarded.OnGUIAd(_btnStyle);
            _appOpen.OnGUIAd(_btnStyle);
        }

        internal void DisposeAd(AdType adType)
        {
            switch (adType)
            {
                case AdType.Banner:
                    throw new NotSupportedException("Use GetAdView(AdSize).Dispose() instead");
                case AdType.Interstitial:
                    _interstitial.Dispose();
                    break;
                case AdType.Rewarded:
                    _rewarded.Dispose();
                    break;
                case AdType.AppOpen:
                    _appOpen.Dispose();
                    break;
                default:
                    throw new NotSupportedException("Destroy ad function not support for AdType: " + adType.ToString());
            }
        }

        internal bool IsReadyAd(AdType adType)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                    return _interstitial.GetReadyError() == AdError.Internal;
                case AdType.Rewarded:
                    return _rewarded.GetReadyError() == AdError.Internal;
                case AdType.AppOpen:
                    return _appOpen.GetReadyError() == AdError.Internal;
            }
            return false;
        }

        internal void LoadAd(AdType adType)
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
                case AdType.AppOpen:
                    _appOpen.Load();
                    break;
                default:
                    throw new NotSupportedException("Load ad function not support for AdType: " + adType.ToString());
            }
        }

        internal void ShowAd(AdType adType)
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
                case AdType.AppOpen:
                    Post(_appOpen.Show);
                    break;
                default:
                    throw new NotSupportedException("Show ad function not support for AdType: " + adType.ToString());
            }
        }

        internal void RemoveAdViewFromFactory(CASViewClient view)
        {
            _adViews.Remove(view);
            client.RemoveAdViewFromFactory(view);
        }

        internal bool isFullscreenAdVisible
        {
            get { return _rewarded.active || _interstitial.active; }
        }

        public void Post(Action action, float delay = 0.0f)
        {
            if (action != null)
            {
                if (delay > 0.0f)
                    StartCoroutine(DelayAction(action, delay));
                else
                    _eventsQueue.Add(action);
            }
        }

        private IEnumerator DelayAction(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action();
        }
    }
}
#endif