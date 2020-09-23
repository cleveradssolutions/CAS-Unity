namespace CAS
{
    public enum LoadingManagerMode
    {
        /// Automatic control loading mediation ads.
        /// Provides frequent polling of mediation networks for advertising content.
        /// May increase coverage with more expensive ads.
        /// But this will run more background processes that slow down the application.
        FastestRequests = 0,

        /// Automatic control loading mediation ads.
        /// Provides frequent polling of mediation networks for advertising content.
        /// May increase coverage with more expensive ads.
        /// But this will run more background processes that slow down the application.
        FastRequests = 1,

        /// Automatic control loading mediation ads.
        /// Provides balanced polling rate of mediation networks for advertising content.
        /// Doesn't significantly affect application performance.
        Optimal = 2,

        /// Automatic control loading mediation ads.
        /// Provides slow polling of mediation networks for advertising content.
        /// This helps to reduce the impact of background processes on the application.
        /// At the same time, do not lose much of the relevance of the high cost of advertising content.
        /// Reduces memory reservations for advertising content.
        HighePerformance = 3,

        /// Automatic control loading mediation ads.
        /// Provides slow polling of mediation networks for advertising content.
        /// This helps to reduce the impact of background processes on the application.
        /// At the same time, do not lose much of the relevance of the high cost of advertising content.
        /// Reduces memory reservations for advertising content.
        HighestPerformance = 4,

        /// Manual control loading mediation ads.
        /// Provides minimal impact on application performance.
        /// But it requires manual preparation of advertising content for display.
        ///
        /// Use ad loading methods before trying to show: <see cref="IMediationManager.LoadAd(AdType)"/>
        ///
        /// Reduces memory reservations for advertising content.
        Manual = 5
    }
}
