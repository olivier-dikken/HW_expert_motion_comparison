using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Ink;

namespace HandwritingFeedback.Models
{
    [Serializable]
    class AlignmentTestSample
    {
        public readonly StrokeCollection eStrokes;
        public readonly StrokeCollection sStrokes;
        public readonly List<(int, int)> alignmentVector;

        public AlignmentTestSample(StrokeCollection eS, StrokeCollection sS, List<(int, int)> aV)
        {
            eStrokes = eS;
            sStrokes = sS;
            alignmentVector = aV;
        }


        public static AlignmentTestSample LoadFromFile(string fileName)
        {
            //save relative to cwd
            fileName = "\\HWtests\\" + fileName; 
            AlignmentTestSample ats = null;
            StrokeCollection eStrokes = null;
            StrokeCollection sStrokes = null;
            List<(int, int)> aV = null;

            string eFileName = fileName + "_expert";
            string sFileName = fileName + "_student";
            string aVFileName = fileName + "_aV";

            var fs_e = new FileStream(eFileName,
                                               FileMode.Open);
            try
            {
                eStrokes = new StrokeCollection(fs_e);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Load file failed");
            }
            fs_e.Close();

            var fs_s = new FileStream(sFileName,
                                               FileMode.Open);
            try
            {
                sStrokes = new StrokeCollection(fs_s);
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Load file failed");
            }

            fs_s.Close();

            if (File.Exists(aVFileName))
            {
                Debug.WriteLine(String.Format("Reading file {0}", aVFileName));
                Stream openFileStream = File.OpenRead(aVFileName);
                BinaryFormatter deserializer = new BinaryFormatter();
                aV = (List<(int, int)>)deserializer.Deserialize(openFileStream);
                openFileStream.Close();                
            } else
            {
                Debug.WriteLine("File load aV not found");
            }
            ats = new AlignmentTestSample(eStrokes, sStrokes, aV);

            return ats;
        }

        public void SaveToFile(string fileName)
        {
            fileName = "\\HWtests\\" + fileName;
            //save strokes with stroke.save() and serialize the alignmentVector
            FileStream fs = new FileStream(fileName + "_expert",
                                               FileMode.Create);
            eStrokes.Save(fs);
            fs.Close();
            fs = new FileStream(fileName + "_student", FileMode.Create);
            sStrokes.Save(fs);
            fs.Close();

            Stream saveFileStream = File.Create(fileName + "_av");
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(saveFileStream, this.alignmentVector);
            saveFileStream.Close();
        }
    }
}
