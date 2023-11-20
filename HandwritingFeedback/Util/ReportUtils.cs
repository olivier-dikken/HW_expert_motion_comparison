using HandwritingFeedback.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;

namespace HandwritingFeedback.Util
{
    public class ReportUtils
    {
        private readonly TraceUtils _expertTraceUtils;
        private readonly TraceUtils _studentTraceUtils;
        private EDMData _edmData;
        private List<(int, int)> _alignmentVector;
        public List<EDMComparisonResult> ComparisonResult;

        public ReportUtils(BFInputData input, EDMData edmData)
        {
            _studentTraceUtils = input.StudentTraceUtils;
            _expertTraceUtils = input.ExpertTraceUtils;
            _edmData = edmData;
            LoadComparisonResult();
        }

        void LoadComparisonResult()
        {
            _alignmentVector = getBestPath(_studentTraceUtils.Trace, _expertTraceUtils.Trace);
            ComparisonResult = ComputeErrorBasedOnEDM();
        }

        private void refactorCompareStudentToEDM()
        {
            StrokeCollection studentTrace = _studentTraceUtils.Trace;
            List<EDMComparisonResult> result = new List<EDMComparisonResult>(); 
            EDMData eDMData = _edmData;

            //for each feature in GlobalState.FeatureNames, 
        }


        private List<EDMComparisonResult> ComputeErrorBasedOnEDM()
        {
            List<float> x_student = new List<float>();
            List<float> x_scores = new List<float>();
            List<float> x_avg = new List<float>();
            List<float> x_std = new List<float>();

            List<float> y_student = new List<float>();
            List<float> y_scores = new List<float>();
            List<float> y_avg = new List<float>();
            List<float> y_std = new List<float>();

            List<float> curv_student = new List<float>();
            List<float> curv_scores = new List<float>();
            List<float> curv_avg = new List<float>();
            List<float> curv_std = new List<float>();

            List<float> pressure_student = new List<float>();
            List<float> pressure_scores = new List<float>();
            List<float> pressure_avg = new List<float>();
            List<float> pressure_std = new List<float>();

            List<float> speed_student = new List<float>();
            List<float> speed_scores = new List<float>();
            List<float> speed_avg = new List<float>();
            List<float> speed_std = new List<float>();


            StrokeCollection trace = _studentTraceUtils.Trace;
            List<EDMComparisonResult> result = new List<EDMComparisonResult>();

            int totalPoints = trace.Sum(s => s.StylusPoints.Count);

            // Use alignmentVector to find corresponding EDMDataPointFeatureValue instances
            // Create a 2D list to store EDMDataPointFeatureValue for each feature
            List<EDMDataPointFeatureValue[]> relevantEDMPoints = new List<EDMDataPointFeatureValue[]>();
            int previousStudentMatchIndex = -1;
            EDMDataPointFeatureValue[] correspondingDataPoints = new EDMDataPointFeatureValue[totalPoints];
            StylusPoint previousStudentPoint;
            StylusPoint studentPoint;
            bool firstPointSet = false;

            string[] featureNames = GlobalState.FeatureNames;

            Debug.WriteLine("Alignment vector count: " + _alignmentVector.Count);

            for (int i = _alignmentVector.Count - 1; i >= 0; i--)
            {
                if (_alignmentVector[i].Item1 == previousStudentMatchIndex || previousStudentMatchIndex == -1)
                {
                    // Student point aligned to several expert points or no previous match
                }
                else // Student index increased by 1
                {
                    // Process relevantEDMPoints to correspondingDataPoints for the previous student index
                    correspondingDataPoints = GetCorrespondingEDMDataPoint(relevantEDMPoints);

                    // Compare EDMDataPointFeatureValue values with the corresponding student data point
                    studentPoint = GetPointFromTraceAt(trace, previousStudentMatchIndex);

                    Debug.WriteLine("Index of X:" + Array.IndexOf(GlobalState.FeatureNames, "X"));

                    float score_x = GetEDMScore(studentPoint.X, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "X")].Mean, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "X")].GetStandardDeviation());
                    float score_y = GetEDMScore(studentPoint.Y, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Y")].Mean, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Y")].GetStandardDeviation());
                    float score_pressure = GetEDMScore(studentPoint.PressureFactor, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Pressure")].Mean, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Pressure")].GetStandardDeviation());

                    x_student.Add((float)studentPoint.X);
                    x_scores.Add(score_x);
                    x_avg.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "X")].Mean);
                    x_std.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "X")].GetStandardDeviation());

                    y_student.Add((float)studentPoint.Y);
                    y_scores.Add(score_y);
                    y_avg.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Y")].Mean);
                    y_std.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "X")].GetStandardDeviation());

                    pressure_student.Add((float)studentPoint.PressureFactor);
                    pressure_scores.Add(score_pressure);
                    pressure_avg.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Pressure")].Mean);
                    pressure_std.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Pressure")].GetStandardDeviation());

                    if (firstPointSet)
                    {
                        float score_curv = GetCurvatureScore(studentPoint, previousStudentPoint, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Curvature")]);
                        curv_student.Add(score_curv);
                        curv_scores.Add(score_curv);
                        curv_avg.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Curvature")].Mean);
                        curv_std.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Curvature")].GetStandardDeviation());

                        float score_speed = GetSpeedScore(studentPoint, previousStudentPoint, correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Speed")]);
                        speed_student.Add(score_speed);
                        speed_scores.Add(score_speed);
                        speed_avg.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Speed")].Mean);
                        speed_std.Add((float)correspondingDataPoints[Array.IndexOf(GlobalState.FeatureNames, "Speed")].GetStandardDeviation());
                    }

                    // Print in the console
                    Debug.WriteLine("Student point " + previousStudentMatchIndex + " comparison with EDM point. X diff: " + score_x + " Y diff: " + score_y);
                    firstPointSet = true;
                }
                //for each feature, add the corresponding EDMDataPointFeatureValue to the list
                relevantEDMPoints.Add(_edmData.GetFeatureData(_alignmentVector[i].Item2));
                previousStudentMatchIndex = _alignmentVector[i].Item1;
            }

            // Create and add EDMComparisonResult instances for each feature
            result.Add(new EDMComparisonResult(x_student, x_scores, x_avg, x_std, "X of student vs EDM", "X position"));
            result.Add(new EDMComparisonResult(y_student, y_scores, y_avg, y_std, "Y of student vs EDM", "Y position"));
            result.Add(new EDMComparisonResult(curv_student, curv_scores, curv_avg, curv_std, "Curvature of student vs EDM", "Curvature angle in degrees"));
            result.Add(new EDMComparisonResult(pressure_student, pressure_scores, pressure_avg, pressure_std, "Pressure of student vs EDM", "Pen PressureFactor on scale [0-1]"));
            result.Add(new EDMComparisonResult(speed_student, speed_scores, speed_avg, speed_std, "Speed of student vs EDM", "Trace speed"));

            return result;
        }

        private float GetEDMScore(double studentValue, double edmValue, double edmStandardDeviation)
        {
            return (float)((studentValue - edmValue) / edmStandardDeviation);
        }

        private float GetCurvatureScore(StylusPoint studentPoint, StylusPoint previousStudentPoint, EDMDataPointFeatureValue edmPoint)
        {
            float x_diff = (float)(studentPoint.X - previousStudentPoint.X);
            float y_diff = (float)(studentPoint.Y - previousStudentPoint.Y);
            return (float)(Math.Acos(x_diff) / Math.Sqrt(x_diff * x_diff + y_diff * y_diff));
        }

        private float GetSpeedScore(StylusPoint studentPoint, StylusPoint previousStudentPoint, EDMDataPointFeatureValue edmPoint)
        {
            float diagonalLength = (float)Math.Sqrt(Math.Pow(studentPoint.X - previousStudentPoint.X, 2) + Math.Pow(studentPoint.Y - previousStudentPoint.Y, 2));
            // TODO: Divide by time passed!
            return (float)((diagonalLength - edmPoint.Mean) / edmPoint.GetStandardDeviation());
        }

        private EDMDataPointFeatureValue[] GetCorrespondingEDMDataPoint(List<EDMDataPointFeatureValue[]> relevantEDMPoints)
        {
            //if list length is 1 then return that point
            if (relevantEDMPoints.Count == 1)
                return relevantEDMPoints[0];

            EDMDataPointFeatureValue[] correspondingDataPoints = new EDMDataPointFeatureValue[GlobalState.FeatureNames.Length];
            //get the average mean, variance and number of samples for each datapoint in the list
            for (int j = 0; j < GlobalState.FeatureNames.Length; j++)
            {
                double mean = 0;
                double variance = 0;
                int numSamples = 0;
                for (int i = 0; i < relevantEDMPoints.Count; i++)
                {
                    mean += relevantEDMPoints[i][j].Mean / relevantEDMPoints.Count;
                    variance += relevantEDMPoints[i][j].Variance / relevantEDMPoints.Count;
                    numSamples = relevantEDMPoints[i][j].NumberOfSamples;
                }

                correspondingDataPoints[j] = new EDMDataPointFeatureValue(mean, variance, numSamples);
            }

            return correspondingDataPoints;
        }


        //private List<EDMComparisonResult> oldComputeErrorBasedOnEDM()
        //{
        //    StrokeCollection trace = _studentTraceUtils.Trace;
        //    List<EDMComparisonResult> result = new List<EDMComparisonResult>();
        //    List<float> x_student = new List<float>();
        //    List<float> x_scores = new List<float>();
        //    List<float> x_avg = new List<float>();
        //    List<float> x_std = new List<float>();

        //    List<float> y_student = new List<float>();
        //    List<float> y_scores = new List<float>();
        //    List<float> y_avg = new List<float>();
        //    List<float> y_std = new List<float>();

        //    List<float> curv_student = new List<float>();
        //    List<float> curv_scores = new List<float>();
        //    List<float> curv_avg = new List<float>();
        //    List<float> curv_std = new List<float>();

        //    List<float> pressure_student = new List<float>();
        //    List<float> pressure_scores = new List<float>();
        //    List<float> pressure_avg = new List<float>();
        //    List<float> pressure_std = new List<float>();

        //    List<float> speed_student = new List<float>();
        //    List<float> speed_scores = new List<float>();
        //    List<float> speed_avg = new List<float>();
        //    List<float> speed_std = new List<float>();

        //    int totalPoints = 0;
        //    for (int i = 0; i < trace.Count; i++)
        //    {
        //        totalPoints += trace[i].StylusPoints.Count;
        //    }

        //    //use alignmentVector to find corresponding edmDataPoints
        //    List<EDMDataPoint> relevantEDMPoints = new List<EDMDataPoint>();
        //    int previousStudentMatchIndex = -1;
        //    EDMDataPoint[] correspondingDataPoints = new EDMDataPoint[totalPoints];
        //    StylusPoint previousStudentPoint;
        //    StylusPoint studentPoint;
        //    bool firstPointSet = false;
        //    Debug.WriteLine("Alignment vector count: " + _alignmentVector.Count);
        //    for (int i = _alignmentVector.Count - 1; i >= 0; i--)
        //    {
        //        if (_alignmentVector[i].Item1 == previousStudentMatchIndex || previousStudentMatchIndex == -1) // student point aligned to several expert points

        //        {
        //        }
        //        else //student index increased by 1
        //        {
        //            //process relevantEDMPoints to correspondingDataPoints, for previous student index
        //            if (relevantEDMPoints.Count == 1)
        //                correspondingDataPoints[previousStudentMatchIndex] = relevantEDMPoints[0];
        //            else
        //            {
        //                correspondingDataPoints[previousStudentMatchIndex] = edmDataPointAvg(relevantEDMPoints, previousStudentMatchIndex);
        //            }
        //            relevantEDMPoints.Clear();

        //            //compare edmDataPoint values with corresponding student datapoint
        //            if (firstPointSet)
        //            {
        //                previousStudentPoint = studentPoint;
        //            }
        //            studentPoint = GetPointFromTraceAt(trace, previousStudentMatchIndex);

        //            float score_x = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "X", 1);
        //            float score_y = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "Y", 1);
        //            float score_pressure = GetEDMScore(studentPoint, correspondingDataPoints[previousStudentMatchIndex], "Pressure", 1);

                    
        //            x_student.Add((float)studentPoint.X);
        //            x_scores.Add(score_x);
        //            x_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].X);
        //            x_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].X_std);

        //            y_student.Add((float)studentPoint.Y);
        //            y_scores.Add(score_y);
        //            y_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Y);
        //            y_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Y_std);

        //            pressure_student.Add((float)studentPoint.PressureFactor);
        //            pressure_scores.Add(score_pressure);
        //            pressure_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Pressure);
        //            pressure_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Pressure_std);

        //            if (firstPointSet)
        //            {
        //                float score_curv = GetEDMScore_Derivative(studentPoint, previousStudentPoint, correspondingDataPoints[previousStudentMatchIndex], "Curvature", 1);
        //                float x_diff = (float)(studentPoint.X - previousStudentPoint.X);
        //                float y_diff = (float)(studentPoint.Y - previousStudentPoint.Y);
        //                float angle = MathF.Atan2(y_diff, x_diff);

        //                curv_student.Add(angle);
        //                curv_scores.Add(score_curv);
        //                curv_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Curvature);
        //                curv_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Curvature_std);

        //                float score_speed = GetEDMScore_Derivative(studentPoint, previousStudentPoint, correspondingDataPoints[previousStudentMatchIndex], "Speed", 1);
        //                float diagonalLength = MathF.Sqrt(MathF.Pow((float)(studentPoint.X - previousStudentPoint.X), 2) + MathF.Pow((float)(studentPoint.Y - previousStudentPoint.Y), 2));
        //                //TODO divide by time passed! 

        //                speed_student.Add(diagonalLength);
        //                speed_scores.Add(score_speed);
        //                speed_avg.Add((float)correspondingDataPoints[previousStudentMatchIndex].Speed);
        //                speed_std.Add((float)correspondingDataPoints[previousStudentMatchIndex].Speed_std);
        //            }

        //            //print in console
        //            Debug.WriteLine("Student point " + previousStudentMatchIndex + " comparison with EDM point. X diff: " + score_x + " Y diff: " + score_y);
        //            firstPointSet = true;
        //        }
        //        //relevantEDMPoints.Add(edmData.dataPoints[_alignmentVector[i].Item2]);                
        //        relevantEDMPoints.Add(_edmData.dataPoints);//TODO fix this, old edmDAta uses dataPOint but now it changed

        //        previousStudentMatchIndex = _alignmentVector[i].Item1;
        //    }
        //    EDMComparisonResult x_comparison = new EDMComparisonResult(x_student, x_scores, x_avg, x_std, "X of student vs EDM", "X position");
        //    result.Add(x_comparison);
        //    EDMComparisonResult y_comparison = new EDMComparisonResult(y_student, y_scores, y_avg, y_std, "Y of student vs EDM", "Y position");
        //    result.Add(y_comparison);
        //    EDMComparisonResult curv_comparison = new EDMComparisonResult(curv_student, curv_scores, curv_avg, curv_std, "Curvature of student vs EDM", "Curvature angle in degrees");
        //    result.Add(curv_comparison);
        //    EDMComparisonResult pressure_comparison = new EDMComparisonResult(pressure_student, pressure_scores, pressure_avg, pressure_std, "Pressure of student vs EDM", "Pen PressureFactor on scale [0-1]");
        //    result.Add(pressure_comparison);
        //    EDMComparisonResult speed_comparison = new EDMComparisonResult(speed_student, speed_scores, speed_avg, speed_std, "Speed of student vs EDM", "Trace speed");
        //    result.Add(speed_comparison);
        //    return result;
        //}

        //private float oldGetEDMFeatureScore()
        //{
        //      return 0;
        //}

        //private float oldGetEDMScore(StylusPoint studentPoint, EDMDataPoint edmPoint, String feature, float scale)
        //{
        //    switch (feature)
        //    {
        //        case "X":
        //            return (float)((studentPoint.X - edmPoint.X) / (edmPoint.X_std * scale));

        //        case "Y":
        //            return (float)((studentPoint.Y - edmPoint.Y) / (edmPoint.Y_std * scale));

        //        case "Pressure":
        //            return (float)((studentPoint.PressureFactor - edmPoint.Pressure) / (edmPoint.Pressure_std * scale));

        //        default:
        //            Debug.WriteLine("Feature not found: " + feature);
        //            break;
        //    }

        //    return 0;
        //}

        //private float oldGetEDMScore_Derivative(StylusPoint studentPoint, StylusPoint previousStudentPoint, EDMDataPoint edmPoint, String feature, float scale)
        //{
        //    switch (feature)
        //    {
        //        case "Curvature":
        //            float x_diff = (float)(studentPoint.X - previousStudentPoint.X);
        //            float y_diff = (float)(studentPoint.Y - previousStudentPoint.Y);
        //            return MathF.Acos(x_diff) / (MathF.Sqrt(MathF.Pow(x_diff, 2) + MathF.Pow(y_diff, 2)));

        //        case "Speed":
        //            float diagonalLength = MathF.Sqrt(MathF.Pow((float)(studentPoint.X - previousStudentPoint.X), 2) + MathF.Pow((float)(studentPoint.Y - previousStudentPoint.Y), 2));
        //            //TODO divide by time passed! 
        //            return (float)((diagonalLength - edmPoint.Speed) / (edmPoint.Speed_std * scale));

        //        default:
        //            Debug.WriteLine("Feature not found: " + feature);
        //            break;
        //    }

        //    return 0;
        //}

        //private EDMDataPoint edmDataPointAvg(List<EDMDataPoint> relevantEDMPoints, int idx)
        //{
        //    EDMDataPoint dataPoint = new EDMDataPoint(idx);
        //    double X = 0;
        //    double X_std = 0;
        //    double Y = 0;
        //    double Y_std = 0;
        //    double Pressure = 0;
        //    double Pressure_std = 0;
        //    double Speed = 0;
        //    double Speed_std = 0;

        //    //take avg of each property
        //    for (int i = 0; i < relevantEDMPoints.Count; i++)
        //    {
        //        X += relevantEDMPoints[i].X / relevantEDMPoints.Count;
        //        X_std += relevantEDMPoints[i].X_std / relevantEDMPoints.Count;
        //        Y += relevantEDMPoints[i].Y / relevantEDMPoints.Count;
        //        Y_std += relevantEDMPoints[i].Y_std / relevantEDMPoints.Count;
        //        Pressure += relevantEDMPoints[i].Pressure / relevantEDMPoints.Count;
        //        Pressure_std += relevantEDMPoints[i].Pressure / relevantEDMPoints.Count;
        //        Speed += relevantEDMPoints[i].Speed / relevantEDMPoints.Count;
        //        Speed_std += relevantEDMPoints[i].Speed / relevantEDMPoints.Count;
        //    }
        //    dataPoint.X = X;
        //    dataPoint.Y = Y;
        //    dataPoint.X_std = X_std;
        //    dataPoint.Y_std = Y_std;
        //    dataPoint.Pressure = Pressure;
        //    dataPoint.Pressure_std = Pressure_std;
        //    dataPoint.Speed = Speed;
        //    dataPoint.Speed_std = Speed_std;
        //    return dataPoint;
        //}

        StylusPoint GetPointFromTraceAt(StrokeCollection trace, int idx)
        {
            int counter = 0;
            foreach (Stroke stroke in trace)
            {
                foreach (StylusPoint sp in stroke.StylusPoints)
                {
                    if (counter == idx)
                        return sp;
                    counter++;
                }
            }

            Debug.WriteLine("ERROR no styluspoint found in GetPointFromTraceAt");
            return new StylusPoint();
        }

        public List<(int, int)> getBestPath(StrokeCollection scExpert, StrokeCollection scStudent)
        {
            List<(float, float)> coordsE = Alignment.TraceToCoords(scExpert);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            return Alignment.TestBestPath(coordDistances);
        }

        public struct EDMComparisonResult
        {
            public EDMComparisonResult(List<float> val_student, List<float> val_scores, List<float> val_avg, List<float> val_std, string theTitle, string theYlabel)
            {
                Value_student = val_student;
                Value_scores = val_scores;
                Value_avg = val_avg;
                Value_std = val_std;
                Value_scores_overall = val_scores.Average();
                title = theTitle;
                ylabel = theYlabel;
            }

            public List<float> Value_student { get; set; }
            public List<float> Value_scores { get; set; }
            public List<float> Value_avg { get; set; }
            public List<float> Value_std { get; set; }

            public float Value_scores_overall { get; set; }

            public string title { get; }
            public string ylabel { get; }

        }
    }
}
