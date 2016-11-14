using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine
{
	public class GroupEngine
	{
		SearchEngine searchEngine;
		List<Group> groups;

		public GroupEngine (SearchEngine searchEngine)
		{
			this.searchEngine = searchEngine;
		}

		public void classify (int groupsNumber, int iterationsNumber)
		{
			int docNumber = searchEngine.documents.Count;
			groups = new List<Group> ();

			Random rnd = new Random ();
			int upperBound = docNumber;
			List<Document> docs = new List<Document> ();
			foreach (Document d in searchEngine.documents) {
				docs.Add (d);
			}
			Document doc;
			Group group;
			for (int i = 0; i < groupsNumber; i++) {
				doc = docs [rnd.Next (0, upperBound)];
				docs.Remove (doc);
				group = new Group ();
				group.documents.Add (new List<Document> ());
				group.documents.Last().Add (doc);
				group.calculateCentroid ();
				groups.Add (group);
				upperBound--;
			}

			iterate (docs);
			Console.WriteLine ("Iteration: 1 done");
			iterationsNumber--;
			for (int i = 0; i < iterationsNumber; i++) {
				if (iterationsUnchanged (i + 1)) {
					break;
				}

				iterate (searchEngine.documents);
				Console.WriteLine ("Iteration: " + (i + 2) + " done");
			}

			foreach (Group g in groups) {
				foreach (Document d in g.documents.Last()) {
					Console.WriteLine (d.title);
				}
				Console.WriteLine ();
			}

				
		}

		public bool iterationsUnchanged(int iterationsPassed) {
			if (iterationsPassed > 2) {
				bool oneBeforeLast = groups.All(group => group.documents.Last().SequenceEqual(group.documents[group.documents.Count - 2]));
				bool twoBeforeLast = groups.All(group => group.documents.Last().SequenceEqual(group.documents[group.documents.Count - 3]));
				if (oneBeforeLast && twoBeforeLast) {
					return true;
				}
			}
			return false;
		}

		public void iterate (List<Document> docs)
		{
			double maxSimilarity;
			Group bestGroup;
			double similarity;
			foreach (Group g in groups) {
				g.documents.Add (new List<Document> ());
			}
			foreach (Document d in docs) {
				maxSimilarity = 0;
				bestGroup = groups [0];
				foreach (Group g in groups) {
					similarity = Utils.calculateVectorsCosinus (g.centroid, d.TFIDFVector);
					if (similarity > maxSimilarity) {
						maxSimilarity = similarity;
						bestGroup = g;
					}
				}
				bestGroup.documents.Last().Add (d);
			}
			foreach (Group g in groups) {
				g.calculateCentroid ();
			}
		}
	}




	public class Group
	{
		public List<List<Document>> documents = new List<List<Document>> ();
		public List<double> centroid;

		public void calculateCentroid ()
		{
			int docsNumber = documents.Last().Count;
			if (docsNumber > 0) {
				List<double> acc = Enumerable.Repeat (0d, documents.Last ()[0].TFIDFVector.Count).ToList ();
				foreach (Document d in documents.Last()) {
					acc = Utils.addVectorsCoords (acc, d.TFIDFVector);
				}
				centroid = acc.Select (i => i / docsNumber).ToList ();
			}

		}
	}
}

