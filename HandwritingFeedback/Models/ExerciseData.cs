using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Ink;

namespace HandwritingFeedback.Models
{
    public class ExerciseData
    {        
        // Target trace strokes
        public StrokeCollection TargetTraceStrokes { get; set; }

        // List to store trace-over attempts
        public List<StrokeCollection> TraceOverAttempts { get; set; }

        // Exercise information
        public string ExerciseName { get; set; }
        public string Description { get; set; }
        public int LineType { get; set; }
        public int LineSpacing { get; set; }
        public int StrictnessRating { get; set; }
        public int RepetitionsAmount { get; set; }
        public List<FeedbackSelectionGridItem> FeatureSelectionGridData { get; set; }

        public DateTime CreationDate { get; }
        public string Path { get; set; }

        public ExerciseData()
        {
            TargetTraceStrokes = new StrokeCollection();
            TraceOverAttempts = new List<StrokeCollection>();
            FeatureSelectionGridData = new List<FeedbackSelectionGridItem>();
            CreationDate = DateTime.Now;
        }
    }
}
