//  Copyright © 2026 CAS.AI. All rights reserved.

using System;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// Font Types for native templates.
    /// </summary>
    [Serializable]
    public enum NativeTemplateFontStyle
    {
        Normal = 0,
        Bold = 1,
        Italic = 2,
        Monospace = 3
    }

    /// <summary>
    /// Text style options for native templates.
    /// </summary>
    [Serializable]
    public sealed class NativeTemplateTextStyle
    {
        /// <summary>
        /// Size of the Text to be displayed.
        /// </summary>
        public int fontSize = -1;
        /// /// <summary>
        /// Color of the Text to be rendered.
        /// </summary>
        public Color fontColor = Color.black;
        /// <summary>
        /// FontStyle for the text.
        /// </summary>
        public NativeTemplateFontStyle fontStyle = NativeTemplateFontStyle.Normal;
        /// <summary>
        /// The background color.
        /// </summary>
        public Color backgroundColor = Color.clear;

        public NativeTemplateTextStyle()
        {
        }

        public bool IsModified()
        {
            return fontSize != -1;
        }
    }

    /// <summary>
    /// Style options for native templates.
    /// </summary>
    [Serializable]
    public sealed class NativeTemplateStyle
    {
        /// <summary>
        /// The main background color.
        /// </summary>
        public Color mainBackgroundColor;
        /// <summary>
        /// The NativeTemplateTextStyle for the headline text.
        /// </summary>
        public NativeTemplateTextStyle headline;
        /// <summary>
        /// The NativeTemplateTextStyle for the body text.
        /// </summary>
        public NativeTemplateTextStyle body;
        /// <summary>
        /// The NativeTemplateTextStyle for the call to action text in button.
        /// </summary>
        public NativeTemplateTextStyle callToAction;
        /// <summary>
        /// The NativeTemplateTextStyle for the other text.
        /// </summary>
        public NativeTemplateTextStyle other;

        public NativeTemplateStyle()
        {
            mainBackgroundColor = Color.white;
        }
    }
}