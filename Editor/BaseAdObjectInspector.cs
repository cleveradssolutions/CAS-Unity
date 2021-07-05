using System.Collections;
using System.Collections.Generic;
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

        protected void OnEnable()
        {
            var obj = serializedObject;
            managerIdProp = obj.FindProperty( "managerId" );

            onAdLoadedProp = obj.FindProperty( "OnAdLoaded" );
            onAdFailedToLoadProp = obj.FindProperty( "OnAdFailedToLoad" );
            onAdShownProp = obj.FindProperty( "OnAdShown" );
            onAdClickedProp = obj.FindProperty( "OnAdClicked" );
        }

        public override void OnInspectorGUI()
        {
            var obj = serializedObject;
            obj.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField( managerIdProp );
            OnAdditionalPropertiesGUI();

            loadEventsFoldout = GUILayout.Toggle( loadEventsFoldout, "Load Ad callbacks", EditorStyles.foldout );
            if (loadEventsFoldout)
            {
                EditorGUILayout.PropertyField( onAdLoadedProp );
                EditorGUILayout.PropertyField( onAdFailedToLoadProp );
            }

            contentEventsFoldout = GUILayout.Toggle( contentEventsFoldout, "Content callbacks", EditorStyles.foldout );
            if (contentEventsFoldout)
                OnCallbacksGUI();

            OnFooterGUI();

            obj.ApplyModifiedProperties();
        }

        protected virtual void OnAdditionalPropertiesGUI() { }

        protected virtual void OnFooterGUI() { }

        protected virtual void OnCallbacksGUI()
        {
            EditorGUILayout.PropertyField( onAdShownProp );
            EditorGUILayout.PropertyField( onAdClickedProp );
        }
    }

    [CustomEditor( typeof( BannerAdObject ) )]
    internal class BannerAdObjectInspector : BaseAdObjectInspector
    {
        private SerializedProperty adPositionProp;
        private SerializedProperty adSizeProp;
        private SerializedProperty onAdHiddenProp;

        private new void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            adPositionProp = obj.FindProperty( "adPosition" );
            adSizeProp = obj.FindProperty( "adSize" );

            onAdHiddenProp = obj.FindProperty( "OnAdHidden" );
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            EditorGUILayout.PropertyField( adPositionProp );
            EditorGUILayout.PropertyField( adSizeProp );
        }

        protected override void OnCallbacksGUI()
        {
            base.OnCallbacksGUI();
            EditorGUILayout.PropertyField( onAdHiddenProp );
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField( "Use `gameObject.SetActive(visible)` method to show/hide banner ad.",
                EditorStyles.wordWrappedMiniLabel );
        }
    }

    [CustomEditor( typeof( InterstitialAdObject ) )]
    internal class InterstitialAdObjectInspector : BaseAdObjectInspector
    {
        private SerializedProperty onAdFailedToShowProp;
        private SerializedProperty onAdClosedProp;

        private new void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            onAdFailedToShowProp = obj.FindProperty( "OnAdFailedToShow" );
            onAdClosedProp = obj.FindProperty( "OnAdClosed" );
        }

        protected override void OnCallbacksGUI()
        {
            EditorGUILayout.PropertyField( onAdFailedToShowProp );
            base.OnCallbacksGUI();
            EditorGUILayout.PropertyField( onAdClosedProp );
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField( "Call `Present()` method to show Interstitial Ad.",
                EditorStyles.wordWrappedMiniLabel );
        }
    }

    [CustomEditor( typeof( RewardedAdObject ) )]
    internal class RewardedAdObjectInspector : BaseAdObjectInspector
    {
        private SerializedProperty onAdFailedToShowProp;
        private SerializedProperty onAdClosedProp;

        private SerializedProperty restartInterstitialIntervalProp;
        private SerializedProperty onRewardProp;

        private new void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            restartInterstitialIntervalProp = obj.FindProperty( "restartInterstitialInterval" );
            onAdFailedToShowProp = obj.FindProperty( "OnAdFailedToShow" );
            onAdClosedProp = obj.FindProperty( "OnAdClosed" );
            onRewardProp = obj.FindProperty( "OnReward" );
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            restartInterstitialIntervalProp.boolValue = EditorGUILayout.ToggleLeft(
                "Restart Interstitial Ad interval on rewarded ad closed",
                restartInterstitialIntervalProp.boolValue
            );
            EditorGUILayout.PropertyField( onRewardProp );
        }

        protected override void OnCallbacksGUI()
        {
            EditorGUILayout.PropertyField( onAdFailedToShowProp );
            base.OnCallbacksGUI();
            EditorGUILayout.PropertyField( onAdClosedProp );
        }

        protected override void OnFooterGUI()
        {
            EditorGUILayout.LabelField( "Call `Present()` method to show Rewarded Ad.",
                EditorStyles.wordWrappedMiniLabel );
        }
    }

    [CustomEditor( typeof( ReturnToPlayAdObject ) )]
    internal class ReturnToPlayAdObjectInspector : BaseAdObjectInspector
    {
        private SerializedProperty allowAdProp;
        private SerializedProperty onAdFailedToShowProp;
        private SerializedProperty onAdClosedProp;

        private new void OnEnable()
        {
            base.OnEnable();
            var obj = serializedObject;
            allowAdProp = obj.FindProperty( "_allowReturnToPlayAd" );
            onAdFailedToShowProp = obj.FindProperty( "OnAdFailedToShow" );
            onAdClosedProp = obj.FindProperty( "OnAdClosed" );
        }

        protected override void OnAdditionalPropertiesGUI()
        {
            allowAdProp.boolValue = EditorGUILayout.ToggleLeft(
                "Allow Allow Return to play ads",
                allowAdProp.boolValue
            );
        }

        protected override void OnCallbacksGUI()
        {
            EditorGUILayout.PropertyField( onAdFailedToShowProp );
            base.OnCallbacksGUI();
            EditorGUILayout.PropertyField( onAdClosedProp );
        }
    }
}