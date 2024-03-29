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
            if (showOnAwakeIfRequired)
                CreateFlow().ShowIfRequired();
        }

        private void HandleFlowResult(ConsentFlow.Status status)
        {
            if (status == ConsentFlow.Status.Obtained
                || status == ConsentFlow.Status.NotRequired
                || status == ConsentFlow.Status.Unavailable)
                OnCompleted.Invoke();
            else
                OnFailed.Invoke(status.ToString());
        }
    }
}
