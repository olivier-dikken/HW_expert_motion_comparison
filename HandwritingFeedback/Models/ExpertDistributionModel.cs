using HandwritingFeedback.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace HandwritingFeedback.Models
{
    [Serializable()]
    public class ExpertDistributionModel
    {
        public Dictionary<string, List<double[]>> transformed_data { get; set; } 

        public int length { get; set; } = -1;


        public ExpertDistributionModel(int targetLength)
        {
            length = targetLength;
            transformed_data = new Dictionary<string, List<double[]>>();
        }

        [JsonConstructor]
        public ExpertDistributionModel(Dictionary<string, List<double[]>> transformed_data, int length)
        {
            this.transformed_data = transformed_data;
            this.length = length;
        }

        public int SampleCount()
        {
            return transformed_data.Count;
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
        public EDMData GetDistributionModel()
        {
            EDMDataPoint[] dataPoints = new EDMDataPoint[length];
            for(int i = 0; i < length; i++) //init return array
            {
                dataPoints[i] = new EDMDataPoint(i);
            }
            //go over key names
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
            string ret = $"{length}\n";
            foreach(KeyValuePair<string, List<double[]>> entry in transformed_data)
            {
                ret += $"{entry.Key} size {entry.Value.Count}\n";
            }
            return ret;
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
