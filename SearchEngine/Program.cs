using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using porter;

namespace SearchEngine
{
	class SearchEngine
	{
		protected List<Document> documents = new List<Document> ();
		protected List<string> keywords = new List<string> ();
		protected List<string> stemmedKeywords = new List<string> ();
		public Stemmer stemmer;
		protected List<double> IDFVector;

		public SearchEngine () {
			stemmer = new Stemmer ();
		}

		public void readDocumentsFile(string filename) {
			
			using (StreamReader sr = new StreamReader (filename)) {
				while (sr.Peek () >= 0) {
					string line = sr.ReadLine();
					Document document = new Document();
					while (!string.IsNullOrEmpty(line)) {
						document.lines.Add(line);
						line = sr.ReadLine();
					}
					document.title = document.lines[0];
					documents.Add(document);
				}
			}
		}

		public void readKeywordsFile(string filename) {

			using (StreamReader sr = new StreamReader (filename)) {
				while (sr.Peek () >= 0) {
					string keyword = sr.ReadLine();
					keywords.Add(keyword);
				}
			}
		}

		public List<double> createIDFVector () {
			List<double> IDFVector = new List<double> ();
			double docsWithKeyword;
			foreach (string keyword in stemmedKeywords) {
				docsWithKeyword = 0;
				foreach (Document doc in documents) {
					if (doc.stemmedTokens.Exists (token => token == keyword)) {
						docsWithKeyword++;
					}
				}
				IDFVector.Add (Math.Log10(documents.Count / (docsWithKeyword == 0 ? 1 : docsWithKeyword)));
			}
			return IDFVector;
		}

		public int readDocumentNumber () {
			Console.WriteLine ("Type document number.");
			while (true) {
				int number;
				bool result = Int32.TryParse (Console.ReadLine (), out number);
				if (result) {
					if (number < 1 || number > documents.Count) {
						Console.WriteLine ("Document index out of bounds. Try again.");
					} else {
						return number - 1;
					}
				} else {
					Console.WriteLine ("Invalid input. Try again.");
				}
			}
		}

		public static void Main (string[] args)
		{
			SearchEngine searchEngine = new SearchEngine ();
			Console.WriteLine ("Type keywords file name.");
			while (true) {
				try {
					searchEngine.readKeywordsFile (Console.ReadLine ());
					foreach (string keyword in searchEngine.keywords) {
						searchEngine.stemmedKeywords.Add(Utils.stemToken(keyword, searchEngine.stemmer));
					}
					searchEngine.stemmedKeywords = searchEngine.stemmedKeywords.Distinct().ToList();
					break;
				} catch (FileNotFoundException) {
					Console.WriteLine ("File not found. Try again.");
				} catch (ArgumentException) {
					Console.WriteLine ("Invalid file name. Try again.");
				}
			}

			Console.WriteLine ("Type documents file name.");
			while (true) {
				try {
					searchEngine.readDocumentsFile (Console.ReadLine ());
					foreach (Document doc in searchEngine.documents) {
						doc.processDocument(searchEngine.stemmedKeywords, searchEngine.stemmer);
					}
					searchEngine.IDFVector = searchEngine.createIDFVector();
					foreach (Document doc in searchEngine.documents) {
						doc.TFIDFVector = Utils.multiplyVectorsCoords(doc.TFVector, searchEngine.IDFVector);
					}
					break;
				} catch (FileNotFoundException) {
					Console.WriteLine ("File not found. Try again.");
				} catch (ArgumentException) {
					Console.WriteLine ("Invalid file name. Try again.");
				}
			}

			string action = "";
			bool closing = false;
			while (true) {
				if (closing) {
					break;
				}
				Console.WriteLine ("Choose action: 1 - search, 2 - display document, " + 
					"3 - display tokens, 4 - display stemmed tokens, 5 - quit");
				action = Console.ReadLine ();
				switch (action) 
				{
				case "1":
					Console.WriteLine ("Type query.");
					string query = Console.ReadLine ();
					string[] queryTokens = Utils.extractTokens (query);
					List<string> stemmedQueryTokens = new List<string> ();
					foreach (string token in queryTokens) {
						stemmedQueryTokens.Add (Utils.stemToken (token, searchEngine.stemmer));
					}
					List<double> queryTFVector;
					queryTFVector = Utils.createTFVector (searchEngine.stemmedKeywords, stemmedQueryTokens);
					List<double> queryTFIDFVector = Utils.multiplyVectorsCoords (queryTFVector, searchEngine.IDFVector);
					Dictionary<Document, double> vectorsCosinus = new Dictionary<Document, double> ();
					double cosinus = 0;
					foreach (Document doc in searchEngine.documents) {
						cosinus = Utils.calculateVectorsCosinus (queryTFIDFVector, doc.TFIDFVector);
						vectorsCosinus.Add (doc, cosinus);
					}

					var vectorsCosinusList = vectorsCosinus.ToList ();
					var sorted = vectorsCosinusList.OrderByDescending (pair => pair.Value);
					foreach (KeyValuePair<Document, double> kvp in sorted) {
						Console.WriteLine (kvp.Value + " " + kvp.Key.title);
					}
					break;
				case "2":
					searchEngine.documents[searchEngine.readDocumentNumber()]
						.displayDocument();
					break;
				case "3":
					searchEngine.documents [searchEngine.readDocumentNumber ()]
						.displayTokens ();
					break;
				case "4":
					searchEngine.documents [searchEngine.readDocumentNumber ()]
						.displayStemmedTokens ();
					break;
				case "5":
					Console.WriteLine ("Closing.");
					closing = true;
					break;
				default:
					Console.WriteLine ("Incorrect option. Try again.");
					break;
				}
			}
		}
	}
}
