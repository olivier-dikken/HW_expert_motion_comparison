using HandwritingFeedback.Util;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents
{
    /// <summary>
    /// Abstract class represents a type of batched feedback based on the WindowsInk API
    /// </summary>
    public abstract class BFCanvasComponent : BFComponent
    {
        protected TraceUtils StudentTraceUtils { get; set; }
        protected TraceUtils ExpertTraceUtils { get; set; }

        /// <summary>
        /// if the datapoints in the plots can/should be linked to the stroke datapoints
        /// </summary>
        public bool LinkeableOnClick { get; set; }

        /// <summary>
        /// Constructor to initialize a canvas-based batched feedback component.<br/>
        /// Any such component will depend on stroke collections on the WindowsInk canvas.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at an exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        protected BFCanvasComponent(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils, bool linkeableDatapoints = false)
        {
            this.StudentTraceUtils = studentTraceUtils;
            this.ExpertTraceUtils = expertTraceUtils;
            this.LinkeableOnClick = linkeableDatapoints;
        }
    }
}
