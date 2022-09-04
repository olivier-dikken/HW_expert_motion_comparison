using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace HandwritingFeedback.Models
{
    public class ExerciseItem
    {
        public string path;
        public string title;
        public string description;
        public DateTime creationDate;
        public int attempts;
        public int bestScore;
        public BitmapImage targetTraceImage;

        public ExerciseItem(string path, string title, string description, DateTime creationDate, int attempts, int bestScore, BitmapImage targetTraceImage)
        {
            this.path = path;
            this.title = title;
            this.description = description;
            this.creationDate = creationDate;
            this.attempts = attempts;
            this.bestScore = bestScore;
            this.targetTraceImage = targetTraceImage;
        }

        public string? Title
        {
            get => title;
        }

        public string? Description
        {
            get => description;
        }

        public DateTime CreationDate
        {
            get => creationDate;
        }

        public int Attempts
        {
            get => attempts;
        }

        public int BestScore
        {
            get => bestScore;
        }

        public static ExerciseItem FromExerciseConfigFile(string exerciseFolderPath)
        {
            //0=title
            //1=description
            //2=creationDate
            //3=attempts
            //4=bestScore
            
            ExerciseItem item = null;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(exerciseFolderPath + "/exerciseConfig.txt");
                string title = Path.GetFileName(exerciseFolderPath);
                string description = lines[1];
                DateTime creationDate = DateTime.Parse(lines[2]);
                int attempts = int.Parse(lines[3]);
                int bestScore = int.Parse(lines[4]);                

                BitmapImage image = new BitmapImage(new Uri(exerciseFolderPath + "/targetTrace.png", UriKind.Absolute));

                item = new ExerciseItem(exerciseFolderPath, title, description, creationDate, attempts, bestScore, image);
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to parse '{0}'", exerciseFolderPath);
            }
            return item;
        }
    }
}
