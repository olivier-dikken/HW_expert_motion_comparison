using System;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;

namespace HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs
{
    /// <summary>
    /// Container for specifications of a line graph visualization.
    /// </summary>
    public class LineGraph : Graph
    {
        // Indicates whether lines in graph should be interpolated between points
        public bool CurvedLine { get; set; }
        public new List<LineSeries> AllSeries { get; }

        // List of x values indicating the starts and ends of errors.
        // Even indices mark starts, odd indices mark ends.
        // Rectangles will be drawn over the graph using these x values
        public List<double> ErrorZonesXValues { get; }

        // List of x values indicating a keypoint is found for this x value.
        public List<(double, double)> Keypoints { get; set; }
        public List<(double, double)> DebugKeypoints { get; set; }

        public int selectedPointIndex { get; set; }

        /// <summary>
        /// Constructor for synthesis that must be visualized as a line graph.
        /// </summary>
        public LineGraph() : base()
        {
            this.CurvedLine = false;
            this.AllSeries = new List<LineSeries>();
            this.ErrorZonesXValues = new List<double>();
            this.Keypoints = new List<(double, double)> { };
            this.DebugKeypoints = new List<(double, double)> { };
            this.selectedPointIndex = -1;
        }

        /// <summary>
        /// Add a new line series to the graph.
        /// </summary>
        /// <param name="lineSeries"></param>
        public void AddSeries(LineSeries lineSeries)
        {
            // Ensure that new series is valid 
            if (lineSeries == null || lineSeries.ItemsSource == null)
            {
                throw new ArgumentNullException();
            }

            this.AllSeries.Add(lineSeries);
        }
        
        /// <summary>
        /// Create a line series with StrokeThickness 2, and the specified parameters.
        /// </summary>
        /// <param name="dataPoints">The data points included in the graph</param>
        /// <param name="oxyColor">The color of the line</param>
        /// <param name="lineTitle">The label associated with the line</param>
        /// <returns>A line series with the specified parameters</returns>
        public static LineSeries CreateSeries(List<DataPoint> dataPoints, OxyColor oxyColor, string lineTitle)
        {
            var series = new LineSeries
            {
                StrokeThickness = 2,
                Color = oxyColor,
                ItemsSource = dataPoints,
                Title = lineTitle
            };
            return series;
        }
    }
}
