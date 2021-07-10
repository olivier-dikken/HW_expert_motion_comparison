using System.Diagnostics;

namespace HandwritingFeedback.StylusPlugins.Wrappers
{
    /// <summary>
    /// A wrapper for the <see cref="Stopwatch"/> class. <br />
    /// Enables testing of methods that utilize a stopwatch.
    /// </summary>
    public class StopwatchWrapper
    {
        /// <summary>
        /// The <see cref="Stopwatch"/> being wrapped.
        /// </summary>
        public Stopwatch WrappedInstance;

        /// <summary>
        /// Calls the actual <see cref="Start"/> method of the <see cref="WrappedInstance"/>.
        /// </summary>
        public virtual void Start()
        {
            WrappedInstance.Start();
        }

        /// <summary>
        /// Gets the actual <see cref="Stopwatch.ElapsedMilliseconds"/> attribute of the <see cref="WrappedInstance"/>.
        /// </summary>
        public virtual long GetElapsedMilliseconds()
        {
            return WrappedInstance.ElapsedMilliseconds;
        }

        /// <summary>
        /// Calls the actual <see cref="Reset"/> method of the <see cref="WrappedInstance"/>.
        /// </summary>
        public void Reset()
        {
            WrappedInstance.Reset();
        }
    }
}