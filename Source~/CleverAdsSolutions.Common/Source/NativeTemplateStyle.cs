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
        BoldItalic = 3
    }

    /// <summary>
    /// Text style options for native templates.
    /// </summary>
    [Serializable]
    public sealed class NativeTemplateTextStyle
    {
        /// <summary>
        /// The background color.
        /// </summary>
        public Color BackgroundColor = Color.clear;
        /// /// <summary>
        /// Color of the Text to be rendered.
        /// </summary>
        public Color FontColor = Color.black;
        /// <summary>
        /// Size of the Text to be displayed.
        /// </summary>
        public float FontSize = 14f;
        /// <summary>
        /// FontStyle for the text.
        /// </summary>
        public NativeTemplateFontStyle Style = NativeTemplateFontStyle.Normal;

        public NativeTemplateTextStyle() { }
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
        public Color MainBackgroundColor;
        /// <summary>
        /// The NativeTemplateTextStyle for the headline text.
        /// </summary>
        public NativeTemplateTextStyle Headline;
        /// <summary>
        /// The NativeTemplateTextStyle for the body text.
        /// </summary>
        public NativeTemplateTextStyle Body;
        /// <summary>
        /// The NativeTemplateTextStyle for the call to action text in button.
        /// </summary>
        public NativeTemplateTextStyle CallToAction;
        /// <summary>
        /// The NativeTemplateTextStyle for the other text.
        /// </summary>
        public NativeTemplateTextStyle Other;

        public NativeTemplateStyle()
        {
            MainBackgroundColor = Color.white;
        }
    }
}