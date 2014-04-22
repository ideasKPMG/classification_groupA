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
        static void Main(string[] args)
        {
            List<Dictionary<string, double>> docWordDicList = new List<Dictionary<string, double>>();
            Dictionary<string, double> dictionary = new Dictionary<string, double>();
            List<int> trainingAnswer = new List<int>();
            List<Dictionary<string, double>> wordIDFDictionary = new List<Dictionary<string, double>>();
            Hashtable stopWordTable = genStopwordTable(@"D:\work\KPMG\learning\project1\stopword.txt");
            
            trainModel(@"D:\work\KPMG\learning\classification\project1_0422\test_data\1\training",
                       @"D:\work\KPMG\learning\classification\project1_0422\test_data\1\log",
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
            List<Dictionary<string, int>> categoryWordCount = new List<Dictionary<string, int>>();
            string[] categories = Directory.GetDirectories(trainPath);
            for (int i = 0; i < categories.Length; i++) //traverse Categories
            {
                categoryWordCount.Add(readCategory(categories[i], ref docWordDicList, stopwordTable));
            }
        }

        private static Dictionary<string, int> readCategory(string path,ref List<Dictionary<string, double>> docWordDicList,Hashtable stopwordTable)
        {
            Dictionary<string,int> categoryWordCount = new Dictionary<string,int>();
            string[] docs = Directory.GetDirectories(path);
            for (int i = 0; i < docs.Length; i++)
            {
                readDoc(docs[i],stopwordTable);
            }
            return categoryWordCount;
        }

        private static Dictionary<string, int> readDoc(string path,Hashtable stopwordTable)
        {
            Dictionary<string, int> docWordCount = new Dictionary<string, int>();
            StreamReader docFile = new StreamReader(path);
            string line;

            while((line = docFile.ReadLine()) != null)
            {
                if (line.Contains(": "))
                {
                }
                if (line.Length != 0)
                {
                    break;
                }
            }
            while ((line = docFile.ReadLine()) != null)
            {
                foreach (string iter_word in splitLine(line))
                {
                    string word = getWord(iter_word, stopwordTable);
                    //word cleansing done
                    if (word != null)
                    {
                        docWordCount[word]++;
                    }
                }
            }
            return docWordCount;
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
