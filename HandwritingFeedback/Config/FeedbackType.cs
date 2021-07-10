using System;
using System.ComponentModel;

namespace HandwritingFeedback.Config
{
    /// <summary>
    /// Identifies all real-time feedback types so that the user may customize feedback options.
    /// </summary>
    [Flags]
    public enum FeedbackType
    {
        // Extend real-time feedback options by adding additional categories below with adjusted bit operator
        [Description("Accuracy Color")]
        AccuracyColor = 1 << 0,
        [Description("Pressure Color")]
        PressureColor = 1 << 1,
        [Description("Pen Tilt Color")]
        PenTiltColor = 1 << 2,
        [Description("Pressure Audio")]
        PressureAudio = 1 << 3,
        [Description("Speed Audio")]
        SpeedAudio = 1 << 4
    }
}
