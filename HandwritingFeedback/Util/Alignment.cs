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
    class Alignment
    {

        /// <summary>
        /// Get distance matrix where d[i, j] corrsponds to the euclidean distance between expertDataPoint[i] and studentDataPoint[j]
        /// </summary>
        /// <param name="expertData"></param>
        /// <param name="studentData"></param>
        /// <returns></returns>
        public static float[,] GetDistanceMatrix(float[] expertData, float[] studentData)
        {
            int m = expertData.Length;
            int n = studentData.Length;
            float[,] distances = new float[m, n];

            for(int i =0; i < m; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    distances[i, j] = Math.Abs(expertData[i] - studentData[j]);
                }
            }

            return distances;
        }

        /// <summary>
        /// distances between coordinates
        /// </summary>
        /// <param name="expertData">list of float[X, Y] pairs</param>
        /// <param name="studentData">list of float[X, Y] pairs</param>
        /// <returns></returns>
        public static float[,] GetCoordinateDistanceMatrix(List<(float, float)> expertData, List<(float, float)> studentData)
        {            
            int m = expertData.Count;
            int n = studentData.Count;
            float[,] distances = new float[m, n];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    distances[i, j] = MathF.Sqrt(MathF.Pow(studentData[j].Item1 - expertData[i].Item1, 2) + MathF.Pow(studentData[j].Item2 - expertData[i].Item2, 2));
                }
            }

            return distances;
        }

        /// <summary>
        /// convert a trace to a list<(float, float)> of X, Y coordinates
        /// </summary>
        /// <param name="trace"></param>
        /// <returns></returns>
        public static List<(float, float)> TraceToCoords(StrokeCollection trace)
        {
            TraceUtils tu = new TraceUtils(trace);
            int length = tu.GetNumberOfStylusPoints();
            List<(float, float)> result = new List<(float, float)>();

            StylusPointCollection spc = tu.GetAllPoints();
            
            foreach(StylusPoint sp in spc)
            {
                result.Add(((float)sp.X, (float)sp.Y));
            }

            return result;
        }


        /// <summary>
        /// gets total length of stroke in pixels
        /// </summary>
        /// <returns></returns>       
        private static float getStrokeLength(Stroke stroke)
        {
            StylusPointCollection strokePoints = stroke.StylusPoints;
            StylusPoint previousPoint = strokePoints[0];
            double x1, x2, y1, y2;
            double dist = 0;
            for (int i = 0; i < strokePoints.Count; i++)
            {
                x1 = previousPoint.X;
                y1 = previousPoint.Y;

                x2 = strokePoints[i].X;
                y2 = strokePoints[i].Y;

                dist += Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

                previousPoint = strokePoints[i];
            }
            return (float)dist;
        }

        /// <summary>
        /// check alignment of student stroke with target trace stroke
        /// distance is normalized based on number of points and stroke length
        /// so when scaling x,y the distance value stays similar
        /// </summary>
        /// <param name="studentStroke"></param>
        /// <param name="targetTraceStroke"></param>
        public static float getAlignmentDistance(Stroke strokeStudent, Stroke strokeTargetTrace)
        {
            //IEnumerable<Stroke> scStrokeEnumerable = strokeStudent;
            StrokeCollection scStudent = new StrokeCollection(new List<Stroke> { strokeStudent });
            StrokeCollection scTargetTrace = new StrokeCollection(new List<Stroke> { strokeTargetTrace });
            List<(float, float)> coordsE = Alignment.TraceToCoords(scTargetTrace);
            List<(float, float)> coordsS = Alignment.TraceToCoords(scStudent);
            float[,] coordDistances = Alignment.GetCoordinateDistanceMatrix(coordsE, coordsS);

            //float totalDistance = coordDistances[coordDistances.GetLength(0)-1, coordDistances.GetLength(1)-1];
            List<(int, int)> bestPath = GetBestPath(coordDistances);
            float bestPathDistance = pathToTotalDistance(bestPath, coordDistances);
            //normalize distance over number of TargetTrace datapoints            
            float strokeStudentLength = getStrokeLength(strokeStudent);
            float strokeTargetTraceLength = getStrokeLength(strokeTargetTrace);

            float normalizedDistance = (bestPathDistance / strokeTargetTrace.StylusPoints.Count) / strokeTargetTraceLength;
            //Debug.WriteLine("ratio student/target trace length: " + (strokeStudentLength / strokeTargetTraceLength));
            //Debug.WriteLine("normalized by TT length: " + (normalizedDistance / strokeTargetTraceLength));
            return normalizedDistance;
        }

        private static float pathToTotalDistance(List<(int, int)> path, float[,] distances)
        {
            float totalDistance = 0;
            for (int i = 0; i < path.Count; i++)
            {
                totalDistance += distances[path[i].Item1, path[i].Item2];
            }

            return totalDistance;
        }


        /// <summary>
        /// dynamic programming, computes path best distance score from end to start
        /// </summary>
        /// <param name="distances"></param>
        /// <returns></returns>
        public static float BestPathDistance(float[,] distances)
        {
            int windowSize = 5;
            int mL = distances.GetLength(0);
            int nL = distances.GetLength(1);

            int offset = 5;

            int limit = Math.Min(mL, nL);

            float[,] small_distances = new float[limit-offset, limit-offset];
            for (int i = offset; i < limit; i++)
            {
                for (int j = offset; j < limit; j++)
                {
                    small_distances[i-offset, j-offset] = distances[i, j];
                }
            }
            distances = small_distances;
            return BestPathDistance(distances, distances.GetLength(0)-1, distances.GetLength(1)-1, windowSize);
        }

        public static List<(int, int)> TestBestPath(float[,] distances)
        {
            int mL = distances.GetLength(0);
            int nL = distances.GetLength(1);

            //int offset = 5;

            int limit = Math.Min(mL, nL);

            //float[,] small_distances = new float[limit - offset, limit - offset];
            //for (int i = offset; i < limit; i++)
            //{
            //    for (int j = offset; j < limit; j++)
            //    {
            //        small_distances[i - offset, j - offset] = distances[i, j];
            //    }
            //}
            //distances = small_distances;
            List<(int, int)> bestPath = GetBestPath(distances);
            return bestPath;
        }

        /// <summary>
        /// dynamic programming, computes path best distance score from end to start
        /// </summary>
        /// <param name="distances"></param>        
        public static float BestPathDistance(float[,] distances, int m, int n, int l)
        {            
            //Debug.WriteLine($"DTW m {m} n {n}");

            //find corresponding % index
            int mL = distances.GetLength(0)-1;
            int nL = distances.GetLength(1)-1;
            int correspondingPercentageIndex = m;
            if (mL != nL)
            {
                float rM = m / (float)mL;
                float rounded = MathF.Round(rM * nL);
                correspondingPercentageIndex = (int)rounded;
                //Debug.WriteLine($"corrM rounded: {rounded}");
            }                        
            
            if (m == 0 && n == 0)
            {
                return 0;
            } else if(m == 0 || n == 0)
            {
                return float.MaxValue;
            }
            if(correspondingPercentageIndex - n >= l)
            {
                //Debug.WriteLine($"In corrM-n>=l with corrM: {correspondingPercentageIndex}");
                return distances[m, n] + MathF.Min(BestPathDistance(distances, m - 1, n, l), BestPathDistance(distances, m - 1, n - 1, l));
            } else if (n - correspondingPercentageIndex >= l)
            {
                //Debug.WriteLine($"In n-corrM>=l with corrM: {correspondingPercentageIndex}");
                return distances[m, n] + MathF.Min(BestPathDistance(distances, m, n - 1, l), BestPathDistance(distances, m - 1, n - 1, l));
            }
            return distances[m, n] + MathF.Min(BestPathDistance(distances, m - 1, n, l), MathF.Min(BestPathDistance(distances, m - 1, n - 1, l), BestPathDistance(distances, m, n - 1, l)));
        }

        public static float[,] GetBestValues(float[,] distances, int m, int n)
        {
            //Debug.WriteLine($"DTW m {m} n {n}");
            float[,] bestValues = new float[m,n];

            bestValues[0, 0] = 0;

            for(int i = 1; i<m; i++)
            {
                bestValues[i, 0] = float.MaxValue;
            }
            for(int j = 1; j < n; j++)
            {
                bestValues[0, j] = float.MaxValue;
            }

            for(int i = 1; i < m; i++)
            {
                for(int j = 1; j < n; j++)
                {
                    bestValues[i, j] = distances[i,j] + MathF.Min(bestValues[i - 1, j], MathF.Min(bestValues[i - 1, j - 1], bestValues[i, j-1]));
                }
            }

            return bestValues;            
        }

        public static List<(int, int)> GetBestPath(float[,] distances)
        {
            int m = distances.GetLength(0);
            int n = distances.GetLength(1);
            float[, ] bestValues = GetBestValues(distances, m, n);

            (int, int) current = (m-1, n-1);
            List<(int, int)> bestPath = new List<(int, int)> { current };
            while (current != (0, 0))
            {
                if(current.Item1 == 0)
                {
                    current.Item2 -= 1;
                    
                } else if(current.Item2 == 0)
                {
                    current.Item1 -= 1;
                } else
                {
                    float down = bestValues[current.Item1 - 1, current.Item2];
                    float left = bestValues[current.Item1, current.Item2 - 1];
                    float diag = bestValues[current.Item1 - 1, current.Item2 - 1];
                    //find minimum
                    if (down < left && down < diag)//min = down
                    {
                        current.Item1 -= 1;
                    }
                    else if (left < diag)//min = left
                    {
                        current.Item2 -= 1;
                    }
                    else//min = diag
                    {
                        current.Item1 -= 1;
                        current.Item2 -= 1;
                    }
                }
                
                bestPath.Add(current);
            }
            return bestPath;
        }


        public static void SaveTest(StrokeCollection expertStrokes, StrokeCollection studentStrokes, List<(int, int)> alignmentVector, string fileName)
        {
            AlignmentTestSample ats = new AlignmentTestSample(expertStrokes, studentStrokes, alignmentVector);
            ats.SaveToFile(fileName);
        }
    }
}
