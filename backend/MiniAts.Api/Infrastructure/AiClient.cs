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

        using var request = new HttpRequestMessage(HttpMethod.Post, _assessUrl);

        // Attach internal service key if configured – prevents unauthorized access to AI service.
        if (!string.IsNullOrEmpty(_apiKey))
            request.Headers.Add("X-AI-Service-Key", _apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"AI service error: {response.StatusCode} – {error}");
        }

        var body = await response.Content.ReadAsStringAsync(ct);

        // Deserialize to AiAssessmentResult – mirrors the Python AssessResponse Pydantic model.
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<AiAssessmentResult>(body, options)
            ?? throw new InvalidOperationException("AI service returned empty or invalid response.");
    }

    public async Task<PipelineAnalyzeResponse> AnalyzePipelineAsync(
        object payload,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _analyzeUrl);

        if (!string.IsNullOrEmpty(_apiKey))
            request.Headers.Add("X-AI-Service-Key", _apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"AI service pipeline analysis error: {response.StatusCode} – {error}");
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<PipelineAnalyzeResponse>(body, options)
            ?? throw new InvalidOperationException("AI service returned empty pipeline analysis response.");
    }
}
