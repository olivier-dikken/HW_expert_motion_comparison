using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using HandwritingFeedback.BatchedFeedback;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes;
using HandwritingFeedback.BatchedFeedback.SynthesisTypes.Graphs;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;

namespace HandwritingFeedbackTests.BatchedFeedback.SynthesisTypes
{
    internal class PlotterTest : ApplicationTest
    {
        private Plotter _plotter;
        
        [SetUp]
        public override void InternalSetUp()
        {
            var unitDock = new StackPanel();
            var graphDock = new StackPanel();

            _plotter = new Plotter(unitDock, graphDock);
        }

        [Test]
        public void NotNullTest()
        {
            Assert.NotNull(_plotter);
        }

        [Test]
        public void GettersTest()
        {
            Assert.NotNull(_plotter.UnitDock);
            Assert.NotNull(_plotter.GraphDock);
        }

        [Test]
        public void RenderUnitValueTest()
        {
            Assert.DoesNotThrow(() => _plotter.RenderUnitValue(new UnitValue("Overall Accuracy")));
            // Check that the unit value was added to the dock
            Assert.AreEqual(1, _plotter.UnitDock.Children.Count);
        }

        [Test]
        public void RenderLineGraphTest()
        {
            var dataPoints = new List<DataPoint>()
            {
                new DataPoint(0, 0),
                new DataPoint(1, 1)
            };
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Black,
                ItemsSource = dataPoints
            };

            var lineGraph = new LineGraph {AllSeries = {lineSeries}};

            Assert.DoesNotThrow(() => _plotter.RenderLineGraph(lineGraph));
            // Check that the graph was added to the dock
            Assert.AreEqual(1, _plotter.GraphDock.Children.Count);
        }

        [Test]
        public void RenderLineGraphCurvedTest()
        {
            var dataPoints = new List<DataPoint>()
            {
                new DataPoint(0, 0),
                new DataPoint(3, 4)
            };
            var lineSeries = new LineSeries
            {
                Color = OxyColors.Red,
                ItemsSource = dataPoints
            };
            
            var lineGraphCurved = new LineGraph {CurvedLine = true, AllSeries = {lineSeries}};
            Assert.DoesNotThrow(() => _plotter.RenderLineGraph(lineGraphCurved));
            // Check that the graph was added to the dock
            Assert.AreEqual(1, _plotter.GraphDock.Children.Count);
        }

        #region HighlightErrorsTests
        
        [Test]
        public void HighlightErrorsOddNumXValuesTest()
        {
            // An exception should be thrown when xValues has an odd number of elements
            Assert.Throws<ArgumentException>(() => Plotter.HighlightErrors(new PlotModel(), new List<double> {1d}, 0.3d));
        }

        [Test]
        public void HighlightErrorsNoErrorsTest()
        {
            PlotModel plot = new PlotModel();
            Plotter.HighlightErrors(plot, new List<double>(), 0.3d);
            
            List<Annotation> actualAnnotations = plot.Annotations.Where(x => x.GetType() == typeof(RectangleAnnotation)).ToList();

            // No rectangle annotations should have been added
            Assert.AreEqual(new List<Annotation>(), actualAnnotations);
            
            // No subtitle for the number of mistakes should have been set
            Assert.AreEqual(null, plot.Subtitle);
        }
        
        [Test]
        public void HighlightErrorsOneErrorTooSmallTest()
        {
            PlotModel plot = new PlotModel();
            // The error is too small to be counted (55 - 3 = 25 < 0.3 * 100)
            Plotter.HighlightErrors(plot, new List<double> {0.3, 0.55}, 0.3d);
            
            List<Annotation> actualAnnotations = plot.Annotations.Where(x => x.GetType() == typeof(RectangleAnnotation)).ToList();

            // No rectangle annotations should have been added
            Assert.AreEqual(new List<Annotation>(), actualAnnotations);
            
            // No subtitle for the number of mistakes should have been set
            Assert.AreEqual(null, plot.Subtitle);
        }
        
        [Test]
        public void HighlightErrorsOneErrorBoundaryTest()
        {
            PlotModel plot = new PlotModel();
            // The error is just large enough to be counted (60 - 29 = 31 >= 0.3 * 100)
            Plotter.HighlightErrors(plot, new List<double> {29, 60}, 0.3d);

            var expectedAnnotations = new List<RectangleAnnotation>
            {
                new RectangleAnnotation
                {
                    Fill = OxyColor.FromArgb(64, 255, 0, 0), // Transparent red
                    Layer = AnnotationLayer.AboveSeries, // Overlayed on top of lines
                    MinimumY = -65536d, // Extends to y=-infinity (-2^16, because graphs might render incorrectly with lower values)
                    MaximumY = 65536d, // Extends to y=infinity (2^16, because graphs might render incorrectly with higher values)
                    MinimumX = 29,
                    MaximumX = 60
                }
            };

            List<Annotation> actualAnnotations = plot.Annotations.Where(x => x.GetType() == typeof(RectangleAnnotation)).ToList();
            
            Assert.AreEqual(expectedAnnotations.Count, actualAnnotations.Count);
            // Check that the correct rectangle annotations were added
            for (int i = 0; i < actualAnnotations.Count; i++)
            {
                // Casting to RectangleAnnotation must work, because we filtered out all other annotations
                RectangleAnnotation actualAnnotation = (RectangleAnnotation)actualAnnotations[i];
                CompareRectangleAnnotations(expectedAnnotations[i], actualAnnotation);
            }
            
            // Check that the subtitle for the number of mistakes has been set
            Assert.AreEqual("1 mistake", plot.Subtitle);
        }
        
        [Test]
        public void HighlightErrorsManyErrorsTest()
        {
            PlotModel plot = new PlotModel();
            // The errors, except for the first one, are all large enough to be counted
            Plotter.HighlightErrors(plot, new List<double> {0, 1, 9, 40, 50, 70, 75, 100}, 0.2d);

            var expectedAnnotations = new List<RectangleAnnotation>
            {
                new RectangleAnnotation
                {
                    Fill = OxyColor.FromArgb(64, 255, 0, 0), // Transparent red
                    Layer = AnnotationLayer.AboveSeries, // Overlayed on top of lines
                    MinimumY = -65536d, // Extends to y=-infinity (-2^16, because graphs might render incorrectly with lower values)
                    MaximumY = 65536d, // Extends to y=infinity (2^16, because graphs might render incorrectly with higher values)
                    MinimumX = 9,
                    MaximumX = 40
                },
                new RectangleAnnotation
                {
                    Fill = OxyColor.FromArgb(64, 255, 0, 0), // Transparent red
                    Layer = AnnotationLayer.AboveSeries, // Overlayed on top of lines
                    MinimumY = -65536d, // Extends to y=-infinity (-2^16, because graphs might render incorrectly with lower values)
                    MaximumY = 65536d, // Extends to y=infinity (2^16, because graphs might render incorrectly with higher values)
                    MinimumX = 50,
                    MaximumX = 70
                },
                new RectangleAnnotation
                {
                    Fill = OxyColor.FromArgb(64, 255, 0, 0), // Transparent red
                    Layer = AnnotationLayer.AboveSeries, // Overlayed on top of lines
                    MinimumY = -65536d, // Extends to y=-infinity (-2^16, because graphs might render incorrectly with lower values)
                    MaximumY = 65536d, // Extends to y=infinity (2^16, because graphs might render incorrectly with higher values)
                    MinimumX = 75,
                    MaximumX = 100
                }
            };

            List<Annotation> actualAnnotations = plot.Annotations.Where(x => x.GetType() == typeof(RectangleAnnotation)).ToList();
            
            Assert.AreEqual(expectedAnnotations.Count, actualAnnotations.Count);
            // Check that the correct rectangle annotations were added
            for (int i = 0; i < actualAnnotations.Count; i++)
            {
                // Casting to RectangleAnnotation must work, because we filtered out all other annotations
                RectangleAnnotation actualAnnotation = (RectangleAnnotation)actualAnnotations[i];
                CompareRectangleAnnotations(expectedAnnotations[i], actualAnnotation);
            }
            
            // Check that the subtitle for the number of mistakes has been set
            Assert.AreEqual("3 mistakes", plot.Subtitle);
        }

        // Asserts that two rectangles have the same positions
        private void CompareRectangleAnnotations(RectangleAnnotation expected, RectangleAnnotation actual)
        {
            Assert.AreEqual(expected.MinimumY, actual.MinimumY);
            Assert.AreEqual(expected.MaximumY, actual.MaximumY);
            Assert.AreEqual(expected.MinimumX, actual.MinimumX);
            Assert.AreEqual(expected.MaximumX, actual.MaximumX);
        }
        
        #endregion
    }
}