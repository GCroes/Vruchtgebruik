namespace Vruchtgebruik.Api.Models
{
    /// <summary>
    /// Represents a single age threshold and associated factor for the "EenLeven" calculation method.
    /// Used to determine the correct factor based on the user's adjusted age.
    /// </summary>
    public class EenLevenFactorRow
    {
        /// <summary>
        /// The minimum age (inclusive) for which this factor applies.
        /// </summary>
        public int MinAge { get; set; }

        /// <summary>
        /// The maximum age (inclusive) for which this factor applies.
        /// </summary>
        public int MaxAge { get; set; }

        /// <summary>
        /// The factor to be used in the calculation for users whose adjusted age falls within this range.
        /// </summary>
        public decimal Factor { get; set; }
    }

}
