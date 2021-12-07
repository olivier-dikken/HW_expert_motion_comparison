using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HandwritingFeedback.Models
{
    class ExpertDistributionModel
    {
        Dictionary<string, List<double[]>> transformed_data = new Dictionary<string, List<double[]>>();

        int length = -1;

        public ExpertDistributionModel(int targetLength)
        {
            length = targetLength;
        }

        public ExpertDistributionModel(string pathToFile)
        {
            //TODO load EDM from file (serialized)
        }

        public void AddTransformed(double [] data, string name)
        {
            if(length == -1) //if not yet initialized with target length, this should be done first
            {
                Debug.WriteLine("Cannot add data, target length is not set");
            } else //try to add the data
            {
                if (transformed_data.ContainsKey(name)) //add to existing list
                {
                    transformed_data[name].Add(data);
                } else //init new list
                {
                    List<double[]> newList = new List<double[]>();
                    newList.Add(data);
                    transformed_data.Add(name, newList);
                }
            }
        }

        /// <summary>
        /// convert dictionary with list of double[] per feature 
        /// to a dictionary with a double_avg[] and double_std[] 
        /// per feature
        /// </summary>
        public void GetDistributionModel()
        {
            
        }

    }
}
