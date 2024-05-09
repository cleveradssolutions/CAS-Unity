//  Copyright © 2024 CAS.AI. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace CAS.AdObject
{
    internal class BaseAdObjectInspector : Editor
    {
        protected bool loadEventsFoldout;
        protected bool contentEventsFoldout;

        protected SerializedProperty managerIdProp;

        protected SerializedProperty onAdLoadedProp;
        protected SerializedProperty onAdFailedToLoadProp;
        protected SerializedProperty onAdShownProp;
        protected SerializedProperty onAdClickedProp;
        protected SerializedProperty onAdImpressionProp;

        protected virtual void OnEnable()
        {
            var obj = serializedObject;
            managerIdProp = obj.FindProperty("managerId");

            onAdLoadedProp = obj.FindProperty("OnAdLoaded");
            onAdFailedToLoadProp = obj.FindProperty("OnAdFailedToLoad");
            onAdShownProp = obj.FindProperty("OnAdShown");
            onAdClickedProp = obj.FindProperty("OnAdClicked");
            onAdImpressionProp = obj.FindProperty("OnAdImpression");
        }

        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            obj.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(managerIdProp);
            OnAdditionalPropertiesGUI();

            loadEventsFoldout = GUILayout.Toggle(loadEventsFoldout, "Ad Load events", EditorStyles.foldout);
            if (loadEventsFoldout)
            {
                EditorGUILayout.PropertyField(onAdLoadedProp);
                EditorGUILayout.PropertyField(onAdFailedToLoadProp);
            }

            contentEventsFoldout = GUILayout.Toggle(contentEventsFoldout, "Ad Content events", EditorStyles.foldout);
            if (contentEventsFoldout)
                OnCallbacksGUI();

            OnFooterGUI();

            obj.ApplyModifiedProperties();
        }

        protected virtual void OnAdditionalPropertiesGUI() { }

        protected virtual void OnFooterGUI() { }

        protected virtual void OnCallbacksGUI()
        {
            EditorGUILayout.PropertyField(onAdShownProp);
            EditorGUILayout.PropertyField(onAdClickedProp);
            EditorGUILayout.PropertyField(onAdImpressionProp);
        }
    }

    [CustomEditor(typeof(ManagerAdObject))]
    internal class ManagerAdObjectInspector : Editor
    {
        protected bool metaPrivacyFoldout;
        private SerializedProperty managerIdProp;
        private SerializedProperty initializeOnAwakeProp;
        private SerializedProperty metaDataProcessingProp;
        private SerializedProperty metaAdvertiserTrackingProp;
        private SerializedProperty onInitializedProp;
        private SerializedProperty onInitializationFailedProp;
        private SerializedProperty consentFlowEnabledProp;
        private SerializedProperty consentFlowProp;

        protected void OnEnable()
        {
            var obj = serializedObject;
            managerIdProp = obj.FindProperty("managerId");
            initializeOnAwakeProp = obj.FindProperty("initializeOnAwake");
            metaDataProcessingProp = obj.FindProperty("metaDataProcessing");
            metaAdvertiserTrackingProp = obj.FindProperty("metaAdvertiserTracking");

            onInitializedProp = obj.FindProperty("OnInitialized");
            onInitializationFailedProp = obj.FindProperty("OnInitializationFailed");

            consentFlowEnabledProp = obj.FindProperty("consentFlowEnabled");
            consentFlowProp = obj.FindProperty("consentFlow");
        }

        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            obj.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(managerIdProp);
            EditorGUILayout.PropertyField(initializeOnAwakeProp);
            EditorGUI.indentLevel++;
            if (initializeOnAwakeProp.boolValue)
                EditorGUILayout.HelpBox("The CAS begin initialization automatically when the component is awake", MessageType.None);
            else
                EditorGUILayout.HelpBox("Call `Initialize()` method to begin CAS initialization", MessageType.None);
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(consentFlowEnabledProp);
            if (consentFlowEnabledProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("If desired, select an ConsentFlow Ad Object for additional options or leave it blank", MessageType.None);
                EditorGUILayout.PropertyField(consentFlowProp);
                if (consentFlowProp.objectReferenceValue)
                    EditorGUILayout.HelpBox("The selected component should not be used to manually show the consent flow.", MessageType.None);
                EditorGUI.indentLevel--;
            }

            metaPrivacyFoldout = GUILayout.Toggle(metaPrivacyFoldout, "Meta Audience Network Privacy", EditorStyles.foldout);
            if (metaPrivacyFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(metaDataProcessingProp);
                switch ((CCPAStatus)metaDataProcessingProp.intValue)
                {
                    case CCPAStatus.OptInSale:
                        EditorGUILayout.HelpBox("CAS will override Limited Data Use flag for Meta Audience Network:\nAdSettings.setDataProcessingOptions([])", MessageType.None);
                        break;
                    case CCPAStatus.OptOutSale:
                        EditorGUILayout.HelpBox("CAS will override Limited Data Use flag for Meta Audience Network:\nAdSettings.setDataProcessingOptions([\"LDU\"])", MessageType.None);
                        break;
                    default:
                        EditorGUILayout.HelpBox("CAS will not override Limited Data Use flag for Meta Audience Network.", MessageType.None);
                        break;
                }
                EditorGUILayout.PropertyField(metaAdvertiserTrackingProp);
                switch ((ConsentStatus)metaAdvertiserTrackingProp.intValue)
                {
                    case ConsentStatus.Accepted:
                        EditorGUILayout.HelpBox("[iOS] CAS will override Advertising Tracking flag for Meta Audience Network:\nAdSettings.setAdvertiserTrackingEnabled(true)", MessageType.None);
                        break;
                    case ConsentStatus.Denied:
                        EditorGUILayout.HelpBox("[iOS] CAS will override Advertising Tracking flag for Meta Audience Network:\nAdSettings.setAdvertiserTrackingEnabled(false)", MessageType.None);
                        break;
                    default:
                        EditorGUILayout.HelpBox("[iOS] CAS will not override Advertising Tracking flag for Meta Audience Network.", MessageType.None);
                        break;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(onInitializedProp);
            EditorGUILayout.PropertyField(onInitializationFailedProp);

            obj.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ConsentFlowAdObject))]
    internal class ConsentFlowAdObjectInspector : Editor
    {
        private SerializedProperty showOnAwakeIfRequiredProp;
        private SerializedProperty privacyPolicyUrlProp;
        private SerializedProperty debugGeographyProp;
        private SerializedProperty OnCompletedProp;
        private SerializedProperty OnFailedProp;

        protected void OnEnable()
        {
            var obj = serializedObject;
            showOnAwakeIfRequiredProp = obj.FindProperty("showOnAwakeIfRequired");
            privacyPolicyUrlProp = obj.FindProperty("privacyPolicyUrl");
            debugGeographyProp = obj.FindProperty("debugGeography");
            OnCompletedProp = obj.FindProperty("OnCompleted");
            OnFailedProp = obj.FindProperty("OnFailed");
        }

        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            obj.UpdateIfRequiredOrScript();
            showOnAwakeIfRequiredProp.boolValue = EditorGUILayout.ToggleLeft(
                "Show On Awake If Required",
                showOnAwakeIfRequiredProp.boolValue
            );
            EditorGUI.indentLevel++;
            if (showOnAwakeIfRequiredProp.boolValue)
                EditorGUILayout.HelpBox("The CAS Consent flow will be displayed automatically, if it is required, when the component is awake", MessageType.None);
            else
                EditorGUILayout.HelpBox("Call `ShowIfRequired()` or `Show()` method to display consent flow", MessageType.None);
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(debugGeographyProp);
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("Note that debug settings only work with Test Ad Mode enabled or for Test devices.", MessageType.None);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(privacyPolicyUrlProp);

            EditorGUILayout.PropertyField(OnCompletedProp);
            EditorGUILayout.PropertyField(OnFailedProp);

            obj.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(BannerAdObject))]
    internal class BannerAdObjectInspector : BaseAdObjectInspector
    {
        private SerializedProperty adPositionProp;
        private SerializedProperty adSizeProp;
        private SerializedProperty adOffsetProp;
        private SerializedProperty onAdHiddenProp;
        private BannerAdObject adView;
        private readonly string[] allowedPositions = new string[]{
            "Top Center",
            "Top Left",
            "Top Right",
            "Bottom Center",
            "Bottom Left",
            "Bottom Right"
        };

        protected override void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            adPositionProp = obj.FindProperty("adPosition");
            adOffsetProp = obj.FindProperty("adOffset");
            adSizeProp = obj.FindProperty("adSize");

            onAdHiddenProp = obj.FindProperty("OnAdHidden");
            adView = target as BannerAdObject;
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            var isPlaying = Application.isPlaying;
            EditorGUI.BeginChangeCheck();
            adPositionProp.intValue = EditorGUILayout.Popup("Ad Position", adPositionProp.intValue, allowedPositions);
            if (EditorGUI.EndChangeCheck())
            {
                adOffsetProp.vector2IntValue = Vector2Int.zero;
                if (isPlaying)
                    adView.SetAdPositionEnumIndex(adPositionProp.intValue);
            }

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(adPositionProp.intValue != (int)AdPosition.TopLeft);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(adOffsetProp);
            GUILayout.Label("DP", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck() && isPlaying)
            {
                var newPos = adOffsetProp.vector2IntValue;
                adView.SetAdPosition(newPos.x, newPos.y);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField("Screen positioning coordinates are only available for the TopLeft position.", EditorStyles.wordWrappedMiniLabel);
            // Calling the calculation in the Editor results in incorrect data
            // because getting the screen size returns the size of the inspector.
            //if (isPlaying)
            //{
            //    EditorGUI.BeginDisabledGroup( true );
            //    EditorGUILayout.RectField( "Rect in pixels", adView.rectInPixels );
            //    EditorGUI.EndDisabledGroup();
            //}
            EditorGUI.indentLevel--;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(adSizeProp);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
                adView.SetAdSizeEnumIndex(adSizeProp.intValue);
        }

        protected override void OnCallbacksGUI()
        {
            base.OnCallbacksGUI();
            EditorGUILayout.PropertyField(onAdHiddenProp);
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField("Use `gameObject.SetActive(visible)` method to show/hide banner ad.",
                EditorStyles.wordWrappedMiniLabel);
        }
    }

    [CustomEditor(typeof(InterstitialAdObject))]
    [CanEditMultipleObjects]
    internal class InterstitialAdObjectInspector : BaseAdObjectInspector
    {
        private SerializedProperty onAdFailedToShowProp;
        private SerializedProperty onAdClosedProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            onAdFailedToShowProp = obj.FindProperty("OnAdFailedToShow");
            onAdClosedProp = obj.FindProperty("OnAdClosed");
        }

        protected override void OnCallbacksGUI()
        {
            EditorGUILayout.PropertyField(onAdFailedToShowProp);
            base.OnCallbacksGUI();
            EditorGUILayout.PropertyField(onAdClosedProp);
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField("Call `Present()` method to show Interstitial Ad.",
                EditorStyles.wordWrappedMiniLabel);
        }
    }


    [CustomEditor(typeof(AppOpenAdObject))]
    [CanEditMultipleObjects]
    internal class AppOpenAdObjectInspector : InterstitialAdObjectInspector
    {
        private SerializedProperty autoshowProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            autoshowProp = obj.FindProperty("autoshow");
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            base.OnAdditionalPropertiesGUI();
            autoshowProp.boolValue = EditorGUILayout.ToggleLeft(
                "Autoshow loaded Ad when user open the app",
                autoshowProp.boolValue
            );
            if (autoshowProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("The ad will be shown when the user opens the app. Additionally, the ad must be loaded, and this component must not be destroyed.", MessageType.None);
                EditorGUI.indentLevel--;
            }
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField("Autoload not supported. Call `Load()` and `Show()` methods to show AppOpen Ad.",
                EditorStyles.wordWrappedMiniLabel);
        }
    }

    [CustomEditor(typeof(RewardedAdObject))]
    [CanEditMultipleObjects]
    internal class RewardedAdObjectInspector : InterstitialAdObjectInspector
    {
        private SerializedProperty restartInterstitialIntervalProp;
        private SerializedProperty onRewardProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            restartInterstitialIntervalProp = obj.FindProperty("restartInterstitialInterval");
            onRewardProp = obj.FindProperty("OnReward");
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            base.OnAdditionalPropertiesGUI();
            restartInterstitialIntervalProp.boolValue = EditorGUILayout.ToggleLeft(
                "Restart Interstitial Ad interval on rewarded ad closed",
                restartInterstitialIntervalProp.boolValue
            );
            EditorGUILayout.PropertyField(onRewardProp);
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField("Call `Present()` method to show Rewarded Ad.",
                EditorStyles.wordWrappedMiniLabel);
        }
    }

    [CustomEditor(typeof(ReturnToPlayAdObject))]
    internal class ReturnToPlayAdObjectInspector : InterstitialAdObjectInspector
    {
        private SerializedProperty allowAdProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            allowAdProp = obj.FindProperty("_allowReturnToPlayAd");
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            base.OnAdditionalPropertiesGUI();
            EditorGUI.BeginChangeCheck();
            allowAdProp.boolValue = EditorGUILayout.ToggleLeft(
                "Allow ads to show on return to game",
                allowAdProp.boolValue
            );
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                ((ReturnToPlayAdObject)target).allowReturnToPlayAd = allowAdProp.boolValue;
            }
        }
    }
}