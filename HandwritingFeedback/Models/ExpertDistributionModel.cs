using HandwritingFeedback.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace HandwritingFeedback.Models
{
    [Serializable()]
    public class ExpertDistributionModel
    {
        public List<Dictionary<string, double[]>> transformed_data { get; set; } 

        public int length;


        private EDMData edmData{ get; set; }


        public ExpertDistributionModel(int targetLength)
        {
            length = targetLength;
            edmData = new EDMData(length);
            transformed_data = new List<Dictionary<string, double[]>>();
        }

        [JsonConstructor]
        public ExpertDistributionModel(List<Dictionary<string, double[]>> transformed_data, int length)
        {
            this.transformed_data = transformed_data;
            this.length = length;
            edmData = new EDMData(length);
        }

        public int SampleCount()
        {
            return transformed_data.Count;
        }


        /// <summary>
        /// add array of stroke data for a certain feature, after transforming the data to match the TargetTrace length
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        public void AddTransformed(Dictionary<string, double[]> add_data)
        {            
            //if (length == -1) //if not yet initialized with target length, this should be done first
            //{
            //    Debug.WriteLine("Cannot add data, target length is not set");
            //} else //try to add the data
            //{
            //    if (add_data.ContainsKey(name)) //check if data already added
            //    {
            //        Debug.WriteLine("Trying to add feature data to non empty dict feature data");
            //    } else 
            //    {
            //        add_data.Add(name, data);
            //    }
            //    //foreach(string ftName in GlobalState.FeatureNames) //Code causes problem
            //    //{
            //    //    if (!add_data.ContainsKey(ftName)) //if not all features present then return
            //    //        return;
            //    //}
            //    //if all features present, then add to transformed_data
                
            //}
            transformed_data.Add(add_data);
        }

        /// <summary>
        /// convert dictionary with list of double[] per feature 
        /// to a dictionary with a double_avg[] and double_std[] 
        /// per feature
        /// </summary>
        public void RecomputeDistributionModel()
        {            
            foreach(Dictionary<string, double[]> ftData in transformed_data) // for each recording sample (sample contains data for each feature)
            {
                foreach (string ft in GlobalState.FeatureNames) //for each feature in a single recording sample
                {
                    edmData.AddFeatureData(ft, ftData[ft]);
                }
            }            
        }

        /// <summary>
        /// check if edm data has all transformed_data, and if not then add the missing data
        /// </summary>
        private void UpdateEDMData()
        {
            int tdSampleCount = transformed_data.Count;
            int edmSampleCount = edmData.GetData()[GlobalState.FeatureNames[0]][0].numberOfSamples;
            if(tdSampleCount != edmSampleCount) //add missing samples
            {
                for (int i = tdSampleCount-edmSampleCount; i < tdSampleCount; i++)
                {
                    foreach (string ft in GlobalState.FeatureNames) //for each feature in a single recording sample
                    {
                        edmData.AddFeatureData(ft, transformed_data[i][ft]);
                    }
                }
            }
        }

        /// <summary>
        /// Get EDMData object, with most recent transformed_data added to the model
        /// </summary>
        /// <returns></returns>
        public EDMData GetEDMData()
        {
            //UpdateEDMData();
            RecomputeDistributionModel();
            return edmData;
        }

        public static void SaveToFile(string fileName, EDMData toSave)
        {
            Stream SaveFileStream = File.Create(fileName);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(SaveFileStream, toSave);
            SaveFileStream.Close();
        }

        public static EDMData LoadFromFile(string fileName)
        {
            EDMData ret = null;
            if (File.Exists(fileName))
            {
                Console.WriteLine("Reading saved file");
                Stream openFileStream = File.OpenRead(fileName);
                BinaryFormatter deserializer = new BinaryFormatter();
                ret = (EDMData)deserializer.Deserialize(openFileStream);                
                openFileStream.Close();
            }
            return ret;
        }

        private double computeSTD(double avg, double[] samples)
        {
            double sum = 0;
            for(int i = 0; i < samples.Length; i++)
            {
                sum += Math.Pow(avg - samples[i], 2);
            }
            return Math.Sqrt(sum / samples.Length);
        }

        public override string ToString()
        {
            return "EDM TOSTRING() not implemented";
        }

    }



    /// <summary>
    /// storing sample count, mean, M2 to use as part of Welford's algorithm
    /// for more info see: https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Compute_running_.28continuous.29_variance
    /// </summary>
    [Serializable()]
    public class EDMDataPointFeatureValue
    {
        public string name;
        public int numberOfSamples;
        public int index;
        
        public double mean;
        public double M2;
        private double std;

        public EDMDataPointFeatureValue(string name, int idx, double mean = 0, double m2 = 0, int nSamples=0)
        {
            this.name = name;
            this.mean = mean;
            this.index = idx;
            this.numberOfSamples = nSamples;
            M2 = m2;
            if (nSamples > 1)
                UpdateSTD();
            else
                std = 0;
        }

        private void UpdateSTD()
        {
            std = M2 / (double) numberOfSamples;
        }

        public double GetSTD()
        {
            return std;
        }

        public void AddSample(double newSample)
        {            
            numberOfSamples++;
            double delta = newSample - mean;
            mean += delta / numberOfSamples;
            double delta2 = newSample - mean;
            M2 += delta * delta2;
            if (numberOfSamples > 1)
                UpdateSTD();
        }


    }

    [Serializable()]
    public class EDMData
    {
        // use array because faster for frequent access compared to list
        Dictionary<string, EDMDataPointFeatureValue[]> edmData = new Dictionary<string, EDMDataPointFeatureValue[]>();
        //Dictionary<string, double[]> edmDataMinMax = new Dictionary<string, double[]>(); //store min and max values per feature, used later for normalization

        private int length;
        public EDMData(int length)
        {
            this.length = length;
            foreach(string ft in GlobalState.FeatureNames)
            {
                edmData[ft] = new EDMDataPointFeatureValue[length];
                for (int i = 0; i < length; i++)
                {
                    edmData[ft][i] = new EDMDataPointFeatureValue(ft, i); //create edmdatapointftvalue for each index of each feature
                }                
            }
        }

        /// <summary>
        /// add feature data double[] of a single sample
        /// </summary>
        public void AddFeatureData(string ftName, double[] ftDataNew)
        {
            Debug.WriteLine("Adding feature data to EDMData");
            for (int i = 0; i < length; i++)
            {
                edmData[ftName][i].AddSample(ftDataNew[i]);
            }
        }

        public Dictionary<string, EDMDataPointFeatureValue[]> GetData()
        {
            return edmData;
        }        

        public int GetLength()
        {
            return length;
        }
    }

    [Serializable()]
    public class oldEDMData
    {
        public EDMDataPoint[] dataPoints;
        public oldEDMData(EDMDataPoint[] dataPoints)
        {
            this.dataPoints = dataPoints;
        }
    }

    [Serializable()]
    public class EDMDataPoint
    {
        public int index;
        public double X;
        public double X_std;
        public double Y;
        public double Y_std;
        public double Pressure;
        public double Pressure_std;
        public double Speed;
        public double Speed_std;
        public double Curvature;
        public double Curvature_std;

        public int numberOfSamples;

        public EDMDataPoint(int i)
        {
            index = i;
        }

        public void SetValue(string name, double val)
        {
            switch (name)
            {
                case "X":
                    this.X = val;
                    break;

                case "X_std":
                    this.X_std = val;
                    break;

                case "Y":
                    this.Y = val;
                    break;

                case "Y_std":
                    this.Y_std = val;
                    break;

                case "Curvature":
                    this.Curvature = val;
                    break;

                case "Curvature_std":
                    this.Curvature_std = val;
                    break;

                case "Pressure":
                    this.Pressure = val;                    
                    break;

                case "Pressure_std":
                    this.Pressure_std = val;
                    break;

                case "Speed":
                    this.Speed = val;
                    break;

                case "Speed_std":
                    this.Speed_std = val;
                    break;

                default:
                    Debug.WriteLine($"Wrong feature name: {name}");
                    break;
            }
        }
    }
}
