using System.Windows.Controls;
using System.Windows.Ink;

namespace HandwritingFeedback.Util
{
    /// <summary>
    /// Class that holds input data used for batched feedback.
    /// </summary>
    public class BFInputData
    {
        public TraceUtils StudentTraceUtils;

        public TraceUtils ExpertTraceUtils;
        
        public StrokeCollection ExpertOutline;

        public StackPanel GraphDock;

        public StackPanel UnitValueDock;

        public StackPanel ParametersDock;
    }
}
