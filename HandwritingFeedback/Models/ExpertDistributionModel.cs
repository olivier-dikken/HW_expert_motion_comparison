using HandwritingFeedback.Util;
using Microsoft.VisualBasic;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Windows.Media;

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
            int edmSampleCount = edmData.GetNumberOfSamples();
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
        public double Mean { get; private set; }
        public double Variance { get; private set; }
        public int NumberOfSamples { get; private set; }

        public EDMDataPointFeatureValue(double mean = 0, double variance = 0, int numberOfSamples = 0)
        {
            Mean = mean;
            Variance = variance;
            NumberOfSamples = numberOfSamples;
        }

        public void AddSample(double newSample)
        {
            NumberOfSamples++;
            double delta = newSample - Mean;
            Mean += delta / NumberOfSamples;
            double delta2 = newSample - Mean;
            Variance += delta * delta2;
        }

        public double GetStandardDeviation()
        {
            if (NumberOfSamples < 2)
                return 0;
            return Math.Sqrt(Variance / (NumberOfSamples - 1));
        }
    }


    [Serializable()]
    public class EDMData
    {
        private EDMDataPointFeatureValue[][] edmData;
        private int length;

        public EDMData(int length)
        {
            this.length = length;
            edmData = new EDMDataPointFeatureValue[length][];

            // Initialize the EDMData array for each data point and feature.
            for (int i = 0; i < length; i++)
            {
                edmData[i] = new EDMDataPointFeatureValue[GlobalState.FeatureNames.Length];
                for (int j = 0; j < GlobalState.FeatureNames.Length; j++)
                {
                    edmData[i][j] = new EDMDataPointFeatureValue();
                }
            }
        }

        public void AddFeatureData(string ftName, double[] ftDataNew)
        {
            for (int i = 0; i < length; i++)
            {
                int ftIndex = GetFeatureIndex(ftName);
                edmData[i][ftIndex].AddSample(ftDataNew[i]);
            }
        }

        public double GetFeatureMean(int dataIndex, string ftName)
        {
            int ftIndex = GetFeatureIndex(ftName);
            return edmData[dataIndex][ftIndex].Mean;
        }

        public double GetFeatureVariance(int dataIndex, string ftName)
        {
            int ftIndex = GetFeatureIndex(ftName);
            return edmData[dataIndex][ftIndex].Variance;
        }

        public double GetFeatureStandardDeviation(int dataIndex, string ftName)
        {
            return edmData[dataIndex][GetFeatureIndex(ftName)].GetStandardDeviation();
        }

        public int GetLength()
        {
            return length;
        }

        /// <summary>
        /// Get the number of samples for a given data point.
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <returns></returns>
        public int GetNumberOfSamples(int dataIndex=0)
        {
            return edmData[dataIndex][0].NumberOfSamples;
        }   

        private int GetFeatureIndex(string ftName)
        {
            return Array.IndexOf(GlobalState.FeatureNames, ftName);
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
