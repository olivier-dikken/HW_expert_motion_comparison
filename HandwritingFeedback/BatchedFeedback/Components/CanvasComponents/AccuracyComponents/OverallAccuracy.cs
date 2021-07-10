using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents
{
    /// <summary>
    /// This component configures what to display about the accuracy of
    /// the student's trace with respect to the expert's trace.
    /// </summary>
    public class OverallAccuracy : BFCanvasComponent
    {
        private readonly string _unit;
        private readonly string _title;
        private CalculationHelper CalculationHelper { get; }

        /// <summary>
        /// Constructor to initialize a component to compute the total accuracy after an exercise.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at a exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        /// <param name="calculationHelper">Calculation helper to assist in calculating overall accuracy, which
        /// was cached by the accuracy over progress batched feedback component</param>
        public OverallAccuracy(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils, CalculationHelper calculationHelper) : base(studentTraceUtils, expertTraceUtils)
        {
            this._title = "Overall accuracy";
            this._unit = "%";
            this.CalculationHelper = calculationHelper;
        }

        /// <summary>
        /// Specifies what is returned and displayed in the view for accuracy.
        /// </summary>
        /// <returns>Synthesis object representing a UnitValue with the value as the calculated accuracy</returns>
        public override Synthesis Synthesize()
        {
            string value = CalculationHelper.GetFinalAccuracy(StudentTraceUtils.Trace, ExpertTraceUtils.Split()).ToString();
            return new UnitValue(value)
            {
                Title = this._title,
                Unit = this._unit
            };
        }
    }
}