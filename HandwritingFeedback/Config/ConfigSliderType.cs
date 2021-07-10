using System.ComponentModel;

namespace HandwritingFeedback.Config
{
    /// <summary>
    /// Identifies all config values that should be adjustable by UI sliders.
    /// </summary>
    public enum ConfigSliderType
    {
        // This field does more than just change the outline thickness,
        // as it also affects the search range for the closest point
        // and the accuracy feedback. But for the teacher, the change in
        // thickness is the only apparent change.
        [Description("Helper Trace Thickness")]
        MaxDeviationRadius,
        
        [Description("Pressure: High Pressure Start")]
        HighPressureStart,
        
        [Description("Pressure: High Pressure Cutoff")]
        HighPressureCutoff,
        
        [Description("Pressure: Low Pressure Start")]
        LowPressureStart,
        
        [Description("Pressure: Low Pressure Cutoff")]
        LowPressureCutoff,
        
        [Description("Speed: High Speed Start")]
        HighSpeedStart,
        
        [Description("Tilt: Maximum Angle Deviation")]
        MaxAngleDeviation
    }
}