using System;
using System.Windows.Input;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.TiltComponents
{
    /// <summary>
    /// Computes the range of pen tilt angles during an exercise.
    /// </summary>
    public class TiltRange : BFCanvasComponent
    {
        private string _unit;
        private readonly string _title;

        /// <summary>
        /// Constructor to compute range of pen tilt after an exercise.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at a exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public TiltRange(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils) : base(studentTraceUtils,
            expertTraceUtils)
        {
            this._unit = "°";
            this._title = "Pen Tilt Range";
        }

        /// <summary>
        /// Calculates minimum and maximum pen tilt during exercise.
        /// </summary>
        /// <returns>Returns range of pen tilt during exercise represented as a UnitValue</returns>
        public override Synthesis Synthesize()
        {
            // Instantiate min and max values
            var min = Int32.MaxValue;
            var max = Int32.MinValue;

            // Check tilt for every stroke in student trace
            foreach (var stroke in StudentTraceUtils.Trace)
            {
                foreach (var stylusPoint in stroke.StylusPoints)
                {
                    // Check if current stylus point does not have tilt data, in-case the user switches input method mid-practice
                    if (!stylusPoint.HasProperty(StylusPointProperties.XTiltOrientation)) continue;
                    
                    // Extract tilt angle for current point
                    var curr = 9000 -
                               Math.Abs(stylusPoint.GetPropertyValue(StylusPointProperties.XTiltOrientation));

                    // Update minimum tilt if necessary
                    if (curr < min) min = curr;

                    // Update maximum tilt if necessary
                    if (curr > max) max = curr;
                }
            }

            // Return N/A unit value if input does not support tilt, else return formatted tilt range as UnitValue Synthesis
            Synthesis result = min == Int32.MaxValue ? this.FormatRange(0, 0, available: false) : this.FormatRange(min, max);
            return result;
        }

        /// <summary>
        /// Formats tilt range into Unit Value Synthesis.
        /// </summary>
        /// <param name="available">Indicates whether tilt data is available</param> 
        /// <param name="minTilt">Minimum tilt of the pen</param>
        /// <param name="maxTilt">Maximum tilt of the pen</param>
        /// <returns>Unit Value Synthesis representing range of pen tilt during exercise</returns>
        public Synthesis FormatRange(Int32 minTilt, Int32 maxTilt, Boolean available = true)
        {
            string message;
            if (available)
            {
                // Format tilt range in string representation
                minTilt /= 100;
                maxTilt /= 100;
                message = minTilt.ToString() + " to " + maxTilt.ToString();
            } else
            {
                // If pen tilt data unavailable, clear units and return n/a message
                message = "N/A";
                this._unit = "";
            }
            
            // Return message as unit value
            return new UnitValue(message)
            {
                Title = this._title,
                Unit = this._unit
            };
        }
    }
}
