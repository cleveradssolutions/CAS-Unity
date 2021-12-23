using System;
using UnityEngine;

namespace CAS
{
    /// <summary>
    /// Wiki page: https://github.com/cleveradssolutions/CAS-Unity/wiki/Configuring-Last-Page-Ad
    /// </summary>
    [Serializable]
    public class LastPageAdContent
    {
        [SerializeField]
        private string headline = string.Empty;
        [SerializeField]
        private string adText = string.Empty;
        [SerializeField]
        private string destinationURL = string.Empty;
        [SerializeField]
        private string imageURL = string.Empty;
        [SerializeField]
        private string iconURL = string.Empty;

        /// <summary>
        /// The latest free ad page for your own promotion.
        /// This ad page will be displayed when there is no paid ad to show or internet availability.
        ///
        /// Apply this content to  <see cref="IMediationManager.lastPageAdContent"/>
        /// </summary>
        /// <param name="headline">Enter the message that you want users to see.</param>
        /// <param name="adText">Enter a description for the app being promoted. Optional property.</param>
        /// <param name="destinationURL">Enter the URL that CAS will direct users to when they click the ad.
        /// This URL is not visible in the ad.</param>
        /// <param name="imageURL">Enter the direct URL of the image to be used as the ad file. Optional property.</param>
        /// <param name="iconURL">Enter the direct URL of the icon or logo (Small square picture). Optional property.</param>
        public LastPageAdContent( string headline, string adText, string destinationURL, string imageURL, string iconURL )
        {
            this.headline = headline;
            this.adText = adText;
            this.destinationURL = destinationURL;
            this.imageURL = imageURL;
            this.iconURL = iconURL;
        }

        public string Headline { get { return headline; } }
        public string AdText { get { return adText; } }
        public string DestinationURL { get { return destinationURL; } }
        public string ImageURL { get { return imageURL; } }
        public string IconURL { get { return iconURL; } }
    }
}