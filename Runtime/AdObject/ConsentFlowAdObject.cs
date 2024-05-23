//  Copyright © 2024 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    [AddComponentMenu("CleverAdsSolutions/Consent Flow Ad Object")]
    [DisallowMultipleComponent]
    public class ConsentFlowAdObject : MonoBehaviour
    {
        [SerializeField]
        internal bool showOnAwakeIfRequired = true;
        public ConsentFlow.DebugGeography debugGeography = ConsentFlow.DebugGeography.EEA;
        public string privacyPolicyUrl = "";

        public UnityEvent OnCompleted;
        public CASUEventWithError OnFailed;
        public CASUToggleEvent EnableOptionsButton;

        /// <summary>
        /// Shows the consent form only if it is required and the user has not responded previously.
        /// If the consent status is required, the SDK loads a form and immediately presents it.
        /// </summary>
        public void ShowIfRequired()
        {
            CreateFlow().ShowIfRequired();

        }

        /// <summary>
        /// Force Shows the consent form.
        /// Some consent forms require the user to modify their consent at any time. 
        /// When a user interacts with your UI element, call function to show the form 
        /// so the user can update their privacy options at any time. 
        /// </summary>
        public void Show()
        {
            CreateFlow().Show();
        }

        public ConsentFlow CreateFlow()
        {
            return new ConsentFlow()
                .WithPrivacyPolicy(privacyPolicyUrl)
                .WithCompletionListener(HandleFlowResult)
                .WithDebugGeography(debugGeography);
        }

        private void Start()
        {
            EnableOptionsButton.Invoke(CASFactory.consentFlowStatus == ConsentFlow.Status.Obtained);
            CASFactory.OnManagerStateChanged += OnManagerStateChanged;

            if (showOnAwakeIfRequired)
                ShowIfRequired();
        }

        private void OnDestroy()
        {
            CASFactory.OnManagerStateChanged -= OnManagerStateChanged;
        }

        private void OnManagerStateChanged(int index, CASManagerBase manager)
        {
            if (!this) return;
            var config = manager.initialConfig;
            if (config == null) return;
            EnableOptionsButton.Invoke(CASFactory.consentFlowStatus == ConsentFlow.Status.Obtained);
        }

        private void HandleFlowResult(ConsentFlow.Status status)
        {
            EnableOptionsButton.Invoke(status == ConsentFlow.Status.Obtained);

            if (status == ConsentFlow.Status.Obtained
                || status == ConsentFlow.Status.NotRequired
                || status == ConsentFlow.Status.Unavailable)
                OnCompleted.Invoke();
            else
                OnFailed.Invoke(status.ToString());
        }
    }
}
