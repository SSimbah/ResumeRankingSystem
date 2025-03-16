//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ResumeRankingSystem.Services
//{
//    public class PreprocessingHelper
//    {
//        /// <summary>
//        /// Preprocesses text by tokenizing, removing stop words, normalizing case, and handling punctuation.
//        /// </summary>
//        /// <param name="text">The input text to preprocess.</param>
//        /// <returns>A list of preprocessed tokens.</returns>
//        public List<string> Preprocess(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return new List<string>();

//            // Tokenize the text by splitting on spaces and punctuation
//            var tokens = text.Split(new[] { ' ', ',', '.', '\n', '\r', ';', ':', '!', '?', '-', '_', '/', '\\' },
//                                    StringSplitOptions.RemoveEmptyEntries);

//            // Normalize case and remove non-alphanumeric characters
//            tokens = tokens.Select(word => new string(word.Where(char.IsLetterOrDigit).ToArray()).ToLower())
//                           .Where(word => !string.IsNullOrEmpty(word))
//                           .ToArray();

//            // Remove stop words
//            tokens = tokens.Where(word => !StopWords.Contains(word)).ToArray();

//            return tokens.ToList();
//        }

//        /// <summary>
//        /// Common English stop words to remove during preprocessing.
//        /// </summary>
//        private static readonly HashSet<string> StopWords = new HashSet<string>
//        {
//            "a", "an", "the", "and", "or", "but", "in", "on", "at", "of", "to",
//            "is", "are", "was", "were", "be", "been", "being", "have", "has", "had",
//            "do", "does", "did", "will", "would", "shall", "should", "can", "could",
//            "may", "might", "must", "i", "you", "he", "she", "it", "we", "they",
//            "me", "him", "her", "us", "them", "my", "your", "his", "our", "their",
//            "mine", "yours", "hers", "ours", "theirs", "this", "that", "these", "those",
//            "for", "with", "about", "against", "between", "into", "through", "during",
//            "before", "after", "above", "below", "from", "up", "down", "out", "off",
//            "over", "under", "again", "further", "then", "once", "here", "there",
//            "when", "where", "why", "how", "all", "any", "both", "each", "few", "more",
//            "most", "other", "some", "such", "no", "nor", "not", "only", "own", "same",
//            "so", "than", "too", "very", "s", "t", "just", "now"
//        };
//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;

//namespace ResumeRankingSystem.Services
//{
//    public class PreprocessingHelper
//    {
//        private readonly HttpClient _httpClient;

//        public PreprocessingHelper(HttpClient httpClient)
//        {
//            _httpClient = httpClient;
//            _httpClient.BaseAddress = new Uri("http://localhost:5000/");
//        }

//        public async Task<List<string>> Preprocess(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return new List<string>();

//            try
//            {
//                // Call the SpaCy API
//                var response = await _httpClient.PostAsJsonAsync("preprocess", new { text });

//                // Ensure the response is successful
//                response.EnsureSuccessStatusCode();

//                // Parse the response
//                var result = await response.Content.ReadFromJsonAsync<SpacyResponse>();
//                if (result == null)
//                    throw new InvalidOperationException("Failed to preprocess text.");

//                // Combine tokens and named entities
//                var tokens = result.Tokens;
//                var namedEntities = result.NamedEntities.Select(ne => ne.Text.ToLower());
//                var combined = tokens.Concat(namedEntities).Distinct().ToList();

//                return combined;
//            }
//            catch (Exception ex)
//            {
//                // Log the exception for debugging
//                Console.WriteLine($"Error calling SpaCy API: {ex.Message}");
//                throw;
//            }
//        }

//        private class SpacyResponse
//        {
//            public List<string> Tokens { get; set; } = new();
//            public List<NamedEntity> NamedEntities { get; set; } = new();
//        }

//        private class NamedEntity
//        {
//            public string Text { get; set; } = "";
//            public string Label { get; set; } = "";
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ResumeRankingSystem.Services
{
    public class PreprocessingHelper
    {
        // Parse skills (comma-separated list)
        public List<string> PreprocessSkills(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.Split(',')
                .Select(skill => skill.Trim())
                .Where(skill => !string.IsNullOrEmpty(skill))
                .ToList();
        }

        // Parse education entries
        public List<(string School, string Degree)> PreprocessEducation(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<(string School, string Degree)>();

            var results = new List<(string School, string Degree)>();
            var entries = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                var parts = entry.Split(',')
                    .Select(part => part.Trim())
                    .ToList();

                if (parts.Count >= 2)
                {
                    string school = parts[0].TrimStart('-').Trim();
                    string degree = parts[1].Trim();
                    results.Add((school, degree));
                }
            }

            return results;
        }

        // Parse work experience entries
        public List<(string Company, string JobTitle, string Duration, string Description)> PreprocessExperience(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<(string Company, string JobTitle, string Duration, string Description)>();

            var results = new List<(string Company, string JobTitle, string Duration, string Description)>();
            var entries = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                var parts = entry.Split(',')
                    .Select(part => part.Trim())
                    .ToList();

                if (parts.Count >= 4)
                {
                    string company = parts[0].TrimStart('-').Trim();
                    string jobTitle = parts[1].Trim();
                    string duration = ParseDuration(parts[2].Trim()); // Parse duration
                    string description = parts.Count > 3 ? string.Join(", ", parts.Skip(3)) : string.Empty;

                    results.Add((company, jobTitle, duration, description));
                }
            }

            return results;
        }

        // Helper method to parse duration
        private string ParseDuration(string durationText)
        {
            // Check if the duration is in the format "YYYY-YYYY"
            if (durationText.Contains('-') && durationText.Length >= 9)
            {
                var yearParts = durationText.Split('-')
                    .Select(part => part.Trim())
                    .Where(part => int.TryParse(part, out _)) // Ensure both parts are valid integers
                    .Select(int.Parse)
                    .ToList();

                if (yearParts.Count == 2)
                {
                    int startYear = yearParts[0];
                    int endYear = yearParts[1];

                    // Calculate the difference in years
                    int yearsDifference = endYear - startYear;

                    // Return the duration as "X years"
                    return $"{yearsDifference} years";
                }
            }

            // If parsing fails, return the original duration text
            return durationText;
        }

        // Combined method to preprocess all fields
        public (List<string> Skills, List<(string School, string Degree)> Education, List<(string Company, string JobTitle, string Duration, string Description)> Experience) PreprocessAll(
            string applicantSkills,
            string applicantEducation,
            string applicantExperience)
        {
            var skills = PreprocessSkills(applicantSkills);
            var education = PreprocessEducation(applicantEducation);
            var experience = PreprocessExperience(applicantExperience);

            return (skills, education, experience);
        }
    }
}