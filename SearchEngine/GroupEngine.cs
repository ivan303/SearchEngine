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
				group.documents.Add (doc);
				group.calculateCentroid ();
				groups.Add (group);
				upperBound--;
			}

			iterate (docs);
			iterationsNumber--;
			for (int i = 0; i < iterationsNumber; i++) {
				clearGroups ();
				iterate (searchEngine.documents);
			}

			foreach (Group g in groups) {
				foreach (Document d in g.documents) {
					Console.WriteLine (d.title);
				}
				Console.WriteLine ();
			}
				
		}

		public void clearGroups ()
		{
			foreach (Group g in groups) {
				g.documents.Clear ();
			}
		}

		public void iterate (List<Document> docs)
		{
			double maxSimilarity;
			Group bestGroup;
			double similarity;
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
				bestGroup.documents.Add (d);
			}
			foreach (Group g in groups) {
				g.calculateCentroid ();
			}
		}
	}




	public class Group
	{
		public List<Document> documents = new List<Document> ();
		public List<double> centroid;

		public void calculateCentroid ()
		{
			int docsNumber = documents.Count;
			if (docsNumber > 0) {
				List<double> acc = Enumerable.Repeat (0d, documents [0].TFIDFVector.Count).ToList ();
				foreach (Document d in documents) {
					acc = Utils.addVectorsCoords (acc, d.TFIDFVector);
				}
				centroid = acc.Select (i => i / docsNumber).ToList ();
			}

		}
	}
}

