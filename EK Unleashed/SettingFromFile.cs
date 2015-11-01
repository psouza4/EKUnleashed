using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EKUnleashed
{
    public class SettingFromFile
    {
        private SettingFromFile() { }

        public static string GetData(string file, string section, string key)
        {
            try
            {
                string[] FileContents = File.ReadAllLines(Utils.AppFolder + "\\" + file);

                string current_section = "";

                for (int i = 0; i < FileContents.Length; i++)
                {
                    try
                    {
                        string current_line = FileContents[i].Trim();

                        if (current_line.StartsWith(";")) continue;
                        if (current_line.StartsWith("#")) continue;

                        if (current_line.Contains(";")) current_line = Utils.ChopperBlank(current_line, null, ";");
                        if (current_line.Contains("#")) current_line = Utils.ChopperBlank(current_line, null, "#");

                        if (current_line.StartsWith("[") && current_line.EndsWith("]"))
                        {
                            current_section = Utils.ChopperBlank(current_line, "[", "]").Trim();
                            continue;
                        }

                        if (current_section.ToLower() == section.Trim().ToLower())
                        {
                            if (current_line.Contains("="))
                            {
                                string Key = Utils.ChopperBlank(current_line, null, "=").Trim().Trim(new char[] { '\t' });
                                string Value = Utils.ChopperBlank(current_line, "=", null).Trim().Trim(new char[] { '\t' });

                                if (Value.StartsWith("\"") && Value.EndsWith("\""))
                                    Value = Utils.ChopperBlank(Value, "\"", "\"");

                                if (Key.ToLower() == key.ToLower())
                                    return Value;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return string.Empty;
        }

        public static bool True(string file, string section, string key)
        {
            try
            {
                string v = GetData(file, section, key);

                if (v.ToLower().Trim().StartsWith("f")) return false;
                if (v.ToLower().Trim().StartsWith("n")) return false;
                if (v.ToLower().Trim() == "0") return false;
                if (v.ToLower().Trim() == "off") return false;
            }
            catch { }

            return true;
        }

        public static bool False(string file, string section, string key)
        {
            try
            {
                string v = GetData(file, section, key);

                if (v.ToLower().Trim().StartsWith("t")) return true;
                if (v.ToLower().Trim().StartsWith("y")) return true;
                if (v.ToLower().Trim() == "1") return true;
                if (v.ToLower().Trim() == "on") return true;
            }
            catch { }

            return false;
        }
    }
}
