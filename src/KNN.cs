using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using porter;
using Accord.Controls;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;
using System.IO;

namespace project1_0422
{
    class KNN
    {
        double[][] feature;
        int[] answer;
        int k;
        int classes;
        int featureCount;
        int docCount;

        internal void initial(List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer, Dictionary<string, double> wordIDFDictionary)
        {
            feature = new double[docCount][];
            answer = trainingAnswer.ToArray();
            for (int i = 0; i < docWordDicList.Count(); i++)
            {
                if (feature[i] == null || feature[i].Length == 0)
                {
                    feature[i] = new double[featureCount];
                }
                for (int j = 0; j < featureCount; j++)
                {
                    feature[i][j] = 0;
                }
                foreach(string word in docWordDicList[i].Keys)
                {
                    if (docWordDicList[i][word] != 0 && dictionary.ContainsKey(word))
                    {
                        feature[i][dictionary[word]] = docWordDicList[i][word];
                    }
                }
            }
        }
        internal void genLog(string logPath)
        {
            StreamWriter logFile = new StreamWriter(logPath + "\\0.csv");
            int last = 0;
            for (int i = 0; i < answer.Length; i++)
            {
                if (answer[i] != last)
                {
                    logFile.Close();
                    last = answer[i];
                    logFile = new StreamWriter(logPath + "\\"+last+".csv");
                }
                for (int j = 0; j < feature[i].Length; j++)
                {
                    if(j!=0)
                    {
                        logFile.Write(",");
                    }
                    logFile.Write(feature[i][j]);
                }
                logFile.Write("\r\n");
            }
        }
        internal void set(int p1, int p2, int dicSize, int p3)
        {
            k = p1;
            classes = p2;
            featureCount = dicSize;
            docCount = p3;
        }
    }
}