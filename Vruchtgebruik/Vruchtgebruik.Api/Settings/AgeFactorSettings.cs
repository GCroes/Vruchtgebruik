namespace Vruchtgebruik.Api.Settings
{
    /// <summary>
    /// Represents configuration settings for adjusting age based on sex
    /// when calculating Dutch tax factors.
    /// </summary>
    public class AgeFactorSettings
    {
        /// <summary>
        /// The number of years to subtract from the user's age if the sex is female.
        /// Default is 5 years, according to business rules.
        /// </summary>
        public int FemaleAdjustment { get; set; } = 5;

        /// <summary>
        /// The number of years to subtract from the user's age if the sex is male.
        /// Default is 0, but can be customized.
        /// </summary>
        public int MaleAdjustment { get; set; } = 0;
    }

}
