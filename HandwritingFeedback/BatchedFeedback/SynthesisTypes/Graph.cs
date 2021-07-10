using System;
using System.Collections.Generic;
using OxyPlot.Series;

namespace HandwritingFeedback.BatchedFeedback.SynthesisTypes
{
    /// <summary>
    /// Abstract container for specifications of a graph visualization.
    /// </summary>
    public abstract class Graph : Synthesis
    {
        // List of all series to be visualized on graph along with the color of the respective series
        public List<Series> AllSeries { get; }

        // Label for x-axis
        public string XAxisLabel { get; set; }

        // Label for y-axis
        public string YAxisLabel { get; set; }
        
        // The minimum range on the y-axis that is initially displayed.
        public double MinimumYRange { get; set; }
        
        // The minimum value on the y-axis that is initially displayed.
        public double AbsoluteMinimumY { get; set; }
        
        // The minimum range on the x-axis that is initially displayed.
        public double MinimumXRange { get; set; }
        
        // The minimum value on the x-axis that is initially displayed.
        public double AbsoluteMinimumX { get; set; }

        /// <summary>
        /// Abstract base constructor for graph visualization.
        /// </summary>
        protected Graph()
        {
            AllSeries = new List<Series>();
            XAxisLabel = "";
            YAxisLabel = "";
        }

        /// <summary>
        /// Add a new series  to the graph.
        /// </summary>
        /// <param name="series"></param>
        public void AddSeries(Series series)
        {
            // Ensure that new series is valid
            if (series == null)
            {
                throw new ArgumentNullException();
            }

            this.AllSeries.Add(series);
        }
    }
}
