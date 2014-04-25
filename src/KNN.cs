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
        int featureSize;
        int docSize;
        KNearestNeighbors knn;
        internal void initial(List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer)
        {
            feature = new double[docSize][];
            answer = trainingAnswer.ToArray();
            for (int i = 0; i < docWordDicList.Count(); i++)
            {
                if (feature[i] == null || feature[i].Length == 0)
                {
                    feature[i] = new double[featureSize];
                }
                for (int j = 0; j < featureSize; j++)
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

        internal List<double[]> getCenter()
        {
            List<double[]> centerList = new List<double[]>();
            int last = 0;
            double[] center = new double[featureSize];
            int categoryDocCount = 0;
            for (int i = 0; i < docSize; i++)
            {
                if (answer[i] != last)
                {
                    for (int j = 0; j < featureSize; j++)
                    {
                        center[j] = center[j] / categoryDocCount;
                    }
                    centerList.Add(center);
                    center = new double[featureSize];
                    for (int j = 0; j < featureSize; j++)
                    {
                        center[j] = 0;
                    }
                    categoryDocCount = 0;
                    last = answer[i];
                }
                for (int j = 0; j < featureSize; j++)
                {
                    center[j] += feature[i][j];
                }
                categoryDocCount++;
            }
            for (int j = 0; j < featureSize; j++)
            {
                center[j] = center[j] / categoryDocCount;
            }
            centerList.Add(center);
            return centerList;
        }

        internal List<double> getAveDistance()
        {
            List<double[]> centerList = getCenter();
            List<double> aveDistanceList = new List<double>();
            int last = 0;
            double distance=0;
            double categoryDocCount = 0;
            double categoryDistanceSum = 0;
            for (int i = 0; i < docSize; i++)
            {
                if (answer[i] != last)
                {
                    aveDistanceList.Add(categoryDistanceSum/categoryDocCount);
                    categoryDocCount = 0;
                    categoryDistanceSum = 0;
                    last = answer[i];
                }
                double powTemp = 0;
                for (int j = 0; j < featureSize; j++)
                {
                    powTemp += Math.Pow(feature[i][j] - centerList[last][j],2);
                }
                categoryDistanceSum += Math.Sqrt(powTemp);
                categoryDocCount++;
            }
            aveDistanceList.Add(categoryDistanceSum / categoryDocCount);
            return aveDistanceList;
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
        internal void set(int dicSize, int p3)
        {
            featureSize = dicSize;
            docSize = p3;
        }

        internal void train(int k, int classes)
        {
            knn = new KNearestNeighbors(k, classes, feature, answer);
        }

        internal int test(Dictionary<string, double> docWordDic, Dictionary<string, int> dictionary, Dictionary<string, double> wordIDFDictionary)
        {
            double[] testFeature = new double[featureSize];
            double docWordSum = 0;
            for (int i = 0; i < featureSize; i++)
            {
                testFeature[i] = 0;
            }
            foreach (string word in docWordDic.Keys)
            {
                docWordSum += docWordDic[word];
            }
            foreach (string word in docWordDic.Keys)
            {
                if (dictionary.ContainsKey(word) && wordIDFDictionary.ContainsKey(word) && wordIDFDictionary[word] != 0)
                {
                    testFeature[dictionary[word]] = (docWordDic[word] / docWordSum) * wordIDFDictionary[word];//TFIDF
                }
            }
            return knn.Compute(testFeature);
        }
    }
}