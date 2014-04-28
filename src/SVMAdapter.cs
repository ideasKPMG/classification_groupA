using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libsvm;
using System.Collections;
using System.IO;

namespace project1_0422
{
    class SVMAdapter
    {
        public static double SVM_C_DEFAULT = 1;
        public static double SVM_GAMMA_DEFAULT = 0.0001;

        static String SVM_MODEL_FILE_NAME = @"sango_svm.model";
        static String SVM_TRAIN_FILE_NAME = @"train.txt";

        public svm_model initial(List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer, double c, double gamma)
        {
            Console.WriteLine("==> Starting training...");

            svm_problem prob = gen_svm_training_data(docWordDicList, dictionary, trainingAnswer);
            svm_parameter param = new svm_parameter();

            // set the default setting value
            param.svm_type = svm_parameter.C_SVC;
            param.kernel_type = svm_parameter.RBF;
            param.degree = 3;
            param.gamma = gamma;
            param.coef0 = 0;
            param.nu = 0.5;
            param.cache_size = 100;
            param.C = c;
            param.eps = 1e-3;
            param.p = 0.1;
            param.shrinking = 1;
            param.probability = 0;
            param.nr_weight = 0;

            svm_model model = svm.svm_train(prob, param);
            svm.svm_save_model(SVM_MODEL_FILE_NAME, model);

            Console.WriteLine("==> Training done!!");
            return model;
        }

        private svm_problem gen_svm_training_data(List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer)
        {
            var prob = new svm_problem();
            var vy = new List<double>(); // label list
            var vx = new List<svm_node[]>(); // node list

            StreamWriter file = new StreamWriter(SVM_TRAIN_FILE_NAME);


            for (int i = 0; i < docWordDicList.Count; i++)
            {
                String trainStr = trainingAnswer[i] + "";
                List<KeyValuePair<int, double>> nodeList = new List<KeyValuePair<int, double>>();
                List<string> wordList = docWordDicList[i].Keys.ToList();

                foreach (string word in wordList)
                {
                    if (dictionary.ContainsKey(word))
                    {
                        int theIndex = dictionary[word];
                        double theValue = (double)docWordDicList[i][word];

                        nodeList.Add(new KeyValuePair<int, double>(theIndex, theValue));
                    }
                }

                if (nodeList.Count > 0)
                {
                    List<svm_node> x = new List<svm_node>();
                    

                    nodeList.Sort(
                        delegate(KeyValuePair<int, double> firstPair,
                        KeyValuePair<int, double> nextPair)
                        {
                            int a = firstPair.Key;
                            int b = nextPair.Key;

                            return a.CompareTo(b);
                        }
                    );

                    double labelValue = (double)trainingAnswer[i];

                    for (int k = 0; k < nodeList.Count; k++)
                    {
                        KeyValuePair<int, double> node = nodeList[k];

                        x.Add(new svm_node() // svm node
                        {
                            index = node.Key,
                            value = node.Value,
                        });

                        Console.WriteLine(@"## train data - label:{2}, index:{0}, value:{1}", node.Key, node.Value, labelValue);

                        // Sango : just for TEST to  output the node to file
                        String theIndex = System.Convert.ToString(node.Key);
                        String theValue = System.Convert.ToString(node.Value);
                        trainStr = trainStr + " " + theIndex + ":" + theValue;
                    }
                    file.WriteLine(trainStr);


                    vy.Add((double)trainingAnswer[i]); // label
                    vx.Add(x.ToArray());
                }

                //Console.WriteLine("## get new data:" + trainStr);
            }

            file.Close();

            prob.l = vy.Count;
            prob.x = vx.ToArray();
            prob.y = vy.ToArray();

            return prob;
        }

        public int test(Dictionary<string, double> docWordDic, Dictionary<string, int> dictionary, Dictionary<string, double> wordIDFDictionary)
        {

            return 0;
        }

        public svm_model getSVMModel(List<Dictionary<string, double>> docWordDicList, Dictionary<string, int> dictionary, List<int> trainingAnswer, double c, double gamma)
        {
            svm_model model;

            try
            {
                model = svm.svm_load_model(SVM_MODEL_FILE_NAME);
                Console.WriteLine("==> SVM model found!!");
            }
            catch (Exception e)
            {
                Console.WriteLine("==> SVM model not found...");
                model = initial(docWordDicList, dictionary, trainingAnswer, c, gamma);
            }

            return model;
        }

        public int runSVMTest(Dictionary<string, double> docWord, Dictionary<string, int> dictionary, Dictionary<string, double> wordIDFDictionary, svm_model model)
        {
            List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();

            // generate svm nodes
            List<KeyValuePair<string, string>> nodeList = new List<KeyValuePair<string, string>>();
            List<svm_node> nodes = new List<svm_node>();
            foreach (string word in docWord.Keys)
            {
                
                if (dictionary.ContainsKey(word))
                {
                    int index = dictionary[word];
                    double TF = docWord[word];
                    double IDF = wordIDFDictionary[word];
                    double value = TF * IDF;
                    nodes.Add(new svm_node() // svm node
                    {
                        index = index,
                        value = value,
                    });
                    nodeList.Add(new KeyValuePair<string, string>(System.Convert.ToString(index), System.Convert.ToString(value)));
                    //trainStr = trainStr + "\t" + index + ":" + value;
                }
            }

            if (nodeList.Count > 0)
            {
                // sort the list by feature index at first
                nodeList.Sort(
                    delegate(KeyValuePair<string, string> firstPair,
                    KeyValuePair<string, string> nextPair)
                    {
                        int a = int.Parse(firstPair.Key);
                        int b = int.Parse(nextPair.Key);

                        return a.CompareTo(b);
                    }
                );

                for (int k = 0; k < nodeList.Count; k++)
                {
                    KeyValuePair<string, string> node = nodeList[k];
                    String index = node.Key;
                    String value = node.Value;
                }

            }

            int resValue = (int)svm.svm_predict(model, nodes.ToArray());
            return resValue;
        }
    }
}
