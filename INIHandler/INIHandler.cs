using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace INIHandler
{
    public class INIFile
    {
        #region Variables
        private readonly string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        private static int capacity = 255;
        #endregion

        #region DLL Importing
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string def,
            StringBuilder retVal,
            int size,
            string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string defaultValue,
            [In, Out] char[] value,
            int size,
            string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileSection(
            string section, 
            IntPtr keyValue,
            int size, 
            string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long WritePrivateProfileString(
            string section,
            string key,
            string val,
            string filePath);
        #endregion

        #region Geting the file path
        public INIFile(string filePath = null)
        {
            FilePath = new FileInfo(filePath ?? EXE + ".ini").FullName.ToString();
        }
        #endregion

        #region Write Function
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, FilePath);
        }
        #endregion

        #region Read Functions
        public string Read(string section, string key)
        {
            StringBuilder SB = new StringBuilder(capacity);
            GetPrivateProfileString(section, key, "", SB, SB.Capacity, FilePath);
            return SB.ToString();
        }

        public string[] ReadSections()
        {
            // first line will not recognize if ini file is saved in UTF-8 with BOM 
            while (true)
            {
                char[] chars = new char[capacity];
                int size = GetPrivateProfileString(null, null, "", chars, capacity, FilePath);

                if (size == 0)
                {
                    return null;
                }

                if (size < capacity - 2)
                {
                    string result = new string(chars, 0, size);
                    string[] sections = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    return sections;
                }

                capacity = capacity * 2;
            }
        }

        public string[] ReadKeys(string section)
        {
            // first line will not recognize if ini file is saved in UTF-8 with BOM 
            while (true)
            {
                char[] chars = new char[capacity];
                int size = GetPrivateProfileString(section, null, "", chars, capacity, FilePath);

                if (size == 0)
                {
                    return null;
                }

                if (size < capacity - 2)
                {
                    string result = new string(chars, 0, size);
                    string[] keys = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    return keys;
                }

                capacity = capacity * 2;
            }
        }

        public string[] ReadKeyValuePairs(string section)
        {
            while (true)
            {
                IntPtr returnedString = Marshal.AllocCoTaskMem(capacity * sizeof(char));
                int size = GetPrivateProfileSection(section, returnedString, capacity, FilePath);

                if (size == 0)
                {
                    Marshal.FreeCoTaskMem(returnedString);
                    return null;
                }

                if (size < capacity - 2)
                {
                    string result = Marshal.PtrToStringAuto(returnedString, size - 1);
                    Marshal.FreeCoTaskMem(returnedString);
                    string[] keyValuePairs = result.Split('\0');
                    return keyValuePairs;
                }

                Marshal.FreeCoTaskMem(returnedString);
                capacity = capacity * 2;
            }
        }
        #endregion

        #region Delete Functions
        public void DeleteKey(string section, string key)
        {
            Write(section, key, null);
        }

        public void DeleteSection(string section)
        {
            Write(section, null, null);
        }
        #endregion

        #region Cheking Functions
        public bool KeyExists(string section, string key)
        {
            return Read(section, key).Length > 0;
        }

        public bool SectionExists(string section)
        {
            return Read(section, null).Length > 0;
        }
        #endregion

        #region Reorder Functions

        public void RenameSection(string oldSection, string newSection)
        {
            File.WriteAllText(FilePath, File.ReadAllText(FilePath).Replace("[" + oldSection + "]", "[" + newSection + "]"));
        }

        public void RenameKey(string section, string oldKey, string newKey)
        {
            var _key = "";
            foreach (var x in ReadKeys(section))
            {
                if (x == oldKey)
                {
                    _key = Read(section, x);
                    DeleteKey(section, x);
                    Write(section, newKey, _key);
                }
                else
                {
                    _key = Read(section, x);
                    DeleteKey(section, x);
                    Write(section, x, _key);
                }
            }
            _key = null;
        }

        public void InterchangeSections(string firstSection, string secondSection)
        {
            var _firstSection = "";
            foreach (var x in ReadKeys(secondSection))
            {
                _firstSection = Read(firstSection, x);
                Write(firstSection, x, Read(secondSection, x));
                Write(secondSection, x, _firstSection);
            }
            RenameSection(firstSection, secondSection + "_");
            RenameSection(secondSection, firstSection);
            RenameSection(secondSection + "_", secondSection);
            _firstSection = null;
        }
        #endregion

        public string FilePath { get; set; }
    }
}