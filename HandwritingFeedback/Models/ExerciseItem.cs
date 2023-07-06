using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Ink;
using HandwritingFeedback.Util;

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
        public StrokeCollection targetTrace;
        public int lineType;
        public int lineSpacing;

        public List<FeedbackSelectionGridItem> fbItems;
        public int starRating;
        public int repetitionAmount;

        public ExerciseItem(string path, string title, string description, DateTime creationDate, int attempts, int bestScore, StrokeCollection targetTrace, BitmapImage targetTraceImage, int lineType, int lineSpacing, List<FeedbackSelectionGridItem> fbGridItems, int stars, int repititionAmount)
        {
            this.path = path;
            this.title = title;
            this.description = description;
            this.creationDate = creationDate;
            this.attempts = attempts;
            this.bestScore = bestScore;
            this.targetTraceImage = targetTraceImage;
            this.targetTrace = targetTrace;
            this.fbItems = fbGridItems;
            this.starRating = stars;
            this.lineType = lineType;
            this.lineSpacing = lineSpacing;
            this.repetitionAmount = repititionAmount;
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

        public string Path
        {
            get => path;
        }

        public List<FeedbackSelectionGridItem> FeedbackItems
        {
            get => fbItems;
        }

        public static ExerciseItem FromExerciseConfigFile(string exerciseFolderPath)
        {
            //0=title
            //1=description
            //2=creationDate
            //3=attempts
            //4=bestScore
            //5=type
            //6=linetype
            //7=linespacing
            //8=startingpoint
            //9=json
            //10=starrating
            //11=repititionAmount

            ExerciseItem item = null;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(exerciseFolderPath + "/exerciseConfig.txt");
                string title = lines[0];
                string description = lines[1];
                DateTime creationDate = DateTime.Parse(lines[2]);
                int attempts = int.Parse(lines[3]);
                int bestScore = int.Parse(lines[4]);
                int lineType = int.Parse(lines[6]);
                int lineSpacing = int.Parse(lines[7]);
                int starRating = int.Parse(lines[10]);
                int repititionAmount = int.Parse(lines[11]);

                BitmapImage image = new BitmapImage(new Uri(exerciseFolderPath + "/TargetTrace.png", UriKind.Absolute));
                StrokeCollection tt = FileHandler.LoadStrokeCollection(exerciseFolderPath + "\\TargetTrace.isf");

                List<FeedbackSelectionGridItem> fbItems = JsonConvert.DeserializeObject<List<FeedbackSelectionGridItem>>(lines[9]);

                item = new ExerciseItem(exerciseFolderPath, title, description, creationDate, attempts, bestScore, tt, image,lineType, lineSpacing, fbItems, starRating, repititionAmount);
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to parse '{0}'", exerciseFolderPath);
            }
            return item;
        }
    }
}
