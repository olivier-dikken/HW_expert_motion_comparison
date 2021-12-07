using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HandwritingFeedback.Config;
using HandwritingFeedback.StylusPlugins.Strokes;
using Microsoft.Win32;
using Newtonsoft.Json;

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
            Parallel.For(0, fetchedStrokes.Count, i => {
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
                        , long)[]>((string) currStroke.GetPropertyData(StrokePropertyIds.TemporalCache));

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
    }
}
