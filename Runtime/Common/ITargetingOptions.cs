namespace CAS
{
    public enum Gender
    {
        Unknown, Male, Female
    }

    /// <summary>
    /// Wiki page: https://github.com/cleveradssolutions/CAS-Unity/wiki/Targeting-Options
    /// </summary>
    public interface ITargetingOptions
    {
        /// <summary>
        /// The user’s gender
        /// </summary>
        Gender gender { get; set; }
        /// <summary>
        /// The user’s age
        /// Limitation: 1-99 and 0 is 'unknown'
        /// </summary>
        int age { get; set; }
    }
}
