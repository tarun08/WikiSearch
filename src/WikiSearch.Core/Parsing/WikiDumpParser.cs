using SharpCompress.Compressors.BZip2;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using WikiSearch.Core.Models;

namespace WikiSearch.Core.Parsing
{
    public static class WikiDumpParser
    {
        public static IEnumerable<Article> ParseArticles(string bz2Path)
        {
            using var fileStream = File.OpenRead(bz2Path);
            using var bz2Stream = new BZip2Stream(fileStream, SharpCompress.Compressors.CompressionMode.Decompress, true);

            using var reader = XmlReader.Create(bz2Stream, new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            });

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "page")
                {
                    var article = ReadPage(reader);
                    if (article != null)
                        yield return article;
                }
            }
        }
        private static Article? ReadPage(XmlReader reader)
        {
            long id = 0;
            string title = string.Empty;
            string content = string.Empty;
            DateTime lastModified = DateTime.MinValue;

            bool isRedirect = false;
            int ns = -1;

            using var pageReader = reader.ReadSubtree();
            pageReader.Read();

            while (pageReader.Read())
            {
                if (pageReader.NodeType != XmlNodeType.Element)
                    continue;

                switch (pageReader.LocalName)
                {
                    case "title":
                        title = pageReader.ReadElementContentAsString();

                        if (title.Contains(":"))
                            return null;

                        break;

                    case "ns":
                        ns = pageReader.ReadElementContentAsInt();
                        break;

                    case "id" when id == 0:
                        id = pageReader.ReadElementContentAsLong();
                        break;

                    case "redirect":
                        isRedirect = true;
                        break;

                    case "timestamp":
                        lastModified = DateTime.Parse(
                            pageReader.ReadElementContentAsString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal
                        );
                        break;

                    case "text":
                        content = pageReader.ReadElementContentAsString();
                        break;
                }
            }


            //if (ns != 0) return null;               // not an article
            if (isRedirect) return null;
            if (string.IsNullOrWhiteSpace(content)) return null;

            return new Article
            {
                Id = id,
                Title = title,
                Content = CleanWikiText(content),
                LastModified = lastModified
            };
        }

        private static string CleanWikiText(string text)
        {
            // Remove tables {| ... |}
            text = Regex.Replace(text, @"\{\|.*?\|\}", "", RegexOptions.Singleline);

            // Remove templates {{...}}
            text = Regex.Replace(text, @"\{\{.*?\}\}", "", RegexOptions.Singleline);

            // Remove references <ref>...</ref>
            text = Regex.Replace(text, @"<ref.*?>.*?</ref>", "", RegexOptions.Singleline);

            // Convert wiki links [[link|text]] -> text
            text = Regex.Replace(
                text,
                @"\[\[(?:[^\]|]*\|)?([^\]]+)\]\]",
                "$1"
            );

            // Remove leftover table markers
            text = Regex.Replace(text, @"\|\-|\|\||\|", " ");

            // Remove section headers == Heading ==
            text = Regex.Replace(text, @"={2,}.*?={2,}", "");

            // Normalize whitespace
            text = Regex.Replace(text, @"\s{2,}", " ");

            return text.Trim();
        }

    }
}
