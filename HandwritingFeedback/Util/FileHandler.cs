using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HandwritingFeedback.Config;
using HandwritingFeedback.Models;
using HandwritingFeedback.StylusPlugins.Strokes;
using Microsoft.Win32;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HandwritingFeedback.Util
{
    /// <summary>
    /// Contains methods for saving and loading files. <br />
    /// </summary>
    public static class FileHandler
    {
        /// <summary>
        /// Provided by the Microsoft Ink API. Saves the contents of a given canvas as .ISF file to disk.
        /// </summary>
        /// <param name="sender">The XAML object that called this method</param>
        /// <param name="e">Event Arguments</param>
        /// <param name="inkCanvas">The canvas to extract stroke data from</param>
        /// <returns>True if the file was saved successfully, false otherwise</returns>
        public static bool ButtonSaveAsClick(object sender, RoutedEventArgs e, InkCanvas inkCanvas)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "ISF File (*.isf)|*.isf",
                FileName = "InkSample"
            };

            if (saveFileDialog1.ShowDialog() == true)
            {
                FileStream fs = new FileStream(saveFileDialog1.FileName,
                                               FileMode.Create);
                inkCanvas.Strokes.Save(fs);
                fs.Close();
                return true;
            }

            return false;
        }

        public static bool SaveTargetTrace(StrokeCollection strokesToSave, string path)
        {
            //save as TargetTrace.isf in the path folder
            try
            {
                FileStream fs = new FileStream(path + "\\TargetTrace.isf", FileMode.Create);
                strokesToSave.Save(fs);
                fs.Close();
                return true;
            }
            catch { }

            return false;
        }

        public static bool SaveCandidateTrace(StrokeCollection strokesToSave, string path)
        {
            //save as TargetTrace.isf in the path folder
            try
            {
                FileStream fs = new FileStream(path + "\\CandidateTrace.isf", FileMode.Create);
                strokesToSave.Save(fs);
                fs.Close();
                return true;
            }
            catch { }

            return false;
        }

        public static void ButtonSaveAsImageClick(object sender, RoutedEventArgs e, Grid fullPage)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = "Images|*.png;*.bmp;*.jpg",
                FileName = "HandwritingSubmission"
            };

            if (saveFileDialog1.ShowDialog() == true)
            {
                FileStream fs = new FileStream(saveFileDialog1.FileName,
                    FileMode.Create);
                // Render whole page to bitmap
                RenderTargetBitmap renderBitmap =
                    new RenderTargetBitmap((int)fullPage.ActualWidth, (int)fullPage.ActualHeight, 96d, 96d, PixelFormats.Default);
                renderBitmap.Render(fullPage);
                // Save to a memory stream
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(fs);
                fs.Close();
            }
        }


        public static void SaveCanvasAsImage(InkCanvas theCanvas, string exerciseFolder, string fileName)
        {
            //theCanvas.Background = new SolidColorBrush(Colors.White);
            //RenderTargetBitmap rtb = new RenderTargetBitmap((int)theCanvas.ActualWidth, (int)theCanvas.ActualHeight, 96d, 96d, PixelFormats.Default);
            //rtb.Render(theCanvas);

            //FileStream fs = File.Open(saveFile, FileMode.Create);//save bitmap to file
            //BitmapEncoder encoder = new BmpBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(rtb));
            //encoder.Save(fs);
            //fs.Close();

            string saveFile = exerciseFolder + "/" + fileName + ".png";

            Rect strokeBounds = theCanvas.Strokes.GetBounds();


            //create new grid
            //create new canvas, add to grid
            Grid canvasGrid = new Grid();
            canvasGrid.Width = strokeBounds.Width;
            canvasGrid.Height = strokeBounds.Height;
            RowDefinition rd = new RowDefinition();
            ColumnDefinition cd = new ColumnDefinition();
            rd.Height = GridLength.Auto;
            cd.Width = GridLength.Auto;
            canvasGrid.RowDefinitions.Add(rd);
            canvasGrid.ColumnDefinitions.Add(cd);
            canvasGrid.Background = new SolidColorBrush(Colors.White);

            InkCanvas croppedCanvas = new InkCanvas();
            croppedCanvas.Background = new SolidColorBrush(Colors.White);

            //offset the strokecollection
            StrokeCollection strokesAdjustedToOrigin = TraceUtils.OffsetStrokeCollection(theCanvas.Strokes, -(int)strokeBounds.X, -(int)strokeBounds.Y);


            croppedCanvas.Strokes.Add(strokesAdjustedToOrigin);
            croppedCanvas.Width = strokeBounds.Width;
            croppedCanvas.Height = strokeBounds.Height;
            Grid.SetRow(croppedCanvas, 0);
            Grid.SetColumn(croppedCanvas, 0);

            canvasGrid.Children.Add(croppedCanvas);


            RenderTargetBitmap rtb = new RenderTargetBitmap((int)strokeBounds.Width, (int)strokeBounds.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvasGrid);



            FileStream fs = File.Open(saveFile, FileMode.Create);//save bitmap to file
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(fs);
            fs.Close();



            //draw all strokes on canvas
            //save grid...
        }


        /// <summary>
        /// Provided by the Microsoft Ink API. Loads a .ISF file into a provided canvas.
        /// </summary>
        /// <param name="sender">The XAML object that called this method</param>
        /// <param name="e">Event Arguments</param>
        /// <param name="inkCanvas">The canvas to load stroke data into</param>
        public static bool ButtonLoadClick(object sender, RoutedEventArgs e, InkCanvas inkCanvas)
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ISF File (*.isf)|*.isf";

            // Return false if user cancels loading of file
            if (openFileDialog1.ShowDialog() == false) return false;

            var fs = new FileStream(openFileDialog1.FileName,
                                               FileMode.Open);

            StrokeCollection fetchedStrokes;

            // If selected file is wrong .ISF type or invalid, do nothing
            try
            {
                // Fetch all strokes from selected file-stream
                fetchedStrokes = new StrokeCollection(fs);
            }
            catch (ArgumentException)
            {
                return false;
            }

            // Parse fetched stroke to extract saved properties per stroke
            ParseStrokeCollection(fetchedStrokes);

            // Add the strokes to the ink canvas
            inkCanvas.Strokes = fetchedStrokes;

            fs.Close();
            return true;
        }

        public static StrokeCollection LoadStrokeCollection(string pathToFile)
        {
            var fs = new FileStream(pathToFile,
                 FileMode.Open);

            StrokeCollection fetchedStrokes;

            // If selected file is wrong .ISF type or invalid, do nothing
            try
            {
                // Fetch all strokes from selected file-stream
                fetchedStrokes = new StrokeCollection(fs);
            }
            catch (ArgumentException)
            {
                return null;
            }

            // Parse fetched stroke to extract saved properties per stroke
            ParseStrokeCollection(fetchedStrokes);

            fs.Close();

            return fetchedStrokes;
        }

        public static void LoadTrace(string pathToTrace, InkCanvas inkCanvas)
        {
            var fs = new FileStream(pathToTrace,
                 FileMode.Open);

            StrokeCollection fetchedStrokes;

            // If selected file is wrong .ISF type or invalid, do nothing
            try
            {
                // Fetch all strokes from selected file-stream
                fetchedStrokes = new StrokeCollection(fs);
            }
            catch (ArgumentException)
            {
                return;
            }

            // Parse fetched stroke to extract saved properties per stroke
            ParseStrokeCollection(fetchedStrokes);

            // Add the strokes to the ink canvas
            inkCanvas.Strokes.Add(fetchedStrokes);

            fs.Close();
        }

        /// <summary>
        /// Extracts properties from strokes collected from file stream, in order to save
        /// extracted properties per stroke.
        /// </summary>
        /// <param name="fetchedStrokes">Stroke collection fetched from file-stream to parse</param>
        public static void ParseStrokeCollection(StrokeCollection fetchedStrokes)
        {
            // Iterate through all strokes in parallel and parse stroke properties in order to accurately
            // represent expert model prior to saving
            Parallel.For(0, fetchedStrokes.Count, i =>
            {
                Stroke currStroke = fetchedStrokes[i];

                // Construct a new general stroke using same stylus points as the current stroke in order to represent
                // expert model accurately as all expert strokes created in the application were originally general
                // strokes
                // New stroke will be identical to the current stroke
                var newStroke = new GeneralStroke(currStroke.StylusPoints);

                // Execute conditional only if current stroke contains property identifying a temporal cache
                // This avoids exceptions for out-dated .ISF files which were created via external software or
                // this application in a version before temporal properties were implemented
                if (currStroke.ContainsPropertyData(StrokePropertyIds.TemporalCache))
                {
                    // De-serialize the json representation of the temporal cache
                    var currTemporalCache = JsonConvert.DeserializeObject<(StylusPointCollection
                        , long)[]>((string)currStroke.GetPropertyData(StrokePropertyIds.TemporalCache));

                    // Set the new strokes temporal cache snapshot to the retrieved cache snapshot
                    newStroke.TemporalCacheSnapshot = currTemporalCache;
                }

                // Determines the color of the expert's outline
                var drawingAttributes = new DrawingAttributes
                {
                    Color = Color.FromRgb(240, 240, 240)
                };
                newStroke.DrawingAttributes = drawingAttributes;

                // Replace stroke in collection with newly instantiated general stroke
                fetchedStrokes[i] = newStroke;
            });
        }

        public static ExerciseItem GetExericseItem(string path)
        {
            if (File.Exists(path + "/exerciseConfig.txt"))
            {
                return ExerciseItem.FromExerciseConfigFile(path);
            }
            return null;
        }

        public static ObservableCollection<ExerciseItem> GetExerciseItems()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string exerciseFolderPath = Directory.GetParent(workingDirectory).Parent.FullName + "\\SavedData\\Exercises\\";
            ObservableCollection<ExerciseItem> exerciseItems = new ObservableCollection<ExerciseItem>();
            try
            {

                string[] dirs = Directory.GetDirectories(exerciseFolderPath, "*", SearchOption.TopDirectoryOnly);
                Debug.WriteLine("The number of directories matched is {0}.", dirs.Length);
                foreach (string dir in dirs)
                {
                    string fileName = System.IO.Path.GetFileName(dir);
                    Debug.WriteLine(fileName);
                    Debug.WriteLine(dir);
                    if (File.Exists(dir + "/exerciseConfig.txt"))
                    {
                        exerciseItems.Add(ExerciseItem.FromExerciseConfigFile(dir));
                    }
                    else
                    {
                        Debug.WriteLine("Exercise config not found in folder: " + fileName);
                    }

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The process failed: {0}", e.ToString());
            }
            return exerciseItems;
        }

        public static async Task WriteConfigTargetTraceView_Async(int lineType, int lineSpacing, string dir)
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
            //11=repititionsAmount

            string[] lines =
            {
                "",
                "",
                DateTime.Now.ToString(),
                "0",
                "0",
                 "0",
                lineType.ToString(),
                lineSpacing.ToString(),
                "0"
            };

            await File.WriteAllLinesAsync(dir + "\\exerciseConfig.txt", lines);
        }


        public static async Task UpdateConfigInfoView_Async(string title, string desc, int starRating, ObservableCollection<FeedbackSelectionGridItem> featureGridData, string dir, int repAmount)
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


            var jsonString = JsonSerializer.Serialize(featureGridData);


            string[] lines = System.IO.File.ReadAllLines(dir + "/exerciseConfig.txt");
            lines[0] = title;
            lines[1] = desc.Replace("\n", "").Replace("\r", "");
            List<string> allLines = lines.Append(jsonString).ToList();
            allLines.Add(starRating.ToString());
            allLines.Add(repAmount.ToString());
            await File.WriteAllLinesAsync(dir + "\\exerciseConfig.txt", allLines);
        }
    }
}
