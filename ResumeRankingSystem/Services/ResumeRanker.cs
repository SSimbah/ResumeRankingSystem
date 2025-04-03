////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Net.Http;
////using System.Text.Json;
////using System.Threading.Tasks;
////using Domain.Entities;
////using MathNet.Numerics.LinearAlgebra;

////namespace ResumeRankingSystem.Services
////{
////    public class ResumeRanker
////    {
////        private const double Alpha = 0.5; // Weight for combining LSA and BM25 scores
////        private readonly HttpClient _httpClient;

////        public ResumeRanker(HttpClient httpClient)
////        {
////            _httpClient = httpClient; // Use HttpClient to call the Flask API
////        }

////        public async Task<(decimal SkillsScoring, decimal EducationScoring, decimal ExperienceScoring, decimal Score)> ScoreApplicant(
////            string applicantSkills,
////            string applicantEducation,
////            string applicantExperience,
////            ICollection<SkillRequirement> jobSkillRequirements,
////            ICollection<EducationRequirement> jobEducationRequirements,
////            ICollection<ExperienceRequirement> jobExperienceRequirements)
////        {
////            if (string.IsNullOrEmpty(applicantSkills) || string.IsNullOrEmpty(applicantEducation) || string.IsNullOrEmpty(applicantExperience))
////                throw new ArgumentNullException("Applicant data cannot be null or empty.");
////            if (jobSkillRequirements == null || jobEducationRequirements == null || jobExperienceRequirements == null)
////                throw new ArgumentNullException("Job requirements cannot be null.");

////            var jobSkillsString = string.Join(", ", jobSkillRequirements.Select(sr => sr.Name));
////            var jobEducationString = string.Join(", ", jobEducationRequirements.Select(sr => sr.Name));
////            var jobExperienceString = string.Join(", ", jobExperienceRequirements.Select(sr => sr.Name));

////            // Preprocess applicant data asynchronously using the Flask API
////            var applicantSkillsTokens = await PreprocessText(applicantSkills);
////            var applicantEducationTokens = await PreprocessText(applicantEducation);
////            var applicantExperienceTokens = await PreprocessText(applicantExperience);

////            // Preprocess job requirements asynchronously using the Flask API
////            var jobSkillsTokens = await PreprocessText(jobSkillsString);
////            var jobEducationTokens = await PreprocessText(jobEducationString);
////            var jobExperienceTokens = await PreprocessText(jobExperienceString);

////            // Compute scores for each category
////            decimal skillsScoring = ComputeCategoryScore(applicantSkillsTokens, jobSkillsTokens);
////            decimal educationScoring = ComputeCategoryScore(applicantEducationTokens, jobEducationTokens);
////            decimal experienceScoring = ComputeCategoryScore(applicantExperienceTokens, jobExperienceTokens);

////            // Calculate the average score
////            decimal score = Math.Round((skillsScoring + educationScoring + experienceScoring) / 3, 3);

////            // Return the scores as a tuple
////            return (skillsScoring, educationScoring, experienceScoring, score);
////        }

////        private decimal ComputeCategoryScore(List<string> applicantTokens, List<string> jobTokens)
////        {
////            // Check if all job tokens are matched by applicant tokens
////            bool allMatched = jobTokens.All(jobToken => applicantTokens.Contains(jobToken));

////            // If all job tokens are matched, return a score of 1
////            if (allMatched)
////                return 1.0m;

////            // Otherwise, compute BM25 and LSA scores
////            var requirementScores = CreateRequirementScores(jobTokens); // Placeholder for scoring weights
////            double bm25Score = ComputeBM25Score(applicantTokens, jobTokens, requirementScores);
////            double lsaScore = ComputeLSAScore(new List<string> { string.Join(" ", applicantTokens), string.Join(" ", jobTokens) }, requirementScores);

////            // Combine BM25 and LSA scores with equal weighting
////            double combinedScore = 0.5 * bm25Score + 0.5 * lsaScore;

////            // Return the combined score rounded to 3 decimal places
////            return Math.Round((decimal)combinedScore, 3);
////        }

////        private Dictionary<string, decimal> CreateRequirementScores(List<string> jobTokens)
////        {
////            // Remove duplicates and assign a fixed weight of 1.0 to each token
////            return jobTokens
////                .Distinct() // Remove duplicate tokens
////                .ToDictionary(token => token, token => 1.0m); // Assign fixed weight
////        }

////        private double ComputeBM25Score(List<string> applicantTokens, List<string> jobTokens, Dictionary<string, decimal> requirementScores)
////        {
////            var idf = ComputeIDF(new List<List<string>> { jobTokens });
////            const double k1 = 1.2;
////            const double b = 0.75;
////            double avgdl = jobTokens.Count;
////            double bm25Score = 0;

////            foreach (var term in jobTokens.Distinct())
////            {
////                if (idf.ContainsKey(term))
////                {
////                    int termFrequency = applicantTokens.Count(word => word == term);
////                    double numerator = termFrequency * (k1 + 1);
////                    double denominator = termFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
////                    bm25Score += numerator / denominator * idf[term];
////                }

////                // Add scoring values
////                if (requirementScores.ContainsKey(term))
////                {
////                    double scoringValue = (double)requirementScores[term];
////                    double scaledScoringValue = scoringValue * idf[term]; // Scale scoring values by IDF
////                    bm25Score += scaledScoringValue;
////                }
////            }

////            // Normalize the score
////            double maxPossibleScore = jobTokens.Distinct().Sum(term =>
////            {
////                double idfValue = idf.ContainsKey(term) ? idf[term] : 0;
////                double scoringValue = requirementScores.ContainsKey(term) ? (double)requirementScores[term] : 0;
////                int maxTermFrequency = applicantTokens.Count(word => word == term);
////                double numerator = maxTermFrequency * (k1 + 1);
////                double denominator = maxTermFrequency + k1 * (1 - b + b * applicantTokens.Count / avgdl);
////                return numerator / denominator * idfValue + scoringValue;
////            });

////            return maxPossibleScore > 0 ? bm25Score / maxPossibleScore : 0.0;
////        }

////        private double ComputeLSAScore(List<string> documents, Dictionary<string, decimal> requirementScores)
////        {
////            // Create the term-document matrix
////            var termDocumentMatrix = CreateTermDocumentMatrix(documents);

////            // Apply requirementScores as weights to the matrix
////            var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
////            for (int i = 0; i < vocabulary.Count; i++)
////            {
////                var term = vocabulary[i];
////                if (requirementScores.ContainsKey(term))
////                {
////                    // Multiply the row corresponding to this term by its scoring value
////                    var row = termDocumentMatrix.Row(i);
////                    row = row.Multiply((double)requirementScores[term]);
////                    termDocumentMatrix.SetRow(i, row);
////                }
////            }

////            // Perform SVD and compute cosine similarity
////            var svd = termDocumentMatrix.Svd(true);
////            var U = svd.U;
////            var S = svd.W;
////            int k = 2;
////            var reducedMatrix = U.SubMatrix(0, U.RowCount, 0, k) * S.SubMatrix(0, k, 0, k);
////            var applicantVector = reducedMatrix.Row(0);
////            var jobVector = reducedMatrix.Row(1);

////            return CosineSimilarity(applicantVector, jobVector);
////        }

////        private Dictionary<string, double> ComputeIDF(List<List<string>> documents)
////        {
////            var idf = new Dictionary<string, double>();
////            int N = documents.Count;
////            var docFrequency = new Dictionary<string, int>();

////            foreach (var doc in documents)
////            {
////                var uniqueTerms = new HashSet<string>(doc);
////                foreach (var term in uniqueTerms)
////                {
////                    if (!docFrequency.ContainsKey(term))
////                        docFrequency[term] = 0;
////                    docFrequency[term]++;
////                }
////            }

////            foreach (var term in docFrequency.Keys)
////            {
////                // Apply a damping factor to reduce the impact of large IDF values
////                idf[term] = Math.Log(1 + (double)(N - docFrequency[term] + 0.5) / (docFrequency[term] + 0.5));
////            }

////            return idf;
////        }

////        private Matrix<double> CreateTermDocumentMatrix(List<string> documents)
////        {
////            var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
////            var matrix = Matrix<double>.Build.Dense(vocabulary.Count, documents.Count);

////            for (int i = 0; i < vocabulary.Count; i++)
////            {
////                var term = vocabulary[i];
////                for (int j = 0; j < documents.Count; j++)
////                {
////                    var tf = documents[j].Split(' ').Count(word => word == term);
////                    var idf = Math.Log((double)documents.Count / (1 + documents.Count(doc => doc.Contains(term))));
////                    matrix[i, j] = tf * idf;
////                }
////            }

////            return matrix;
////        }

////        private double CosineSimilarity(Vector<double> vec1, Vector<double> vec2)
////        {
////            if (vec1.L2Norm() == 0 || vec2.L2Norm() == 0)
////                return 0.0;
////            return vec1.DotProduct(vec2) / (vec1.L2Norm() * vec2.L2Norm());
////        }

////        private async Task<List<string>> PreprocessText(string text)
////        {
////            // Prepare the request payload
////            var payload = new { text };
////            var jsonPayload = JsonSerializer.Serialize(payload);
////            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

////            // Send the request to the Flask API
////            var response = await _httpClient.PostAsync("http://localhost:5000/preprocess", content);
////            response.EnsureSuccessStatusCode();

////            // Parse the response
////            var jsonResponse = await response.Content.ReadAsStringAsync();
////            var responseObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);

////            // Extract stemmed tokens from the response
////            var tokens = responseObject["tokens"].EnumerateArray()
////                .Select(token => token.GetProperty("stem").GetString()) // Use stemmed tokens
////                .Where(token => !string.IsNullOrEmpty(token)) // Filter out empty tokens
////                .ToList();

////            return tokens;
////        }
////    }
////}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Domain.Entities;
//using MathNet.Numerics.LinearAlgebra;

//namespace ResumeRankingSystem.Services
//{
//    public class ResumeRanker
//    {
//        private const double Alpha = 0.5; // Weight for combining LSA and BM25 scores
//        private readonly HttpClient _httpClient;

//        public ResumeRanker(HttpClient httpClient)
//        {
//            _httpClient = httpClient; // Use HttpClient to call the Flask API
//        }

//        public async Task<(decimal SkillsScoring, decimal EducationScoring, decimal ExperienceScoring, decimal Score)> ScoreApplicant(
//    string applicantSkills,
//    string applicantEducation,
//    string applicantExperience,
//    ICollection<SkillRequirement> jobSkillRequirements,
//    ICollection<EducationRequirement> jobEducationRequirements,
//    ICollection<ExperienceRequirement> jobExperienceRequirements)
//        {
//            if (string.IsNullOrEmpty(applicantSkills) || string.IsNullOrEmpty(applicantEducation) || string.IsNullOrEmpty(applicantExperience))
//                throw new ArgumentNullException("Applicant data cannot be null or empty.");
//            if (jobSkillRequirements == null || jobEducationRequirements == null || jobExperienceRequirements == null)
//                throw new ArgumentNullException("Job requirements cannot be null.");

//            // Separate applicant data into individual items
//            var applicantSkillsList = applicantSkills.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
//                                                     .Select(skill => skill.Trim().ToLower())
//                                                     .ToList();
//            // Split the input string into individual education entries
//            var educationEntries = applicantEducation.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

//            // Extract only the degree (second field) from each entry and convert to lowercase
//            var applicantEducationList = educationEntries
//                .Select(entry => entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) // Split by commas
//                .Where(parts => parts.Length >= 2) // Ensure there are at least two fields
//                .Select(parts => parts[1].Trim().ToLower()) // Select the second field (degree), trim whitespace, and convert to lowercase
//                .Distinct() // Remove duplicates
//                .ToList();

//            // Split the input string into individual experience entries
//            var experienceEntries = applicantExperience.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

//            // Extract Job Title and Description from each entry and convert to lowercase
//            var applicantExperienceList = experienceEntries
//                .Select(entry => entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) // Split each entry by commas
//                .Where(parts => parts.Length >= 4) // Ensure there are at least four fields
//                .Select(parts => new
//                {
//                    JobTitle = parts[1].Trim().ToLower(), // Second field is the Job Title, trimmed and converted to lowercase
//                    Description = parts[3].Trim().ToLower() // Fourth field is the Description, trimmed and converted to lowercase
//                })
//                .ToList();

//            // If you want to combine Job Title and Description into a single list:
//            var combinedExperienceList = applicantExperienceList
//                .SelectMany(exp => new[] { exp.JobTitle, exp.Description }) // Flatten into a single list
//                .Distinct() // Remove duplicates
//                .ToList();

//            // Preprocess job requirements asynchronously using the Flask API
//            var jobSkillsSynonyms = await PreprocessJobRequirements(jobSkillRequirements);
//            var jobEducationSynonyms = await PreprocessJobRequirements(jobEducationRequirements);
//            var jobExperienceSynonyms = await PreprocessJobRequirements(jobExperienceRequirements);

//            decimal skillsScoring = ComputeCategoryScore(
//                applicantSkillsList,
//                jobSkillsSynonyms.ToDictionary(
//                    kvp => kvp.Key,
//                    kvp => (
//                        SynonymsWithScores: kvp.Value.SynonymsWithScores, // Pass the full list of (Synonym, Score) tuples
//                        Scoring: kvp.Value.Scoring
//                    )
//                )
//            );

//            decimal educationScoring = ComputeCategoryScore(
//                applicantEducationList,
//                jobEducationSynonyms.ToDictionary(
//                    kvp => kvp.Key,
//                    kvp => (
//                        SynonymsWithScores: kvp.Value.SynonymsWithScores, // Pass the full list of (Synonym, Score) tuples
//                        Scoring: kvp.Value.Scoring
//                    )
//                )
//            );

//            decimal experienceScoring = ComputeCategoryScore(
//                combinedExperienceList,
//                jobExperienceSynonyms.ToDictionary(
//                    kvp => kvp.Key,
//                    kvp => (
//                        SynonymsWithScores: kvp.Value.SynonymsWithScores, // Pass the full list of (Synonym, Score) tuples
//                        Scoring: kvp.Value.Scoring
//                    )
//                )
//            );

//            // Calculate the average score
//            decimal score = Math.Round((skillsScoring + educationScoring + experienceScoring) / 3, 3);

//            // Return the scores as a tuple
//            return (skillsScoring, educationScoring, experienceScoring, score);
//        }
//        //private decimal ComputeCategoryScore(List<string> applicantItems, Dictionary<string, (List<string> Synonyms, decimal Scoring)> jobRequirements)
//        //{
//        //    // Total possible score (sum of all Scoring values)
//        //    //decimal totalPossibleScore = jobRequirements.Values.Sum(req => req.Scoring);

//        //    //if (totalPossibleScore == 0)
//        //    //{
//        //    //    Console.WriteLine("Warning: Total possible score is zero. Returning 0.");
//        //    //    return 0;
//        //    //}

//        //    // Prepare lists for BM25 and LSA computation
//        //    var jobTokens = jobRequirements.Keys.ToList();
//        //    var requirementScores = jobRequirements.ToDictionary(
//        //        req => req.Key,
//        //        req => req.Value.Scoring
//        //    );

//        //    // Compute BM25 and LSA scores
//        //    double bm25Score = ComputeBM25Score(applicantItems, jobTokens, jobRequirements);
//        //    double lsaScore = ComputeLSAScore(new List<string> { string.Join(" ", applicantItems), string.Join(" ", jobTokens) }, requirementScores);

//        //    // Combine BM25 and LSA scores with 50/50 weighting
//        //    double combinedScore = 0.5 * bm25Score + 0.5 * lsaScore;

//        //    // Normalize the score by dividing by the total possible score
//        //    //decimal normalizedScore = totalPossibleScore > 0
//        //    //    ? Math.Round((decimal)(combinedScore) / totalPossibleScore, 4)
//        //    //    : 0;

//        //    //Console.WriteLine($"BM25 Score: {bm25Score}, LSA Score: {lsaScore}, Combined Score: {combinedScore}, Normalized Score: {normalizedScore}");

//        //    return (decimal)combinedScore;
//        //}
//        private decimal ComputeCategoryScore(List<string> applicantItems, Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)> jobRequirements)
//        {
//            // Expand job tokens to include synonyms
//            var expandedJobTokens = jobRequirements.Keys.SelectMany(term =>
//                new[] { term }.Concat(jobRequirements[term].SynonymsWithScores.Select(s => s.Synonym))
//            ).Distinct().ToList();

//            // Prepare requirement scores for the expanded tokens
//            var requirementScores = jobRequirements.ToDictionary(
//                req => req.Key,
//                req => req.Value.Scoring
//            );

//            // Compute BM25 and LSA scores
//            //double bm25Score = ComputeBM25Score(applicantItems, expandedJobTokens, jobRequirements);
//            double bm25Score = ComputeBM25Score(
//                applicantItems,
//                expandedJobTokens,
//                jobRequirements.ToDictionary(
//                    kvp => kvp.Key,
//                    kvp => (
//                        Synonyms: kvp.Value.SynonymsWithScores.Select(s => s.Synonym).ToList(), // Extract only the synonym strings
//                        Scoring: kvp.Value.Scoring
//                    )
//                )
//            );
//            double lsaScore = ComputeLSAScore(new List<string> { string.Join(" ", applicantItems), string.Join(" ", expandedJobTokens) }, requirementScores);

//            // Combine BM25 and LSA scores with equal weighting
//            double combinedScore = Alpha * bm25Score + (1 - Alpha) * lsaScore;

//            return (decimal)combinedScore;
//        }

//        //private decimal ComputeCategoryScore(List<string> applicantItems, Dictionary<string, (List<string> Synonyms, decimal Scoring)> jobRequirements)
//        //{
//        //    // Total possible score (sum of all Scoring values)
//        //    decimal totalPossibleScore = jobRequirements.Values.Sum(req => req.Scoring);

//        //    // Prepare lists for BM25 and LSA computation
//        //    var jobTokens = jobRequirements.Keys.ToList();
//        //    var requirementScores = jobRequirements.ToDictionary(
//        //        req => req.Key,
//        //        req => req.Value.Scoring
//        //    );

//        //    // Compute BM25 and LSA scores
//        //    double bm25Score = ComputeBM25Score(applicantItems, jobTokens, requirementScores);
//        //    double lsaScore = ComputeLSAScore(new List<string> { string.Join(" ", applicantItems), string.Join(" ", jobTokens) }, requirementScores);

//        //    // Combine BM25 and LSA scores with equal weighting
//        //    double combinedScore = Alpha * bm25Score + (1 - Alpha) * lsaScore;

//        //    // Normalize the score by dividing by the total possible score
//        //    // Normalize the score by dividing by the total possible score
//        //    return totalPossibleScore > 0
//        //        ? Math.Round((decimal)(combinedScore) / totalPossibleScore, 3)
//        //        : 0;
//        //}

//        //    private async Task<Dictionary<string, (List<string> Synonyms, decimal Scoring)>> PreprocessJobRequirements<T>(ICollection<T> requirements)
//        //where T : class
//        //    {
//        //        var result = new Dictionary<string, (List<string> Synonyms, decimal Scoring)>();

//        //        foreach (var requirement in requirements)
//        //        {
//        //            // Convert the requirement name to lowercase immediately
//        //            string name = (requirement switch
//        //            {
//        //                SkillRequirement sr => sr.Name ?? throw new InvalidOperationException("Name cannot be null."),
//        //                EducationRequirement er => er.Name ?? throw new InvalidOperationException("Name cannot be null."),
//        //                ExperienceRequirement exr => exr.Name ?? throw new InvalidOperationException("Name cannot be null."),
//        //                _ => throw new InvalidOperationException("Unsupported requirement type.")
//        //            }).ToLower(); // Ensure lowercase

//        //            decimal scoring = requirement switch
//        //            {
//        //                SkillRequirement sr => sr.Scoring,
//        //                EducationRequirement er => er.Scoring,
//        //                ExperienceRequirement exr => exr.Scoring,
//        //                _ => throw new InvalidOperationException("Unsupported requirement type.")
//        //            };

//        //            Console.WriteLine($"Processing requirement: {name}, Scoring: {scoring}");

//        //            var payload = new { text = name, is_job_requirements = true };
//        //            var jsonPayload = JsonSerializer.Serialize(payload);
//        //            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

//        //            Console.WriteLine($"Sending request to Flask API: {jsonPayload}");

//        //            var response = await _httpClient.PostAsync("http://localhost:5000/preprocess", content);
//        //            response.EnsureSuccessStatusCode();

//        //            var jsonResponse = await response.Content.ReadAsStringAsync();
//        //            Console.WriteLine($"Flask API Response: {jsonResponse}");

//        //            var responseObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);

//        //            // Ensure the "requirements" key exists in the response
//        //            if (!responseObject.ContainsKey("requirements"))
//        //            {
//        //                Console.WriteLine($"Warning: The 'requirements' key was not found in the Flask API response for requirement '{name}'.");
//        //                continue;
//        //            }

//        //            var requirementsArray = responseObject["requirements"].EnumerateArray().ToList();
//        //            Console.WriteLine($"Parsed requirements array: {JsonSerializer.Serialize(requirementsArray)}");

//        //            if (requirementsArray.Count == 0)
//        //            {
//        //                Console.WriteLine($"Warning: No synonyms found for requirement '{name}'. Using the original value as a synonym.");
//        //                result[name] = (new List<string> { name }, scoring); // Add the requirement with the original name as a synonym
//        //                continue;
//        //            }

//        //            //// Extract synonyms and ensure they are lowercase
//        //            //var synonyms = requirementsArray
//        //            //    .SelectMany(req => req.GetProperty("synonyms").EnumerateArray()
//        //            //    .Select(synonym => synonym.GetString()?.ToLower())) // Convert synonyms to lowercase
//        //            //    .Where(synonym => !string.IsNullOrEmpty(synonym)) // Filter out any null or empty strings
//        //            //    .ToList();

//        //            //Console.WriteLine($"Synonyms for '{name}': {string.Join(", ", synonyms)}");

//        //            //result[name] = (synonyms, scoring);
//        //            // Extract synonyms and ensure they are lowercase
//        //            var synonyms = requirementsArray
//        //                .SelectMany(req => req.GetProperty("synonyms").EnumerateArray()
//        //                .Select(synonym => synonym.GetString()?.ToLower())) // Convert synonyms to lowercase
//        //                .Where(synonym => !string.IsNullOrEmpty(synonym)) // Filter out any null or empty strings
//        //                .Select(synonym => synonym!) // Assert that synonym is non-null
//        //                .ToList();

//        //            Console.WriteLine($"Synonyms for '{name}': {string.Join(", ", synonyms)}");

//        //            if (synonyms.Count == 0)
//        //            {
//        //                Console.WriteLine($"Warning: No synonyms extracted for requirement '{name}'. Using the original value as a synonym.");
//        //                result[name] = (new List<string> { name }, scoring); // Fallback to the original value
//        //            }
//        //            else
//        //            {
//        //                result[name] = (synonyms, scoring);
//        //            }
//        //        }

//        //        return result;
//        //    }
//        private async Task<Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)>> PreprocessJobRequirements<T>(ICollection<T> requirements)
//        where T : class
//        {
//            var result = new Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)>();

//            foreach (var requirement in requirements)
//            {
//                // Convert the requirement name to lowercase immediately
//                string name = (requirement switch
//                {
//                    SkillRequirement sr => sr.Name ?? throw new InvalidOperationException("Name cannot be null."),
//                    EducationRequirement er => er.Name ?? throw new InvalidOperationException("Name cannot be null."),
//                    ExperienceRequirement exr => exr.Name ?? throw new InvalidOperationException("Name cannot be null."),
//                    _ => throw new InvalidOperationException("Unsupported requirement type.")
//                }).ToLower(); // Ensure lowercase

//                decimal scoring = requirement switch
//                {
//                    SkillRequirement sr => sr.Scoring,
//                    EducationRequirement er => er.Scoring,
//                    ExperienceRequirement exr => exr.Scoring,
//                    _ => throw new InvalidOperationException("Unsupported requirement type.")
//                };

//                Console.WriteLine($"Processing requirement: {name}, Scoring: {scoring}");

//                var payload = new { text = name, is_job_requirements = true };
//                var jsonPayload = JsonSerializer.Serialize(payload);
//                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

//                Console.WriteLine($"Sending request to Flask API: {jsonPayload}");

//                var response = await _httpClient.PostAsync("http://localhost:5000/preprocess", content);
//                response.EnsureSuccessStatusCode();

//                var jsonResponse = await response.Content.ReadAsStringAsync();
//                Console.WriteLine($"Flask API Response: {jsonResponse}");

//                var responseObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);

//                // Ensure the "requirements" key exists in the response
//                if (!responseObject.ContainsKey("requirements"))
//                {
//                    Console.WriteLine($"Warning: The 'requirements' key was not found in the Flask API response for requirement '{name}'.");
//                    continue;
//                }

//                var requirementsArray = responseObject["requirements"].EnumerateArray().ToList();
//                Console.WriteLine($"Parsed requirements array: {JsonSerializer.Serialize(requirementsArray)}");

//                if (requirementsArray.Count == 0)
//                {
//                    Console.WriteLine($"Warning: No synonyms found for requirement '{name}'. Using the original value as a synonym.");
//                    result[name] = (new List<(string Synonym, double Score)> { (name, 1.0) }, scoring); // Default score of 1.0 for the original term
//                    continue;
//                }

//                // Extract synonyms and their scores, ensuring they are lowercase
//                var synonymsWithScores = requirementsArray
//                    .SelectMany(req => req.GetProperty("synonyms").EnumerateArray()
//                        .Select(synonymObj =>
//                        {
//                            var synonym = synonymObj.GetProperty("synonym").GetString()?.ToLower();
//                            var score = synonymObj.GetProperty("score").GetDouble();
//                            return (Synonym: synonym, Score: score);
//                        }))
//                    .Where(synonymScore => !string.IsNullOrEmpty(synonymScore.Synonym) && synonymScore.Score > 0.5) // Filter out low-quality synonyms
//                    .ToList();

//                Console.WriteLine($"Synonyms with scores for '{name}': {string.Join(", ", synonymsWithScores.Select(ss => $"{ss.Synonym}: {ss.Score:F4}"))}");

//                if (synonymsWithScores.Count == 0)
//                {
//                    Console.WriteLine($"Warning: No valid synonyms extracted for requirement '{name}'. Using the original value as a synonym.");
//                    result[name] = (new List<(string Synonym, double Score)> { (name, 1.0) }, scoring); // Fallback to the original value
//                }
//                else
//                {
//                    result[name] = (synonymsWithScores, scoring);
//                }
//            }

//            return result;
//        }

//        private double ComputeBM25Score(List<string> applicantItems, List<string> jobTokens, Dictionary<string, (List<string> Synonyms, decimal Scoring)> requirementScores)
//        {
//            // Normalize applicant items to avoid duplicates
//            var normalizedApplicantItems = applicantItems.Distinct().ToList();

//            // Expand job tokens to include synonyms
//            var expandedJobTokens = jobTokens.SelectMany(term =>
//                new[] { term }.Concat(requirementScores.ContainsKey(term) ? requirementScores[term].Synonyms : Enumerable.Empty<string>())
//            ).Distinct().ToList();

//            var idf = ComputeIDF(new List<List<string>> { expandedJobTokens });
//            const double k1 = 1.5;
//            const double b = 0.75;
//            double avgdl = expandedJobTokens.Count;
//            double bm25Score = 0;

//            foreach (var term in expandedJobTokens)
//            {
//                if (idf.ContainsKey(term))
//                {
//                    int termFrequency = normalizedApplicantItems.Count(item => item == term);
//                    double numerator = termFrequency * (k1 + 1);
//                    double denominator = termFrequency + k1 * (1 - b + b * normalizedApplicantItems.Count / avgdl);
//                    bm25Score += numerator / denominator * idf[term];
//                    //bm25Score += numerator / denominator;
//                }

//                // Add scoring values
//                if (requirementScores.ContainsKey(term))
//                {
//                    double scoringValue = (double)requirementScores[term].Scoring;
//                    double scaledScoringValue = scoringValue * (idf[term]); // Hybrid weighting
//                    bm25Score += scaledScoringValue;    
//                }

//                Console.WriteLine($"Processing term: {term}, Term Frequency: {normalizedApplicantItems.Count(item => item == term)}, IDF: {idf[term]}");
//            }

//            // Calculate maxPossibleScore as the sum of all scoring values
//            double maxPossibleScore = requirementScores.Values.Sum(req => (double)req.Scoring);

//            // Ensure the score is capped at 1
//            double normalizedScore = maxPossibleScore > 0 ? bm25Score / maxPossibleScore : 0.0;
//            normalizedScore = Math.Min(normalizedScore, 1.0);

//            Console.WriteLine($"BM25 Score: {bm25Score}, Max Possible Score: {maxPossibleScore}, Normalized Score: {normalizedScore}");

//            return normalizedScore;
//        }

//        //private double ComputeLSAScore(List<string> documents, Dictionary<string, decimal> requirementScores)
//        //{
//        //    // Create the term-document matrix
//        //    var termDocumentMatrix = CreateTermDocumentMatrix(documents);

//        //    // Apply requirementScores as weights to the matrix
//        //    var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
//        //    for (int i = 0; i < vocabulary.Count; i++)
//        //    {
//        //        var term = vocabulary[i];
//        //        if (requirementScores.ContainsKey(term))
//        //        {
//        //            // Multiply the row corresponding to this term by its scoring value
//        //            var row = termDocumentMatrix.Row(i);
//        //            row = row.Multiply((double)requirementScores[term]);
//        //            termDocumentMatrix.SetRow(i, row);
//        //        }
//        //    }

//        //    // Perform SVD and compute cosine similarity
//        //    var svd = termDocumentMatrix.Svd(true);
//        //    var U = svd.U;
//        //    var S = svd.W;
//        //    int k = 2;
//        //    var reducedMatrix = U.SubMatrix(0, U.RowCount, 0, k) * S.SubMatrix(0, k, 0, k);
//        //    var applicantVector = reducedMatrix.Row(0);
//        //    var jobVector = reducedMatrix.Row(1);
//        //    return CosineSimilarity(applicantVector, jobVector);
//        //}

//        private double ComputeLSAScore(List<string> documents, Dictionary<string, decimal> requirementScores)
//        {
//            // Create the term-document matrix
//            var termDocumentMatrix = CreateTermDocumentMatrix(documents);

//            // Apply requirementScores as weights to the matrix
//            var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
//            for (int i = 0; i < vocabulary.Count; i++)
//            {
//                var term = vocabulary[i];
//                if (requirementScores.ContainsKey(term))
//                {
//                    // Multiply the row corresponding to this term by its scoring value
//                    var row = termDocumentMatrix.Row(i);
//                    row = row.Multiply((double)requirementScores[term]);
//                    termDocumentMatrix.SetRow(i, row);
//                }
//            }

//            // Perform SVD and compute cosine similarity
//            var svd = termDocumentMatrix.Svd(true);
//            var U = svd.U;
//            var S = svd.W;
//            int k = 2;
//            var reducedMatrix = U.SubMatrix(0, U.RowCount, 0, k) * S.SubMatrix(0, k, 0, k);
//            var applicantVector = reducedMatrix.Row(0);
//            var jobVector = reducedMatrix.Row(1);

//            return CosineSimilarity(applicantVector, jobVector);
//        }

//        private Dictionary<string, double> ComputeIDF(List<List<string>> documents)
//        {
//            var idf = new Dictionary<string, double>();
//            int N = documents.Count;
//            var docFrequency = new Dictionary<string, int>();

//            foreach (var doc in documents)
//            {
//                var uniqueTerms = new HashSet<string>(doc);
//                foreach (var term in uniqueTerms)
//                {
//                    if (!docFrequency.ContainsKey(term))
//                        docFrequency[term] = 0;
//                    docFrequency[term]++;
//                }
//            }

//            foreach (var term in docFrequency.Keys)
//            {
//                // Apply a damping factor to reduce the impact of large IDF values
//                idf[term] = Math.Log(1 + (double)(N - docFrequency[term] + 0.5) / (docFrequency[term] + 0.5));
//            }

//            return idf;
//        }

//        //private Matrix<double> CreateTermDocumentMatrix(List<string> documents)
//        //{
//        //    var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
//        //    var matrix = Matrix<double>.Build.Dense(vocabulary.Count, documents.Count);

//        //    for (int i = 0; i < vocabulary.Count; i++)
//        //    {
//        //        var term = vocabulary[i];
//        //        for (int j = 0; j < documents.Count; j++)
//        //        {
//        //            var tf = documents[j].Split(' ').Count(word => word == term);
//        //            var idf = Math.Log((double)documents.Count / (1 + documents.Count(doc => doc.Contains(term))));
//        //            matrix[i, j] = tf * idf;
//        //        }
//        //    }

//        //    return matrix;
//        //}
//        private Matrix<double> CreateTermDocumentMatrix(List<string> documents)
//        {
//            // Split documents into terms and create a vocabulary
//            var vocabulary = documents.SelectMany(doc => doc.Split(' ')).Distinct().ToList();
//            var matrix = Matrix<double>.Build.Dense(vocabulary.Count, documents.Count);

//            for (int i = 0; i < vocabulary.Count; i++)
//            {
//                var term = vocabulary[i];
//                for (int j = 0; j < documents.Count; j++)
//                {
//                    var tf = documents[j].Split(' ').Count(word => word == term);
//                    var idf = Math.Log((double)documents.Count / (1 + documents.Count(doc => doc.Contains(term))));
//                    matrix[i, j] = tf * idf;
//                }
//            }

//            return matrix;
//        }

//        private double CosineSimilarity(Vector<double> vec1, Vector<double> vec2)
//        {
//            if (vec1.L2Norm() == 0 || vec2.L2Norm() == 0)
//                return 0.0;
//            return vec1.DotProduct(vec2) / (vec1.L2Norm() * vec2.L2Norm());
//        }
//    }
//}

using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ResumeRankingSystem.Services
{
    public class ResumeRanker
    {
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

            // Separate applicant data into individual items
            var applicantSkillsList = applicantSkills.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(skill => skill.Trim().ToLower())
                                                     .ToList();

            var educationEntries = applicantEducation.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var applicantEducationList = educationEntries
                .Select(entry => entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length >= 2)
                .Select(parts => parts[1].Trim().ToLower())
                .Distinct()
                .ToList();

            var experienceEntries = applicantExperience.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var applicantExperienceList = experienceEntries
                .Select(entry => entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length >= 4)
                .SelectMany(parts => new[] { parts[1].Trim().ToLower(), parts[3].Trim().ToLower() })
                .Distinct()
                .ToList();

            // Preprocess job requirements asynchronously using the Flask API
            var jobSkillsSynonyms = await PreprocessJobRequirements(jobSkillRequirements);
            var jobEducationSynonyms = await PreprocessJobRequirements(jobEducationRequirements);
            var jobExperienceSynonyms = await PreprocessJobRequirements(jobExperienceRequirements);

            // Compute scores for each category
            decimal skillsScoring = ComputeCategoryScore(applicantSkillsList, jobSkillsSynonyms);
            decimal educationScoring = ComputeCategoryScore(applicantEducationList, jobEducationSynonyms);
            decimal experienceScoring = ComputeCategoryScore(applicantExperienceList, jobExperienceSynonyms);

            // Calculate the average score
            decimal score = Math.Round((skillsScoring + educationScoring + experienceScoring) / 3, 3);

            // Return the scores as a tuple
            return (skillsScoring, educationScoring, experienceScoring, score);
        }

        //    private decimal ComputeCategoryScore(
        //List<string> applicantItems,
        //Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)> jobRequirements)
        //    {
        //        decimal totalScore = 0;

        //        foreach (var requirement in jobRequirements)
        //        {
        //            string requirementName = requirement.Key.ToLower();
        //            var synonymsWithScores = requirement.Value.SynonymsWithScores;
        //            decimal scoringWeight = requirement.Value.Scoring;

        //            // Combine the original requirement name with its synonyms for matching
        //            var allTermsToMatch = new HashSet<string> { requirementName }; // Start with the original value
        //            allTermsToMatch.UnionWith(synonymsWithScores.Select(synonym => synonym.Synonym)); // Add all synonyms

        //            // Check if any term (original value or synonym) matches the applicant's items
        //            bool isMatch = allTermsToMatch.Any(term => applicantItems.Contains(term));

        //            if (isMatch)
        //            {
        //                totalScore += scoringWeight; // Add the scoring weight if a match is found
        //            }
        //        }

        //        // Normalize the score by dividing by the total possible score
        //        decimal maxPossibleScore = jobRequirements.Values.Sum(req => req.Scoring);
        //        decimal normalizedScore = maxPossibleScore > 0 ? Math.Round(totalScore / maxPossibleScore, 4) : 0;

        //        return normalizedScore;
        //    }

        private decimal ComputeCategoryScore(
    List<string> applicantItems,
    Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)> jobRequirements)
        {
            // Create a dictionary to store all terms (original + synonyms) with their weighted scores
            var termScores = new Dictionary<string, decimal>();

            foreach (var requirement in jobRequirements)
            {
                string requirementName = requirement.Key.ToLower();
                var synonymsWithScores = requirement.Value.SynonymsWithScores;
                decimal scoringWeight = requirement.Value.Scoring;

                // Add the original requirement name with a similarity score of 1.0
                termScores[requirementName] = scoringWeight; // Original term has full scoring weight

                // Add synonyms with their similarity scores multiplied by the scoring weight
                foreach (var synonym in synonymsWithScores)
                {
                    string synonymKey = synonym.Synonym.ToLower();
                    decimal weightedScore = scoringWeight * (decimal)synonym.Score; // Scale by similarity score
                    if (termScores.ContainsKey(synonymKey))
                    {
                        // If the synonym already exists, take the maximum score
                        termScores[synonymKey] = Math.Max(termScores[synonymKey], weightedScore);
                    }
                    else
                    {
                        termScores[synonymKey] = weightedScore;
                    }
                }
            }

            // Calculate the total score by summing up the scores for matched terms
            decimal totalScore = 0;
            foreach (var item in applicantItems)
            {
                string lowerItem = item.ToLower(); // Normalize the applicant item
                if (termScores.ContainsKey(lowerItem))
                {
                    totalScore += termScores[lowerItem]; // Add the precomputed weighted score
                }
            }

            // Normalize the score by dividing by the total possible score
            decimal maxPossibleScore = jobRequirements.Values.Sum(req => req.Scoring);
            decimal normalizedScore = maxPossibleScore > 0 ? Math.Round(totalScore / maxPossibleScore, 4) : 0;

            return normalizedScore;
        }

        private async Task<Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)>> PreprocessJobRequirements<T>(ICollection<T> requirements)
            where T : class
        {
            var result = new Dictionary<string, (List<(string Synonym, double Score)> SynonymsWithScores, decimal Scoring)>();

            foreach (var requirement in requirements)
            {
                string name = requirement switch
                {
                    SkillRequirement sr => sr.Name ?? throw new InvalidOperationException("Name cannot be null."),
                    EducationRequirement er => er.Name ?? throw new InvalidOperationException("Name cannot be null."),
                    ExperienceRequirement exr => exr.Name ?? throw new InvalidOperationException("Name cannot be null."),
                    _ => throw new InvalidOperationException("Unsupported requirement type.")
                };

                decimal scoring = requirement switch
                {
                    SkillRequirement sr => sr.Scoring,
                    EducationRequirement er => er.Scoring,
                    ExperienceRequirement exr => exr.Scoring,
                    _ => throw new InvalidOperationException("Unsupported requirement type.")
                };

                var payload = new { text = name.ToLower(), is_job_requirements = true };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:5000/preprocess", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);

                if (!responseObject.ContainsKey("requirements"))
                {
                    Console.WriteLine($"Warning: The 'requirements' key was not found in the Flask API response for requirement '{name}'.");
                    continue;
                }

                var requirementsArray = responseObject["requirements"].EnumerateArray().ToList();

                if (requirementsArray.Count == 0)
                {
                    Console.WriteLine($"Warning: No synonyms found for requirement '{name}'. Using the original value as a synonym.");
                    result[name] = (new List<(string Synonym, double Score)> { (name.ToLower(), 1.0) }, scoring);
                    continue;
                }

                var synonymsWithScores = requirementsArray
                    .SelectMany(req => req.GetProperty("synonyms").EnumerateArray()
                        .Select(synonymObj =>
                        {
                            var synonym = synonymObj.GetProperty("synonym").GetString()?.ToLower();
                            var score = synonymObj.GetProperty("score").GetDouble();
                            return (Synonym: synonym, Score: score);
                        }))
                    .Where(synonymScore => !string.IsNullOrEmpty(synonymScore.Synonym) && synonymScore.Score > 0.5)
                    .ToList();

                if (synonymsWithScores.Count == 0)
                {
                    Console.WriteLine($"Warning: No valid synonyms extracted for requirement '{name}'. Using the original value as a synonym.");
                    result[name] = (new List<(string Synonym, double Score)> { (name.ToLower(), 1.0) }, scoring);
                }
                else
                {
                    result[name] = (synonymsWithScores, scoring);
                }
            }

            return result;
        }
    }
}