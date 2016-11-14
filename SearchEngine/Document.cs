﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using porter;

namespace SearchEngine
{
	public class Document
	{
		public string title { get; set; }

		public List<string> lines = new List<string> ();
		public string[] tokens;
		public List<string> stemmedTokens = new List<string> ();
		public List<double> TFVector;
		public List<double> TFIDFVector;

		public Document ()
		{
		}

		public void processDocument (List<string> keywords, Stemmer stemmer, bool withTitle = true)
		{
			extractTokens (withTitle);
			foreach (string token in tokens) {
				stemmedTokens.Add (Utils.stemToken (token, stemmer));
			}
			TFVector = Utils.createTFVector (keywords, stemmedTokens);
		}

		public void extractTokens (bool withTitle)
		{
			string allLines = "";
			var linesToProcess = withTitle ? lines : lines.Skip (1);
			foreach (var line in linesToProcess) {
				allLines = string.Concat (allLines, line + " ");
			}
			tokens = Utils.extractTokens (allLines);
		}

		public void displayDocument ()
		{
			foreach (string line in lines) {
				Console.WriteLine (line);
			}
		}

		public void displayTokens ()
		{
			foreach (string token in tokens) {
				Console.Write (token + " ");
			}
			Console.WriteLine ();
		}

		public void displayStemmedTokens ()
		{
			foreach (string token in stemmedTokens) {
				Console.Write (token + " ");
			}
			Console.WriteLine ();
		}
	}
}

