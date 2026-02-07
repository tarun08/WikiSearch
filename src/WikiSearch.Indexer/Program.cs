using WikiSearch.Core.Parsing;

string bz2Path = @"D:\WikiSearch\data\simplewiki-latest-pages-articles.xml.bz2"; // Update this path to your dump file

Console.WriteLine("Starting to parse Wikipedia dump...");
Console.WriteLine();

ParseDump( bz2Path );

Console.WriteLine();



void ParseDump(string bz2Path)
{
    Console.WriteLine($"Reading from: {bz2Path}");
    int count = 0;
    foreach (var article in WikiDumpParser.ParseArticles(bz2Path))
    {   
        count++;
        Console.WriteLine($"[{count}] {article.Title} : {article.Content}");
        Console.WriteLine(new string('-', 50));
    }
    Console.WriteLine($"Parsed {count} articles.");
}