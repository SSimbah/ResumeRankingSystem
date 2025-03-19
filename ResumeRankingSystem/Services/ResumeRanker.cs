using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities;
using MathNet.Numerics.LinearAlgebra;

namespace ResumeRankingSystem.Services
{
    public class ResumeRanker
    {
        private const double Alpha = 0.5; // Weight for combining LSA and BM25 scores
        private readonly HttpClient _httpClient;

        public ResumeRanker(HttpClient httpClient)
        {
            _httpClient = httpClient; // Use HttpClient to call the Flask API
        }

        public async Task<(decimal SkillsScoring, decimal EducationScoring, decimal ExperienceScoring, decimal Score)> ScoreApplicant(
            string applicantSkills,
            string applicantEducation,
            string applicantExperience,
            ICollection<SkillRequirement> jobSkillRequirements,
            ICollection<EducationRequirement> jobEducationRequirements,
            ICollection<ExperienceRequirement> jobExperienceRequirements)
        {
            if (string.IsNullOrEmpty(applicantSkills) || string.IsNullOrEmpty(applicantEducation) || string.IsNullOrEmpty(applicantExperience))
                throw new ArgumentNullException("Applicant data cannot be null or empty.");
            if (jobSkillRequirements == null || jobEducationRequirements == null || jobExperienceRequirements == null)
                throw new ArgumentNullException("Job requirements cannot be null.");

            var jobSkillsString = string.Join(", ", jobSkillRequirements.Select(sr => sr.Name));
            var jobEducationString = string.Join(", ", jobEducationRequirements.Select(sr => sr.Name));
            var jobExperienceString = string.Join(", ", jobExperienceRequirements.Select(sr => sr.Name));

            // Preprocess applicant data asynchronously using the Flask API
            var applicantSkillsTokens = await PreprocessText(applicantSkills);
            var applicantEducationTokens = await PreprocessText(applicantEducation);
            var applicantExperienceTokens = await PreprocessText(applicantExperience);

            // Preprocess job requirements asynchronously using the Flask API
            var jobSkillsTokens = await PreprocessText(jobSkillsString);
            var jobEducationTokens = await PreprocessText(jobEducationString);
            var jobExperienceTokens = await PreprocessText(jobExperienceString);

            // Compute scores for each category
            decimal skillsScoring = ComputeCategoryScore(applicantSkillsTokens, jobSkillsTokens);
            decimal educationScoring = ComputeCategoryScore(applicantEducationTokens, jobEducationTokens);
            decimal experienceScoring = ComputeCategoryScore(applicantExperienceTokens, jobExperienceTokens);

            // Calculate the average score
            decimal score = Math.Round((skillsScoring + educationScoring + experienceScoring) / 3, 3);

            // Return the scores as a tuple
            return (skillsScoring, educationScoring, experienceScoring, score);
        }

        private decimal ComputeCategoryScore(List<string> applicantTokens, List<string> jobTokens)
        {
            // Check if all job tokens are matched by applicant tokens
            bool allMatched = jobTokens.All(jobToken => applicantTokens.Contains(jobToken));

            // If all job tokens are matched, return a score of 1
            if (allMatched)
                return 1.0m;

            // Otherwise, compute BM25 and LSA scores
            var requirementScores = CreateRequirementScores(jobTokens); // Placeholder for scoring weights
            double bm25Score = ComputeBM25Score(applicantTokens, jobTokens, requirementScores);
            double lsaScore = ComputeLSAScore(new List<string> { string.Join(" ", applicantTokens), string.Join(" ", jobTokens) }, requirementScores);

            // Combine BM25 and LSA scores with equal weighting
            double combinedScore = 0.5 * bm25Score + 0.5 * lsaScore;

            // Return the combined score rounded to 3 decimal places
            return Math.Round((decimal)combinedScore, 3);
        }

        private Dictionary<string, decimal> CreateRequirementScores(List<string> jobTokens)
        {
            // Remove duplicates and assign a fixed weight of 1.0 to each token
            return jobTokens
                .Distinct() // Remove duplicate tokens
                .ToDictionary(token => token, token => 1.0m); // Assign fixed weight
        }

        private double ComputeBM25Score(List<string> applicantTokens, List<string> jobTokens, Dictionary<string, decimal> requirementScores)
        {
            var idf = ComputeIDF(new List<List<string>> { jobTokens });
            const double k1 = 1.2;
            const double b = 0.75;
            double avgdl = jobTokens.Count;
            double bm25Score = 0;

            foreach (var term in jobTokens.Distinct())
            {
                if (idf.ContainsKey(term))
                {
                    int termFrequency = applicantTokens.Count(word => word == term);
                    double numerator = termFrequency * (k1 + 1);
                    double denominator = termFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
                    bm25Score += numerator / denominator * idf[term];
                }

                // Add scoring values
                if (requirementScores.ContainsKey(term))
                {
                    double scoringValue = (double)requirementScores[term];
                    double scaledScoringValue = scoringValue * idf[term]; // Scale scoring values by IDF
                    bm25Score += scaledScoringValue;
                }
            }

            // Normalize the score
            double maxPossibleScore = jobTokens.Distinct().Sum(term =>
            {
                double idfValue = idf.ContainsKey(term) ? idf[term] : 0;
                double scoringValue = requirementScores.ContainsKey(term) ? (double)requirementScores[term] : 0;
                int maxTermFrequency = applicantTokens.Count(word => word == term);
                double numerator = maxTermFrequency * (k1 + 1);
                double denominator = maxTermFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
                return numerator / denominator * idfValue + scoringValue;
            });

            return maxPossibleScore > 0 ? bm25Score / maxPossibleScore : 0.0;
        }

        private double ComputeLSAScore(List<string> documents, Dictionary<string, decimal> requirementScores)
        {
            // Create the term-document matrix
            var termDocumentMatrix = CreateTermDocumentMatrix(documents);

            // Apply requirementScores as weights to the matrix
            var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
            for (int i = 0; i < vocabulary.Count; i++)
            {
                var term = vocabulary[i];
                if (requirementScores.ContainsKey(term))
                {
                    // Multiply the row corresponding to this term by its scoring value
                    var row = termDocumentMatrix.Row(i);
                    row = row.Multiply((double)requirementScores[term]);
                    termDocumentMatrix.SetRow(i, row);
                }
            }

            // Perform SVD and compute cosine similarity
            var svd = termDocumentMatrix.Svd(true);
            var U = svd.U;
            var S = svd.W;
            int k = 2;
            var reducedMatrix = U.SubMatrix(0, U.RowCount, 0, k) * S.SubMatrix(0, k, 0, k);
            var applicantVector = reducedMatrix.Row(0);
            var jobVector = reducedMatrix.Row(1);

            return CosineSimilarity(applicantVector, jobVector);
        }

        private Dictionary<string, double> ComputeIDF(List<List<string>> documents)
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
                // Apply a damping factor to reduce the impact of large IDF values
                idf[term] = Math.Log(1 + (double)(N - docFrequency[term] + 0.5) / (docFrequency[term] + 0.5));
            }

            return idf;
        }

        private Matrix<double> CreateTermDocumentMatrix(List<string> documents)
        {
            var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
            var matrix = Matrix<double>.Build.Dense(vocabulary.Count, documents.Count);

            for (int i = 0; i < vocabulary.Count; i++)
            {
                var term = vocabulary[i];
                for (int j = 0; j < documents.Count; j++)
                {
                    var tf = documents[j].Split(' ').Count(word => word == term);
                    var idf = Math.Log((double)documents.Count / (1 + documents.Count(doc => doc.Contains(term))));
                    matrix[i, j] = tf * idf;
                }
            }

            return matrix;
        }

        private double CosineSimilarity(Vector<double> vec1, Vector<double> vec2)
        {
            if (vec1.L2Norm() == 0 || vec2.L2Norm() == 0)
                return 0.0;
            return vec1.DotProduct(vec2) / (vec1.L2Norm() * vec2.L2Norm());
        }

        private async Task<List<string>> PreprocessText(string text)
        {
            // Prepare the request payload
            var payload = new { text };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            // Send the request to the Flask API
            var response = await _httpClient.PostAsync("http://localhost:5000/preprocess", content);
            response.EnsureSuccessStatusCode();

            // Parse the response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);

            // Extract stemmed tokens from the response
            var tokens = responseObject["tokens"].EnumerateArray()
                .Select(token => token.GetProperty("stem").GetString()) // Use stemmed tokens
                .Where(token => !string.IsNullOrEmpty(token)) // Filter out empty tokens
                .ToList();

            return tokens;
        }
    }
}