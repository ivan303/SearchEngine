using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SearchEngine
{
	public class Document
	{
		public string title { get; set; }

		public List<string> lines = new List<string>();
		public string[] tokens;
		public List<string> stemmedTokens = new List<string>();

		public Document () {}

		public void extractTokens() {
			string allLines = "";
			foreach (var line in lines) {
				allLines = string.Concat (allLines, line + " ");
			}
			allLines = allLines.ToLower ();
			string tokensString = "";
			foreach (char c in allLines) {
				if (!char.IsPunctuation (c)) {
					tokensString += c;
				}
			}
			tokensString = Regex.Replace(tokensString, @"\s+", " ");
			tokensString = tokensString.TrimEnd (' ');
			char delimiter = ' ';
			tokens = tokensString.Split (delimiter);
		}
	}
}

