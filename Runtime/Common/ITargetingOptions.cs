namespace CAS
{
    public enum Gender
    {
        Unknown, Male, Female
    }

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
