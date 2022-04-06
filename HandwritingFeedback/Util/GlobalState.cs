using System;
using System.Collections.Generic;
using System.Text;

namespace HandwritingFeedback.Util
{
    static class GlobalState
    {
        public static int SelectedExercise = 0;
        public static string SelectedExercisePath = "";
        public static string CreateContentsPreviousFolder = "";
        public static string TestAlignmentPath = "C:\\Users\\olivi\\HW_expert_motion_comparison\\HandwritingFeedback\\bin\\SavedData\\TestAlignment\\";
        public static Dictionary<int, string> exercises =
            new Dictionary<int, string>()
            {
                { 0, "Trace 1"},
                { 1, "Trace 2"},
                { 2, "Trace 3"},
                { 3, "Trace 4"},
                { 4, "Copy 1"},
                { 5, "Copy 2"},
                { 6, "Copy 3"},
                { 7, "Copy 4"},
                { 8, "Copy 5"},
                { 9, "Copy 6"}
            };

        public static Dictionary<int, int> exercisesToHelperLineType =
            new Dictionary<int, int>()
            {
                { 0, 0},
                { 1, 3},
                { 2, 3},
                { 3, 3},
                { 4, 3},
                { 5, 2},
                { 6, 2},
                { 7, 1},
                { 8, 2},
                { 9, 1}
            };

        public enum WritingScale
        {
            S = 0,
            M = 1,
            L = 2,
            XL = 3
        };

        public static Dictionary<int, WritingScale> exercisesToCharacterSize =
            new Dictionary<int, WritingScale>()
            {
                { 0, WritingScale.XL},
                { 1, WritingScale.XL},
                { 2, WritingScale.L},
                { 3, WritingScale.M},
                { 4, WritingScale.L},
                { 5, WritingScale.L},
                { 6, WritingScale.M},
                { 7, WritingScale.M},
                { 8, WritingScale.M},
                { 9, WritingScale.M}
            };

        public static string[] FeatureNames = new string[2] { "X", "Y" };
    }
}
