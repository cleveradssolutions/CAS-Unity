using System;
using UnityEngine;

namespace CAS
{
    [Serializable]
    public class LastPageAdContent
    {
        [SerializeField]
        private string _headline;
        [SerializeField]
        private string _adText;
        [SerializeField]
        private string _destinationURL;
        [SerializeField]
        private string _imageURL;
        [SerializeField]
        private string _iconURL;

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
            _headline = headline;
            _adText = adText;
            _destinationURL = destinationURL;
            _imageURL = imageURL;
            _iconURL = iconURL;
        }

        public string headline { get { return _headline; } }
        public string adText { get { return _adText; } }
        public string destinationURL { get { return _destinationURL; } }
        public string imageURL { get { return _imageURL; } }
        public string iconURL { get { return _iconURL; } }
    }
}