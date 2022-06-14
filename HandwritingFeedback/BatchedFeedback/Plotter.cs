using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using HandwritingFeedback.Config;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace HandwritingFeedback.BatchedFeedback
{
    /// <summary>
    /// Plotter utility tool generates differing types of visualizations for batched feedback.
    /// </summary>
    public class Plotter
    {
        public StackPanel UnitDock { get; }
        public StackPanel GraphDock { get; }

        /// <summary>
        /// Construct an instance of a plotter utility tool.
        /// </summary>
        public Plotter(StackPanel unitDock, StackPanel graphDock)
        {
            this.UnitDock = unitDock;
            this.GraphDock = graphDock;
        }

        /// <summary>
        /// Format unitary values to be displaying in unit value docking pane on UI.
        /// </summary>
        /// <param name="synthesis">Batched feedback component instance to be visualized as a unit value</param>
        public void RenderUnitValue(UnitValue synthesis)
        {
            // Format text to be added to dock
            var view = new TextBlock
            {
                Text = synthesis.Title + " :    " + synthesis.Value + " " + synthesis.Unit
            };

            // Add new text to dock
            UnitDock.Children.Add(view);
        }

        /// <summary>
        /// Renders batched feedback components as a line graph.
        /// </summary>
        /// <param name="synthesis">Batched feedback component instance to be visualized in a line graph</param>
        public void RenderLineGraph(LineGraph synthesis)
        {
            // Instantiate x-axis
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = synthesis.XAxisLabel,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MinimumRange = synthesis.MinimumXRange,
                AbsoluteMinimum = synthesis.AbsoluteMinimumX
                
            };

            // Instantiate y-axis
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = synthesis.YAxisLabel,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MinimumRange = synthesis.MinimumYRange,
                AbsoluteMinimum = synthesis.AbsoluteMinimumY
            };

            // Instantiate plot model
            var plot = new PlotModel
            {
                Title = synthesis.Title,
                LegendPosition = LegendPosition.TopLeft                
            };

            // Attach axes to plot model
            plot.Axes.Add(xAxis);
            plot.Axes.Add(yAxis);

            //draw ellipse on selected point
            if(synthesis.selectedPointIndex != -1) //check if a point has been selected, if yes then draw ellipse annotation
            {                
                List<DataPoint> dps = (List<DataPoint>)synthesis.AllSeries[0].ItemsSource;
                DataPoint selected_dp = dps[synthesis.selectedPointIndex];              

                PointAnnotation ptAnn_outer = new PointAnnotation();
                ptAnn_outer.X = selected_dp.X;
                ptAnn_outer.Y = selected_dp.Y;
                ptAnn_outer.Fill = OxyColors.Blue;
                ptAnn_outer.Size = 10;

                PointAnnotation ptAnn = new PointAnnotation();
                ptAnn.X = selected_dp.X;
                ptAnn.Y = selected_dp.Y;
                ptAnn.Fill = OxyColors.Yellow;
                ptAnn.Size = 8;

                plot.Annotations.Add(ptAnn_outer);
                plot.Annotations.Add(ptAnn);
            }

            HighlightErrors(plot, synthesis.ErrorZonesXValues);
            //HighlightErrors(plot, synthesis.ErrorZonesXValues, ApplicationConfig.Instance.MinErrorHighlightingFraction);

            //MarkKeypoints(plot, synthesis.Keypoints, 3, "Keypoints Algorithm 1: Neighbor difference > Threshold");
            //MarkKeypoints(plot, synthesis.DebugKeypoints, 6, "Keypoints Algorithm: exponentially weighted MVA");

            // Iterate through all lists of data points synthesized by component
            foreach (LineSeries candidate in synthesis.AllSeries)
            {
                // Interpolate line graph (to create a smooth curved line) between points
                if (synthesis.CurvedLine)
                {
                    candidate.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
                }
                                    
                plot.Series.Add(candidate);                                
            }
            

            // Attach model to a view and add to the graph docking panel
            var plotView = new OxyPlot.Wpf.PlotView
            {
                Model = plot,
                Height = 450                
            };
            
            GraphDock.Children.Add(plotView);
        }

        public static void HighlightErrors(PlotModel plot, List<double> xValues)
        {
            for(int i = 0; i < xValues.Count; i++)
            {
                RectangleAnnotation annotation = new RectangleAnnotation
                {
                    Fill = OxyColor.FromArgb(64, 255, 0, 0), // Transparent red
                    Layer = AnnotationLayer.AboveSeries, // Overlayed on top of lines
                    MinimumY = -65536d, // Extends to y=-infinity (-2^16, because graphs might render incorrectly with lower values)
                    MaximumY = 65536d, // Extends to y=infinity (2^16, because graphs might render incorrectly with higher values)
                    MinimumX = xValues[i]-0.5,
                    MaximumX = xValues[i]+0.5
                };

                plot.Annotations.Add(annotation);
            }


        }

        /// <summary>
        /// Draws transparent rectangles on a plot using the provided x-values and <br />
        /// displays the total error count in the subtitle of the plot. Used for highlighting <br />
        /// areas where real-time feedback was provided (which means that an error occured there).
        /// </summary>
        /// <param name="plot">The plot to add rectangle annotations to</param>
        /// <param name="xValues">List of x-values for the rectangles, must have an even number <br />
        /// of elements (so that every rectangle has a start and end value)</param>
        /// <param name="minErrorFraction">Minimum fraction of the x-axis that an error must span <br />
        /// before it is counted and highlighted</param>
        /// <exception cref="ArgumentException">Thrown when xValues has an odd number of elements</exception>
        //public static void HighlightErrors(PlotModel plot, List<double> xValues, double minErrorFraction)
        //{
        //    int xValuesCount = xValues.Count;
        //    if (xValuesCount % 2 != 0) throw new ArgumentException(
        //        "Not every error zone has an end x-value (xValues should have an even number of elements)!");
        //    int totalErrorCount = 0;

        //    // Draw rectangles over areas where an error occured, using the indices in synthesis.
        //    // Looping happens in pairs, where each pair has x values for the start and end of the rectangle
        //    for (int i = 0; i < xValuesCount / 2; i++)
        //    {
        //        double rectangleStart = xValues[i * 2];
        //        double rectangleEnd = xValues[i * 2 + 1];

        //        // Continue if the error is too small to be counted
        //        if (rectangleEnd - rectangleStart < minErrorFraction * 100d)
        //            continue;

        //        RectangleAnnotation annotation = new RectangleAnnotation
        //        {
        //            Fill = OxyColor.FromArgb(64, 255, 0, 0), // Transparent red
        //            Layer = AnnotationLayer.AboveSeries, // Overlayed on top of lines
        //            MinimumY = -65536d, // Extends to y=-infinity (-2^16, because graphs might render incorrectly with lower values)
        //            MaximumY = 65536d, // Extends to y=infinity (2^16, because graphs might render incorrectly with higher values)
        //            MinimumX = rectangleStart,
        //            MaximumX = rectangleEnd
        //        };

        //        plot.Annotations.Add(annotation);
        //        totalErrorCount++;
        //    }

        //    // Display total error count if there were any errors
        //    if (totalErrorCount > 0)
        //    {
        //        plot.SubtitleColor = OxyColors.Black;
        //        plot.SubtitleFontSize = 20d;
        //        plot.Subtitle = totalErrorCount == 1 ? "1 mistake" : totalErrorCount + " mistakes";
        //    }
        //}

        public static void MarkKeypoints(PlotModel plot, List<(double, double)> kpValues, int colorValue, string label)
        {

            int ValuesCount = kpValues.Count;
            Console.WriteLine("keypoints valuescount: {0}", ValuesCount);

            var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle, Title =  label};

            // Draw rectangles over areas where an error occured, using the indices in synthesis.
            // Looping happens in pairs, where each pair has x values for the start and end of the rectangle
            for (int i = 0; i < ValuesCount; i++)
            {
                var x = kpValues[i].Item1;
                var y = kpValues[i].Item2 * 100; //*100 to visualize value between [0-1] on scale [0-100]
                var size = 7;
                scatterSeries.Points.Add(new ScatterPoint(x, y, size, colorValue));
               
            }

            plot.Series.Add(scatterSeries);
        }
    }
}