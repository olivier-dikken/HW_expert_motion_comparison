using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace HandwritingFeedback.Models
{
    public class FeedbackSelectionGridItem
    {
        public string feedbackName;
        public PriorityGroup priority = PriorityGroup.Normal;
        private bool _isSelected = true;

        public FeedbackSelectionGridItem(string feedbackName)
        {
            this.feedbackName = feedbackName;
        }

        public void SetPriority(PriorityGroup newPriority)
        {
            priority = newPriority;
        }

        public bool Enabled
        {
            get => _isSelected;
            set => _isSelected = value;
        }

        public string? FeedbackName
        {
            get => feedbackName;
        }
        
        public PriorityGroup PriorityGroup
        {
            get=> priority;
            set => priority = value;
        }        
    }

    public enum PriorityGroup
    {
        [Display(Name = "Very Low")]
        VeryLow = 0,
        Low = 1,
        Normal = 2,
        High = 3,
        [Display(Name = "Very High")]
        VeryHigh = 4,
    }
}
