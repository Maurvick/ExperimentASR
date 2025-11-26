using System.Text.Json;
using ExperimentASR.Models;

namespace ExperimentASR.Services
{
    public class LogParser
    {
        public TranscriptResult Parse(string output, string error)
        {
            // 1. Handle cases where the script crashed or produced no output
            if (string.IsNullOrWhiteSpace(output))
            {
                return new TranscriptResult
                {
                    Status = "error",
                    Message = !string.IsNullOrWhiteSpace(error)
                        ? $"Python error: {error}"
                        : "No output received from process."
                };
            }

            try
            {
                // 2. Configure options to be forgiving with casing (e.g., "Status" vs "status")
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                // 3. Deserialize directly. This replaces the manual "TryGetProperty" checks.
                var result = JsonSerializer.Deserialize<TranscriptResult>(output, options);

                if (result == null)
                {
                    throw new JsonException("Result was null.");
                }

                // 4. Normalize the data (Business Logic)

                // Map "ok" to "success" to match your previous logic
                if (string.Equals(result.Status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "success";
                }

                // Ensure fields are not null
                result.Message ??= "";
                result.Transcript ??= "";

                // If deserialization worked but status is missing, flag it
                if (string.IsNullOrEmpty(result.Status))
                {
                    result.Status = "error";
                    result.Message = "Parsed JSON but found no 'status' field.";
                }

                return result;
            }
            catch (JsonException ex)
            {
                // 5. Handle Malformed JSON
                return new TranscriptResult
                {
                    Status = "error",
                    Message = $"Invalid JSON format. Error: {ex.Message}\nRaw output: {output}"
                };
            }
        }
    }
}