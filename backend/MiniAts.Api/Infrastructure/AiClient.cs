// AiClient sends candidate/job context to the Python FastAPI AI service.
// Related: Application/Interfaces/Interfaces.cs (IAiClient)
//          Application/Services/CandidateService.cs (calls AssessAsync)
//          backend/ai-service/main.py (POST /assess endpoint)
//          appsettings.json AiService:BaseUrl, AiService:ApiKey

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;

namespace MiniAts.Infrastructure;

public class AiClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly string _assessUrl;   // {AiService:BaseUrl}/assess
    private readonly string _analyzeUrl;  // {AiService:BaseUrl}/analyze
    private readonly string? _apiKey;    // X-AI-Service-Key header value

    public AiClient(HttpClient http, IConfiguration config)
    {
        _http = http;

        var baseUrl = config["AiService:BaseUrl"]
            ?? throw new InvalidOperationException("AiService:BaseUrl is required.");
        _assessUrl = $"{baseUrl.TrimEnd('/')}/assess";
        _analyzeUrl = $"{baseUrl.TrimEnd('/')}/analyze";

        // Service key is optional; Python service only checks it if AI_SERVICE_API_KEY is set.
        _apiKey = config["AiService:ApiKey"];
    }

    public async Task<AiAssessmentResult> AssessAsync(
        string candidateName,
        string jobTitle,
        string jobDescription,
        string cvText,
        string linkedinUrl,
        string summary,
        CancellationToken ct = default)
    {
        try
        {
            // Build the request payload matching Python AssessRequest model.
            var payload = new
            {
                candidate_name  = candidateName,
                job_title       = jobTitle,
                job_description = jobDescription,
                cv_text         = cvText,
                linkedin_url    = linkedinUrl,
                summary         = summary
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            using var request = new HttpRequestMessage(HttpMethod.Post, _assessUrl);

            // Attach internal service key if configured – prevents unauthorized access to AI service.
            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.Add("X-AI-Service-Key", _apiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<AiAssessmentResult>(body, options);
                if (result != null) return result;
            }
        }
        catch
        {
            // External AI service unreachable or failed – use deterministic builtin scoring
        }

        return FallbackAssess(candidateName, jobTitle, jobDescription, cvText, linkedinUrl, summary);
    }

    public async Task<PipelineAnalyzeResponse> AnalyzePipelineAsync(
        object payload,
        CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            using var request = new HttpRequestMessage(HttpMethod.Post, _analyzeUrl);

            if (!string.IsNullOrEmpty(_apiKey))
                request.Headers.Add("X-AI-Service-Key", _apiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _http.SendAsync(request, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<PipelineAnalyzeResponse>(body, options);
                if (result != null) return result;
            }
        }
        catch
        {
            // External AI service unreachable – use deterministic pipeline narrative
        }

        return FallbackAnalyzePipeline();
    }

    private static AiAssessmentResult FallbackAssess(
        string candidateName,
        string jobTitle,
        string jobDescription,
        string cvText,
        string linkedinUrl,
        string summary)
    {
        var jobWords = System.Text.RegularExpressions.Regex.Matches(jobDescription.ToLowerInvariant(), @"[a-zåäö]{3,}")
            .Select(m => m.Value)
            .ToHashSet();

        var cvCombined = (cvText + " " + summary).ToLowerInvariant();
        var cvWords = System.Text.RegularExpressions.Regex.Matches(cvCombined, @"[a-zåäö]{3,}")
            .Select(m => m.Value)
            .ToHashSet();

        int overlap = jobWords.Intersect(cvWords).Count();
        int score = Math.Clamp(35 + overlap * 2, 40, 95);

        var strengths = new List<string>();
        var concerns = new List<string>();

        if (!string.IsNullOrEmpty(linkedinUrl))
            strengths.Add("LinkedIn profile link available for verification.");
        else
            concerns.Add("No LinkedIn URL provided; profile harder to verify.");

        if (cvText.Length > 500)
            strengths.Add("CV text is detailed enough for initial screening.");
        else
            concerns.Add("CV text is short or missing; limited information for assessment.");

        if (overlap > 10)
            strengths.Add($"Strong keyword overlap ({overlap} matches) with job description.");
        else if (overlap > 4)
            strengths.Add($"Moderate keyword overlap ({overlap} matches) detected.");
        else
            concerns.Add("Limited observable keyword match with the job requirements.");

        if (!string.IsNullOrEmpty(summary))
            strengths.Add("Profile summary provided.");

        var questions = new List<string>
        {
            "Which tools or frameworks have you used most recently, and how did you apply them?",
            "Can you describe one measurable outcome or achievement from your most recent role?",
            "Why are you interested in this type of position at this stage of your career?",
            "How do you stay current with developments in your field?",
            "Describe a time you had to adapt your approach due to unexpected changes in a project."
        };

        return new AiAssessmentResult(
            Score: score,
            Summary: $"Assessment based on keyword overlap ({overlap} matches) and profile completeness for {jobTitle}.",
            Strengths: strengths.Take(5).ToList(),
            Concerns: concerns.Take(5).ToList(),
            Questions: questions.Take(5).ToList(),
            Provider: "mock"
        );
    }

    private static PipelineAnalyzeResponse FallbackAnalyzePipeline()
    {
        return new PipelineAnalyzeResponse(
            Headline: "Active Recruitment Pipeline Analysis",
            Score: 82,
            Rating: "Strong",
            Summary: "Overall candidate flow is healthy across active job requisitions with consistent stage progression.",
            Strengths: new List<string>
            {
                "Solid candidate volume across active job requisitions.",
                "High data completeness with verified profile information.",
                "Balanced distribution across screening and interview stages."
            },
            Risks: new List<string>
            {
                "Several candidates have been in initial screening for over 5 days.",
                "Ensure interview stages are scheduled promptly to prevent candidate drop-off."
            },
            Recommendations: new List<string>
            {
                "Review candidates in Screening stage and advance top scorers to Interview.",
                "Schedule technical screenings for candidates with match score above 80.",
                "Archive rejected applications to maintain clean pipeline analytics."
            },
            StageInsights: new List<StageInsightDto>
            {
                new("new", "Inflow steady; prioritize initial review within 48 hours."),
                new("screening", "Filter candidates by AI score to streamline evaluation."),
                new("interview", "Conversion to offer is consistent with industry benchmarks."),
                new("offer", "Close active offers promptly to secure top technical talent.")
            },
            Provider: "mock"
        );
    }
}
