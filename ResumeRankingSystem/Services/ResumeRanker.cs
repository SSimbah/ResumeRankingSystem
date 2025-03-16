using Domain.Entities;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Text.Json;
using ResumeRankingSystem.Services; // Add this namespace to access PreprocessingHelper
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

namespace ResumeRankingLibrary.Services
{
    //public class ResumeRanker
    //{
    //    private const double Alpha = 0.5; // Weight for combining LSA and BM25 scores
    //    private readonly PreprocessingHelper _preprocessingHelper;

    //    public ResumeRanker()
    //    {
    //        _preprocessingHelper = new PreprocessingHelper(); // Instantiate PreprocessingHelper
    //    }

    //    //public double ScoreApplicant(Applicant applicant, JobPosting jobPosting)
    //    //{
    //    //    if (applicant == null || jobPosting == null)
    //    //        throw new ArgumentNullException("Applicant or job posting cannot be null.");

    //    //    // Serialize and preprocess applicant and job data
    //    //    string applicantJson = System.Text.Json.JsonSerializer.Serialize(applicant);
    //    //    //string jobJson = JsonSerializer.Serialize(jobPosting);
    //    //    string jobJson = System.Text.Json.JsonSerializer.Serialize(
    //    //        jobPosting,
    //    //        new JsonSerializerOptions
    //    //        {
    //    //            ReferenceHandler = ReferenceHandler.Preserve, // Handle circular references
    //    //            WriteIndented = true // Optional: For better readability
    //    //        }
    //    //    );
    //    //    var jobTokens = _preprocessingHelper.Preprocess(jobJson); // Use PreprocessingHelper
    //    //    var applicantTokens = _preprocessingHelper.Preprocess(applicantJson); // Use PreprocessingHelper

    //    //    // Combine applicant and job tokens into a single list of documents
    //    //    var allDocuments = new List<string>
    //    //    {
    //    //        string.Join(" ", applicantTokens),
    //    //        string.Join(" ", jobTokens)
    //    //    };

    //    //    // Compute LSA score with weighted term-document matrix
    //    //    double lsaScore = ComputeLSAScore(allDocuments, jobPosting);

    //    //    // Compute BM25 score for the single document
    //    //    double bm25Score = ComputeBM25Score(applicantTokens, jobTokens, jobPosting);

    //    //    // Combine scores using weights
    //    //    const double Alpha = 0.5; // Weight for combining LSA and BM25 scores
    //    //    return Alpha * lsaScore + (1 - Alpha) * bm25Score;
    //    //}

    //    public (decimal SkillsScoring, decimal EducationScoring, decimal ExperienceScoring, decimal Score) ScoreApplicant(
    //string applicantSkills,
    //string applicantEducation,
    //string applicantExperience,
    //List<string> jobSkills,
    //List<string> jobEducationRequirements,
    //List<string> jobExperienceRequirements)
    //    {
    //        if (string.IsNullOrEmpty(applicantSkills) || string.IsNullOrEmpty(applicantEducation) || string.IsNullOrEmpty(applicantExperience))
    //            throw new ArgumentNullException("Applicant data cannot be null or empty.");
    //        if (jobSkills == null || jobEducationRequirements == null || jobExperienceRequirements == null)
    //            throw new ArgumentNullException("Job requirements cannot be null.");

    //        // Preprocess the input data
    //        var applicantSkillTokens = _preprocessingHelper.Preprocess(applicantSkills);
    //        var applicantEducationTokens = _preprocessingHelper.Preprocess(applicantEducation);
    //        var applicantExperienceTokens = _preprocessingHelper.Preprocess(applicantExperience);

    //        var jobSkillTokens = _preprocessingHelper.Preprocess(string.Join(" ", jobSkills));
    //        var jobEducationTokens = _preprocessingHelper.Preprocess(string.Join(" ", jobEducationRequirements));
    //        var jobExperienceTokens = _preprocessingHelper.Preprocess(string.Join(" ", jobExperienceRequirements));

    //        // Combine tokens into documents for LSA computation
    //        var skillDocuments = new List<string> { string.Join(" ", applicantSkillTokens), string.Join(" ", jobSkillTokens) };
    //        var educationDocuments = new List<string> { string.Join(" ", applicantEducationTokens), string.Join(" ", jobEducationTokens) };
    //        var experienceDocuments = new List<string> { string.Join(" ", applicantExperienceTokens), string.Join(" ", jobExperienceTokens) };

    //        // Compute BM25 and LSA scores for each category
    //        double skillsBM25 = ComputeBM25Score(applicantSkillTokens, jobSkillTokens, CreateRequirementScores(jobSkills));
    //        double skillsLSA = ComputeLSAScore(skillDocuments);

    //        double educationBM25 = ComputeBM25Score(applicantEducationTokens, jobEducationTokens, CreateRequirementScores(jobEducationRequirements));
    //        double educationLSA = ComputeLSAScore(educationDocuments);

    //        double experienceBM25 = ComputeBM25Score(applicantExperienceTokens, jobExperienceTokens, CreateRequirementScores(jobExperienceRequirements));
    //        double experienceLSA = ComputeLSAScore(experienceDocuments);

    //        // Combine BM25 and LSA scores with 50/50 weighting
    //        double skillsScore = 0.5 * skillsBM25 + 0.5 * skillsLSA;
    //        double educationScore = 0.5 * educationBM25 + 0.5 * educationLSA;
    //        double experienceScore = 0.5 * experienceBM25 + 0.5 * experienceLSA;

    //        // Normalize and round scores
    //        const double maxPossibleScore = 1.0; // Assuming scores are normalized between 0 and 1
    //        decimal skillsScoring = Math.Round((decimal)(skillsScore / maxPossibleScore), 3);
    //        decimal educationScoring = Math.Round((decimal)(educationScore / maxPossibleScore), 3);
    //        decimal experienceScoring = Math.Round((decimal)(experienceScore / maxPossibleScore), 3);

    //        // Calculate the average score
    //        decimal score = Math.Round((skillsScoring + educationScoring + experienceScoring) / 3, 3);

    //        // Return the scores as a tuple
    //        return (skillsScoring, educationScoring, experienceScoring, score);
    //    }
    //    private Dictionary<string, decimal> CreateRequirementScores(List<string> requirements)
    //    {
    //        var requirementScores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
    //        foreach (var requirement in requirements)
    //        {
    //            requirementScores[requirement.ToLower()] = 1.0m; // Default scoring value
    //        }
    //        return requirementScores;
    //    }

    //    private double ComputeLSAScore(List<string> allDocuments)
    //    {
    //        var termDocumentMatrix = CreateTermDocumentMatrix(allDocuments);
    //        var svd = termDocumentMatrix.Svd(true);
    //        var U = svd.U;
    //        var S = svd.W;
    //        int k = 2;
    //        var reducedMatrix = U.SubMatrix(0, U.RowCount, 0, k) * S.SubMatrix(0, k, 0, k);
    //        var applicantVector = reducedMatrix.Row(0);
    //        var jobVector = reducedMatrix.Row(1);
    //        return CosineSimilarity(applicantVector, jobVector);
    //    }

    //    private double ComputeBM25Score(List<string> applicantTokens, List<string> jobTokens, Dictionary<string, decimal> requirementScores)
    //    {
    //        var idf = ComputeIDF(new List<List<string>> { jobTokens });
    //        const double k1 = 1.2;
    //        const double b = 0.75;
    //        double avgdl = jobTokens.Count;
    //        double bm25Score = 0;

    //        foreach (var term in jobTokens.Distinct())
    //        {
    //            if (idf.ContainsKey(term))
    //            {
    //                int termFrequency = applicantTokens.Count(word => word == term);
    //                double numerator = termFrequency * (k1 + 1);
    //                double denominator = termFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
    //                bm25Score += (numerator / denominator) * idf[term];
    //            }

    //            // Add scoring values
    //            if (requirementScores.ContainsKey(term))
    //            {
    //                double scoringValue = (double)requirementScores[term];
    //                double scaledScoringValue = scoringValue * idf[term]; // Scale scoring values by IDF
    //                bm25Score += scaledScoringValue;
    //            }
    //        }

    //        // Normalize the score
    //        //double maxPossibleScore = jobTokens.Distinct().Sum(term =>
    //        //{
    //        //    double idfValue = idf.ContainsKey(term) ? idf[term] : 0;
    //        //    double scoringValue = requirementScores.ContainsKey(term) ? (double)requirementScores[term] : 0;
    //        //    int maxTermFrequency = applicantTokens.Count(word => word == term);
    //        //    double numerator = maxTermFrequency * (k1 + 1);
    //        //    double denominator = maxTermFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
    //        //    return (numerator / denominator) * idfValue + scoringValue;
    //        //});

    //        double maxPossibleScore = jobTokens.Distinct().Sum(term =>
    //        {
    //            double idfValue = idf.ContainsKey(term) ? idf[term] : 0;
    //            double scoringValue = requirementScores.ContainsKey(term) ? (double)requirementScores[term] : 0;
    //            int maxTermFrequency = applicantTokens.Count(word => word == term);
    //            double numerator = maxTermFrequency * (k1 + 1);
    //            double denominator = maxTermFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
    //            return (numerator / denominator) * idfValue + scoringValue;
    //        });

    //        return maxPossibleScore > 0 ? bm25Score / maxPossibleScore : 0.0;
    //    }
    //    private Dictionary<string, double> ComputeIDF(List<List<string>> documents)
    //    {
    //        var idf = new Dictionary<string, double>();
    //        int N = documents.Count;

    //        var docFrequency = new Dictionary<string, int>();

    //        foreach (var doc in documents)
    //        {
    //            var uniqueTerms = new HashSet<string>(doc);
    //            foreach (var term in uniqueTerms)
    //            {
    //                if (!docFrequency.ContainsKey(term))
    //                    docFrequency[term] = 0;
    //                docFrequency[term]++;
    //            }
    //        }

    //        foreach (var term in docFrequency.Keys)
    //        {
    //            // Apply a damping factor to reduce the impact of large IDF values
    //            idf[term] = Math.Log(1 + (double)(N - docFrequency[term] + 0.5) / (docFrequency[term] + 0.5));
    //        }

    //        return idf;
    //    }
    //    private Matrix<double> CreateTermDocumentMatrix(List<string> documents)
    //    {
    //        var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
    //        var matrix = Matrix<double>.Build.Dense(vocabulary.Count, documents.Count);

    //        for (int i = 0; i < vocabulary.Count; i++)
    //        {
    //            var term = vocabulary[i];
    //            for (int j = 0; j < documents.Count; j++)
    //            {
    //                var tf = documents[j].Split(' ').Count(word => word == term);
    //                var idf = Math.Log((double)documents.Count / (1 + documents.Count(doc => doc.Contains(term))));
    //                matrix[i, j] = tf * idf;
    //            }
    //        }
    //        return matrix;
    //    }

    //    private double CosineSimilarity(MathNet.Numerics.LinearAlgebra.Vector<double> vec1, MathNet.Numerics.LinearAlgebra.Vector<double> vec2)
    //    {
    //        if (vec1.L2Norm() == 0 || vec2.L2Norm() == 0)
    //            return 0.0;

    //        return vec1.DotProduct(vec2) / (vec1.L2Norm() * vec2.L2Norm());
    //    }
    //}
    public class ResumeRanker
    {
        private const double Alpha = 0.5; // Weight for combining LSA and BM25 scores
        private readonly PreprocessingHelper _preprocessingHelper;

        public ResumeRanker(HttpClient httpClient)
        {
            _preprocessingHelper = new PreprocessingHelper(); // Instantiate PreprocessingHelper
        }

        public (decimal SkillsScoring, decimal EducationScoring, decimal ExperienceScoring, decimal Score) ScoreApplicant(
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

            // Preprocess applicant data
            var applicantSkillsTokens = _preprocessingHelper.PreprocessSkills(applicantSkills);
            var applicantEducationTokens = _preprocessingHelper.PreprocessEducation(applicantEducation)
                .Select(e => $"{e.Degree}") // Combine school and degree into a single string
                .ToList();
            var applicantExperienceTokens = _preprocessingHelper.PreprocessExperience(applicantExperience)
                .Select(e => $"{e.JobTitle} {e.Duration}") // Combine company, job title, and duration
                .ToList();

            // Preprocess job requirements
            var jobSkillsTokens = jobSkillRequirements.Select(sr => sr.Name).ToList();
            var jobEducationTokens = jobEducationRequirements.Select(er => er.Name).ToList();
            var jobExperienceTokens = jobExperienceRequirements.Select(exr => exr.Name).ToList();

            // Combine tokens into documents for LSA computation
            var skillDocuments = new List<string> { string.Join(" ", applicantSkillsTokens), string.Join(" ", jobSkillsTokens) };
            var educationDocuments = new List<string> { string.Join(" ", applicantEducationTokens), string.Join(" ", jobEducationTokens) };
            var experienceDocuments = new List<string> { string.Join(" ", applicantExperienceTokens), string.Join(" ", jobExperienceTokens) };

            // Compute BM25 and LSA scores for each category
            double skillsBM25 = ComputeBM25Score(applicantSkillsTokens, jobSkillsTokens, CreateRequirementScores(jobSkillRequirements));
            double skillsLSA = ComputeLSAScore(skillDocuments, CreateRequirementScores(jobSkillRequirements));

            double educationBM25 = ComputeBM25Score(applicantEducationTokens, jobEducationTokens, CreateRequirementScores(jobEducationRequirements));
            double educationLSA = ComputeLSAScore(educationDocuments, CreateRequirementScores(jobEducationRequirements));

            double experienceBM25 = ComputeBM25Score(applicantExperienceTokens, jobExperienceTokens, CreateRequirementScores(jobExperienceRequirements));
            double experienceLSA = ComputeLSAScore(experienceDocuments, CreateRequirementScores(jobExperienceRequirements));

            // Combine BM25 and LSA scores with 50/50 weighting
            double skillsScore = 0.5 * skillsBM25 + 0.5 * skillsLSA;
            double educationScore = 0.5 * educationBM25 + 0.5 * educationLSA;
            double experienceScore = 0.5 * experienceBM25 + 0.5 * experienceLSA;

            // Round the scores directly without normalization
            decimal skillsScoring = Math.Round((decimal)skillsScore, 3);
            decimal educationScoring = Math.Round((decimal)educationScore, 3);
            decimal experienceScoring = Math.Round((decimal)experienceScore, 3);

            // Calculate the average score
            decimal score = Math.Round((skillsScoring + educationScoring + experienceScoring) / 3, 3);

            // Return the scores as a tuple
            return (skillsScoring, educationScoring, experienceScoring, score);
        }

        private Dictionary<string, decimal> CreateRequirementScores<T>(ICollection<T> requirements) where T : class
        {
            var requirementScores = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var requirement in requirements)
            {
                if (requirement is SkillRequirement skillReq)
                {
                    requirementScores[skillReq.Name.ToLower()] = skillReq.Scoring;
                }
                else if (requirement is EducationRequirement eduReq)
                {
                    requirementScores[eduReq.Name.ToLower()] = eduReq.Scoring;
                }
                else if (requirement is ExperienceRequirement expReq)
                {
                    requirementScores[expReq.Name.ToLower()] = expReq.Scoring;
                }
            }
            return requirementScores;
        }

        private double ComputeLSAScore(List<string> allDocuments, Dictionary<string, decimal> requirementScores)
        {
            // Create the term-document matrix
            var termDocumentMatrix = CreateTermDocumentMatrix(allDocuments);

            // Apply requirementScores as weights to the matrix
            var vocabulary = allDocuments.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
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

        private double ComputeBM25Score(List<string> applicantTokens, List<string> jobTokens, Dictionary<string, decimal> requirementScores)
        {
            // Implementation remains unchanged
            // Incorporate requirementScores into the BM25 calculation
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
                    bm25Score += (numerator / denominator) * idf[term];
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
                return (numerator / denominator) * idfValue + scoringValue;
            });
            return maxPossibleScore > 0 ? bm25Score / maxPossibleScore : 0.0;
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

        private double CosineSimilarity(MathNet.Numerics.LinearAlgebra.Vector<double> vec1, MathNet.Numerics.LinearAlgebra.Vector<double> vec2)
        {
            if (vec1.L2Norm() == 0 || vec2.L2Norm() == 0)
                return 0.0;
            return vec1.DotProduct(vec2) / (vec1.L2Norm() * vec2.L2Norm());
        }
    }
}