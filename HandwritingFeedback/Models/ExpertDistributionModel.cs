using HandwritingFeedback.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Controls.Primitives;

namespace HandwritingFeedback.Models
{
    [Serializable()]
    public class ExpertDistributionModel
    {
        public List<Dictionary<string, double[]>> transformed_data { get; set; } 

        public int length { get; set; } = -1;


        private EDMData edmData{ get; set; }


        public ExpertDistributionModel(int targetLength)
        {
            length = targetLength;
            transformed_data = new List<Dictionary<string, double[]>>();
        }

        [JsonConstructor]
        public ExpertDistributionModel(List<Dictionary<string, double[]>> transformed_data, int length)
        {
            this.transformed_data = transformed_data;
            this.length = length;
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
        public void AddTransformed(double [] data, string name)
        {
            Dictionary<string, double[]> add_data = new Dictionary<string, double[]>();
            if (length == -1) //if not yet initialized with target length, this should be done first
            {
                Debug.WriteLine("Cannot add data, target length is not set");
            } else //try to add the data
            {
                if (add_data.ContainsKey(name)) //check if data already added
                {
                    Debug.WriteLine("Trying to add feature data to non empty dict feature data");
                } else 
                {
                    add_data.Add(name, data);
                }
                foreach(string ftName in GlobalState.FeatureNames)
                {
                    if (!add_data.ContainsKey(ftName)) //if not all features present then return
                        return;
                }
                //if all features present, then add to transformed_data
                transformed_data.Add(add_data);
            }
        }

        /// <summary>
        /// convert dictionary with list of double[] per feature 
        /// to a dictionary with a double_avg[] and double_std[] 
        /// per feature
        /// </summary>
        public EDMData GetDistributionModel()
        {
            EDMDataPoint[] dataPoints = new EDMDataPoint[length];
            for(int i = 0; i < length; i++) //init return array
            {
                dataPoints[i] = new EDMDataPoint(i);
            }

            foreach (string ft in GlobalState.FeatureNames)
            {
                //get avg and std for each ft
                
                //add to EDM function? where a single dict can be added in weighted manner to existing EDM
            }

            //go over key names OLD VERSION
            foreach (string ft in GlobalState.FeatureNames)
            {                
                List<double[]> data = transformed_data[ft];
                //get avg and std for ft
                double[] avg = new double[length];
                double[] std = new double[length];
                for (int i = 0; i < length; i++) //for each datapoint
                {
                    double avgSum = 0;
                    double[] samples = new double[data.Count];
                    for(int j = 0; j < data.Count; j++) //for each sample recording
                    {
                        Debug.WriteLine($"{j} {i}");
                        avgSum += data[j][i];
                        samples[j] = data[j][i];
                    }
                    avg[i] = avgSum / data.Count;
                    std[i] = computeSTD(avg[i], samples);

                    dataPoints[i].SetValue($"{ft}", avg[i]);
                    dataPoints[i].SetValue($"{ft}_std", std[i]);
                }
            }
            return new EDMData(dataPoints);
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
            //string ret = $"{length}\n";
            //foreach(KeyValuePair<string, List<double[]>> entry in transformed_data)
            //{
            //    ret += $"{entry.Key} size {entry.Value.Count}\n";
            //}
            //return ret;
            return "EDM TOSTRING() not implemented";
        }

    }

    [Serializable()]
    public class EDMData
    {
        public EDMDataPoint[] dataPoints;        
        public EDMData(EDMDataPoint[] dataPoints)
        {
            this.dataPoints = dataPoints;
        }

        
    }

    /// <summary>
    /// storing sample count, mean, M2 to use as part of Welford's algorithm
    /// for more info see: https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Compute_running_.28continuous.29_variance
    /// </summary>
    public class EDMDataPointFeatureValue
    {
        public string name;
        public int numberOfSamples;
        
        public double mean;
        public double M2;
        private double std;

        public EDMDataPointFeatureValue(string name, double mean, double m2, int nSamples=1)
        {
            this.name = name;
            this.mean = mean;
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
            double delta2 = newSample = mean;
            M2 += delta * delta2;
            if (numberOfSamples > 1)
                UpdateSTD();
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
