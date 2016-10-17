using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using porter;

namespace SearchEngine
{
	public class Utils
	{
		public Utils () {}
		public static double vectorLength (List<double> vector) {
			double squaresSum = 0;
			foreach (double val in vector) {
				squaresSum += Math.Pow (val, 2);
			}
			return Math.Sqrt(squaresSum);
		}
		public static double vectorsProduct (List<double> vectorA, List<double> vectorB) {
			if (vectorA.Count != vectorB.Count) {
				throw new DifferentVectorsDimensions ();
			}
			double sum = 0;
			for (var i = 0; i < vectorA.Count; i++) {
				sum += vectorA [i] * vectorB [i];
			}
			return sum;
		}
		public static List<double> multiplyVectorsCoords (List<double> vectorA, List<double> vectorB) {
			if (vectorA.Count != vectorB.Count) {
				throw new DifferentVectorsDimensions ();
			}
			List<double> newVector = new List<double> ();
			for (var i = 0; i < vectorA.Count; i++) {
				newVector.Add (vectorA [i] * vectorB [i]);
			}
			return newVector;
		}


		public static double calculateVectorsCosinus (List<double> queryVector, List<double> documentVector) {
			double queryVectorLen = vectorLength (queryVector);
			double documentVectorLen = vectorLength (documentVector);
			if (queryVectorLen == 0 || documentVectorLen == 0) {
				return 0;
			}
			return vectorsProduct (queryVector, documentVector) / (queryVectorLen * documentVectorLen);
		}

		public static string stemToken (string token, Stemmer stemmer) {
			stemmer.add (token.ToCharArray (), token.Length);
			stemmer.stem ();
			return new String(stemmer.getResultBuffer(), 0, stemmer.getResultLength());
		}

		public static string[] extractTokens (string inputString) {
			inputString = inputString.ToLower ();
			string tokensString = "";
			foreach (char c in inputString) {
				if (!char.IsPunctuation (c)) {
					tokensString += c;
				}
			}
			tokensString = Regex.Replace (tokensString, @"\s+", " ");
			tokensString = tokensString.TrimEnd (' ');
			char delimiter = ' ';
			return tokensString.Split (delimiter);
		}

		public static List<double> createTFVector (List<string> keywords, List<string> tokens) {
			Dictionary<string, int> tokensCardinality = new Dictionary<string, int> ();
			foreach (string token in tokens) {
				if (tokensCardinality.ContainsKey (token)) {
					tokensCardinality [token] += 1;
				} else {
					tokensCardinality.Add (token, 1);
				}
			}
			int maxValue = 1;
			List<double> TFVector = new List<double> ();
			foreach (string keyword in keywords) {
				if (tokensCardinality.ContainsKey (keyword)) {
					TFVector.Add (tokensCardinality [keyword]);
					if (tokensCardinality [keyword] > maxValue)
						maxValue = tokensCardinality [keyword];
				} else {
					TFVector.Add (0);
				}
			}
			return TFVector.Select (i => i / maxValue).ToList ();
		}
	}

	public class DifferentVectorsDimensions : Exception {
		public DifferentVectorsDimensions () {}
	}


}

