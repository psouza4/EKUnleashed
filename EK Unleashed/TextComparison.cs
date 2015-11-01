using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EKUnleashed
{
    public class TextComparison
    {
        private TextComparison() { }

        public class Result
        {
            public int FlatLevDistance = 0;
            public bool bSoundExMatch = false;
            public double dSoundExPenalty = 0;
            public double AverageLevDistance = 0.0;

            public int ProbabilityOfMatch
            {
                get
                {
                    try
                    {
                        double dProb = 100;

                        dProb -= dSoundExPenalty;

                        dProb -= (AverageLevDistance * 1.00);

                        if (dProb > 0.0)
                        {
                            if (dProb > 100.0)
                                return 100;

                            return (int)dProb;
                        }
                    }
                    catch { }

                    return 0;
                }
            }
        }

        public static string FullSoundex(string s)
        {
            // the encoding information  
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string codes = "0123012D02245501262301D2021234567890";

            if (s.Trim() == string.Empty)
                return "0";

            // some helpful regexes  
            Regex hwBeginString = new Regex("^D+");
            Regex simplify = new Regex(@"(\d)\1*D?\1+");
            Regex cleanup = new Regex("[D0]");

            // i need a capitalized string  
            s = s.ToUpper();

            // i'm building the coded string using a string builder  
            // because i think this is probably the fastest and least  
            // intensive way  
            StringBuilder coded = new StringBuilder();

            // do the encoding  
            for (int i = 0; i < s.Length; i++)
            {
                int index = chars.IndexOf(s[i]);
                if (index >= 0)
                    coded.Append(codes[index]);
            }

            // okay, so here's how this goes . . .  
            // the first thing I do is assign the coded string  
            // so that i can regex replace on it  
            string result = coded.ToString();

            // then i remove repeating characters  
            //result = repeating.Replace(result, "$1");  
            try
            {
                result = simplify.Replace(result, "$1").Substring(1);
            }
            catch
            {
                try
                {
                    result = simplify.Replace(result, "$1");
                }
                catch { }
            }

            // now i need to remove any characters coded as D  from  
            // the front of the string because they're not really   
            // valid as the first code because they don't have an   
            // actual soundex code value  
            result = hwBeginString.Replace(result, string.Empty);

            // i used the char D to indicate that an h or w existed  
            // so that if to similar sounds were separated by an h or  
            // a w that I could remove one of them.  if the h or w does  
            // not separate two similar sounds, then i need to remove  
            // it now  
            result = cleanup.Replace(result, string.Empty);

            // return the first character followed by the coded  
            // string  
            return string.Format("{0}{1}", s[0], result);
        }

        public class Levenshtein
        {


            ///*****************************
            /// Compute Levenshtein distance 
            /// Memory efficient version
            ///*****************************
            public int iLD(String sRow, String sCol)
            {
                int RowLen = sRow.Length;  // length of sRow
                int ColLen = sCol.Length;  // length of sCol
                int RowIdx;                // iterates through sRow
                int ColIdx;                // iterates through sCol
                char Row_i;                // ith character of sRow
                char Col_j;                // jth character of sCol
                int cost;                   // cost

                /// Test string length
                if (Math.Max(sRow.Length, sCol.Length) > Math.Pow(2, 31))
                    throw (new Exception("\nMaximum string length in Levenshtein.iLD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(sRow.Length, sCol.Length) + "."));

                // Step 1

                if (RowLen == 0)
                {
                    return ColLen;
                }

                if (ColLen == 0)
                {
                    return RowLen;
                }

                /// Create the two vectors
                int[] v0 = new int[RowLen + 1];
                int[] v1 = new int[RowLen + 1];
                int[] vTmp;



                /// Step 2
                /// Initialize the first vector
                for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                {
                    v0[RowIdx] = RowIdx;
                }

                // Step 3

                /// Fore each column
                for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
                {
                    /// Set the 0'th element to the column number
                    v1[0] = ColIdx;

                    Col_j = sCol[ColIdx - 1];


                    // Step 4

                    /// Fore each row
                    for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                    {
                        Row_i = sRow[RowIdx - 1];


                        // Step 5

                        if (Row_i == Col_j)
                        {
                            cost = 0;
                        }
                        else
                        {
                            cost = 1;
                        }

                        // Step 6

                        /// Find minimum
                        int m_min = v0[RowIdx] + 1;
                        int b = v1[RowIdx - 1] + 1;
                        int c = v0[RowIdx - 1] + cost;

                        if (b < m_min)
                        {
                            m_min = b;
                        }
                        if (c < m_min)
                        {
                            m_min = c;
                        }

                        v1[RowIdx] = m_min;
                    }

                    /// Swap the vectors
                    vTmp = v0;
                    v0 = v1;
                    v1 = vTmp;

                }


                // Step 7

                /// Value between 0 - 100
                /// 0==perfect match 100==totaly different
                /// 
                /// The vectors where swaped one last time at the end of the last loop,
                /// that is why the result is now in v0 rather than in v1
                System.Console.WriteLine("iDist=" + v0[RowLen]);
                int max = System.Math.Max(RowLen, ColLen);
                return ((100 * v0[RowLen]) / max);
            }





            ///*****************************
            /// Compute the min
            ///*****************************

            private int Minimum(int a, int b, int c)
            {
                int mi = a;

                if (b < mi)
                {
                    mi = b;
                }
                if (c < mi)
                {
                    mi = c;
                }

                return mi;
            }

            ///*****************************
            /// Compute Levenshtein distance         
            ///*****************************

            public int LD(String sNew, String sOld)
            {
                int[,] matrix;              // matrix
                int sNewLen = sNew.Length;  // length of sNew
                int sOldLen = sOld.Length;  // length of sOld
                int sNewIdx; // iterates through sNew
                int sOldIdx; // iterates through sOld
                char sNew_i; // ith character of sNew
                char sOld_j; // jth character of sOld
                int cost; // cost

                /// Test string length
                if (Math.Max(sNew.Length, sOld.Length) > Math.Pow(2, 31))
                    throw (new Exception("\nMaximum string length in Levenshtein.LD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(sNew.Length, sOld.Length) + "."));

                // Step 1

                if (sNewLen == 0)
                {
                    return sOldLen;
                }

                if (sOldLen == 0)
                {
                    return sNewLen;
                }

                matrix = new int[sNewLen + 1, sOldLen + 1];

                // Step 2

                for (sNewIdx = 0; sNewIdx <= sNewLen; sNewIdx++)
                {
                    matrix[sNewIdx, 0] = sNewIdx;
                }

                for (sOldIdx = 0; sOldIdx <= sOldLen; sOldIdx++)
                {
                    matrix[0, sOldIdx] = sOldIdx;
                }

                // Step 3

                for (sNewIdx = 1; sNewIdx <= sNewLen; sNewIdx++)
                {
                    sNew_i = sNew[sNewIdx - 1];

                    // Step 4

                    for (sOldIdx = 1; sOldIdx <= sOldLen; sOldIdx++)
                    {
                        sOld_j = sOld[sOldIdx - 1];

                        // Step 5

                        if (sNew_i == sOld_j)
                        {
                            cost = 0;
                        }
                        else
                        {
                            cost = 1;
                        }

                        // Step 6

                        matrix[sNewIdx, sOldIdx] = Minimum(matrix[sNewIdx - 1, sOldIdx] + 1, matrix[sNewIdx, sOldIdx - 1] + 1, matrix[sNewIdx - 1, sOldIdx - 1] + cost);

                    }
                }

                // Step 7

                /// Value between 0 - 100
                /// 0==perfect match 100==totaly different
                System.Console.WriteLine("Dist=" + matrix[sNewLen, sOldLen]);
                int max = System.Math.Max(sNewLen, sOldLen);
                return (100 * matrix[sNewLen, sOldLen]) / max;
            }
        }

        private static int LevenshteinDistance(string TextA, string TextB)
        {
            //Utils.DebugLogger("Comparing:\t" + TextA);
            //Utils.DebugLogger("\tto:\t" + TextB);

            Levenshtein l = new Levenshtein();

            return l.iLD(TextA, TextB);
        }

        public static string PrepareForCompare(string Text)
        {
            Text = Text.ToLower() + " ";

            Text = Text.Replace("-0 ", " o ");
            Text = Text.Replace("-o ", " o ");
            Text = Text.Replace(" o ", " 0 ");

            Text = Text.Replace("-", string.Empty);

            Text = Utils.PrepTextForComparison(Utils.UnHTML(Text));

            Text = Utils.MapInternationals(Regex.Replace(Text, @"\(.*?\)", string.Empty).Trim());

            return Text.Trim();
        }

        private static double ScoreSoundEx(string WordA, string WordB)
        {
            //Utils.DebugLogger("SoundEx of <b>" + WordA + "</b> is " + FullSoundex(WordA).ToString());
            //Utils.DebugLogger("SoundEx of <b>" + WordB + "</b> is " + FullSoundex(WordB).ToString());

            double dCurrentWordScore = (FullSoundex(WordA) == FullSoundex(WordB)) ? 100.0 : 0.0;

            if (dCurrentWordScore == 0.0)
            {
                try
                {
                    if (FullSoundex(WordA).Substring(0, 4) == FullSoundex(WordB).Substring(0, 4))
                        dCurrentWordScore = 70.0;
                }
                catch { }
            }

            if (dCurrentWordScore == 0.0)
            {
                try
                {
                    if (FullSoundex(WordA).Substring(0, 3) == FullSoundex(WordB).Substring(0, 3))
                        dCurrentWordScore = 50.0;
                }
                catch { }
            }

            if (dCurrentWordScore == 0.0)
            {
                try
                {
                    if (FullSoundex(WordA).Substring(0, 2) == FullSoundex(WordB).Substring(0, 2))
                        dCurrentWordScore = 25.0;
                }
                catch { }
            }

            if (dCurrentWordScore == 0.0)
            {
                try
                {
                    if (FullSoundex(WordA).Substring(0, 1) == FullSoundex(WordB).Substring(0, 1))
                        dCurrentWordScore = 10.0;
                }
                catch { }
            }

            return dCurrentWordScore;
        }

        private static double AverageSoundExMatch(string TextA, string TextB)
        {
            double dTotalScore = 0.0;
            double dWordCount = 0.0;
            int iOffsetBoostForSkippedWords = 0;
            int iCountMissingWords = 0;
            int iCountSkippedWords = 0;

            string[] sTextB_Words = Utils.SubStringsDups(TextB, " ");

            foreach (string word in Utils.SubStringsDups(TextA, " "))
            {
                //Utils.DebugLogger("... soundex(\"" + word + "\") = \"" + FullSoundex(word) + "\"");

                dWordCount++;
                int iOffset = ((int)(dWordCount - 1.0)) + iOffsetBoostForSkippedWords;
                bool bUsedOther = false;

                try
                {
                    //Utils.DebugLogger("\t<b>" + word + "</b>: " + FullSoundex(word));
                    double dCurrentWordScore = ScoreSoundEx(word, sTextB_Words[iOffset]);

                    if (dCurrentWordScore < 1.0)
                    {
                        double dNextWordScore = 0.0;
                        double dPreviousWordScore = 0.0;

                        try
                        {
                            dNextWordScore = ScoreSoundEx(word, sTextB_Words[iOffset + 1]);
                        }
                        catch { }

                        try
                        {
                            dNextWordScore = ScoreSoundEx(word, sTextB_Words[iOffset - 1]);
                        }
                        catch { }

                        if (dNextWordScore >= dCurrentWordScore && dNextWordScore > dPreviousWordScore)
                        {
                            iCountSkippedWords++;
                            iOffsetBoostForSkippedWords++;
                            dTotalScore += dNextWordScore;
                            bUsedOther = true;
                        }
                        else if (dPreviousWordScore >= dCurrentWordScore)
                        {
                            iCountSkippedWords++;
                            iOffsetBoostForSkippedWords--;
                            dTotalScore += dPreviousWordScore;
                            bUsedOther = true;
                        }
                    }

                    if (!bUsedOther)
                        dTotalScore += dCurrentWordScore;
                }
                catch
                {
                    iCountMissingWords++;
                    break;
                }
            }

            //Utils.DebugLogger("\tscore:\t" + dTotalScore.ToString());
            //Utils.DebugLogger("\taverage:\t" + (dTotalScore / (Utils.SubStringsDups(TextA, " ").Length)).ToString());
            //Utils.DebugLoggerNoTimestamp("\tmissing:\t" + iCountMissingWords.ToString());
            //Utils.DebugLogger("\tskipped:\t" + iCountSkippedWords.ToString());

            return dTotalScore / (dWordCount + iCountMissingWords + iCountSkippedWords);
        }

        private static double AverageLevenshteinScore(string TextA, string TextB)
        {
            double dTotalScore = 0.0;
            double dWordCount = 0.0;
            int iOffsetBoostForSkippedWords = 0;
            int iCountMissingWords = 0;
            int iCountSkippedWords = 0;

            string[] sTextB_Words = Utils.SubStringsDups(TextB, " ");

            foreach (string word in Utils.SubStringsDups(TextA, " "))
            {
                dWordCount++;
                int iOffset = ((int)(dWordCount - 1.0)) + iOffsetBoostForSkippedWords;
                bool bUsedOther = false;

                try
                {
                    double dCurrentWordScore = (double)LevenshteinDistance(word, sTextB_Words[iOffset]);

                    if (dCurrentWordScore >= 1)
                    {
                        double dNextWordScore = 99999.0;
                        double dPreviousWordScore = 99999.0;

                        try
                        {
                            dNextWordScore = (double)LevenshteinDistance(word, sTextB_Words[iOffset + 1]);
                        }
                        catch { }

                        try
                        {
                            dPreviousWordScore = (double)LevenshteinDistance(word, sTextB_Words[iOffset - 1]);
                        }
                        catch { }

                        if (dNextWordScore <= dCurrentWordScore && dNextWordScore < dPreviousWordScore)
                        {
                            iCountSkippedWords++;
                            iOffsetBoostForSkippedWords++;
                            dTotalScore += dNextWordScore;
                            dTotalScore += 30.0; // 30 point penalty for missing a word
                            bUsedOther = true;
                        }
                        else if (dPreviousWordScore <= dCurrentWordScore)
                        {
                            iCountSkippedWords++;
                            iOffsetBoostForSkippedWords--;
                            dTotalScore += dPreviousWordScore;
                            dTotalScore += 30.0; // 30 point penalty for missing a word
                            bUsedOther = true;
                        }
                    }

                    if (!bUsedOther)
                        dTotalScore += dCurrentWordScore;
                }
                catch
                {
                    iCountMissingWords++;
                    dTotalScore += 30.0;
                    break;
                }
            }

            dTotalScore += (((double)Utils.SubStringsDups(TextA, " ").Length) - dWordCount) * 30.0;
            return dTotalScore;
        }

        public static Result Compare(string TextA, string TextB)
        {
            return Compare_Prepared(PrepareForCompare(TextA), PrepareForCompare(TextB));
        }

        public static Result Compare_Prepared(string TextA, string TextB)
        {
            Result result = new Result();

            result.FlatLevDistance = LevenshteinDistance(TextA, TextB);

            double dForwardScore = AverageLevenshteinScore(TextA, TextB);
            double dBackwardScore = AverageLevenshteinScore(TextB, TextA);

            // use the average
            result.AverageLevDistance = (dForwardScore + dBackwardScore) / 2.0;

            // use the best between average and flat
            if (result.FlatLevDistance < result.AverageLevDistance)
                result.AverageLevDistance = result.FlatLevDistance;

            double dForwardSoundexScore = AverageSoundExMatch(TextA, TextB);
            double dBackwardSoundexScore = AverageSoundExMatch(TextB, TextA);

            // use the better SoundEx Score            
            if (dForwardSoundexScore > dBackwardSoundexScore)
            {
                if (dBackwardSoundexScore >= 85)
                    result.dSoundExPenalty = -10.0;
                else if (dBackwardSoundexScore >= 70)
                    result.dSoundExPenalty = 0.0;
                else if (dBackwardSoundexScore >= 55)
                    result.dSoundExPenalty = 10.0;
                else if (dBackwardSoundexScore >= 40)
                    result.dSoundExPenalty = 20.0;
                else
                    result.dSoundExPenalty = 30.0;

                result.bSoundExMatch = dBackwardSoundexScore >= 70.0;
            }
            else
            {
                if (dForwardSoundexScore >= 85)
                    result.dSoundExPenalty = -10.0;
                else if (dForwardSoundexScore >= 70)
                    result.dSoundExPenalty = 0.0;
                else if (dForwardSoundexScore >= 55)
                    result.dSoundExPenalty = 10.0;
                else if (dForwardSoundexScore >= 40)
                    result.dSoundExPenalty = 20.0;
                else
                    result.dSoundExPenalty = 30.0;

                result.bSoundExMatch = dForwardSoundexScore >= 70.0;
            }

            return result;
        }
        public static double ProbabilityForMatch(string TextA, string TextB)
        {
            TextComparison.Result result;

            result = TextComparison.Compare(TextA, TextB);

            return result.ProbabilityOfMatch;
        }

        public static double ProbabilityForMatch_Prepared(string TextA, string TextB)
        {
            TextComparison.Result result;

            result = TextComparison.Compare_Prepared(TextA, TextB);

            return result.ProbabilityOfMatch;
        }

        public static bool IsExactMatch(string TextA, string TextB)
        {
            return ProbabilityForMatch(TextA, TextB) >= 98.5;
        }

        public static bool IsReallyCloseMatch(string TextA, string TextB)
        {
            return ProbabilityForMatch(TextA, TextB) >= 92.5;
        }

        public static bool IsCloseMatch(string TextA, string TextB)
        {
            return ProbabilityForMatch(TextA, TextB) >= 80.0;
        }

        public static bool IsCloseMatch_Prepared(string TextA, string TextB)
        {
            return ProbabilityForMatch_Prepared(TextA, TextB) >= 80.0;
        }

        public static bool IsPossibleMatch(string TextA, string TextB)
        {
            return ProbabilityForMatch(TextA, TextB) >= 65.0;
        }
    }
}
