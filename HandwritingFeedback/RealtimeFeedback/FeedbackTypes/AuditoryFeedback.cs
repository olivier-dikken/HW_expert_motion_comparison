using System.Diagnostics;
using System.Windows.Input;
using HandwritingFeedback.RealtimeFeedback.InputSources;
using HandwritingFeedback.StylusPlugins.Renderers;
using HandwritingFeedback.Util;
using HandwritingFeedback.View;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace HandwritingFeedback.RealtimeFeedback.FeedbackTypes
{
    /// <summary>
    /// Singleton class that contains code to modify the color of wet ink strokes,
    /// based on different handwriting attributes (pressure, accuracy, etc.).
    /// </summary>
    public class AuditoryFeedback : RealtimeFeedback
    {
        private static AuditoryFeedback _instance;
        
        /// <summary>
        /// The sine wave provider to provide audio to the player.
        /// </summary>
        private static readonly SineWaveProvider SineProvider = new SineWaveProvider();

        /// <summary>
        /// The audio player. 
        /// </summary>
        public IWavePlayer Player { get; }

        /// <summary>
        /// Constructor for this component.
        /// </summary>
        private AuditoryFeedback()
        {
            try
            {
                // Initialize the player with an audio source.
                Player = new WaveOutEvent
                {
                    DesiredLatency = 100
                };
                Player.Init(new SampleToWaveProvider(SineProvider));
            }
            catch (MmException)
            {
                Player = null;
                Debug.WriteLine(
                    "Failed to obtain NAudio player." +
                    " This usually means that no suitable audio driver was found on the device.");
            }
        }
        
        /// <summary>
        /// Getter for the class instance, which makes this class a Singleton.
        /// </summary>
        /// <returns>The instance of the class</returns>
        public static AuditoryFeedback GetInstance()
        {
            _instance ??= new AuditoryFeedback();
            return _instance;
        }

        /// <summary>
        /// Collects feedback, processes, and produces sound. <br />
        /// Invoked when new input from Stylus/Touch/Mouse is received.
        /// </summary>
        public void Feedback(IWavePlayer player, StylusPointCollection stylusPoints)
        {
            // Time at which method was invoked
            // Each stylus point will use this timestamp as its estimate of when the point was 
            // drawn
            long startTime = GeneralDynamicRenderer.GetElapsedTimeInMilliseconds();
            
            // Iterate over all stylus points and adjust frequency
            // The frequency of the sound is affected by the attributes of the current stylus point
            foreach (var stylusPoint in stylusPoints)
            {
                var pt = stylusPoint.ToPoint();
                var input = new RTFInputData
                {
                    StylusPoint = stylusPoint, 
                    Timestamp = startTime
                };
                
                // Calculate the closest expert point for the current student point.
                double distance = PracticeMode.ExpertTraceUtils.GetClosestPoint(pt).distance;

                // If a closest expert point was found calculate feedback
                if (double.IsFinite(distance))
                {
                    // Gather feedback from the FeedbackComponents of selected InputSources in order to update the player or audio source.
                    foreach (InputSource source in InputSources)
                    {
                        source.CollectAuditory(player, SineProvider, input);
                    }
                }
                // Student is outside of scan diameter
                else 
                {
                    player?.Pause();
                }
            }
        }
    }
}