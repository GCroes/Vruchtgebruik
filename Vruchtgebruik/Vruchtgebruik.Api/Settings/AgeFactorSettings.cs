namespace Vruchtgebruik.Api.Settings
{
    public class AgeFactorSettings
    {
        public int FemaleAdjustment { get; set; } = 5; // Default is 5, as per your business rule
        public int MaleAdjustment { get; set; } = 0;   // Usually 0, but could be changed
    }
}
