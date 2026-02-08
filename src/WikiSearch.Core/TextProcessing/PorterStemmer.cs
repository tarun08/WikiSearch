namespace WikiSearch.Core.TextProcessing
{
    public static class PorterStemmer
    {
        private static readonly (string Suffix, string Replacement)[] Level1SuffixRules =
        {
            ("ational", "ate"),
            ("tional",  "tion"),
            ("enci",    "ence"),
            ("anci",    "ance"),
            ("izer",    "ize"),
            ("abli",    "able"),
            ("alli",    "al"),
            ("entli",   "ent"),
            ("eli",     "e"),
            ("ousli",   "ous"),
            ("ization", "ize"),
            ("ation",   "ate"),
            ("ator",    "ate"),
            ("iveness", "ive"),
            ("fulness", "ful"),
            ("ousness", "ous"),
            ("biliti",  "ble")
        };

        private static readonly (string Suffix, string Replacement)[] Level2SuffixRules =
        {
            ("icate", "ic"),
            ("ative", ""),
            ("alize", "al"),
            ("iciti", "ic"),
            ("ical", "ic"),
            ("ful", ""),
            ("ness", "")
        };


        public static string Stem(string word)
        {
            if (word.Length < 3)
                return word;

            word = word.ToLowerInvariant();

            word = RemovePluralSuffixes(word);
            word = RemovePastTenseSuffixes(word);
            word = NormalizeTerminalY(word);
            word = ReduceDerivationalSuffixesLevel1(word);
            word = ReduceDerivationalSuffixesLevel2(word);
            word = RemoveResidualSuffixes(word);
            word = CleanupTerminalEAndL(word);

            return word;
        }

        private static string RemovePluralSuffixes(string word)
        {
            if (word.EndsWith("sses"))
                return word.Substring(0, word.Length - 2);
            if (word.EndsWith("ies"))
                return word.Substring(0, word.Length - 2);
            if (word.EndsWith("ss"))
                return word;
            if (word.EndsWith("s"))
                return word.Substring(0, word.Length - 1);
            return word;
        }

        private static string RemovePastTenseSuffixes(string word)
        {
            if (word.EndsWith("eed"))
            {
                if (Measure(word.Substring(0, word.Length - 3)) > 0)
                    return word.Substring(0, word.Length - 1);
                return word;
            }
            if ((word.EndsWith("ed") && ContainsVowel(word.Substring(0, word.Length - 2))) ||
                (word.EndsWith("ing") && ContainsVowel(word.Substring(0, word.Length - 3))))
            {
                var stem = word.EndsWith("ed") ? word.Substring(0, word.Length - 2) : word.Substring(0, word.Length - 3);
                if (stem.EndsWith("at") || stem.EndsWith("bl") || stem.EndsWith("iz"))
                    return stem + "e";
                if (EndsWithDoubleConsonant(stem) && !stem.EndsWith("l") && !stem.EndsWith("s") && !stem.EndsWith("z"))
                    return stem.Substring(0, stem.Length - 1);
                if (Measure(stem) == 1 && IsCVC(stem))
                    return stem + "e";
                return stem;
            }
            return word;
        }

        private static int Measure(string word)
        {
            int count = 0;
            bool prevVowel = false;

            foreach (char c in word)
            {
                bool isVowel = IsVowel(c);
                if (isVowel && !prevVowel)
                    count++;
                prevVowel = isVowel;
            }

            return count - 1;
        }

        private static bool IsVowel(char c)
        {
            return "aeiou".IndexOf(c) >= 0;
        }

        private static bool ContainsVowel(string word)
        {
            return word.Any(IsVowel);
        }

        private static bool EndsWithDoubleConsonant(string word)
        {
            if (word.Length < 2)
                return false;
            char last = word[^1];
            char secondLast = word[^2];
            return last == secondLast && !IsVowel(last);
        }

        private static bool IsCVC(string word)
        {
            if (word.Length < 3)
                return false;
            char last = word[^1];
            char secondLast = word[^2];
            char thirdLast = word[^3];
            return !IsVowel(last) && IsVowel(secondLast) && !IsVowel(thirdLast) &&
                   (last != 'w' && last != 'x' && last != 'y');
        }

        private static string NormalizeTerminalY(string word)
        {
            if (word.EndsWith("y") && ContainsVowel(word.Substring(0, word.Length - 1)))
                return word.Substring(0, word.Length - 1) + "i";
            return word;
        }

        private static string ReduceDerivationalSuffixesLevel1(string word)
        {
            foreach (var (suffix, replacement) in Level1SuffixRules)
            {
                if (EndsWith(word, suffix, out var stem) && Measure(stem) > 0)
                    return stem + replacement;
            }
            return word;
        }

        private static bool EndsWith(string word, string suffix, out string stem)
        {
            if (word.EndsWith(suffix))
            {
                stem = word[..^suffix.Length];
                return true;
            }
            stem = word;
            return false;
        }

        private static string ReduceDerivationalSuffixesLevel2(string word)
        {
            foreach (var (suffix, replacement) in Level2SuffixRules)
            {
                if (EndsWith(word, suffix, out var stem) && Measure(stem) > 0)
                    return stem + replacement;
            }
            return word;
        }

        private static string RemoveResidualSuffixes(string word)
        {
            if (word.EndsWith("e"))
            {
                var stem = word.Substring(0, word.Length - 1);
                if (Measure(stem) > 1 || (Measure(stem) == 1 && !IsCVC(stem)))
                    return stem;
            }
            if (word.EndsWith("l") && EndsWithDoubleConsonant(word) && Measure(word.Substring(0, word.Length - 1)) > 1)
                return word.Substring(0, word.Length - 1);
            return word;
        }

        private static string CleanupTerminalEAndL(string word)
        {
            if (word.EndsWith("e"))
            {
                var stem = word.Substring(0, word.Length - 1);
                if (Measure(stem) > 1 || (Measure(stem) == 1 && !IsCVC(stem)))
                    return stem;
            }
            if (word.EndsWith("l") && EndsWithDoubleConsonant(word) && Measure(word.Substring(0, word.Length - 1)) > 1)
                return word.Substring(0, word.Length - 1);
            return word;
        }
    }

}
