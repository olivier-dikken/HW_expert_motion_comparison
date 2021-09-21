using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using HandwritingFeedback.BatchedFeedback;
using HandwritingFeedback.Util;

namespace HandwritingFeedback.View
{
    /// <summary>
    /// Interaction logic for BatchedAnalytics.xaml
    /// </summary>
    public partial class BatchedAnalytics : Page
    {
        private readonly TraceUtils _expertTraceUtils;
        private readonly TraceUtils _studentTraceUtils;
        private readonly StrokeCollection _expertOutline;
        BFViewManager manager;

        /// <summary>
        /// Constructor for batched analytics view.
        /// </summary>
        /// <param name="input">Data transferred from previous page</param>
        public BatchedAnalytics(BFInputData input)
        {
            InitializeComponent();

            // Extract student and expert trace from input
            _expertTraceUtils = input.ExpertTraceUtils;
            _studentTraceUtils = input.StudentTraceUtils;
            _expertOutline = input.ExpertOutline;
            
            inkCanvas.Strokes = _expertOutline.Clone();
            inkCanvas.Strokes.Add(_expertTraceUtils.Trace.Clone());
            inkCanvas.Strokes.Add(_studentTraceUtils.Trace);            

            // Populate unit and graphing docks
            input.UnitValueDock = unitValueDock;
            input.GraphDock = graphDock;
            input.ParametersDock = parametersDock;

            manager = new BFViewManager(input);
            manager.PopulateDocks();

            AddDots(manager.kpDetection.getKeypointsCanvasLocation());
            
                        
        }

        /// <summary>
        /// Draw dots on canvas for debugging. Initially used to draw keypoint locations.
        /// </summary>
        /// <param name="locations"></param>
        public void AddDots(List<(double, double)> locations)
        {
            double radius = 5;            

            StylusPointCollection pts = new StylusPointCollection();
            foreach((double, double) loc in locations)
            {
                pts.Add(new StylusPoint(loc.Item1, loc.Item2));
            }
            
            Stroke st = new customDotStroke(pts, radius);
            st.DrawingAttributes.Color = Colors.DarkOrange;

            inkCanvas.Strokes.Add(st);
        }

        /// <summary>
        /// Navigates to the page indicated by the button tag.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Navigate(object sender, RoutedEventArgs e)
        {
            CommonUtils.Navigate(sender, e, this);
        }
        
        /// <summary>
        /// Opens file explorer to save student trace.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void SaveStudentTrace(object sender, RoutedEventArgs e)
        {
            var result = new InkCanvas { Strokes = _studentTraceUtils.Trace };
            FileHandler.ButtonSaveAsClick(sender, e, result);
        }

        /// <summary>
        /// reload the keypoint calculations
        /// </summary>
        private void ReloadCalculations(object sender, RoutedEventArgs e)
        {
            List<(string, int)> parameterValues = manager.getParameterDockValues();
            manager.UpdateKeypointParameters(parameterValues);
        }

        /// <summary>
        /// Restarts exercise by loading the previous expert model in practice mode,
        /// and navigates to the practice mode.
        /// </summary>
        /// <param name="sender">The Button which invoked the method</param>
        /// <param name="e">Event arguments</param>
        private void Restart(object sender, RoutedEventArgs e)
        {
            // Add expert trace to be sent to practice mode
            var inputData = new BFInputData
            {
                ExpertTraceUtils = _expertTraceUtils,
                ExpertOutline = _expertOutline
            };

            // Navigate to next page and send package
            var destination = new PracticeMode(inputData);
            NavigationService.Navigate(destination);
        }

        private void SaveAllBatchedFeedback(object sender, RoutedEventArgs e)
        {
            FileHandler.ButtonSaveAsImageClick(sender, e, MainGrid);
        }
    }

    public class customDotStroke : Stroke
    {
        double radius;
        public customDotStroke(StylusPointCollection pts, double radius)
          : base(pts)
        {
            this.StylusPoints = pts;
            this.radius = radius;
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }
            if (null == drawingAttributes)
            {
                throw new ArgumentNullException("drawingAttributes");
            }
            DrawingAttributes originalDa = drawingAttributes.Clone();
            SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
            brush2.Freeze();

            foreach (StylusPoint sp in this.StylusPoints)
            {                                           
                drawingContext.DrawEllipse(brush2, null, new Point(sp.X, sp.Y), radius, radius);
            }
        }

    }
}
