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
using HandwritingFeedback.Models;
using HandwritingFeedback.View;
using OxyPlot;
using static HandwritingFeedback.Util.ReportUtils;

namespace HandwritingFeedback.BatchedFeedback
{
    /// <summary>
    /// Manages the batched feedback components and populates the docking panes in the batched analytics view.
    /// </summary>
    public class BFViewManager_EDM
    {
        public readonly List<BFComponent> Components;
        private readonly Plotter _plotter;
        private CalculationHelper _calculationHelper;
        public KeypointDetection kpDetection;
        private StackPanel _parametersDock;

        private StackPanel _graphDock;

        /// <summary>
        /// Constructor for BFManager instantiate new engine and load components.
        /// </summary>
        /// <param name="input">Input data transferred from sensors</param>
        public BFViewManager_EDM(BFInputData input, List<EDMComparisonResult> comparisonResults, Canvas overlayCanvas, BatchedAnalytics_EDM BA_EDM)
        {
            // Instantiate engine and load all batched feedback components
            this.Components = new List<BFComponent>();            
            this.LoadCanvasComponents(input.StudentTraceUtils, input.ExpertTraceUtils, comparisonResults, overlayCanvas, BA_EDM);

            // Instantiate plotter
            _plotter = new Plotter(input.UnitValueDock, input.GraphDock);
            _graphDock = input.GraphDock;

            _parametersDock = input.ParametersDock;            
        }

        /// <summary>
        /// Load all batched feedback components into engine.
        /// </summary>
        /// <param name="studentTraceUtils">TraceUtils of trace of student's attempt at an exercise</param>
        /// <param name="expertTraceUtils">TraceUtils of trace upon which the student practiced</param>
        public void LoadCanvasComponents(TraceUtils studentTraceUtils, TraceUtils expertTraceUtils, List<EDMComparisonResult> comparisonResults, Canvas overlayCanvas, BatchedAnalytics_EDM BA_EDM)
        {
            // Add any additional canvas components below
            _calculationHelper = new CalculationHelper();

            foreach(EDMComparisonResult comparisonResult in comparisonResults)
            {
                this.Components.Add(new FeatureOverProgress_EDM(studentTraceUtils, expertTraceUtils, comparisonResult, comparisonResult.title, comparisonResult.ylabel, overlayCanvas, BA_EDM));
            }
            
        }


        public void UpdateSelectedPoint(int newSelectedPointIndex)
        {
            foreach(BFComponent component in this.Components)
            {
                FeatureOverProgress_EDM ft_comp = (FeatureOverProgress_EDM)component;
                ft_comp.setSelectedPoint(newSelectedPointIndex);
            }
            //redraw
            PopulateDocks();
        }

        /// <summary>
        /// Populates all docks on batched analytics view in parallel.
        /// </summary>
        public void PopulateDocks()
        {
            //start by clearing the dock (useful for redrawing)
            _graphDock.Children.RemoveRange(0, _graphDock.Children.Count);

            var syntheses = new List<Synthesis>();

            // Synthesize every other batched feedback component in parallel
            Parallel.ForEach(Components.Take(Components.Count), component =>
            {
                Synthesis newSynth = component.Synthesize();
                lock (syntheses)
                {
                    syntheses.Add(newSynth);
                }                
            });


            //keep consistent order
            syntheses = syntheses.OrderBy(o => o.Title).ToList();
            
            
            
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
        }
        
    }
}
