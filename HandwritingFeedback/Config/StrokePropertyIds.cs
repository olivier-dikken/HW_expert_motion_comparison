using System;

namespace HandwritingFeedback.Config
{
    /// <summary>
    /// Contains all GUID identifiers for custom stroke properties.
    /// </summary>
    public static class StrokePropertyIds
    {
        /// <summary>
        /// Identifies json serialized string representing the temporal cache <br />
        /// snapshot of a given stroke, used to calculate feedback such as stroke speed. <br />
        /// Chosen GUID id was provided as an example by Microsoft and is therefore guaranteed to be unused.
        /// </summary>
        public static Guid TemporalCache = new Guid("03457307-3475-3450-3035-640435034540");
    }
}
