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

		public Document getNthDocument (int number) {
			try {
				return documents[number];
			} catch (ArgumentOutOfRangeException e) {
				throw e;
			}
		}

		public void readDocumentsFile(string filename) {
			try {
				StreamReader sr = new StreamReader (filename);

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
			} catch (Exception e) {
				throw e;
			}
		}

		public void readKeywordsFile(string filename) {
			try {
				StreamReader sr = new StreamReader(filename);

				while (sr.Peek () >= 0) {
					string keyword = sr.ReadLine();
					keywords.Add(keyword);
				}
			} catch (Exception e) {
				throw e;
			}
		}

		public string getNthKeyword (int number) {
			try {
				return keywords[number];
			} catch (ArgumentOutOfRangeException e) {
				throw e;
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
					// TODO check if it's necessary to get rid of repeated words
					break;
				} catch (FileNotFoundException) {
					Console.WriteLine ("File not found. Try again.");
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
				}
			}

			string action = "";
			bool closing = false;
			while (true) {
				if (closing) {
					break;
				}
				Console.WriteLine ("Choose action: 1 - search, 2 - display document, 3 - quit");
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
					Console.WriteLine ("Type document number.");
					while (true) {
						int number;
						bool result = Int32.TryParse (Console.ReadLine (), out number);
						if (result) {
							if (number < 1 || number > searchEngine.documents.Count) {
								Console.WriteLine ("Document index out of bounds. Try again.");
							} else {
								searchEngine.documents [number - 1].displayDocument ();
								break;
							}
						} else {
							Console.WriteLine ("Invalid input. Try again.");
						}
					}
					break;
				case "3":
					Console.WriteLine ("Closing.");
					closing = true;
					break;
				default:
					Console.WriteLine ("Incorrect option. Try again.");
					break;
				}
			}
				

//			foreach (string token in searchEngine.getNthDocument (0).tokens) {
//				Console.WriteLine (token);
//			}
//
//			foreach (string token in searchEngine.getNthDocument (0).stemmedTokens) {
//				Console.WriteLine (token);
//			}
//			Console.WriteLine(searchEngine.documents[0].lines[0]);
//			Console.WriteLine (searchEngine.documents [0].title);
//			searchEngine.documents[5].displayDocument();
//			searchEngine.documents [5].displayTokens ();
//			searchEngine.documents [5].displayStemmedTokens ();
//			searchEngine.documents [5].createTFVector (searchEngine.stemmedKeywords);
//			searchEngine.documents [5].createIDFVector (2);
			 

		}
	}
}
