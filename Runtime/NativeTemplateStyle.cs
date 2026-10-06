using System;
using UnityEngine;

namespace CAS
{
    public enum NativeTemplateFontStyle
    {
        Normal = 0,
        Bold = 1,
        Italic = 2,
        BoldItalic = 3
    }

    [Serializable]
    public sealed class NativeTemplateTextStyle
    {
        public Color BackgroundColor = Color.clear;
        public Color FontColor = Color.black;

        public float FontSize = 14f;

        public NativeTemplateFontStyle Style =
            NativeTemplateFontStyle.Normal;
    }

    [Serializable]
    public sealed class NativeTemplateStyle
    {
        public Color MainBackgroundColor = Color.white;

        public NativeTemplateTextStyle Headline;
        public NativeTemplateTextStyle Body;
        public NativeTemplateTextStyle Advertiser;
        public NativeTemplateTextStyle CallToAction;

        public NativeTemplateTextStyle Store;
        public NativeTemplateTextStyle Price;
        public NativeTemplateTextStyle ReviewCount;
        public NativeTemplateTextStyle AdLabel;
    }
}