using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using porter;

namespace SearchEngine
{
    class SearchEngine
    {
        protected List<Document> documents = new List<Document>();
        protected List<string> keywords = new List<string>();
        protected List<string> stemmedKeywords = new List<string>();
        public Stemmer stemmer;
        protected List<double> IDFVector;

        public SearchEngine()
        {
            stemmer = new Stemmer();
        }

        public void readDocumentsFile(string filename)
        {

            using (StreamReader sr = new StreamReader(filename))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    Document document = new Document();
                    while (!string.IsNullOrEmpty(line))
                    {
                        document.lines.Add(line);
                        line = sr.ReadLine();
                    }
                    document.title = document.lines[0];
                    documents.Add(document);
                }
            }
        }

        public void readKeywordsFile(string filename)
        {

            using (StreamReader sr = new StreamReader(filename))
            {
                while (sr.Peek() >= 0)
                {
                    string keyword = sr.ReadLine();
                    keywords.Add(keyword);
                }
            }
        }

        public List<double> createIDFVector()
        {
            List<double> IDFVector = new List<double>();
            double docsWithKeyword;
            foreach (string keyword in stemmedKeywords)
            {
                docsWithKeyword = 0;
                foreach (Document doc in documents)
                {
                    if (doc.stemmedTokens.Exists(token => token == keyword))
                    {
                        docsWithKeyword++;
                    }
                }
                IDFVector.Add(Math.Log10(documents.Count / (docsWithKeyword == 0 ? 1 : docsWithKeyword)));
            }
            return IDFVector;
        }

        public int readDocumentNumber()
        {
            Console.WriteLine("Type document number.");
            while (true)
            {
                int number;
                bool result = Int32.TryParse(Console.ReadLine(), out number);
                if (result)
                {
                    if (number < 1 || number > documents.Count)
                    {
                        Console.WriteLine("Document index out of bounds. Try again.");
                    }
                    else
                    {
                        return number - 1;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Try again.");
                }
            }
        }

        public static void Main(string[] args)
        {
            SearchEngine searchEngine = new SearchEngine();

			const int SEARCH_ENGINE = 1;
			const int GROUPING_ENGINE = 2;
			int mode;

			Console.WriteLine ("Choose mode: 1 - search engine, 2 - k-means grouping");
			while (true) {
				bool result = Int32.TryParse(Console.ReadLine(), out mode);
				if (result) 
				{
					if (mode != 1 && mode != 2) {
						Console.WriteLine ("Invalid mode. Try again.");	
					} else {
						break;
					}
				} 
				else {
					Console.WriteLine ("Invalid input. Try again.");
				}
			}

            Console.WriteLine("Type keywords file name.");
            while (true)
            {
                try
                {
                    searchEngine.readKeywordsFile(Console.ReadLine());
                    foreach (string keyword in searchEngine.keywords)
                    {
                        searchEngine.stemmedKeywords.Add(Utils.stemToken(keyword, searchEngine.stemmer));
                    }
                    searchEngine.stemmedKeywords = searchEngine.stemmedKeywords.Distinct().ToList();
                    break;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("File not found. Try again.");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid file name. Try again.");
                }
            }

            Console.WriteLine("Type documents file name.");
            while (true)
            {
                try
                {
                    searchEngine.readDocumentsFile(Console.ReadLine());
                    foreach (Document doc in searchEngine.documents)
                    {
						if (mode == SEARCH_ENGINE) {
							doc.processDocument(searchEngine.stemmedKeywords, searchEngine.stemmer, true);
						} else {
							doc.processDocument(searchEngine.stemmedKeywords, searchEngine.stemmer, false);
						}
                    }
                    searchEngine.IDFVector = searchEngine.createIDFVector();
                    foreach (Document doc in searchEngine.documents)
                    {
                        doc.TFIDFVector = Utils.multiplyVectorsCoords(doc.TFVector, searchEngine.IDFVector);
                    }
                    break;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("File not found. Try again.");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid file name. Try again.");
                }
            }





            string action = "";
            bool closing = false;
            while (true)
            {
                if (closing)
                {
                    break;
                }
                Console.WriteLine("Choose action: 1 - search, 2 - display document, " +
                    "3 - display tokens, 4 - display stemmed tokens, 5 - quit");
                action = Console.ReadLine();
                switch (action)
                {
                    case "1":
                        Console.WriteLine("Type query.");
                        string query = Console.ReadLine();
                        string[] queryTokens = Utils.extractTokens(query);
                        List<string> stemmedQueryTokens = new List<string>();
                        foreach (string token in queryTokens)
                        {
                            stemmedQueryTokens.Add(Utils.stemToken(token, searchEngine.stemmer));
                        }
                        List<double> queryTFVector;
                        queryTFVector = Utils.createTFVector(searchEngine.stemmedKeywords, stemmedQueryTokens);
                        List<double> queryTFIDFVector = Utils.multiplyVectorsCoords(queryTFVector, searchEngine.IDFVector);
                        Dictionary<Document, double> vectorsCosinus = new Dictionary<Document, double>();
                        double cosinus = 0;
                        foreach (Document doc in searchEngine.documents)
                        {
                            cosinus = Utils.calculateVectorsCosinus(queryTFIDFVector, doc.TFIDFVector);
                            vectorsCosinus.Add(doc, cosinus);
                        }

                        var vectorsCosinusList = vectorsCosinus.ToList();
                        var sorted = vectorsCosinusList.OrderByDescending(pair => pair.Value);
                        int index = 0;
                        foreach (KeyValuePair<Document, double> kvp in sorted)
                        {
                            Console.WriteLine(index + "." + kvp.Value + " " + kvp.Key.title);
                            index++;
                        }
                        Console.WriteLine("Do you want to choose relevent documents. 1 - yes, 2 - no");
                        var choose = Console.ReadLine();
                        switch (choose)
                        {
                            case "1":
                                {
                                    Console.WriteLine("Choose numbers of relevant documents. Format: 'number of document' 'number od document ...");
                                    var StringDocumentsIndexes = Console.ReadLine();
                                    List<int> intDocumentsIndexes = Utils.takeIndexesOfDocuments(StringDocumentsIndexes);
                                    var relevant = Utils.getRelevantDocuments(intDocumentsIndexes, sorted.ToList());
                                    var unRelevant = Utils.getUnRelevantDocuments(intDocumentsIndexes, sorted.ToList());
                                    var newQuery = Utils.calculateNewQueryTFIDFQuery(relevant, unRelevant, queryTFIDFVector);
                                    //create new list of keys 
                                    Dictionary<string, double> keyWords = new Dictionary<string, double>();
                                    for (int i = 0; i < newQuery.Count(); i++)
                                    {
                                        keyWords.Add(searchEngine.stemmedKeywords[i], newQuery[i]);
                                    }
                                    var sortedKeyWords = keyWords.OrderByDescending(i => i.Value);
                                    foreach (var k in sortedKeyWords)
                                    {
                                        if (k.Value != 0)
                                        {
                                            Console.WriteLine($"{k.Key} {k.Value}");
                                        }
                                    }
                                    Console.WriteLine("Type new query");
                                    string query1 = Console.ReadLine();
                                    string[] queryTokens1 = Utils.extractTokens(query1);
                                    List<string> stemmedQueryTokens1 = new List<string>();
                                    foreach (string token in queryTokens1)
                                    {
                                        stemmedQueryTokens1.Add(Utils.stemToken(token, searchEngine.stemmer));
                                    }
                                    List<double> queryTFVector1 = Utils.createTFVector(searchEngine.stemmedKeywords, stemmedQueryTokens1);
                                    List<double> queryTFIDFVector1 = Utils.multiplyVectorsCoords(queryTFVector1, searchEngine.IDFVector);

                                    Dictionary<Document, double> vectorsCosinus1 = new Dictionary<Document, double>();
                                    double cosinus1 = 0;
                                    foreach (Document doc in searchEngine.documents)
                                    {
                                        cosinus1 = Utils.calculateVectorsCosinus(queryTFIDFVector1, doc.TFIDFVector);
                                        vectorsCosinus1.Add(doc, cosinus1);
                                    }

                                    var vectorsCosinusList1 = vectorsCosinus1.ToList();
                                    var sorted1 = vectorsCosinusList1.OrderByDescending(pair => pair.Value);
                                    int index1 = 0;
                                    foreach (KeyValuePair<Document, double> kvp in sorted1)
                                    {
                                        Console.WriteLine(index1 + ".   " + kvp.Value + " " + kvp.Key.title);
                                        index1++;
                                    }
                                    break;
                                }
                            case "2":
                                {
                                    break;
                                }
                        }
                        break;
                    case "2":
                        searchEngine.documents[searchEngine.readDocumentNumber()]
                            .displayDocument();
                        break;
                    case "3":
                        searchEngine.documents[searchEngine.readDocumentNumber()]
                            .displayTokens();
                        break;
                    case "4":
                        searchEngine.documents[searchEngine.readDocumentNumber()]
                            .displayStemmedTokens();
                        break;
                    case "5":
                        Console.WriteLine("Closing.");
                        closing = true;
                        break;
                    default:
                        Console.WriteLine("Incorrect option. Try again.");
                        break;
                }
            }
        }
    }
}
