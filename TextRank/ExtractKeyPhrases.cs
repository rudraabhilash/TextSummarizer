using System;
using System.Collections.Generic;
using System.Linq;
using OpenNLP.Tools.PosTagger;
using OpenNLP.Tools.SentenceDetect;
using OpenNLP.Tools.Tokenize;
using PageRank.Graph;
using PageRank.Rank;

namespace TextRank
{
    public class ExtractKeyPhrases
    {
        private readonly string _modelPath;
        private readonly EnglishRuleBasedTokenizer _tokenizer;
        private readonly EnglishMaximumEntropySentenceDetector _sentence_tokenizer;

        private static readonly IList<string> _requiredTags = new List<string> {"NN", "JJ", "NNP",};

        public ExtractKeyPhrases()
        {
            _modelPath = AppDomain.CurrentDomain.BaseDirectory + "../../Resources/Models/";
            _tokenizer = new EnglishRuleBasedTokenizer(false);
            _sentence_tokenizer = new EnglishMaximumEntropySentenceDetector(_modelPath + "/EnglishSD.nbin");
        }

        private IList<Tuple<string, string>> GetFilteredTokens(IList<Tuple<string, string>> taggedTokens)
        {
            for (int i = taggedTokens.Count - 1; i >= 0; i--)
            {
                if (_requiredTags.All(x => x != taggedTokens[i].Item2))
                {
                    taggedTokens.RemoveAt(i);
                }
            }
            return taggedTokens;
        }

        private IList<Tuple<string, string>> GetPosTaggedTokens(string sentence)
        {
            var posTagger =
                new EnglishMaximumEntropyPosTagger(_modelPath + "/EnglishPOS.nbin", _modelPath + @"/Parser/tagdict");
            var tokens = _tokenizer.Tokenize(sentence);
            var taggedList = posTagger.Tag(tokens);
            IList<Tuple<string, string>> tagged = new List<Tuple<string, string>>();
            for (int i = 0; i < tokens.Length; i++)
            {
                tagged.Add(Tuple.Create(tokens[i], taggedList[i]));
            }
            return GetFilteredTokens(tagged);
        }

        //ref: stackoverflow.com/questions/1952153/
        private static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1) return list.Select(t => new[] {t});
            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new[] {t2}));
        }

        private IList<string> GetNormalizedList(IList<string> words)
        {
            return words.Select(x => x.Replace(".", "")).ToList();
        }

        private IList<Tuple<string, string>> BuildGraph(IList<string> words)
        {
            return GetKCombs(words, 2).Select(x =>
            {
                var enumerable = x as string[] ?? x.ToArray();
                return new Tuple<string, string>(enumerable.ToList()[0], enumerable.ToList()[1]);
            }).ToList();
        }

        private IList<string> GetNormalizedUniqueWordList(IList<Tuple<string, string>> keywords)
        {
            return GetNormalizedList(keywords.Select(x => x.Item1).ToList()).Distinct().ToList();
        }

        public void Extract(string sentence)
        {
            var pqr1 = new List<string>()
                {"video", "’", "own", "Pigskin", "survival-horror", "big", "Z-Connect", "today", "technology", "Zenith", "event", "Radiation", "Nov", "price", "Sony", "Gamespace", "silver-and-gray", "thriller", "“", "s", "CEO", "Xbox", "hitting", "‘", "game", "next-generation", "such", "release", "press", "”", "double-analog-stick", "way", "Microsoft", "Cris", "world", "sleek", "month", "Michael", "MoonChaser", "Pro", "box", "One", "Move", "player", "IL", "internet", "InZomnia", "system", "launch", "ability", "Collinsworth", "manufacturer", "LINCOLNSHIRE", "Ahn", "Playstation", "console"};

            var taggedList = GetPosTaggedTokens(sentence);
            var uniqueWords = BuildGraph(pqr1);
            var levDistance = new LevenhteinDistance();
            IList<Tuple<string, string, int>> graph = uniqueWords.Select(x =>
                new Tuple<string, string, int>(x.Item1, x.Item2, levDistance.Calculate(x.Item1, x.Item2))).ToList();

            var dg = new DirectedGraph<string>();

            foreach (var node in graph)
            {
                dg.AddEdge(node.Item1, node.Item2, (double)node.Item3);
            }
            var a = new PageRank<string>();
            var rankedDictionary = a.Rank(dg, maxItteration: 100);
            var q = rankedDictionary.ToList().OrderByDescending(p => p.Value).ToList();

            var pqr = _sentence_tokenizer.SentenceDetect(
                @"To test easily the various NLP tools, run the ToolsExample winform project. You'll find below a more detailed description of the tools and how code snippets to use them directly in your code. All NLP tools based on the maxent algorithm need model files to run. You'll find those files for English in Resources/Models. If you want to train your own models (to improve precision on English or to use those tools on other languages), please refer to the last section."
                    .Trim());

            
        }

        public IList<string> JoinAdjacentWords(IList<string> wordList, IList<string> keywordsList)
        {
            if (wordList.Count == keywordsList.Count)
                return null;

            var modifiedPhrases = new HashSet<string>();

            var dealtWith = new HashSet<string>();

            string firstWord = null , secondWord = null;

            for (int i = 0, j = 1; j < wordList.Count; i++, j++, firstWord = wordList[i], secondWord = wordList[j])
            {

                if (keywordsList.Contains(firstWord) && keywordsList.Contains(secondWord))
                {
                    modifiedPhrases.Add($"{firstWord} {secondWord}");
                    dealtWith.AddMultipleElements<string>(firstWord, secondWord);
                }
                else
                {
                    if (keywordsList.Contains(firstWord) && !dealtWith.Contains(firstWord))
                        modifiedPhrases.Add(firstWord);

                    //Last Word condition
                    if (j == wordList.Count - 1 && keywordsList.Contains(secondWord) && !dealtWith.Contains(secondWord))
                        modifiedPhrases.Add(secondWord);
                }
            }

            return modifiedPhrases.ToList();
        }

    }
}

public static class ExtentionMethod
{
    public static HashSet<T> AddMultipleElements<T>(this HashSet<T> set, T firstWord, T secondWord)
    {
        set.Add(firstWord);
        set.Add(secondWord);
        return set;
    }
}
    


