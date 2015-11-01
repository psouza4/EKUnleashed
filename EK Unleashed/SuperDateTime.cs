using System;

namespace EKUnleashed
{
    class SuperDateTime
    {
        private SuperDateTime() { } // CA1053, http://msdn2.microsoft.com/library/ms182169(VS.90).aspx

        public static DateTime Parse(string sDateToParse)
        {
            return SuperDateTime.Parse(sDateToParse, DateTime.MinValue);
        }

        public static DateTime Parse(string sDateToParse, DateTime dtDefaultValue)
        {
            try
            {
                sDateToParse = sDateToParse.Trim();

                string sYearPrefix = (((int)(DateTime.Now.Year / 100)) * 100).ToString(); // convert current year prefix (to help out later with two-digit years)

                if (Utils.CDbl(sDateToParse) > 0)
                {
                    if (sDateToParse.Length == 6)
                    {
                        if ((Utils.CDbl(sDateToParse) % 100) > 12)
                        {
                            if (Utils.CInt(sYearPrefix) + (Utils.CDbl(sDateToParse) % 100) > DateTime.Now.Year)
                                sYearPrefix = (Utils.CInt(sYearPrefix) - 100).ToString();
                            sDateToParse = sDateToParse.Substring(0, 2) + "/" + sDateToParse.Substring(2, 2) + "/" + sYearPrefix.Substring(0, 2) + sDateToParse.Substring(4, 2);
                        }
                        else
                            sDateToParse = sDateToParse.Substring(2, 2) + "/" + sDateToParse.Substring(4, 2) + "/" + sDateToParse.Substring(0, 2);
                    }
                    else if (sDateToParse.Length == 8)
                    {
                        for (int iYear = 1800; iYear <= 2300; iYear++)
                        {
                            if (sDateToParse.StartsWith(iYear.ToString()))
                            {
                                sDateToParse = sDateToParse.Substring(0, 4) + "/" + sDateToParse.Substring(4, 2) + "/" + sDateToParse.Substring(6, 2);
                                break;
                            }
                            if (sDateToParse.EndsWith(iYear.ToString()))
                            {
                                sDateToParse = sDateToParse.Substring(4, 4) + "/" + sDateToParse.Substring(0, 2) + "/" + sDateToParse.Substring(2, 2);
                                break;
                            }
                        }
                    }
                }

                return DateTime.Parse(sDateToParse);
            }
            catch { }

            return dtDefaultValue;
        }
    }
}
