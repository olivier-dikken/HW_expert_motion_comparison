using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandwritingFeedback.BatchedFeedback.Components;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.PressureComponents;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.SpeedComponents;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.TiltComponents;
using HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.KeypointComponents;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Util;
using System.Windows.Controls;
using System.Diagnostics;

namespace HandwritingFeedback.BatchedFeedback
{
    /// <summary>
    /// Manages the batched feedback components and populates the docking panes in the batched analytics view.
    /// </summary>
    public class BFViewManager
    {
        public readonly List<BFComponent> Components;
        private readonly Plotter _plotter;
        private CalculationHelper _calculationHelper;
        public KeypointDetection kpDetection;
        private StackPanel _parametersDock;

        /// <summary>
        /// Constructor for BFManager instantiate new engine and load components.
        /// </summary>
        /// <param name="input">Input data transferred from sensors</param>
        public BFViewManager(BFInputData input)
        {
            // Instantiate engine and load all batched feedback components
            this.Components = new List<BFComponent>();
            kpDetection = new KeypointDetection(input.StudentTraceUtils, input.ExpertTraceUtils);
            this.LoadCanvasComponents(input.StudentTraceUtils, input.ExpertTraceUtils);

            // Instantiate plotter
            _plotter = new Plotter(input.UnitValueDock, input.GraphDock);

            _parametersDock = input.ParametersDock;
        }

        /// <summary>
        /// Load all batched feedback components into engine.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at an exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public void LoadCanvasComponents(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils)
        {
            // Add any additional canvas components below
            _calculationHelper = new CalculationHelper();
            this.Components.Add(new AccuracyOverProgress(studentTraceUtils, expertTraceUtils, _calculationHelper));
            //this.Components.Add(new PressureOverProgress(studentTraceUtils, expertTraceUtils));
            this.Components.Add(kpDetection);
            this.Components.Add(new TiltRange(studentTraceUtils, expertTraceUtils));
            this.Components.Add(new CompletionTime(studentTraceUtils, expertTraceUtils));
            this.Components.Add(new SpeedOverProgress(studentTraceUtils, expertTraceUtils));
            this.Components.Add(new AccuracyDistance(studentTraceUtils, expertTraceUtils));
            // The overall accuracy component should be synthesized last,
            // such that it can use the computed final accuracy from AccuracyOverProgress.
            this.Components.Add(new OverallAccuracy(studentTraceUtils, expertTraceUtils, _calculationHelper));
        }

        /// <summary>
        /// Populates all docks on batched analytics view in parallel.
        /// </summary>
        public void PopulateDocks()
        {
            var syntheses = new List<Synthesis>();

            // Synthesize every other batched feedback component in parallel
            Parallel.ForEach(Components.Take(Components.Count-1), component =>
            {
                syntheses.Add(component.Synthesize());
            });
            
            // Synthesize OverallAccuracy last so it uses the computed accuracy from AccuracyOverProgress.
            syntheses.Add(Components.Last().Synthesize());
            
            // Render every synthesis on the batched analytics view depending on their types
            foreach (Synthesis s in syntheses)
            {
                switch (s)
                {
                    case UnitValue value:
                        _plotter.RenderUnitValue(value);
                        break;
                    case LineGraph graph:
                        _plotter.RenderLineGraph(graph);
                        break;
                    default:
                        throw new Exception("The synthesis could not be added to the plotter due to it being" +
                                            "of an unknown type!");
                }
            }

            //populate parameters dock
            

            List<(string, int)> guiparams = kpDetection.GetGUIParameters();

            foreach((string, int) guiparam in guiparams)
            {
                TextBox tb = new TextBox();
                Label label = new Label();
                label.Content = guiparam.Item1;
                tb.Width = 120;
                tb.Text = guiparam.Item2.ToString();
                _parametersDock.Children.Add(label);
                _parametersDock.Children.Add(tb);
            }

            
           
        }

        /// <summary>
        /// get string,value pairs from parameter dock
        /// </summary>
        /// <returns></returns>
        public List<(string, int)> getParameterDockValues()
        {
            List<(string, int)> result = new List<(string, int)>();
            string lab = "";
            int val;
            for(int i = 0; i < _parametersDock.Children.Count; i++)
            {
                if(i%2 == 0) //even
                {
                    Label labelElement = (Label)_parametersDock.Children[i];
                    lab = (string)labelElement.Content;                    
                } else //odd
                {
                    TextBox tbElement = (TextBox)_parametersDock.Children[i];
                    val = Int32.Parse(tbElement.Text);
                    result.Add((lab, val));
                }
            }
            return result;
        }

        public void UpdateKeypointParameters(List<(string, int)> newValues)
        {
            kpDetection.UpdateParameters(newValues);
            _plotter.RenderLineGraph((LineGraph)kpDetection.SynthesizeNew("newplot"));
        }

    }
}
