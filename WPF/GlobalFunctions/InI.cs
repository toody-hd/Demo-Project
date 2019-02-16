using INIHandler;
using System.IO;

namespace WPF
{
    public class InI
    {
        public static INIFile settingsFile = new INIFile("settings.ini");
        public static INIFile panelsFile = new INIFile("panels.ini");
        public static INIFile filtersFile = new INIFile("filters.ini");
        public static INIFile scriptsFile = new INIFile("scripts.ini");

        public static bool SettingsFileExist()
        {
            if (File.Exists(@"settings.ini"))
                return true;
            else
                return false;
        }

        public static bool PanelsFileExist()
        {
            if (File.Exists(@"panels.ini"))
                return true;
            else
                return false;
        }

        public static bool FiltersFileExist()
        {
            if (File.Exists(@"filters.ini"))
                return true;
            else
                return false;
        }

        public static bool ScriptsFileExist()
        {
            if (File.Exists(@"scripts.ini"))
                return true;
            else
                return false;
        }
    }
}
