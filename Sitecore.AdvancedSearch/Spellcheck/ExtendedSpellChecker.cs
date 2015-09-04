using Lucene.Net.Store;
namespace Sitecore.AdvancedSearch.Spellcheck
{
    public class ExtendedSpellChecker : SpellChecker.Net.Search.Spell.SpellChecker
    {
        public ExtendedSpellChecker(Directory spellIndex) : base(spellIndex)
        {
        }

        //public override string[] SuggestSimilar(string word, int numSug, IndexReader ir, string field, bool morePopular)
        //{
        //    string[] strArrays;
        //    IndexSearcher indexSearcher = this.ObtainSearcher();
        //    try
        //    {
        //        float single = this.minScore;
        //        int length = word.Length;
        //        int num = (ir == null || field == null ? 0 : ir.DocFreq(new Term(field, word)));
        //        int num1 = (!morePopular || ir == null || field == null ? 0 : num);
        //        if (morePopular || num <= 0)
        //        {
        //            BooleanQuery booleanQueries = new BooleanQuery();
        //            HashSet<string> strs = new HashSet<string>();
        //            for (int i = this.GetMin(length); i <= this.GetMax(length); i++)
        //            {
        //                string str = string.Concat("gram", i);
        //                string[] strArrays1 = SpellChecker.FormGrams(word, i);
        //                if ((int)strArrays1.Length != 0)
        //                {
        //                    SpellChecker.Add(booleanQueries, string.Concat("start", i), strArrays1[0], 2f);
        //                    SpellChecker.Add(booleanQueries, string.Concat("end", i), strArrays1[(int)strArrays1.Length - 1], 1f);
        //                    for (int j = 0; j < (int)strArrays1.Length; j++)
        //                    {
        //                        SpellChecker.Add(booleanQueries, str, strArrays1[j]);
        //                    }
        //                }
        //            }
        //            int num2 = 10 * numSug;
        //            ScoreDoc[] scoreDocs = indexSearcher.Search(booleanQueries, null, num2).ScoreDocs;
        //            SuggestWordQueue suggestWordQueue = new SuggestWordQueue(numSug);
        //            int num3 = Math.Min((int)scoreDocs.Length, num2);
        //            SuggestWord suggestWord = new SuggestWord();
        //            for (int k = 0; k < num3; k++)
        //            {
        //                suggestWord.termString = indexSearcher.Doc(scoreDocs[k].Doc).Get("word");
        //                if (!suggestWord.termString.Equals(word))
        //                {
        //                    suggestWord.score = this.sd.GetDistance(word, suggestWord.termString);
        //                    if (suggestWord.score >= single)
        //                    {
        //                        if (ir != null && field != null)
        //                        {
        //                            suggestWord.freq = ir.DocFreq(new Term(field, suggestWord.termString));
        //                            if (morePopular && num1 > suggestWord.freq || suggestWord.freq < 1)
        //                            {
        //                                goto Label0;
        //                            }
        //                        }
        //                        if (strs.Add(suggestWord.termString))
        //                        {
        //                            suggestWordQueue.InsertWithOverflow(suggestWord);
        //                            if (suggestWordQueue.Size() == numSug)
        //                            {
        //                                single = suggestWordQueue.Top().score;
        //                            }
        //                            suggestWord = new SuggestWord();
        //                        }
        //                    }
        //                }
        //            Label0:
        //            }
        //            string[] strArrays2 = new string[suggestWordQueue.Size()];
        //            for (int l = suggestWordQueue.Size() - 1; l >= 0; l--)
        //            {
        //                strArrays2[l] = suggestWordQueue.Pop().termString;
        //            }
        //            strArrays = strArrays2;
        //        }
        //        else
        //        {
        //            strArrays = new string[] { word };
        //        }
        //    }
        //    finally
        //    {
        //        this.ReleaseSearcher(indexSearcher);
        //    }
        //    return strArrays;
        //}
    }
}
