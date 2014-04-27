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
            Hashtable stopWordTable = genStopwordTable(@"D:\work\KPMG\learning\project1\stopword.txt");
            List<string> testFileNameList = new List<string>();
            int dicSize = 1000;
            trainModel(@"D:\work\KPMG\learning\classification\project1_0422\test_data\1\Training",
                       @"D:\work\KPMG\learning\classification\project1_0422\log",
                       ref docWordDicList,
                       ref dictionary,
                       dicSize,
                       ref trainingAnswer,
                       ref wordIDFDictionary,
                       stopWordTable
                );
            KNN knn = new KNN();
            knn.set(dicSize,docWordDicList.Count());
            knn.initial(docWordDicList,dictionary,trainingAnswer);
            knn.train(3, 20);
            knn.getAveDistance();

            //knn.genLog(@"D:\work\KPMG\learning\classification\project1_0422\log");
            List<KeyValuePair<int,int>> testAnswer = runKnnTest(knn, @"D:\work\KPMG\learning\classification\project1_0422\test_data\1\Testing", @"D:\work\KPMG\learning\classification\project1_0422\test_data\log", dictionary, wordIDFDictionary, stopWordTable,ref testFileNameList);
            genStatistic(testAnswer, testFileNameList , @"D:\work\KPMG\learning\classification\project1_0422\log");
        }

        private static void genStatistic(List<KeyValuePair<int, int>> testAnswer, List<string> testFileNameList, string logPath)
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
                    statisticFile.WriteLine(last+","+categoryCorrectCount/categoryCount);
                    categoryCount = 0;
                    categoryCorrectCount = 0;
                    last = testAnswer[i].Value;
                }
                totalCount += 1;
                categoryCount += 1;
                totalCorrectCount += (testAnswer[i].Key == testAnswer[i].Value) ? 1 : 0;
                categoryCorrectCount += (testAnswer[i].Key == testAnswer[i].Value) ? 1 : 0;
                resultFile.WriteLine(testFileNameList[i]+ "," + testAnswer[i].Key + "," + testAnswer[i].Value + "," + ((testAnswer[i].Key == testAnswer[i].Value) ? 1 : 0));
            }
            statisticFile.WriteLine(last + "," + categoryCorrectCount / categoryCount);
            statisticFile.WriteLine("total," + totalCorrectCount/totalCount);
            resultFile.Close();
            statisticFile.Close();
        }

        private static List<KeyValuePair<int,int>> runKnnTest(KNN knn, string testPath, string logPath, Dictionary<string, int> dictionary, Dictionary<string, double> wordIDFDictionary, Hashtable stopWordTable,ref List<string> testFileNameList)
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
                    testResult = knn.test(readDoc(files[j],stopWordTable),dictionary,wordIDFDictionary);
                    testAnswer.Add(new KeyValuePair<int,int>(testResult,i));
                    Console.WriteLine(testResult+","+i);
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

        private static void trainModel(string trainPath, string logPath, ref List<Dictionary<string, double>> docWordDicList, ref Dictionary<string, int> dictionary,int dicSize, ref List<int> trainingAnswer, ref Dictionary<string, double> wordIDFDictionary,Hashtable stopwordTable)
        {
            List<Dictionary<string, double>> categoryWordCountList = new List<Dictionary<string, double>>();
            Dictionary<string, int> tempDictionary = new Dictionary<string, int>();
            string[] categories = Directory.GetDirectories(trainPath);
            for (int i = 0; i < categories.Length; i++) //traverse Categories, generate traingAnswer
            {
                categoryWordCountList.Add(readCategory(categories[i],i, ref docWordDicList, ref trainingAnswer, stopwordTable));
            }

            // generate wordIDFDictionary
            for(int i=0;i<categoryWordCountList.Count();i++)
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
            for (int i = 0; i < keys.Length;i++ ) 
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
                    sortedCategoryTFIDF.Add(new KeyValuePair<string,double>(words[j],(categoryWordCountList[i][words[j]] / categoryWordCountSum) * wordIDFDictionary[words[j]]));//category TFIDF
                }
                //sortedCategoryTFIDF = categoryWordCountList[i].ToList();
                sortedCategoryTFIDF.Sort((a,b) => b.Value.CompareTo(a.Value));
                sortedCategoryTFIDFList.Add(sortedCategoryTFIDF);
            }
            for (int i = 0; i < dicSize*2; i++)
            {
                for (int j = 0; j < sortedCategoryTFIDFList.Count(); j++)
                {
                    if (dicCount >= dicSize*2)
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
                if (dicCount >= dicSize*2)
                {
                    dicFile.Close();
                    break;
                }
            }
            dictionary = coOccurrence(ref categoryWordCountList, ref docWordDicList, ref wordIDFDictionary, tempDictionary, dicSize);
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

        private static Dictionary<string, int> coOccurrence(ref List<Dictionary<string, double>> categoryWordCountList, ref List<Dictionary<string, double>> docWordDicList, ref Dictionary<string, double> wordIDFDictionary, Dictionary<string, int> tempDictionary, int dicSize)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            //add co-word to docWordDicList
            for (int i = 0; i < docWordDicList.Count(); i++)
            {
                string[] words = docWordDicList[i].Keys.ToArray();
                for (int j = 0; j < words.Length; j++)
                {
                    if (tempDictionary.ContainsKey(words[j]))
                    {
                        for (int k = 0; k < words.Length; k++)
                        {
                            if (j != k)
                            {
                                if (tempDictionary.ContainsKey(words[k]))
                                {
                                    docWordDicList[i].Add(words[j] + " " + words[k], docWordDicList[i][words[j]]);
                                }
                            }
                        }
                    }
                }
            }
            //add co-word to categoryWordCountList
            for (int i = 0; i < categoryWordCountList.Count(); i++)
            {
                string[] words = categoryWordCountList[i].Keys.ToArray();
                for (int j = 0; j < words.Length; j++)
                {
                    if (tempDictionary.ContainsKey(words[j]))
                    {
                        for (int k = 0; k < words.Length; k++)
                        {
                            if (j != k)
                            {
                                if (tempDictionary.ContainsKey(words[k]))
                                {
                                    categoryWordCountList[i].Add(words[j] + " " + words[k], categoryWordCountList[i][words[j]]);
                                }
                            }
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
            return dictionary;
        }
        private static Dictionary<string, double> readCategory(string path,int categoryIndex,ref List<Dictionary<string, double>> docWordDicList,ref List<int> trainingAnswer,Hashtable stopwordTable)
        {
            Dictionary<string,double> categoryWordCount = new Dictionary<string,double>();
            Dictionary<string, double> docWordCount = new Dictionary<string, double>();
            string[] docs = Directory.GetFiles(path);
            for (int i = 0; i < docs.Length; i++)
            {
                trainingAnswer.Add(categoryIndex);
                docWordCount = readDoc(docs[i], stopwordTable);
                docWordDicList.Add(docWordCount);
                foreach (string word in docWordCount.Keys)
                {
                    if (categoryWordCount.ContainsKey(word))
                    {
                        if (docWordCount[word] != 0)
                        {
                            categoryWordCount[word] += docWordCount[word];
                        }
                    }
                    else
                    {
                        if (docWordCount[word] != 0)
                        {
                            categoryWordCount.Add(word, docWordCount[word]);
                        }
                    }
                }
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

        private static IEnumerable<string> splitNewsgroup(string content)
        {
            return content.Split(new char[]{','});
        }

        private static IEnumerable<string> splitPath(string content)
        {
            return content.Split(new char[]{'!'});
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
            return Regex.Split(line, @"[^A-Za-z0-9_\-\.]");
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
