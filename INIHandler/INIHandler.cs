using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace INIHandler
{
    public class INIFile
    {
        private string filePath;
        private string EXE = Assembly.GetExecutingAssembly().GetName().Name;


        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
        string key,
        string val,
        string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

        public INIFile(string filePath = null)
        {
            //this.filePath = filePath;
            this.filePath = new FileInfo(filePath ?? EXE + ".ini").FullName.ToString();
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.filePath);
        }

        public string Read(string section, string key)
        {
            StringBuilder SB = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", SB, 255, this.filePath);
            return SB.ToString();
        }

        public void DeleteKey(string section, string key)
        {
            Write(section, key, null);
        }

        public void DeleteSection(string section)
        {
            Write(section, null, null);
        }

        public bool KeyExists(string section, string key)
        {
            return Read(section, key).Length > 0;
        }

        public bool SectionExists(string section)
        {
            return Read(section, null).Length > 0;
        }

        public string ReadSection(string section)
        {
            StringBuilder SB = new StringBuilder(255);
            GetPrivateProfileString(section, null, null, SB, 255, this.filePath);
            return section.ToString();
        }

        public void EditSection(string section, string newSection)
        {
            ReadSection(section);
            WritePrivateProfileString(newSection, "", "", this.filePath);
        }

        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
    }
}