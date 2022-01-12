namespace CAS
{
    public enum LoadingManagerMode
    {
        /// <summary>
        /// Automatic control loading mediation ads.
        /// <para>Provides frequent polling of mediation networks for advertising content.</para>
        /// <para>May increase coverage with more expensive ads.</para>
        /// <para>But this will run more background processes that slow down the application.</para>
        /// </summary>
        FastestRequests = 0,

        /// <summary>
        /// Automatic control loading mediation ads.
        /// <para>Provides frequent polling of mediation networks for advertising content.</para>
        /// <para>May increase coverage with more expensive ads.</para>
        /// <para>But this will run more background processes that slow down the application.</para>
        /// </summary>
        FastRequests = 1,

        /// <summary>
        /// Automatic control loading mediation ads.
        /// <para>Provides balanced polling rate of mediation networks for advertising content.</para>
        /// <para>Doesn't significantly affect application performance.</para>
        /// </summary>
        Optimal = 2,

        /// <summary>
        /// Automatic control loading mediation ads.
        /// <para>Provides slow polling of mediation networks for advertising content.</para>
        /// <para>This helps to reduce the impact of background processes on the application.</para>
        /// <para>At the same time, do not lose much of the relevance of the high cost of advertising content.</para>
        /// <para>Reduces memory reservations for advertising content.</para>
        /// </summary>
        HighePerformance = 3,

        /// <summary>
        /// Automatic control loading mediation ads.
        /// <para>Provides slow polling of mediation networks for advertising content.</para>
        /// <para>This helps to reduce the impact of background processes on the application.</para>
        /// <para>At the same time, do not lose much of the relevance of the high cost of advertising content.</para>
        /// <para>Reduces memory reservations for advertising content.</para>
        HighestPerformance = 4,

        /// <summary>
        /// Manual control loading mediation ads.
        /// <para>Provides minimal impact on application performance.</para>
        /// <para>But it requires manual preparation of advertising content for display.</para>
        /// <para>Use ad loading methods before trying to show: <see cref="IMediationManager.LoadAd(AdType)"/></para>
        /// <para>Reduces memory reservations for advertising content.</para>
        /// </summary>
        Manual = 5
    }
}
