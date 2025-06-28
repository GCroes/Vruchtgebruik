using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Settings
{
    public class EenLevenSettings
    {
        public string ActiveVersion { get; set; } = String.Empty;
        public Dictionary<string, List<EenLevenFactorRow>> Versions { get; set; } = null!;
    }
}
