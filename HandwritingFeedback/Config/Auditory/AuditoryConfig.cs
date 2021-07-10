namespace HandwritingFeedback.Config.Auditory
{
    /// <summary>
    /// Contains fields for each auditory real-time feedback attribute.<br />
    /// A AuditoryConfig instance by default does not have any feedback component configs <br /> associated with it,
    /// but they can manually be instantiated and their properties modified.
    /// </summary>
    public class AuditoryConfig
    {
        #region Feedback Component Configs

        public PressureConfig PressureConfig;
        
        public SpeedConfig SpeedConfig;

        #endregion
    }
}