using System.Windows.Input;
using HandwritingFeedback.Config.Visual;
using HandwritingFeedback.Config.Auditory;

namespace HandwritingFeedback.Config
{
    /// <summary>
    /// Class that contains all configurable values that are used for batched and real-time feedback.
    /// </summary>
    public class ApplicationConfig
    {
        /// <summary>
        /// The instance that is used by the application. <br />
        /// The ApplicationConfig can be modified and fully replaced at runtime. <br />
        /// This allows the User to enable/disable feedback components through the UI, and it makes <br />
        /// testing much easier.
        /// </summary>
        public static ApplicationConfig Instance { get; set; } = new ApplicationConfig();
        
        public VisualConfig VisualConfig = new VisualConfig();
        public AuditoryConfig AuditoryConfig = new AuditoryConfig();

        /// <summary>
        /// Description for Stylus Points. This is used to add additional data to the <br />
        /// stylus points used throughout the application.
        /// </summary>
        public StylusPointDescription StylusPointDescription = new StylusPointDescription(new[]
        {
            new StylusPointPropertyInfo(StylusPointProperties.X),
            new StylusPointPropertyInfo(StylusPointProperties.Y),
            new StylusPointPropertyInfo(StylusPointProperties.NormalPressure),
            new StylusPointPropertyInfo(StylusPointProperties.XTiltOrientation)
        });

        /// <summary>
        /// The angle deviation threshold compared to the expert's stylus tilt
        /// after which feedback is provided on tilt.
        /// </summary>
        public int AngleDeviation = 15;
        
        /// <summary>
        /// The pressure above which feedback starts being provided.
        /// </summary>
        public float HighPressureStart = 0.2f;
        
        /// <summary>
        /// The pressure above which feedback is provided at maximum intensity.
        /// </summary>
        public float HighPressureCutoff = 0.4f;

        /// <summary>
        /// The pressure below which feedback starts being provided.
        /// </summary>
        public float LowPressureStart = 0.2f;
        
        /// <summary>
        /// The pressure below which feedback is provided at maximum intensity.
        /// </summary>
        public float LowPressureCutoff = 0.4f;

        /// <summary>
        /// The speed at which feedback on high writing speed is provided.
        /// </summary>
        public float HighSpeedStart = 2000f;

        /// <summary>
        /// The student's current point needs to be within <see cref="MaxDeviationRadius"/>
        /// from the closest expert point to be considered "on the trace". <br />
        /// The student can be at most four times <see cref="MaxDeviationRadius"/>
        /// (i.e. 2 diameters) away from the closest expert point, before a dashed <br />
        /// line is drawn (student outside of search range).
        /// Cannot be 0!
        /// </summary>
        public double MaxDeviationRadius = 2d;
        
        /// <summary>
        /// Limits the area in which to search for a closest expert point. <br />
        /// An ellipse with width and height set to ClosestPointScanDiameter <br />
        /// will be placed over the point to search from, in order to <br />
        /// then perform a hit test using the ellipse. If 4 * <see cref="MaxDeviationRadius"/>
        /// is greater than this field's value, then 4 * <see cref="MaxDeviationRadius"/> will
        /// be used as <see cref="ClosestPointScanDiameter"/> instead.
        /// </summary>
        public double ClosestPointScanDiameter = 100d;

        /// <summary>
        /// Determines the leniency of the error highlighting for batched feedback graphs. <br />
        /// The minimum fraction of the total x-axis length that an error should span before <br />
        /// it is counted and highlighted. This is used to account for tiny 'errors'. <br />
        /// An example would be lifting the pen, which might cause pressure feedback on a <br />
        /// very small section at the end of the trace.
        /// </summary>
        public double MinErrorHighlightingFraction = 0.025d;

        /// <summary>
        /// Defines the number of neighbors to consider when averaging data points in batched <br />
        /// feedback, such that the series is smoother and minimizes erratic fluctuations.
        /// Default value was chosen through functional testing.
        /// </summary>
        public int DataPointsAveragingNeighbors = 32;
    }
}