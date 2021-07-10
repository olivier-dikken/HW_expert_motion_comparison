using System.Collections.Generic;
using System.Windows.Input;
using HandwritingFeedback.Config;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.RealtimeFeedback.InputSources.Tablet;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.RealtimeFeedback
{
    /// <summary>
    /// All real-time feedback types extend from this class.
    /// </summary>
    public abstract class RealtimeFeedback
    {
        /// <summary>
        /// An InputSource is a class which represents a device (e.g. Tablet, Camera, ...). <br />
        /// Calling CollectFeedback will iterate over all Sources and trigger each Source <br />
        /// to collect feedback from its Components.
        /// </summary>
        public IList<InputSource> InputSources { get; set; }
            = new List<InputSource> { new TabletSource() };

        /// <summary>
        /// Generates a ratio to be used for accuracy feedback based on the given expert trace <br />
        /// and student point, using the configured thresholds for accuracy.
        /// </summary>
        /// <param name="expertTraceUtils">The TraceUtils that will be used for computation</param>
        /// <param name="stylusPoint">The student point</param>
        /// <returns>The calculated ratio (unbounded, as described in <see cref="Generate" />)</returns>
        public static double AccuracyRatio(TraceUtils expertTraceUtils, StylusPoint stylusPoint)
        {
            // Obtain the distance to the closest expert point (this will be a cache hit,
            // because VisualFeedback has already calculated this point)
            double distance = expertTraceUtils.GetClosestPoint(stylusPoint.ToPoint()).distance;
            
            // Generate a ratio using the appropriate parameters.
            // expertValue=0, because expert is at distance 0 from themselves.
            // lowStart=0 and lowCutoff=0, because a student can never be too close to the expert.
            return Generate(distance, 0, 0, 0,
                ApplicationConfig.Instance.MaxDeviationRadius,
                ApplicationConfig.Instance.MaxDeviationRadius);
        }

        /// <summary>
        /// Generates a ratio to be used for pressure feedback based on the given expert trace <br />
        /// and student point, using the configured thresholds for pressure.
        /// </summary>
        /// <param name="expertTraceUtils">The TraceUtils that will be used for computation</param>
        /// <param name="stylusPoint">The student point</param>
        /// <returns>The calculated ratio (unbounded, as described in <see cref="Generate" />)</returns>
        public static double PressureRatio(TraceUtils expertTraceUtils, StylusPoint stylusPoint)
        {
            // Obtain the pressure value of the closest expert point (this will be a cache hit,
            // because VisualFeedback has already calculated this point)
            float expertPressure = expertTraceUtils.GetClosestPoint(stylusPoint.ToPoint()).stylusPoint.PressureFactor;

            float studentPressure = stylusPoint.PressureFactor;
            
            // Generate a ratio using the appropriate parameters
            return Generate(studentPressure, expertPressure,
                ApplicationConfig.Instance.LowPressureStart, ApplicationConfig.Instance.LowPressureCutoff,
                ApplicationConfig.Instance.HighPressureStart, ApplicationConfig.Instance.HighPressureCutoff);
        }

        /// <summary>
        /// Generates a ratio to be used for tilt feedback based on the given expert trace <br />
        /// and student point, using the configured thresholds for tilt.
        /// </summary>
        /// <param name="expertTraceUtils">The TraceUtils that will be used for computation</param>
        /// <param name="stylusPoint">The student point</param>
        /// <returns>The calculated ratio (unbounded, as described in <see cref="Generate" />)</returns>
        public static double TiltRatio(TraceUtils expertTraceUtils, StylusPoint stylusPoint)
        {
            // We get the closest point to the student's stylus point
            // This will be used to compare the angle
            var expertPoint = expertTraceUtils.GetClosestPoint(stylusPoint.ToPoint()).stylusPoint;

            // We only give feedback if both the expert point and the student point
            // have the needed property
            if (!expertPoint.HasProperty(StylusPointProperties.XTiltOrientation) ||
                !stylusPoint.HasProperty(StylusPointProperties.XTiltOrientation)) return 0;
            
            // The stylus point's angle is a value between 0 and 9000, measured from the surface normal.
            // We convert this to get the angle from the surface in degrees
            var studentAngle = (9000 - stylusPoint.GetPropertyValue(StylusPointProperties.XTiltOrientation)) / 100;
            var expertAngle = (9000 - expertPoint.GetPropertyValue(StylusPointProperties.XTiltOrientation)) / 100;
            
            return Generate(studentAngle, expertAngle,
                ApplicationConfig.Instance.AngleDeviation, ApplicationConfig.Instance.AngleDeviation,
                ApplicationConfig.Instance.AngleDeviation, ApplicationConfig.Instance.AngleDeviation);
        }

        /// <summary>
        /// Generates a ratio based on the provided input data.  <br />
        /// This ratio can be used for any feedback type, such as <br />
        /// determining a color gradient for visual feedback based on distance to <br />
        /// the expert, or calculating a pitch gradient for auditory feedback based on pressure.
        /// </summary>
        /// <param name="studentValue">The value of the student</param>
        /// <param name="expertValue">The value of the expert</param>
        /// <param name="lowStart">Between 0 and 1. Where the ratio for being below the expert should start decreasing. Relative to <paramref name="expertValue"/></param>
        /// <param name="lowCutoff">Between 0 and 1. Where the ratio for being below the expert should reach -1. Relative to <paramref name="expertValue"/> </param>
        /// <param name="highStart">Between 0 and 1. Where the ratio for being above the expert should start increasing. Relative to <paramref name="expertValue"/></param>
        /// <param name="highCutoff">Between 0 and 1. Where the ratio for being above the expert should reach 1. Relative to <paramref name="expertValue"/> </param>
        /// <returns>The ratio, which indicates how far <paramref name="studentValue"/> deviated from <paramref name="expertValue"/>, w.r.t. the low and high start
        /// and cutoff values. The ratio is not bounded, and can therefore fall below -1 or rise above 1.
        /// When the ratio is negative, <paramref name="studentValue"/> was below <paramref name="expertValue"/> - <paramref name="lowStart"/>.
        /// If the ratio is positive, <paramref name="studentValue"/> was above <paramref name="expertValue"/> + <paramref name="highStart"/>. A ratio of 0
        /// indicates that <paramref name="studentValue"/> was within the acceptable range around <paramref name="expertValue"/>. A ratio smaller than -1 indicates
        /// that <paramref name="studentValue"/> was below <paramref name="expertValue"/> - <paramref name="lowCutoff"/>. A ratio larger than 1 indicates that
        /// <paramref name="studentValue"/> was above <paramref name="expertValue"/> + <paramref name="highCutoff"/>.</returns>
        public static double Generate(
            double studentValue, double expertValue,
            double lowStart, double lowCutoff,
            double highStart, double highCutoff)
        {
            // The ratio determines how far we advance the gradient
            // It represents how close studentValue is to the cutoff,
            // relative to the start value.
            double ratio = 0d;

            // The lower start is offset from expertValue
            double relativeLowStart = expertValue - lowStart;
            
            // The upper start is offset from expertValue
            double relativeHighStart = expertValue + highStart;
            
            // studentValue is below the threshold, relative to expertValue
            if (studentValue < relativeLowStart)
            {
                // First calculate the range we will use for the gradient.
                // This range can be reduced by adjusting lowCutoff.
                double rangeLength = lowCutoff - lowStart;

                // Determine how far to advance the gradient
                // If the rangeLength is 0 (i.e. cutoff equals start value), we set the ratio to 1
                // in order to avoid division by 0.
                ratio = rangeLength == 0d ? 1d : (relativeLowStart - studentValue) / rangeLength;
                
                // Make the ratio negative to indicate to calling classes that the < case was evaluated
                ratio = -ratio;
            }

            // studentValue is above the threshold, relative to expertValue
            else if (studentValue > relativeHighStart)
            {
                // Calculate rangeLength, similar to the low value case
                double rangeLength = highCutoff - highStart;

                // The ratio behaves similar as for the low value case,
                // but we swap studentValue and start value, since studentValue is
                // greater than expertValue + highStart
                ratio = rangeLength == 0d ? 1d : (studentValue - relativeHighStart) / rangeLength;
            }
            
            return ratio;
        }
    }
}
