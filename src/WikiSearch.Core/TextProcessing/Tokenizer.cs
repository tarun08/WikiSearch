using System.Text.RegularExpressions;

namespace WikiSearch.Core.TextProcessing
{
    public class Tokenizer
    {
        public static readonly Regex TokenRegex = new Regex(@"\p{L}[\p{L}\p{Nd}]*", RegexOptions.Compiled);

        public static IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            foreach (Match match in TokenRegex.Matches(text))
            {
                var token = match.Value.ToLowerInvariant();

                if (token.Length < 2)
                    continue;

                if (StopWordFilter.IsStopWord(token))
                    continue;

                yield return PorterStemmer.Stem(token);
            }
        }
    }
}
