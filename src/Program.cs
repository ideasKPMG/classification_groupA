using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using porter;
using Accord.Controls;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;

namespace project1_0422
{
    class Program
    {
        public static Dictionary<string, double> weight = new Dictionary<string, double>()
        {
            {"subject",5.0},
            {"word",1.0},
            {"email",1.0}
        };
        static void Main(string[] args)
        {
            List<Dictionary<string, double>> docWordDicList = new List<Dictionary<string, double>>();
            Dictionary<string, double> dictionary = new Dictionary<string, double>();
            List<int> trainingAnswer = new List<int>();
            List<Dictionary<string, double>> wordIDFDictionary = new List<Dictionary<string, double>>();
            Hashtable stopWordTable = genStopwordTable(@"D:\work\KPMG\learning\project1\stopword.txt");
            
            trainModel(@"D:\work\KPMG\learning\classification\project1_0422\test_data\1\Training",
                       @"D:\work\KPMG\learning\classification\project1_0422\log",
                       ref docWordDicList,
                       ref dictionary,
                       ref trainingAnswer,
                       ref wordIDFDictionary,
                       stopWordTable
                );
        }

        private static Hashtable genStopwordTable(string path)
        {
            Hashtable stopwordTable = new Hashtable();
            StreamReader stopFile = new StreamReader(path);
            string line;
            string word;
            Stemmer stemmer = new Stemmer();
            while ((line = stopFile.ReadLine()) != null)
            {
                stemmer.add(line.Trim().ToCharArray(), line.Length);
                stemmer.stem();
                word = stemmer.ToString();
                stopwordTable[word.ToLower()] = 1;
            }
            stopFile.Close();
            return stopwordTable;
        }

        private static void trainModel(string trainPath, string logPath, ref List<Dictionary<string, double>> docWordDicList, ref Dictionary<string, double> dictionary, ref List<int> trainingAnswer, ref List<Dictionary<string, double>> wordIDFDictionary,Hashtable stopwordTable)
        {
            List<Dictionary<string, double>> categoryWordCountList = new List<Dictionary<string, double>>();
            string[] categories = Directory.GetDirectories(trainPath);
            for (int i = 0; i < categories.Length; i++) //traverse Categories
            {
                categoryWordCountList.Add(readCategory(categories[i], ref docWordDicList, stopwordTable));
            }
        }

        private static Dictionary<string, double> readCategory(string path,ref List<Dictionary<string, double>> docWordDicList,Hashtable stopwordTable)
        {
            Dictionary<string,double> categoryWordCount = new Dictionary<string,double>();
            string[] docs = Directory.GetFiles(path);
            for (int i = 0; i < docs.Length; i++)
            {
                docWordDicList.Add(readDoc(docs[i],stopwordTable));
            }
            return categoryWordCount;
        }

        private static Dictionary<string, double> readDoc(string path,Hashtable stopwordTable)
        {
            Dictionary<string, double> docWordCount = new Dictionary<string, double>();
            StreamReader docFile = new StreamReader(path);
            string line;

            while((line = docFile.ReadLine()) != null)
            {
                if (isColumn(line))
                {
                    string column = getColumnName(line);
                    string content = getColumnContent(line);
                    if (column == "subject")
                    {
                        foreach (string iter_word in splitLine(content))
                        {
                            string word = getWord(iter_word, stopwordTable);
                            //word cleansing done
                            if (word != null)
                            {
                                if (docWordCount.ContainsKey(word))
                                {
                                    docWordCount[word] += weight["subject"];
                                }
                                else
                                {
                                    docWordCount.Add(word, weight["subject"]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (line.Length != 0)
                    {
                        break;
                    }
                }
            }
            while (line != null)
            {
                line = processSpecialField(line,ref docWordCount);
                foreach (string iter_word in splitLine(line))
                {
                    string word = getWord(iter_word, stopwordTable);
                    //word cleansing done
                    if (word != null)
                    {
                        if (docWordCount.ContainsKey(word))
                        {
                            docWordCount[word] += weight["word"];
                        }
                        else
                        {
                            docWordCount.Add(word, weight["word"]);
                        }
                    }
                }
                line = docFile.ReadLine();
            }
            return docWordCount;
        }


        private static string processSpecialField(string line, ref Dictionary<string, double> docWordCount)
        {
            string[] emails = getEmail(line);
            string result;
            for(int i=0;i<emails.Length;i++)
            {
                if (docWordCount.ContainsKey(emails[i]))
                {
                    docWordCount[emails[i]] += weight["email"];
                }
                else
                {
                    docWordCount.Add(emails[i], weight["email"]);
                }
            }
            result = replaceEmail(line);
            return result;
        }

        private static string replaceEmail(string line)
        {
            string replacement = " ";
            Regex rgx = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
            string result = rgx.Replace(line, replacement);
            return result;
        }

        private static string[] getEmail(string line)
        {
            Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
            MatchCollection emailMatches = emailRegex.Matches(line);
            return emailMatches.Cast<Match>().Select(m => m.Value).ToArray();
        }

        private static string getColumnContent(string line)
        {
            string[] part = line.Split(new string[] { ": " }, StringSplitOptions.None);
            return part[part.Length - 1].Trim();
        }

        private static string getColumnName(string line)
        {
            return line.Split(new string[] { ": " }, StringSplitOptions.None)[0].Trim().ToLower();
        }

        private static bool isColumn(string line)
        {
            if (line.Contains(": "))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static IEnumerable<string> splitLine(string line)
        {
            return Regex.Split(line, @"[^A-Za-z0-9_-]");
        }

        private static string getWord(string word,Hashtable stopwordTable)
        {
            Stemmer stemmer = new Stemmer();
            string result = word.ToLower().Trim(new Char[] { '_', '-' });
            double Num;
            bool isNum = double.TryParse(word, out Num);
            if (isNum)
            {
                return null;
            }
            stemmer.add(result.ToCharArray(), result.Length);
            stemmer.stem();
            result = stemmer.ToString();
            if (result.Length == 0)
            {
                return null;
            }
            if (stopwordTable.ContainsKey(result))
            {
                return null;
            }
            return result;
        }
    }
}
