using System;
using System.Diagnostics;
using System.Reflection;

namespace EKUnleashed
{
    public partial class Errors
    {
        private Errors() { } // CA1053, http://msdn2.microsoft.com/library/ms182169(VS.90).aspx

        public static string GetShortErrorDetails(Exception e)
        {
            if (e == null)
                return "No exception";

            try
            {
                return e.GetType().ToString() + ": " + e.Message;
            }
            catch
            {
                return e.GetType().ToString();
            }
        }

        public static string GetAllErrorDetails(Exception e)
        {
            return Errors.GetAllErrorDetails(e, new System.Collections.Generic.List<object>());
        }

        public static string GetAllErrorDetails(Exception e, System.Collections.Generic.List<object> oParams)
        {
            try
            {
                string sTXTError = "";

                if (!object.Equals(e.GetBaseException(), e))
                {
                    sTXTError += GetAllErrorDetails(e.GetBaseException());
                    sTXTError += "\r\n";
                    sTXTError += "\r\n";
                }

                sTXTError += "-----------------------------------------------------------\r\n";
                sTXTError += "[EXCEPTION] " + e.GetType() + "\r\n";
                if (!string.IsNullOrEmpty(e.Message))
                    sTXTError += "[MESSAGE]   " + e.Message + "\r\n";

                try
                {
                    if ((!string.IsNullOrEmpty(e.StackTrace)) && (e.TargetSite != null))
                    {
                        string sFileAndLineOfException = string.Empty;

                        foreach (string s in Utils.SubStringsDups(e.StackTrace.TrimEnd().Replace(" in c:\\", " in: C:\\"), "\r\n"))
                        {
                            if (s.Contains(".cs:line "))
                            {
                                sFileAndLineOfException = s.TrimEnd().Replace(".cs:line ", ".cs (line ") + ")";
                                break;
                            }
                            else if (s.Contains(".vb:line "))
                            {
                                sFileAndLineOfException = s.TrimEnd().Replace(".vb:line ", ".vb (line ") + ")";
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(sFileAndLineOfException))
                        {
                            StackTrace stTrace = new StackTrace(e, true);
                            StackFrame[] stFrames = stTrace.GetFrames();

                            if (sFileAndLineOfException.Contains("\\" + System.Windows.Forms.Application.ProductName + "\\"))
                                sFileAndLineOfException = sFileAndLineOfException.Substring(sFileAndLineOfException.LastIndexOf("\\" + System.Windows.Forms.Application.ProductName + "\\") + ("\\" + System.Windows.Forms.Application.ProductName + "\\").Length).Replace("\\", "/");
                            sTXTError += "[FILE]      " + sFileAndLineOfException.Trim() + "\r\n";

                            if ((e.TargetSite.Attributes & System.Reflection.MethodAttributes.Private) > 0)
                                sTXTError += "[METHOD]    this." + e.TargetSite.Name + "()\r\n";
                            else if ((e.TargetSite.Attributes & System.Reflection.MethodAttributes.Static) > 0)
                            {
                                if (stFrames != null)
                                    sTXTError += "[METHOD]    " + stFrames[stFrames.Length - 1].GetMethod().ReflectedType.Namespace + "." + e.TargetSite.Name + "()\r\n";
                            }
                            else
                                sTXTError += "[METHOD]    " + e.TargetSite.Name + "()\r\n";

                            try
                            {
                                if (oParams.Count > 0)
                                {
                                    System.Reflection.ParameterInfo[] pParams = e.TargetSite.GetParameters();

                                    if (pParams.Length > 0)
                                    {
                                        foreach (ParameterInfo t in pParams)
                                        {
                                            sTXTError += "[PARAMETER] " + t.ParameterType + " " + t.Name + " = " + t.DefaultValue + "\r\n";
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                sTXTError += "-----------------------------------------------------------\r\n";

                try
                {
                    if (Object.Equals(e.InnerException, null) == false)
                    {
                        sTXTError += "[INNER EXCEPTION]\r\n";
                        sTXTError += GetAllErrorDetails(e.InnerException);
                        sTXTError += "-----------------------------------------------------------\r\n";
                    }
                }
                catch { }

                try
                {
                    if (e.TargetSite != null)
                    {
                        sTXTError += "[METHOD]\r\n";
                        sTXTError += "Name:       " + e.TargetSite.Name + "()\r\n";
                        sTXTError += "Module:     " + e.TargetSite.Module.Name + "\r\n";
                        sTXTError += "Attributes: " + e.TargetSite.Attributes + "\r\n";
                        sTXTError += "-----------------------------------------------------------\r\n";
                    }
                }
                catch { }

                try
                {
                    if (e.Data != null)
                    {
                        if (e.Data.Count > 0)
                        {
                            sTXTError += "[DATA]\r\n";
                            System.Collections.IEnumerator ienum = e.Data.GetEnumerator();
                            while (ienum.MoveNext())
                                sTXTError += ienum.Current + "\r\n";
                            sTXTError += "-----------------------------------------------------------\r\n";
                        }
                    }
                }
                catch { }

                try
                {
                    if (!string.IsNullOrEmpty(e.StackTrace))
                    {
                        sTXTError += "[STACK TRACE]\r\n";
                        string sTemp = string.Empty;
                        foreach (string s in Utils.SubStringsDups(e.StackTrace.TrimEnd().Replace(" in c:\\", " in: C:\\"), "\r\n"))
                        {
                            if (s.Contains(".cs:line "))
                                sTemp += s.TrimEnd().Replace(".cs:line ", ".cs (line ") + ")\r\n";
                            else if (s.Contains(".vb:line "))
                                sTemp += s.TrimEnd().Replace(".vb:line ", ".vb (line ") + ")\r\n";
                            else
                                sTemp += s.TrimEnd() + "\r\n";
                        }
                        sTXTError += sTemp;
                        sTXTError += "-----------------------------------------------------------\r\n";
                    }
                }
                catch { }

                //sTXTError += Errors.GetILFromException(e);
                //sTXTError += "-----------------------------------------------------------\r\n";

                return sTXTError;
            }
            catch
            {
                try
                {
                    return e.GetType().ToString() + ": " + e.Message;
                }
                catch
                {
                    if (e == null)
                        return "No exception";

                    return e.GetType().ToString();
                }
            }
        }
    }
}