using System;
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
		private Stemmer stemmer;

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
				Console.WriteLine (e);
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
				Console.WriteLine (e);
			}
		}

		public string getNthKeyword (int number) {
			try {
				return keywords[number];
			} catch (ArgumentOutOfRangeException e) {
				throw e;
			}
		}

		public string stemToken (string token) {
			stemmer.add (token.ToCharArray (), token.Length);
			stemmer.stem ();
			return new String(stemmer.getResultBuffer(), 0, stemmer.getResultLength());
		}

		public static void Main (string[] args)
		{
			SearchEngine searchEngine = new SearchEngine ();
			searchEngine.readDocumentsFile ("documents.txt");
			searchEngine.readKeywordsFile ("keywords.txt");

			foreach (Document doc in searchEngine.documents) {
				doc.extractTokens ();
				foreach (string token in doc.tokens) {
					doc.stemmedTokens.Add (searchEngine.stemToken (token));
				}
			}

			foreach (string keyword in searchEngine.keywords) {
				searchEngine.stemmedKeywords.Add (searchEngine.stemToken (keyword));
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
			 

		}
	}
}
