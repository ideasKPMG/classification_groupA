using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using java.util;

namespace project1_0422
{
    class NLPAdapter
    {
        private OpenNLP.Tools.SentenceDetect.MaximumEntropySentenceDetector mSentenceDetector;
        private OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer mTokenizer;
        private OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger mPosTagger;
        String mModelPath;
        
        
        public static String POS_TAG_CC   = "CC";     //Coordinating conjunction
        public static String POS_TAG_CD   = "CD";     //Cardinal number
        public static String POS_TAG_DT   = "DT";     //Determiner
        public static String POS_TAG_EX   = "EX";     //Existential there
        public static String POS_TAG_FW   = "FW";     //Foreign word
        public static String POS_TAG_IN   = "IN";     //Preposition or subordinating conjunction
        public static String POS_TAG_JJ   = "JJ";     //Adjective
        public static String POS_TAG_JJR  = "JJR";    //Adjective, comparative
        public static String POS_TAG_JJS  = "JJS";    //Adjective, superlative
        public static String POS_TAG_LS   = "LS";     //List item marker
        public static String POS_TAG_MD   = "MD";     //Modal
        public static String POS_TAG_NN   = "NN";     //Noun, singular or mass
        public static String POS_TAG_NNS  = "NNS";    //Noun, plural
        public static String POS_TAG_NNP  = "NNP";    //Proper noun, singular
        public static String POS_TAG_NNPS = "NNPS";   //Proper noun, plural
        public static String POS_TAG_PDT  = "PDT";    //Predeterminer
        public static String POS_TAG_POS  = "POS";    //Possessive ending
        public static String POS_TAG_PRP  = "PRP";    //Personal pronoun
        public static String POS_TAG_PRP_S= "PRP$";   //Possessive pronoun
        public static String POS_TAG_RB   = "RB";     //Adverb
        public static String POS_TAG_RBR  = "RBR";    //Adverb, comparative
        public static String POS_TAG_RBS  = "RBS";    // Adverb, superlative
        public static String POS_TAG_RP   = "RP";     // Particle
        public static String POS_TAG_SYM  = "SYM";    // Symbol
        public static String POS_TAG_TO   = "TO";     // to
        public static String POS_TAG_UH   = "UH";     // Interjection
        public static String POS_TAG_VB   = "VB";     // Verb, base form
        public static String POS_TAG_VBD  = "VBD";    // Verb, past tense
        public static String POS_TAG_VBG  = "VBG";    // Verb, gerund or present participle
        public static String POS_TAG_VBN  = "VBN";    // Verb, past participle
        public static String POS_TAG_VBP  = "VBP";    // Verb, non­3rd person singular present
        public static String POS_TAG_VBZ  = "VBZ";    // Verb, 3rd person singular present
        public static String POS_TAG_WDT  = "WDT";    // Wh­determiner
        public static String POS_TAG_WP   = "WP";     // Wh­pronoun
        public static String POS_TAG_WP_S = "WP$";    // Possessive wh­pronoun
        public static String POS_TAG_WRB  = "WRB";    // Wh­adverb

        public NLPAdapter(String modelPath)
        {
            mModelPath = modelPath;
        }

        public String getFilterResult(String content, Dictionary<String, String> posTags)
        {
            StringBuilder output = new StringBuilder();

            string[] sentences = SplitSentences(content);
            
            foreach (string sentence in sentences)
            {
                string[] tokens = TokenizeSentence(sentence);
                string[] tags = PosTagTokens(tokens);

                for (int currentTag = 0; currentTag < tags.Length; currentTag++)
                {
                    if (posTags.ContainsKey(tags[currentTag]))
                    {
                        output.Append(" ").Append(tokens[currentTag]);
                    }
                }
            }

            return output.ToString();
        }

        private string[] SplitSentences(string paragraph)
        {
            if (mSentenceDetector == null)
            {
                mSentenceDetector = new OpenNLP.Tools.SentenceDetect.EnglishMaximumEntropySentenceDetector(mModelPath + @"\EnglishSD.nbin");
            }

            return mSentenceDetector.SentenceDetect(paragraph);
        }

        private string[] TokenizeSentence(string sentence)
        {
            if (mTokenizer == null)
            {
                mTokenizer = new OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer(mModelPath + @"\EnglishTok.nbin");
            }

            return mTokenizer.Tokenize(sentence);
        }

        private string[] PosTagTokens(string[] tokens)
        {
            if (mPosTagger == null)
            {
                mPosTagger = new OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger(mModelPath + @"\EnglishPOS.nbin", mModelPath + @"\Parser\tagdict");
            }

            return mPosTagger.Tag(tokens);
        }
    }
}
