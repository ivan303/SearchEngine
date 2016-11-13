using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using porter;

namespace SearchEngine
{
	public class Utils
	{
		public Utils ()
		{
		}

		public static double vectorLength (List<double> vector)
		{
			double squaresSum = 0;
			foreach (double val in vector) {
				squaresSum += Math.Pow (val, 2);
			}
			return Math.Sqrt (squaresSum);
		}

		public static double vectorsProduct (List<double> vectorA, List<double> vectorB)
		{
			if (vectorA.Count != vectorB.Count) {
				throw new DifferentVectorsDimensions ();
			}
			double sum = 0;
			for (var i = 0; i < vectorA.Count; i++) {
				sum += vectorA [i] * vectorB [i];
			}
			return sum;
		}

		public static List<double> multiplyVectorsCoords (List<double> vectorA, List<double> vectorB)
		{
			if (vectorA.Count != vectorB.Count) {
				throw new DifferentVectorsDimensions ();
			}
			List<double> newVector = new List<double> ();
			for (var i = 0; i < vectorA.Count; i++) {
				newVector.Add (vectorA [i] * vectorB [i]);
			}
			return newVector;
		}

		public static List<double> addVectorsCoords (List<double> vectorA, List<double> vectorB)
		{
			if (vectorA.Count != vectorB.Count) {
				throw new DifferentVectorsDimensions ();
			}
			List<double> newVector = new List<double> ();
			for (var i = 0; i < vectorA.Count; i++) {
				newVector.Add (vectorA [i] + vectorB [i]);
			}
			return newVector;
		}


		public static double calculateVectorsCosinus (List<double> queryVector, List<double> documentVector)
		{
			double queryVectorLen = vectorLength (queryVector);
			double documentVectorLen = vectorLength (documentVector);
			if (queryVectorLen == 0 || documentVectorLen == 0) {
				return 0;
			}
			return vectorsProduct (queryVector, documentVector) / (queryVectorLen * documentVectorLen);
		}

		public static string stemToken (string token, Stemmer stemmer)
		{
			stemmer.add (token.ToCharArray (), token.Length);
			stemmer.stem ();
			return new String (stemmer.getResultBuffer (), 0, stemmer.getResultLength ());
		}

		public static string[] extractTokens (string inputString)
		{
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

		public static List<double> createTFVector (List<string> keywords, List<string> tokens)
		{
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

		public static List<double> createNewTFVector (List<string> keywords, List<string> tokens)
		{
			return null;
		}

		public static List<int> takeIndexesOfDocuments (string inputString)
		{
			List<int> returnList = new List<int> ();
			var charArray = inputString.ToArray ();
			foreach (var i in charArray) {
				if (i != ' ') {
					int index = 0;
					int.TryParse (i.ToString (), out index);
					returnList.Add (index);
				}
			}
			return returnList;
		}

		public static List<Document> getRelevantDocuments (List<int> indexes, List<KeyValuePair<Document, double>> wholeDocuments)
		{
			List<Document> returnList = new List<Document> ();
			int docListIndex = 0;
			foreach (var doc in wholeDocuments) {
				if (indexes.Contains (docListIndex)) {
					returnList.Add (doc.Key);
				}
				docListIndex++;
			}
			return returnList;
		}

		public static List<Document> getUnRelevantDocuments (List<int> indexes, List<KeyValuePair<Document, double>> wholeDocuments)
		{
			List<Document> returnList = new List<Document> ();
			int docListIndex = 0;
			foreach (var doc in wholeDocuments) {
				if (!indexes.Contains (docListIndex)) {
					returnList.Add (doc.Key);
				}
				docListIndex++;
			}
			return returnList;
		}

		public static List<double> calculateNewQueryTFIDFQuery (List<Document> relevant, List<Document> unRelevant, List<double> queryTFIDFVector)
		{
			var alfa = 1;
			var beta = 0.75;
			var gamma = 0.25;
			List<double> newQueryVector = new List<double> ();
			for (int i = 0; i < queryTFIDFVector.Count; i++) {
				//relevant
				var sumOneRecorsRelevant = 0.0;
				foreach (var doc in relevant) {
					sumOneRecorsRelevant = sumOneRecorsRelevant + doc.TFIDFVector [i];
				}
				var relevantCentroid = sumOneRecorsRelevant / relevant.Count ();
				//
				var sumOneRecorsUnRelevant = 0.0;
				foreach (var doc in relevant) {
					sumOneRecorsUnRelevant = sumOneRecorsUnRelevant + doc.TFIDFVector [i];
				}
				var unRelevantCentroid = sumOneRecorsUnRelevant / unRelevant.Count ();

				var result = (alfa * queryTFIDFVector [i]) + (beta * relevantCentroid) - (gamma * unRelevantCentroid);

				if (result >= 0) {
					newQueryVector.Add (result);
				} else {
					newQueryVector.Add (0);
				}
			}
			return newQueryVector;
		}
	}

	public class DifferentVectorsDimensions : Exception
	{
		public DifferentVectorsDimensions ()
		{
		}
	}


}

