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
            trainModel(@"D:\work\KPMG\learning\classification\project1_0422\test_data\1\training",
                       @"D:\work\KPMG\learning\classification\project1_0422\test_data\1\log",
                       ref docWordDicList,
                       ref dictionary,
                       ref trainingAnswer,
                       ref wordIDFDictionary
                );
        }

        private static void trainModel(string trainPath, string logPath, ref List<Dictionary<string, double>> docWordDicList, ref Dictionary<string, double> dictionary, ref List<int> trainingAnswer, ref List<Dictionary<string, double>> wordIDFDictionary)
        {
            List<Dictionary<string, int>> categoryWordCount = new List<Dictionary<string, int>>();
            string[] categories = Directory.GetDirectories(trainPath);
            for (int i = 0; i < categories.Length; i++) //traverse Categories
            {
                categoryWordCount.Add(readCategory(categories[i],ref docWordDicList));
            }
        }

        private static Dictionary<string, int> readCategory(string path,ref List<Dictionary<string, double>> docWordDicList)
        {
            Dictionary<string,int> categoryWordCount = new Dictionary<string,int>();
            string[] docs = Directory.GetDirectories(path);
            for (int i = 0; i < docs.Length; i++)
            {
                readDoc(docs[i]);
            }
            return categoryWordCount;
        }

        private static Dictionary<string, int> readDoc(string path)
        {
            Dictionary<string, int> docWordCount = new Dictionary<string, int>();
            return docWordCount;
        }
    }
}
