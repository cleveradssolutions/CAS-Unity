//  Copyright © 2025 CAS.AI. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CAS.AdObject
{
    /// <summary>
    /// A component that shows the user consent form without code,
    /// using <see cref="UnityEvent"/> callbacks configured in the Inspector.
    /// <para>Link it to <see cref="ManagerAdObject.consentFlow"/> to show the form during manager initialization,
    /// or call <see cref="Show"/> from a privacy options button.</para>
    /// </summary>
    [AddComponentMenu("CleverAdsSolutions/Consent Flow Ad Object")]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.page/cleveradssolutions/docs/Unity/User-Consent-Flow#inspector-implementation")]
    public class ConsentFlowAdObject : MonoBehaviour
    {
        [Tooltip("Call ShowIfRequired on Start. Ignored when this component is linked to a ManagerAdObject.")]
        [SerializeField]
        internal bool showOnAwakeIfRequired = false;
        /// <summary>
        /// The user location to emulate for testing the consent form on test devices.
        /// </summary>
        public ConsentFlow.DebugGeography debugGeography = ConsentFlow.DebugGeography.EEA;
        /// <summary>
        /// The URL of the app privacy policy shown in the consent form.
        /// </summary>
        public string privacyPolicyUrl = "";

        /// <summary>
        /// Invoked when the consent flow is completed:
        /// the consent is obtained, not required, or the form is unavailable.
        /// </summary>
        public UnityEvent OnCompleted;
        /// <summary>
        /// Invoked with the <see cref="ConsentFlow.Status"/> name when the consent flow fails.
        /// </summary>
        public CASUEventWithError OnFailed;
        /// <summary>
        /// Invoked with <c>true</c> when the consent is obtained and the user can change it later,
        /// so a privacy options button that calls <see cref="Show"/> should be visible.
        /// Invoked with <c>false</c> otherwise.
        /// </summary>
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
        /// Shows the consent form even if the user has already responded.
        /// <para>Some regulations require the user to be able to change their consent at any time.
        /// Call this from a privacy options button so the user can update their choice.</para>
        /// </summary>
        public void Show()
        {
            CreateFlow().Show();
        }

        /// <summary>
        /// Creates a <see cref="ConsentFlow"/> with the settings and callbacks of this component.
        /// </summary>
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
