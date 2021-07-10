using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.StylusPlugins.Strokes;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.SpeedComponents
{
    /// <summary>
    /// Computes the completion time for the student stroke, in comparison with the expert model.
    /// </summary>
    public class CompletionTime : BFCanvasComponent
    {
        private readonly string _unit;
        private string _title;

        /// <summary>
        /// Constructor to compute completion time after an exercise.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at a exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public CompletionTime(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils) : base(studentTraceUtils,
            expertTraceUtils)
        {
            _unit = "s";
            _title = "Completion Time";
        }

        /// <summary>
        /// Computes the competition time for the student stroke, compared to the completion time <br />
        /// for the associated expert stroke. 
        /// If expert temporal data is unavailable due to an out-dated .ISF file or file created in <br />
        /// external application, total student completion time is calculated instead.
        /// </summary>
        /// <returns>Completion time of exercise represented as a Unit Value</returns>
        public override Synthesis Synthesize()
        {
            float completionTime;

            // Collect the starting and ending time of stylus input in practice mode
            float studentStartTime = ((GeneralStroke)StudentTraceUtils.Trace[0]).TemporalCacheSnapshot[0].timestamp;
            float studentEndTime = ((GeneralStroke)StudentTraceUtils.Trace[^1]).TemporalCacheSnapshot[^1].timestamp;

            // Calculate the total completion time of input in practice mode
            float studentCompletionTime = studentEndTime - studentStartTime;

            // Check if temporal data is available in the expert trace
            // Expert temporal data will be unavailable if the .ISF file is out-dated
            // or the file was created in an external application
            if (ExpertTraceUtils.ContainsValidTemporalCaches())
            {
                // Collect the starting and ending time of the expert's stylus input
                float expertStartTime = ((GeneralStroke)ExpertTraceUtils.Trace[0]).TemporalCacheSnapshot[0].timestamp;
                float expertEndTime = ((GeneralStroke)ExpertTraceUtils.Trace[^1]).TemporalCacheSnapshot[^1].timestamp;

                // Calculate the total completion time of the expert's input
                var expertCompletionTime = expertEndTime - expertStartTime;

                // Calculate the difference between the student and expert's completion time
                completionTime = studentCompletionTime - expertCompletionTime;

                // Update the Unit Value's title to reflect this adaptation of type of feedback provided
                _title = "Student Completion Time Compared to Expert";
            } else
            {
                // If expert temporal data is unavailable, total completion time of student input
                // is returned as batched feedback instead
                completionTime = studentCompletionTime;
            }

            // Convert the completion time from milliseconds to seconds
            completionTime /= 1000;
            
            // Append positive sign to completion time string if it is a positive value
            var completionTimeString = completionTime >= 0 ? "+" : "";
            completionTimeString = completionTimeString + completionTime;

            // Format result in Unit Value to be visualized on view
            return new UnitValue(completionTimeString)
            {
                Title = _title,
                Unit = _unit
            };
        }
    }
}