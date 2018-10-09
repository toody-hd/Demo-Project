namespace WPF
{
    static class Settings
    {
        public static bool _miniMode = false;
        public static bool _topmostMode = false;
        public static string _lang = "en-US";
        public static bool _messageMode = true;
        public static string[] _excUpd = new string[] { };

        public static bool miniMode = false;
        public static bool topmostMode = false;
        public static string lang = "en-US";
        public static bool messageMode = true;
        public static string[] excUpd = new string[] { };

        //public static CustomVar[] excList = new CustomVar[] { };

        //public static bool _miniMode { get; set; }
        //public static bool miniMode { get; set; }
        //private static string _lang = "en-US";
        //public static string lang { get { return _lang; } set { _lang = value; } }

        public static void Reset()
        {
            _miniMode = miniMode = false;
            _topmostMode = topmostMode = false;
            _lang = lang = "en-US";
            _messageMode = messageMode = true;
        }

        /*
        public class CustomVar
        {
            public string Name { get; set; }
            public bool Value { get; set; }

            public CustomVar(string _name ,bool _value)
            {
                Name = _name;
                Value = _value;
            }
        }
        */
    }
}