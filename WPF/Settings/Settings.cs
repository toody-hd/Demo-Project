using System;
using System.Collections.Generic;

namespace WPF
{
    static class Settings
    {
        #region Variables
        public static bool _miniMode = false;
        public static bool _topmostMode = false;
        public static string _lang = "en-US";
        public static bool _messageMode = true;
        public static List<string[]> _filters = new List<string[]>();
        public static List<string[]> _scripts = new List<string[]>();

        public static bool miniMode = false;
        public static bool topmostMode = false;
        public static string lang = "en-US";
        public static bool messageMode = true;
        public static List<string[]> filters = new List<string[]>();
        public static List<string[]> scripts = new List<string[]>();
        #endregion

        public static void Reset()
        {
            _miniMode = miniMode = false;
            _topmostMode = topmostMode = false;
            _lang = lang = "en-US";
            _messageMode = messageMode = true;
        }

        public static void ResetData()
        {
            _filters = filters = new List<string[]>();
            _scripts = scripts = new List<string[]>();
        }
    }
}