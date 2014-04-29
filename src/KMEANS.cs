using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Controls;
using Accord.MachineLearning;
using Accord.Math;
using Accord.Statistics;

namespace project1_0422
{
    class KMEANS
    {
        double[][] feature;
        int[] answer;
        int featureSize;
        int docSize;
        int k;
        KMeans kmeans;
        internal void initial(List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer)
        {
            feature = new double[docSize][];
            answer = trainingAnswer.ToArray();
            kmeans = new KMeans(k);
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
                foreach (string word in docWordDicList[i].Keys)
                {
                    if (docWordDicList[i][word] != 0 && dictionary.ContainsKey(word))
                    {
                        feature[i][dictionary[word]] = docWordDicList[i][word];
                    }
                }
            }
        }
        internal void set(int dicSize, int p3, int p4)
        {
            featureSize = dicSize;
            docSize = p3;
            k = p4;
        }
        internal int[] compute()
        {
            return kmeans.Compute(feature);
        }

        internal List<Dictionary<int, int>> compare(int[] kmeansResult, List<int> trainingAnswer)
        {
            List<Dictionary<int, int>> result = new List<Dictionary<int, int>>();
            //result.Add(new Dictionary<int, int>());
            for (int i = 0; i < trainingAnswer.Count(); i++)
            {
                if (result.Count < trainingAnswer[i]+1)
                {
                    result.Add(new Dictionary<int, int>());
                }
                if (result[trainingAnswer[i]].ContainsKey(kmeansResult[i]))
                {
                    result[trainingAnswer[i]][kmeansResult[i]] += 1;
                }
                else 
                {
                    result[trainingAnswer[i]].Add(kmeansResult[i], 1);
                }
            }
            return result;
        }

        internal void genStatistic(string LOG_DIR, List<Dictionary<int, int>> compareResult)
        {
            StreamWriter logFile = new StreamWriter(LOG_DIR + "\\kmeans_result.csv");
            for (int i = 0; i < compareResult.Count; i++)
            {
                List<KeyValuePair<int, int>> sortedResult = compareResult[i].ToList();
                sortedResult.Sort((a, b) => b.Value.CompareTo(a.Value));
                for (int j = 0; j < sortedResult.Count; j++)
                {
                    logFile.WriteLine(i + "," + sortedResult[j].Key + "," + sortedResult[j].Value);
                }
            }
            logFile.Close();
        }
    }
}
