using HandwritingFeedback.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace HandwritingFeedback.Models
{
    class ExpertDistributionModel
    {
        Dictionary<string, List<double[]>> transformed_data = new Dictionary<string, List<double[]>>();

        int length = -1;

        int numberOfSamplesPerFeatures = -1;

        public ExpertDistributionModel(int targetLength)
        {
            length = targetLength;
        }

        public ExpertDistributionModel(string pathToFile)
        {
            //TODO load EDM from file (serialized)
        }

        public void SetNumberOfSamplesPerFeature(int num)
        {
            numberOfSamplesPerFeatures = num;
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
        int index;
        double X;
        double X_std;
        double Y;
        double Y_std;
        double pressure;
        double speed;

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

                case "pressure":
                    Debug.WriteLine($"feature not yet implemented in EDM.SetValue() : {name}");
                    break;

                case "pressure_std":
                    Debug.WriteLine($"feature not yet implemented in EDM.SetValue() : {name}");
                    break;

                case "speed":
                    Debug.WriteLine($"feature not yet implemented in EDM.SetValue() : {name}");
                    break;

                case "speed_std":
                    Debug.WriteLine($"feature not yet implemented in EDM.SetValue() : {name}");
                    break;

                default:
                    Debug.WriteLine($"Wrong feature name: {name}");
                    break;
            }
        }
    }
}
