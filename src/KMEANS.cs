using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project1_0422
{
    class KMEANS
    {
        double[][] feature;
        int[] answer;
        int featureSize;
        int docSize;
        int k;
        internal void set(int dicSize, int p1, int p2)
        {
            featureSize = dicSize;
            docSize = p1;
            k = p2;
        }

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
                foreach (string word in docWordDicList[i].Keys)
                {
                    if (docWordDicList[i][word] != 0 && dictionary.ContainsKey(word))
                    {
                        feature[i][dictionary[word]] = docWordDicList[i][word];
                    }
                }
            }
        }

        internal int[] compute()
        {
            int[] docClass;
            double[][] center = new double[k][];
            int[] lastdocClass;
            docClass = knnInitial();
            lastdocClass = docClass;
            List<int> deprecatedClass = new List<int>();
            while (true)
            {
                center = getCenter(center,docClass,ref deprecatedClass);
                docClass = assignDoc(center, ref deprecatedClass);
                if (compareDocClass(lastdocClass, docClass) == true)
                {
                    break;
                }
                lastdocClass = docClass;
            }
            return docClass;
        }

        private bool compareDocClass(int[] lastdocClass, int[] docClass)
        {
            for (int i = 0; i < docSize; i++)
            {
                if (lastdocClass[i] != docClass[i])
                {
                    return false;
                }
            }
            return true;
        }

        private double[][] getCenter(double[][] center,int[] docClass,ref List<int> deprecatedClass)
        {
            int[] classSum = new int[k];
            for (int i = 0; i < k; i++)
            {
                if (center[i] == null || center[i].Length == 0)
                {
                    center[i] = new double[featureSize];
                }
                for (int j = 0; j < featureSize; j++)
                {
                    center[i][j] = 0;
                }
                classSum[i] = 0;
            }
            for (int i = 0; i < docSize; i++)
            {
                for (int j = 0; j < featureSize; j++)
                {
                    center[docClass[i]][j] += feature[i][j];
                }
                classSum[docClass[i]] += 1;
            }
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < featureSize; j++)
                {
                    if (classSum[i] != 0)
                    {
                        center[i][j] = center[i][j] / classSum[i];
                    }
                    else
                    {
                        if (!deprecatedClass.Contains(i))
                        {
                            deprecatedClass.Add(i);
                        }
                    }
                }
            }
            return center;
        }

        private int[] assignDoc(double[][] center, ref List<int> deprecatedClass)
        {
            int[] result = new int[docSize];
            for (int i = 0; i < docSize; i++)
            {
                result[i] = assignCenter(feature[i],center,deprecatedClass);
                if (result[i] == 0)
                { }
            }
            return result;
        }

        private int assignCenter(double[] docFeature, double[][] center, List<int> deprecatedClass)
        {
            int result = 0;
            double minDistance = Double.MaxValue;

            for (int j = 0; j < k; j++)
            {
                if (deprecatedClass.Contains(j))
                    continue;
                double distance = getDistance(docFeature, center[j]);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    result = j;
                }
            }
            return result;
        }

        private double getDistance(double[] p1, double[] p2)
        {
            double featureSum = 0.0;
            for (int i = 0; i < featureSize; i++)
            {
                featureSum += Math.Pow(p1[i] - p2[i], 2);
            }
            return Math.Sqrt(featureSum);
        }

        private int[] knnInitial()
        {
            int[] result = new int[docSize];
            for (int i = 0; i < docSize; i++)
            {
                result[i] = i % k;
            }
            return result;
        }

        internal List<Dictionary<int, int>> compare(int[] kmeansResult, List<int> trainingAnswer)
        {
            throw new NotImplementedException();
        }

        internal void genStatstiic(string LOG_DIR, List<Dictionary<int, int>> compareResult)
        {
            throw new NotImplementedException();
        }
    }
}
