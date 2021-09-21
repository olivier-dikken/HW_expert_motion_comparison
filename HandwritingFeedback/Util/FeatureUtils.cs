using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HandwritingFeedback.Util
{
    class FeatureUtils
    {

        /// <summary>
        /// get slope of list of (x,y) values
        /// </summary>
        /// <param name="points"></param>
        /// <returns>list with the slopes of the points</returns>
        public static List<double> getSlope(List<(double, double)> points)
        {
            if(points.Count < 1)
            {
                //TODO throw error
                Debug.WriteLine("getSlope() error: not enough datapoints");
            }


            List<double> result = new List<double>();

            //add first point
            double diffX = points[1].Item1 - points[0].Item1;
            double diffY = points[1].Item2 - points[0].Item2;
            double delta = diffY / diffX;
            result.Add(delta);

            for (int i = 1; i < points.Count-1; i++)
            {
                diffX = points[i + 1].Item1 - points[i - 1].Item1;
                diffY = points[i + 1].Item2 - points[i - 1].Item2;
                delta = diffY / diffX;
                result.Add(delta);
            }

            //add last point
            diffX = points[points.Count-1].Item1 - points[points.Count - 2].Item1;
            diffY = points[points.Count-1].Item2 - points[points.Count - 2].Item2;
            delta = diffY / diffX;
            result.Add(delta);

            return result;
        }
    }
}
