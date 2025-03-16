using System;
using System.Collections.Generic;
using System.Linq;

namespace ResumeRankingLibrary.Services
{
    public class BM25
    {
        private readonly List<string[]> _documents;
        private readonly Dictionary<string, double> _idf;
        private readonly double _avgdl;
        private const double K1 = 1.2;
        private const double B = 0.75;

        public BM25(List<List<string>> documents)
        {
            _documents = documents.Select(doc => doc.ToArray()).ToList();
            _idf = ComputeIDF(_documents);
            _avgdl = _documents.Average(doc => doc.Length);
        }

        public List<(int DocIndex, double Score)> RankDocuments(string[] query)
        {
            var scores = new List<(int DocIndex, double Score)>();
            for (int i = 0; i < _documents.Count; i++)
            {
                double score = 0;
                foreach (var term in query)
                {
                    if (_idf.ContainsKey(term))
                    {
                        double tf = _documents[i].Count(word => word == term);
                        double numerator = tf * (_idf[term] + 1);
                        double denominator = tf + K1 * (1 - B + B * _documents[i].Length / _avgdl);
                        score += (numerator / denominator) * _idf[term];
                    }
                }
                scores.Add((i, score));
            }
            return scores.OrderByDescending(x => x.Score).ToList();
        }

        private Dictionary<string, double> ComputeIDF(List<string[]> documents)
        {
            var idf = new Dictionary<string, double>();
            int N = documents.Count;

            var docFrequency = new Dictionary<string, int>();
            foreach (var doc in documents)
            {
                var uniqueTerms = new HashSet<string>(doc);
                foreach (var term in uniqueTerms)
                {
                    if (!docFrequency.ContainsKey(term))
                        docFrequency[term] = 0;
                    docFrequency[term]++;
                }
            }

            foreach (var term in docFrequency.Keys)
            {
                idf[term] = Math.Log((double)(N - docFrequency[term] + 0.5) / (docFrequency[term] + 0.5) + 1);
            }

            return idf;
        }
    }
}