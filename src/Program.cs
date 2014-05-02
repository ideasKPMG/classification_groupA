//#define ACCORD_KMEANS_MODE
//#define USE_POSTAG
#define KMEANS_MODE

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
using libsvm;

namespace project1_0422
{
    class Program
    {
        public static String STOP_WORD_PATH = @"D:\work\KPMG\learning\project1\stopword.txt";
        public static String TRAINING_DATA_DIR = @"D:\work\KPMG\learning\classification\project1_0422\test_data\1\Training";
        public static String TEST_DATA_DIR = @"D:\work\KPMG\learning\classification\project1_0422\test_data\1\Testing";

        public static String LOG_DIR = @"D:\work\KPMG\learning\classification\project1_0422\log";
        public static String TEST_LOG_DIR = @"D:\work\KPMG\learning\classification\project1_0422\test_data\log";
        public static String NLP_MODEL_PATH = @"D:\work\KPMG\learning\classification\project1_0422\NLPModels\Models";

        public static Dictionary<string, double> weight = new Dictionary<string, double>()
        {
            {"subject",1.0},
            {"word",1.0},
            {"email",1.0},
            {"path",1.0},
            {"newsgroups",1.0},
            {"from",1.0}
        };
        static void Main(string[] args)
        {
            List<Dictionary<string, double>> docWordDicList = new List<Dictionary<string, double>>();
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            List<int> trainingAnswer = new List<int>();
            Dictionary<string, double> wordIDFDictionary = new Dictionary<string, double>();
            Hashtable stopWordTable = genStopwordTable(STOP_WORD_PATH);
            List<string> testFileNameList = new List<string>();
            int dicSize = 200;

            Console.WriteLine("==> Starting prepare data...");
            NLPAdapter nlpAdapter = new NLPAdapter(NLP_MODEL_PATH);

            trainModel(TRAINING_DATA_DIR,
                       LOG_DIR,
                       ref docWordDicList,
                       ref dictionary,
                       dicSize,
                       ref trainingAnswer,
                       ref wordIDFDictionary,
                       stopWordTable,
                       nlpAdapter
                );
#if KNN_MODE
            KNN knn = new KNN();
            knn.set(dicSize, docWordDicList.Count());
            knn.initial(docWordDicList, dictionary, trainingAnswer);
            knn.train(3, 20);
            knn.getAveDistance();

            //knn.genLog(@"D:\work\KPMG\learning\classification\project1_0422\log");
            List<KeyValuePair<int, int>> testAnswer = runKnnTest(knn, TEST_DATA_DIR, TEST_LOG_DIR, dictionary, wordIDFDictionary, stopWordTable, ref testFileNameList, nlpAdapter);
#elif ACCORD_KMEANS_MODE
            accordKmeans kmeans = new accordKmeans();
            kmeans.set(dicSize, docWordDicList.Count(),20);
            kmeans.initial(docWordDicList, dictionary, trainingAnswer);
            int[] kmeansResult = kmeans.compute();
            List<Dictionary<int,int>> compareResult = kmeans.compare(kmeansResult, trainingAnswer);
            kmeans.genStatistic(LOG_DIR,compareResult);
#elif KMEANS_MODE
            KMEANS kmeans = new KMEANS();
            kmeans.set(dicSize, docWordDicList.Count(), 20);
            kmeans.initial(docWordDicList, dictionary, trainingAnswer);
            int[] kmeansResult = kmeans.compute();
            List<Dictionary<int, int>> compareResult = kmeans.compare(kmeansResult, trainingAnswer);
            //dumpFeature(LOG_DIR, docWordDicList, dictionary, trainingAnswer);
            kmeans.genStatistic(LOG_DIR, compareResult);
            kmeans.dumpFeature(LOG_DIR);
#elif SVM_MODE
            Console.WriteLine("==> Starting get model...");
            SVMAdapter svmAdapter = new SVMAdapter();
            svm_model model = svmAdapter.getSVMModel(docWordDicList, dictionary, trainingAnswer, SVMAdapter.SVM_C_DEFAULT, SVMAdapter.SVM_GAMMA_DEFAULT);

            Console.WriteLine("==> Starting SVM test...");
            List<KeyValuePair<int, int>> testAnswer = runSVMTest(svmAdapter, TEST_DATA_DIR, TEST_LOG_DIR, dictionary, wordIDFDictionary, stopWordTable, ref testFileNameList, model, nlpAdapter);
            Console.WriteLine("==> Starting SVM test done!!");
#endif
            Console.WriteLine("==> Starting saving result...");
            //genClassifyStatistic(testAnswer, testFileNameList, LOG_DIR);
        }
/*        private static void dumpFeature(string LOG_DIR, List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer)
        {
            int last = 0;
            StreamWriter logFile = new StreamWriter(LOG_DIR + "\\0.csv");
            for (int i = 0; i < trainingAnswer.Count; i++)
            {
                if (last != trainingAnswer[i])
                {
                    last = trainingAnswer[i];
                    logFile.Close();
                    logFile = new StreamWriter(LOG_DIR + "\\" + last + ".csv");
                }
                for (int j = 0; j < dictionary.Count; j++)
                {
 
                }
            }
        }*/

        private static void genClassifyStatistic(List<KeyValuePair<int, int>> testAnswer, List<string> testFileNameList, string logPath)
        {
            StreamWriter resultFile = new StreamWriter(logPath + "\\result.csv");
            double totalCorrectCount = 0;
            double totalCount = 0;
            double categoryCorrectCount = 0;
            double categoryCount = 0;
            int last = 0;
            StreamWriter statisticFile = new StreamWriter(logPath + "\\statistic.csv");
            for (int i = 0; i < testAnswer.Count(); i++)
            {
                if (testAnswer[i].Value != last)
                {
                    statisticFile.WriteLine(last + "," + categoryCorrectCount / categoryCount);
                    categoryCount = 0;
                    categoryCorrectCount = 0;
                    last = testAnswer[i].Value;
                }
                totalCount += 1;
                categoryCount += 1;
                totalCorrectCount += (testAnswer[i].Key == testAnswer[i].Value) ? 1 : 0;
                categoryCorrectCount += (testAnswer[i].Key == testAnswer[i].Value) ? 1 : 0;
                resultFile.WriteLine(testFileNameList[i] + "," + testAnswer[i].Key + "," + testAnswer[i].Value + "," + ((testAnswer[i].Key == testAnswer[i].Value) ? 1 : 0));
            }
            statisticFile.WriteLine(last + "," + categoryCorrectCount / categoryCount);
            statisticFile.WriteLine("total," + totalCorrectCount / totalCount);
            resultFile.Close();
            statisticFile.Close();
        }

        private static List<KeyValuePair<int, int>> runKnnTest(KNN knn, string testPath, string logPath, Dictionary<string, int> dictionary, Dictionary<string, double> wordIDFDictionary, Hashtable stopWordTable, ref List<string> testFileNameList, NLPAdapter nlpAdapter)
        {
            string[] categories = Directory.GetDirectories(testPath);
            List<KeyValuePair<int, int>> testAnswer = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < categories.Length; i++) //traverse Categories
            {
                Console.WriteLine(categories[i]);
                string[] files = Directory.GetFiles(categories[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    int testResult = -1;
                    testFileNameList.Add(Path.GetFileName(files[j]));
                    Dictionary<string, double> docWordDic = readDoc(files[j], stopWordTable, nlpAdapter);
                    docWordDic = docCooccurrence(docWordDic, dictionary);
                    testResult = knn.test(docWordDic, dictionary, wordIDFDictionary);
                    testAnswer.Add(new KeyValuePair<int, int>(testResult, i));
                    Console.WriteLine(testResult + "," + i);
                }
            }
            return testAnswer;
        }

        private static List<KeyValuePair<int, int>> runSVMTest(SVMAdapter svmAdapter, string testPath, string logPath, Dictionary<string, int> dictionary, Dictionary<string, double> wordIDFDictionary, Hashtable stopWordTable, ref List<string> testFileNameList, svm_model model, NLPAdapter nlpAdapter)
        {
            string[] categories = Directory.GetDirectories(testPath);
            List<KeyValuePair<int, int>> testAnswer = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < categories.Length; i++) //traverse Categories
            {
                Console.WriteLine(categories[i]);
                string[] files = Directory.GetFiles(categories[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    int testResult = -1;
                    testFileNameList.Add(Path.GetFileName(files[j]));
                    testResult = svmAdapter.runSVMTest(readDoc(files[j], stopWordTable, nlpAdapter), dictionary, wordIDFDictionary, model);
                    testAnswer.Add(new KeyValuePair<int, int>(testResult, i));
                    Console.WriteLine(testResult + "," + i);
                }
            }

            return testAnswer;
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

        private static void trainModel(string trainPath, string logPath, ref List<Dictionary<string, double>> docWordDicList, ref Dictionary<string, int> dictionary, int dicSize, ref List<int> trainingAnswer, ref Dictionary<string, double> wordIDFDictionary, Hashtable stopwordTable, NLPAdapter nlpAdapter)
        {
            List<Dictionary<string, double>> categoryWordCountList = new List<Dictionary<string, double>>();
            Dictionary<string, int> tempDictionary = new Dictionary<string, int>();
            string[] categories = Directory.GetDirectories(trainPath);
            for (int i = 0; i < categories.Length; i++) //traverse Categories, generate traingAnswer
            {
                categoryWordCountList.Add(readCategory(categories[i], i, ref docWordDicList, ref trainingAnswer, stopwordTable, nlpAdapter));
            }

            // generate wordIDFDictionary
            for (int i = 0; i < categoryWordCountList.Count(); i++)
            {
                foreach (string word in categoryWordCountList[i].Keys)
                {
                    if (wordIDFDictionary.ContainsKey(word))
                    {
                        wordIDFDictionary[word] += 1;
                    }
                    else
                    {
                        wordIDFDictionary.Add(word, 1);
                    }
                }
            }
            string[] keys = wordIDFDictionary.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                wordIDFDictionary[keys[i]] = Math.Log(categoryWordCountList.Count() / wordIDFDictionary[keys[i]]);
            }

            // generate dictionary
            List<List<KeyValuePair<string, double>>> sortedCategoryTFIDFList = new List<List<KeyValuePair<string, double>>>();
            StreamWriter dicFile = new StreamWriter(logPath + "\\" + "dictionary.csv");
            int dicCount = 0;
            for (int i = 0; i < categoryWordCountList.Count(); i++)
            {
                string[] words = categoryWordCountList[i].Keys.ToArray();
                double categoryWordCountSum = 0;
                List<KeyValuePair<string, double>> sortedCategoryTFIDF = new List<KeyValuePair<string, double>>();
                for (int j = 0; j < words.Length; j++)
                {
                    categoryWordCountSum += categoryWordCountList[i][words[j]];
                }
                for (int j = 0; j < words.Length; j++)
                {
                    sortedCategoryTFIDF.Add(new KeyValuePair<string, double>(words[j], (categoryWordCountList[i][words[j]] / categoryWordCountSum) * wordIDFDictionary[words[j]]));//category TFIDF
                }
                //sortedCategoryTFIDF = categoryWordCountList[i].ToList();
                sortedCategoryTFIDF.Sort((a, b) => b.Value.CompareTo(a.Value));
                sortedCategoryTFIDFList.Add(sortedCategoryTFIDF);
            }
            for (int i = 0; i < dicSize * 2; i++)
            {
                for (int j = 0; j < sortedCategoryTFIDFList.Count(); j++)
                {
                    if (dicCount >= dicSize * 2)
                    {
                        break;
                    }
                    if (!tempDictionary.ContainsKey(sortedCategoryTFIDFList[j][i].Key))
                    {
                        dicFile.WriteLine(sortedCategoryTFIDFList[j][i].Key + "," + sortedCategoryTFIDFList[j][i].Value);
                        tempDictionary.Add(sortedCategoryTFIDFList[j][i].Key, dicCount);
                        dicCount++;
                    }
                }
                if (dicCount >= dicSize * 2)
                {
                    dicFile.Close();
                    break;
                }
            }

            dictionary = trainCooccurrence(logPath, ref categoryWordCountList, ref docWordDicList, ref wordIDFDictionary, trainingAnswer, tempDictionary, dicSize);
            //generate docWordDicList
            for (int i = 0; i < docWordDicList.Count(); i++)
            {
                string[] words = docWordDicList[i].Keys.ToArray();
                double docWordCountSum = 0;
                for (int j = 0; j < words.Length; j++)
                {
                    docWordCountSum += docWordDicList[i][words[j]];
                }
                for (int j = 0; j < words.Length; j++)
                {
                    if (docWordDicList[i][words[j]] != 0)
                    {
                        docWordDicList[i][words[j]] = (docWordDicList[i][words[j]] / docWordCountSum) * wordIDFDictionary[words[j]];//docWordDic TFIDF
                    }
                }
            }
        }

        private static Dictionary<string, int> trainCooccurrence(string logPath, ref List<Dictionary<string, double>> categoryWordCountList, ref List<Dictionary<string, double>> docWordDicList, ref Dictionary<string, double> wordIDFDictionary, List<int> trainingAnswer, Dictionary<string, int> tempDictionary, int dicSize)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            //add co-word to docWordDicList
            for (int i = 0; i < docWordDicList.Count(); i++)
            {
                docWordDicList[i] = docCooccurrence(docWordDicList[i], tempDictionary);
            }

            //add co-word to categoryWordCountList
            for (int i = 0; i < docWordDicList.Count(); i++) //every document
            {
                string[] words = docWordDicList[i].Keys.ToArray();
                for (int j = 0; j < words.Length; j++) // every word
                {
                    if (words[j].Contains(' '))
                    {
                        if (categoryWordCountList[trainingAnswer[i]].ContainsKey(words[j]))
                        {
                            categoryWordCountList[trainingAnswer[i]][words[j]] += 1;
                        }
                        else
                        {
                            categoryWordCountList[trainingAnswer[i]].Add(words[j], 1);
                        }
                    }
                }
            }
            //add co-word to wordIDFDictionary
            for (int i = 0; i < categoryWordCountList.Count(); i++)
            {
                foreach (string word in categoryWordCountList[i].Keys)
                {
                    if (word.Contains(' '))
                    {
                        if (wordIDFDictionary.ContainsKey(word))
                        {
                            wordIDFDictionary[word] += 1;
                        }
                        else
                        {
                            wordIDFDictionary.Add(word, 1);
                        }
                    }
                }
            }
            string[] keys = wordIDFDictionary.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].Contains(' '))
                {
                    wordIDFDictionary[keys[i]] = Math.Log(categoryWordCountList.Count() / wordIDFDictionary[keys[i]]);
                }
            }

            // generate dictionary
            StreamWriter dicFile = new StreamWriter(logPath + "\\" + "co-dictionary.csv");
            List<List<KeyValuePair<string, double>>> sortedCategoryTFIDFList = new List<List<KeyValuePair<string, double>>>();
            int dicCount = 0;
            for (int i = 0; i < categoryWordCountList.Count(); i++)
            {
                string[] words = categoryWordCountList[i].Keys.ToArray();
                double categoryWordCountSum = 0;
                List<KeyValuePair<string, double>> sortedCategoryTFIDF = new List<KeyValuePair<string, double>>();
                for (int j = 0; j < words.Length; j++)
                {
                    categoryWordCountSum += categoryWordCountList[i][words[j]];
                }
                for (int j = 0; j < words.Length; j++)
                {
                    sortedCategoryTFIDF.Add(new KeyValuePair<string, double>(words[j], (categoryWordCountList[i][words[j]] / categoryWordCountSum) * wordIDFDictionary[words[j]]));//category TFIDF
                }
                //sortedCategoryTFIDF = categoryWordCountList[i].ToList();
                sortedCategoryTFIDF.Sort((a, b) => b.Value.CompareTo(a.Value));
                sortedCategoryTFIDFList.Add(sortedCategoryTFIDF);
            }
            for (int i = 0; i < dicSize; i++)
            {
                for (int j = 0; j < sortedCategoryTFIDFList.Count(); j++)
                {
                    if (dicCount >= dicSize)
                    {
                        break;
                    }
                    if (!dictionary.ContainsKey(sortedCategoryTFIDFList[j][i].Key))
                    {
                        dicFile.WriteLine(sortedCategoryTFIDFList[j][i].Key + "," + sortedCategoryTFIDFList[j][i].Value);
                        dictionary.Add(sortedCategoryTFIDFList[j][i].Key, dicCount);
                        dicCount++;
                    }
                }
                if (dicCount >= dicSize)
                {
                    dicFile.Close();
                    break;
                }
            }
            return dictionary;
        }

        private static Dictionary<string, double> docCooccurrence(Dictionary<string, double> docWordDic, Dictionary<string, int> dictionary)
        {
            string[] words = docWordDic.Keys.ToArray();
            for (int j = 0; j < words.Length; j++)
            {
                if (dictionary.ContainsKey(words[j]))
                {
                    for (int k = 0; k < words.Length; k++)
                    {
                        if (j != k)
                        {
                            if (dictionary.ContainsKey(words[k]))
                            {
                                docWordDic.Add(words[j] + " " + words[k], docWordDic[words[j]]);
                            }
                        }
                    }
                }
            }
            return docWordDic;
        }

        private static Dictionary<string, double> readCategory(string path, int categoryIndex, ref List<Dictionary<string, double>> docWordDicList, ref List<int> trainingAnswer, Hashtable stopwordTable, NLPAdapter nlpAdapter)
        {
            Dictionary<string, double> categoryWordCount = new Dictionary<string, double>();
            Dictionary<string, double> docWordCount = new Dictionary<string, double>();
            string[] docs = Directory.GetFiles(path);
            for (int i = 0; i < docs.Length; i++)
            {
                trainingAnswer.Add(categoryIndex);
                docWordCount = readDoc(docs[i], stopwordTable, nlpAdapter);
                docWordDicList.Add(docWordCount);
                foreach (string word in docWordCount.Keys)
                {
                    if (categoryWordCount.ContainsKey(word))
                    {
                        if (docWordCount[word] != 0)
                        {
                            //categoryWordCount[word] += docWordCount[word];
                            categoryWordCount[word] += 1;
                        }
                    }
                    else
                    {
                        if (docWordCount[word] != 0)
                        {
                            //categoryWordCount.Add(word, docWordCount[word]);
                            categoryWordCount.Add(word, 1);
                        }
                    }
                }
            }
            return categoryWordCount;
        }

        private static Dictionary<string, double> readDoc(string path, Hashtable stopwordTable, NLPAdapter nlpAdapter)
        {
            Dictionary<string, double> docWordCount = new Dictionary<string, double>();
            StreamReader docFile = new StreamReader(path);
            string line;

            Dictionary<String, String> posTags = new Dictionary<String, String>();
            posTags.Add(NLPAdapter.POS_TAG_NN, NLPAdapter.POS_TAG_NN);
            posTags.Add(NLPAdapter.POS_TAG_NNS, NLPAdapter.POS_TAG_NNS);
            posTags.Add(NLPAdapter.POS_TAG_NNP, NLPAdapter.POS_TAG_NNP);
            posTags.Add(NLPAdapter.POS_TAG_NNPS, NLPAdapter.POS_TAG_NNPS);

            while ((line = docFile.ReadLine()) != null)
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
                    else if (column == "path")
                    {
                        foreach (string iter_word in splitPath(content))
                        {
                            string word = getWord(iter_word, stopwordTable);
                            //word cleansing done
                            if (word != null)
                            {
                                if (docWordCount.ContainsKey(word))
                                {
                                    docWordCount[word] += weight["path"];
                                }
                                else
                                {
                                    docWordCount.Add(word, weight["path"]);
                                }
                            }
                        }
                    }
                    else if (column == "newsgroups")
                    {
                        foreach (string iter_word in splitNewsgroup(content))
                        {
                            string word = getWord(iter_word, stopwordTable);
                            //word cleansing done
                            if (word != null)
                            {
                                if (docWordCount.ContainsKey(word))
                                {
                                    docWordCount[word] += weight["newsgroups"];
                                }
                                else
                                {
                                    docWordCount.Add(word, weight["newsgroups"]);
                                }
                            }
                        }
                    }
                    else if (column == "from")
                    {
                        foreach (string iter_word in getEmail(content))
                        {
                            string word = getWord(iter_word, stopwordTable);
                            //word cleansing done
                            if (word != null)
                            {
                                if (docWordCount.ContainsKey(word))
                                {
                                    docWordCount[word] += weight["from"];
                                }
                                else
                                {
                                    docWordCount.Add(word, weight["from"]);
                                }
                            }
                        }
                    }
                    else
                    {
                        content = processSpecialField(content, ref docWordCount);
                        foreach (string iter_word in splitLine(content))
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
                    }
                }
                else
                {
                    if (line.Length != 0)
                    {
                        //return docWordCount;
                        break;
                    }
                }
            }

            while (line != null)
            {
                line = processSpecialField(line, ref docWordCount);

#if USE_POSTAG
                // Sango: just left noun.
                line = nlpAdapter.getFilterResult(line, posTags);
#endif

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

        private static IEnumerable<string> splitNewsgroup(string content)
        {
            return content.Split(new char[] { ',' });
        }

        private static IEnumerable<string> splitPath(string content)
        {
            return content.Split(new char[] { '!' });
        }


        private static string processSpecialField(string line, ref Dictionary<string, double> docWordCount)
        {
            string[] emails = getEmail(line);
            string result;
            for (int i = 0; i < emails.Length; i++)
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
            return Regex.Split(line, @"[^A-Za-z0-9_\-\.]");
        }

        private static string getWord(string word, Hashtable stopwordTable)
        {
            Stemmer stemmer = new Stemmer();
            string result = word.ToLower().Trim(new Char[] { '_', '-', '.' });
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
