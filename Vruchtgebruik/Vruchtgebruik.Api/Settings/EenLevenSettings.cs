using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Settings
{
    /// <summary>
    /// Represents configuration settings for the "ÉénLeven" factor calculation method,
    /// including the active version and all available factor tables by version.
    /// </summary>
    public class EenLevenSettings
    {
        /// <summary>
        /// Gets or sets the version key of the currently active factor table.
        /// This determines which set of factors will be used for calculations.
        /// </summary>
        public string ActiveVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets all available factor tables for "EenLeven", 
        /// organized by version key (e.g., "2024").
        /// Each version contains a list of <see cref="EenLevenFactorRow"/>.
        /// </summary>
        public Dictionary<string, List<EenLevenFactorRow>> Versions { get; set; } = null!;
    }

}
