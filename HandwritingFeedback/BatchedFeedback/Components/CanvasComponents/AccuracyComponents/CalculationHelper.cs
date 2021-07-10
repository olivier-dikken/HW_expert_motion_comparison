using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using HandwritingFeedback.Config;

namespace HandwritingFeedback.BatchedFeedback.Components.CanvasComponents.AccuracyComponents
{
    /// <summary>
    /// Class to calculate accuracy of a trace with respect to another trace.
    /// This class uses memoization if it is called with a number of sections
    /// to obtain intermediate accuracy results up until a specific section.
    /// This uses trace overlap of student-expert and expert-student. 
    /// </summary>
    public class CalculationHelper
    {
        /// <summary>
        /// Array that stores the number of times the expert points have <br />
        /// hit the student trace up until each section of the student trace. 
        /// </summary>
        internal int[] HitsOnExpertTrace;

        /// <summary>
        /// Array that stores the number of times the student points have <br />
        /// hit the expert trace up until each section of the student trace.
        /// </summary>
        internal int[] HitsOnStudentTrace;

        /// <summary>
        /// The number of points in the student trace. This is updated when <br/>
        /// a new section of the student trace comes in.
        /// </summary>
        internal int StudentPointsCount;

        /// <summary>
        /// The final accuracy of the student trace.
        /// </summary>
        private double _finalAccuracy = -1.0;
        
        /// <summary>
        /// Stores the points that have already been hit by the student trace.
        /// </summary>
        internal List<(Stroke, StylusPoint)> PointsHit;
        
        /// <summary>
        /// Calculates the accuracy of a trace with respect to another trace, by considering the overlap in both ways.
        /// Arrays are initialized to store the number of hits up until each section.
        /// This class does not split the trace into sections. The section and section number must be supplied as an argument.
        /// 
        /// </summary>
        /// <param name="studentStrokeSection">The section of the student stroke to consider</param>
        /// <param name="expertTrace">The expert trace to consider</param>
        /// <param name="sections">The number of sections that the graph will be split into.</param>
        /// <param name="currentSection">The section of which the accuracy must be calculated.
        /// The previously stored hits of previous sections will be taken into account.</param>
        /// <returns>The accuracy of the student trace</returns>
        public double CalculateAccuracy(StrokeCollection studentStrokeSection, StrokeCollection expertTrace, int sections, int currentSection)
        {
            // We have looped through all section, so the final accuracy can be returned.
            if (currentSection >= sections)
            {
                return _finalAccuracy;
            }
            // Initialize array for memoization.
            if (currentSection == 0)
            {
                // The number of hits up until a section will be stored in these arrays. 
                HitsOnStudentTrace = new int[sections];
                HitsOnExpertTrace = new int[sections];
                PointsHit = new List<(Stroke, StylusPoint)>();
            }

            var expertTask = Task.Factory.StartNew(() => HitFractionStudent(studentStrokeSection, expertTrace, currentSection));
            var studentTask = Task.Factory.StartNew(() => HitFractionExpert(expertTrace, studentStrokeSection, currentSection));

            double accuracy = Math.Round(Math.Min(expertTask.Result * 100, studentTask.Result * 100), 0);
            
            // This is the final section, so the FinalAccuracy can be set.
            if (currentSection == sections -1)
            {
                _finalAccuracy = accuracy;
            }
            
            return accuracy;
        }

        /// <summary>
        /// Calculates the fraction of student points up to and including this section that have hit the expert trace.
        /// It also saves the number of hits up and including this part of the student stroke,
        /// such that it can be used to calculate the next section.
        /// </summary>
        /// <param name="studentSection">The section of the student trace that is checked to hit the expert trace.</param>
        /// <param name="expertTrace">The trace that is checked to see if the other points hit this trace.</param>
        /// <param name="currentSection">The section of this student trace that is now being considered.</param>
        /// <returns>The number indicating the fraction of the points of the student trace that hit the expert trace.</returns>
        internal double HitFractionStudent(StrokeCollection studentSection, StrokeCollection expertTrace, int currentSection)
        {
            var expertPointCount = 0;
            var hits = CalculateHitsExpert(studentSection, expertTrace, ref expertPointCount);
            
            if (currentSection != 0)
            {
                // Add the previously calculated hits in previous sections.
                // This is now the total number of hits until the current section.
                hits += HitsOnStudentTrace[currentSection - 1];
            }
            HitsOnStudentTrace[currentSection] = hits;
            
            return (double) hits / expertPointCount;
        }

        /// <summary>
        /// Calculates the fraction of expert points that have hit the student trace including this section and previous sections.
        /// It also saves the number of times the expert trace has hit the full student trace so far,
        /// such that it can be used to calculate the next section.
        /// </summary>
        /// <param name="expertTrace">The expert trace that is checked to hit the student trace</param>
        /// <param name="studentSection">The trace that is checked to see if the other points hit this trace.</param>
        /// <param name="currentSection">The section of this student trace that is now being considered.</param>
        /// <returns>The number indicating the fraction of the points of the expert trace that hit the student trace.</returns>
        internal double HitFractionExpert(StrokeCollection expertTrace, StrokeCollection studentSection, int currentSection)
        {
            var hitsInThisSection = CalculateHitsStudent(expertTrace, studentSection, ref StudentPointsCount);
            if (currentSection != 0)
            {
                // Add the previously calculated hits. This is now the total number of hits until the current section.
                hitsInThisSection += HitsOnExpertTrace[currentSection - 1];
            }
            HitsOnExpertTrace[currentSection] = hitsInThisSection;
            
            return (double) hitsInThisSection/StudentPointsCount;
        }

        /// <summary>
        /// Calculates the number of times each point of the student section hits the expert trace.
        /// It also adds the number of points in the student section to the total point count. 
        /// </summary>
        /// <param name="expertTrace">The trace to hit.</param>
        /// <param name="studentSection">The trace that is tested to hit the other trace.</param>
        /// <param name="studentPointCount">The count of which the number of points of the student section is added to.</param>
        /// <returns>The number of times the student section hits the expert trace.</returns>
        private static int CalculateHitsStudent(StrokeCollection expertTrace, StrokeCollection studentSection, ref int studentPointCount)
        {
            var hits = 0;
            foreach (var stroke in studentSection)
            {
                foreach (var point in stroke.StylusPoints)
                {
                    // Add 1 hit if it hits, otherwise add 0 hits.
                    hits += Math.Min(expertTrace.HitTest(point.ToPoint(),
                        ApplicationConfig.Instance.MaxDeviationRadius * 2d).Count, 1);
                    studentPointCount++;
                }
            }
            
            return hits;
        }
        
        /// <summary>
        /// Calculates the number of times each point of the expert trace hits the student section.
        /// It also counts the number of points in the expert trace.
        /// </summary>
        /// <param name="studentSection">The section of the student trace to hit.</param>
        /// <param name="expertTrace">The trace that is tested to hit the other trace.</param>
        /// <param name="expertPointCount">The number of points in the expert trace.</param>
        /// <returns>The number of times the each point of the expert trace hits the student section.</returns>
        private int CalculateHitsExpert(StrokeCollection studentSection, StrokeCollection expertTrace, ref int expertPointCount)
        {
            var hits = 0;
            foreach (var stroke in expertTrace)
            {
                foreach (var point in stroke.StylusPoints)
                {
                    // Prevent the student getting hits for tracing parts of the expert trace they
                    // have already covered.
                    var existing = PointsHit.Find(item => item.Equals((stroke, point)));
                    var current = (stroke, point);
                    
                    if (existing.Equals(current))
                    {
                        expertPointCount++;
                        continue;
                    }
                    
                    // Add 1 hit if it hits, otherwise add 0 hits.
                    int hit = Math.Min(studentSection.HitTest(point.ToPoint(),
                        ApplicationConfig.Instance.MaxDeviationRadius * 2d).Count, 1);
                    hits += hit;
                    
                    // Remember that the student hit this expert point in the current stroke
                    if (hit == 1)
                    {
                        PointsHit.Add(current);
                    }
                    
                    expertPointCount++;
                }
            }
            
            return hits;
        }

        /// <summary>
        /// Get the final accuracy if it was already calculated. Otherwise it will be calculated.
        /// </summary>
        /// <param name="studentTrace">The student trace to consider.</param>
        /// <param name="expertTrace">The expert trace to consider.</param>
        /// <returns>Accuracy of the student's trace with respect of the expert's trace.</returns>
        public double GetFinalAccuracy(StrokeCollection studentTrace, StrokeCollection expertTrace)
        {
            if (_finalAccuracy == -1.0)
            {
                // The final accuracy was not calculated yet, so we calculate it. 
                // We want the final accuracy of the whole stroke, so we only have 1 section.
                CalculateAccuracy(studentTrace, expertTrace, 1, 0);
            }
            
            return _finalAccuracy;
        }
    }
}