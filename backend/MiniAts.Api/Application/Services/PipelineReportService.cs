// PipelineReportService aggregates recruitment data and coordinates with the Python AI service.
// Related: Application/Interfaces/Interfaces.cs (IPipelineReportService, IAiClient, ICandidateRepository, IJobRepository)
//          Application/Dtos/Dtos.cs (PipelineReportDto, PipelineReportRequest)
//          Infrastructure/AiClient.cs (POST /analyze)
//          backend/ai-service/main.py (PipelineAnalyzeRequest/Response)
//          frontend/src/components/ui/AiPipelineIntelligence.vue

using MiniAts.Application.Dtos;
using MiniAts.Application.Interfaces;
using MiniAts.Domain;

namespace MiniAts.Application.Services;

public class PipelineReportService : IPipelineReportService
{
    private readonly ICandidateRepository _candidates;
    private readonly IJobRepository _jobs;
    private readonly IProfileRepository _profiles;
    private readonly IAiClient _aiClient;

    public PipelineReportService(
        ICandidateRepository candidates,
        IJobRepository jobs,
        IProfileRepository profiles,
        IAiClient aiClient)
    {
        _candidates = candidates;
        _jobs       = jobs;
        _profiles   = profiles;
        _aiClient   = aiClient;
    }

    private static Guid ResolveCustomerId(Guid callerId, string callerRole, Guid customerId)
        => callerRole == "admin" ? customerId : callerId;

    public async Task<PipelineReportDto> GenerateReportAsync(
        Guid callerId,
        string callerRole,
        Guid customerId,
        CancellationToken ct = default)
    {
        var effectiveId = ResolveCustomerId(callerId, callerRole, customerId);

        // Fetch scoped company profile for narrative header
        var profile = await _profiles.GetByIdAsync(effectiveId, ct);
        var companyName = profile?.CompanyName ?? "Nordic Tech AB";

        // Fetch scoped jobs and candidates
        var jobs = await _jobs.ListByCustomerAsync(effectiveId, ct);
        var candidates = await _candidates.ListAsync(effectiveId, null, null, ct);

        var totalCandidates = candidates.Count;
        var totalJobs = jobs.Count;

        // Stage counts
        var stages = new[] { "new", "screening", "interview", "offer", "hired", "rejected" };
        var stageCounts = stages.ToDictionary(
            s => s,
            s => candidates.Count(c => string.Equals(c.Stage.ToString(), s, StringComparison.OrdinalIgnoreCase)));

        var hiredCount = stageCounts["hired"];
        var hireRate = totalCandidates > 0 ? Math.Round((double)hiredCount * 100.0 / totalCandidates, 1) : 0.0;

        // Funnel metrics & conversions
        var funnel = new List<FunnelStageMetrics>
        {
            new("new", stageCounts["new"], 100.0),
            new("screening", stageCounts["screening"], stageCounts["new"] > 0 ? Math.Round((double)stageCounts["screening"] * 100.0 / stageCounts["new"], 1) : 100.0),
            new("interview", stageCounts["interview"], stageCounts["screening"] > 0 ? Math.Round((double)stageCounts["interview"] * 100.0 / stageCounts["screening"], 1) : 100.0),
            new("offer", stageCounts["offer"], stageCounts["interview"] > 0 ? Math.Round((double)stageCounts["offer"] * 100.0 / stageCounts["interview"], 1) : 100.0),
            new("hired", stageCounts["hired"], stageCounts["offer"] > 0 ? Math.Round((double)stageCounts["hired"] * 100.0 / stageCounts["offer"], 1) : 100.0),
            new("rejected", stageCounts["rejected"], totalCandidates > 0 ? Math.Round((double)stageCounts["rejected"] * 100.0 / totalCandidates, 1) : 0.0)
        };

        var conversionRatesDict = funnel.ToDictionary(f => f.Stage, f => f.ConversionPct);

        // AI score statistics
        var scoredCandidates = candidates.Where(c => c.AiScore.HasValue).ToList();
        var avgAiScore = scoredCandidates.Count > 0
            ? Math.Round((double)scoredCandidates.Average(c => c.AiScore!.Value), 1)
            : 0.0;

        // Score distribution buckets
        var buckets = new (string Label, int Min, int Max)[]
        {
            ("0-19", 0, 19),
            ("20-39", 20, 39),
            ("40-59", 40, 59),
            ("60-79", 60, 79),
            ("80-100", 80, 100)
        };

        var scoreDistribution = buckets.Select(b =>
        {
            var count = scoredCandidates.Count(c => (int)c.AiScore!.Value >= b.Min && (int)c.AiScore!.Value <= b.Max);
            var pct = scoredCandidates.Count > 0 ? Math.Round((double)count * 100.0 / scoredCandidates.Count, 1) : 0.0;
            return new ScoreDistributionBucket(b.Label, count, pct);
        }).ToList();

        var scoreDistributionDict = scoreDistribution.ToDictionary(b => b.Bucket, b => b.Count);

        // Job metrics
        var jobMap = jobs.ToDictionary(j => j.Id, j => j.Title);
        var jobMetrics = jobs.Select(j =>
        {
            var jc = candidates.Where(c => c.JobId == j.Id).ToList();
            var jScored = jc.Where(c => c.AiScore.HasValue).ToList();
            var jAvg = jScored.Count > 0 ? Math.Round((double)jScored.Average(c => c.AiScore!.Value), 1) : 0.0;
            return new JobMetricItem(j.Id, j.Title, jc.Count, jAvg);
        }).ToList();

        // Stale candidates (> 7 days without stage update)
        var staleCandidates = candidates
            .Select(c =>
            {
                var days = (int)Math.Max(0, (DateTime.UtcNow - c.UpdatedAt).TotalDays);
                return new { Candidate = c, Days = days };
            })
            .Where(x => x.Days >= 7)
            .OrderByDescending(x => x.Days)
            .Select(x => new StaleCandidateItem(
                x.Candidate.Id,
                x.Candidate.FullName,
                x.Candidate.Stage.ToString().ToLowerInvariant(),
                x.Days))
            .ToList();

        // Data quality coverage
        var denom = Math.Max(1, totalCandidates);
        var coverage = new DataQualityCoverage(
            LinkedinPct: Math.Round(candidates.Count(c => !string.IsNullOrWhiteSpace(c.LinkedinUrl)) * 100.0 / denom, 1),
            CvTextPct: Math.Round(candidates.Count(c => !string.IsNullOrWhiteSpace(c.CvText)) * 100.0 / denom, 1),
            ShortCvPct: Math.Round(candidates.Count(c => string.IsNullOrWhiteSpace(c.CvText) || c.CvText.Length < 500) * 100.0 / denom, 1),
            MissingEmailPct: Math.Round(candidates.Count(c => string.IsNullOrWhiteSpace(c.Email)) * 100.0 / denom, 1)
        );

        // Outliers and risk detection
        var outliers = new List<string>();

        // High AI score but rejected
        foreach (var rejected in candidates.Where(c => c.Stage == CandidateStage.Rejected && c.AiScore >= 75))
        {
            outliers.Add($"High-match candidate '{rejected.FullName}' ({rejected.AiScore:0}%) is currently marked Rejected.");
        }

        // High score candidate still in early stage
        foreach (var early in candidates.Where(c => (c.Stage == CandidateStage.New || c.Stage == CandidateStage.Screening) && c.AiScore >= 80))
        {
            outliers.Add($"Top candidate '{early.FullName}' ({early.AiScore:0}%) is awaiting advancement from {early.Stage}.");
        }

        // Jobs with zero candidates
        foreach (var emptyJob in jobs.Where(j => !candidates.Any(c => c.JobId == j.Id)))
        {
            outliers.Add($"Active requisition '{emptyJob.Title}' has 0 active applicants.");
        }

        // Top 3 and Bottom 3 candidates
        var topCandidates = scoredCandidates
            .OrderByDescending(c => c.AiScore!.Value)
            .Take(3)
            .Select(c => new CandidateRankItem(
                c.Id,
                c.FullName,
                jobMap.GetValueOrDefault(c.JobId ?? Guid.Empty, "General"),
                c.Stage.ToString().ToLowerInvariant(),
                (int)c.AiScore!.Value))
            .ToList();

        var bottomCandidates = scoredCandidates
            .OrderBy(c => c.AiScore!.Value)
            .Take(3)
            .Select(c => new CandidateRankItem(
                c.Id,
                c.FullName,
                jobMap.GetValueOrDefault(c.JobId ?? Guid.Empty, "General"),
                c.Stage.ToString().ToLowerInvariant(),
                (int)c.AiScore!.Value))
            .ToList();

        // Build payload for Python AI service /analyze endpoint
        var aiPayload = new
        {
            customer_id = effectiveId.ToString(),
            company_name = companyName,
            aggregates = new
            {
                total_candidates = totalCandidates,
                total_jobs = totalJobs,
                stage_counts = stageCounts,
                conversion_rates = conversionRatesDict,
                hire_rate = hireRate,
                avg_ai_score = avgAiScore,
                score_distribution = scoreDistributionDict,
                stale_count = staleCandidates.Count,
                stale_candidate_names = staleCandidates.Select(s => s.Name).ToList(),
                coverage = new Dictionary<string, double>
                {
                    ["linkedin_pct"] = coverage.LinkedinPct,
                    ["cv_pct"] = coverage.CvTextPct,
                    ["short_cv_pct"] = coverage.ShortCvPct,
                    ["email_pct"] = 100.0 - coverage.MissingEmailPct
                },
                outliers = outliers,
                top_candidate_names = topCandidates.Select(t => t.Name).ToList(),
                bottom_candidate_names = bottomCandidates.Select(b => b.Name).ToList()
            },
            jobs = jobs.Select(j => new
            {
                id = j.Id.ToString(),
                title = j.Title,
                candidate_count = candidates.Count(c => c.JobId == j.Id),
                avg_ai_score = jobMetrics.FirstOrDefault(m => m.Id == j.Id)?.AvgAiScore ?? 0.0
            }).ToList(),
            candidates = candidates.Select(c => new
            {
                name = c.FullName,
                stage = c.Stage.ToString().ToLowerInvariant(),
                ai_score = c.AiScore.HasValue ? (int?)((int)c.AiScore.Value) : null,
                has_linkedin = !string.IsNullOrWhiteSpace(c.LinkedinUrl),
                has_cv = !string.IsNullOrWhiteSpace(c.CvText),
                cv_length = c.CvText?.Length ?? 0,
                days_in_stage = (int)Math.Max(0, (DateTime.UtcNow - c.UpdatedAt).TotalDays)
            }).ToList()
        };

        // Call Python service (with automatic fallback to mock inside Python service or handled here)
        PipelineAnalyzeResponse aiResult;
        try
        {
            aiResult = await _aiClient.AnalyzePipelineAsync(aiPayload, ct);
        }
        catch (Exception)
        {
            // Resilient fallback in case AI service is temporarily offline
            aiResult = new PipelineAnalyzeResponse(
                Headline: $"Pipeline intelligence computed for {companyName}: {totalCandidates} candidates across {totalJobs} roles.",
                Score: (int)Math.Max(30, Math.Min(95, 70 + (hireRate > 10 ? 10 : 0) - (staleCandidates.Count * 2))),
                Rating: hireRate >= 10 ? "healthy" : (staleCandidates.Count > 3 ? "risk" : "watch"),
                Summary: $"Analyzed {totalCandidates} candidates for {companyName}. Average qualification index is {avgAiScore:0.0}%, with {coverage.LinkedinPct:0.0}% verified LinkedIn coverage.",
                Strengths: new List<string>
                {
                    $"{coverage.LinkedinPct:0.0}% of applicants include verified LinkedIn profiles.",
                    $"Average role qualification alignment score is {avgAiScore:0.0}%."
                },
                Risks: staleCandidates.Count > 0
                    ? new List<string> { $"{staleCandidates.Count} candidate(s) are stagnant (>7 days without stage update)." }
                    : new List<string> { "Monitor conversion rates as pipeline scales." },
                Recommendations: new List<string>
                {
                    "Advance top-scoring technical candidates to interview rounds.",
                    "Review stagnant candidate profiles to maintain candidate momentum."
                },
                StageInsights: stages.Select(s => new StageInsightDto(s, $"{stageCounts[s]} candidate(s) currently in {s} stage.")).ToList(),
                Provider: "mock"
            );
        }

        return new PipelineReportDto(
            Headline: aiResult.Headline,
            Score: aiResult.Score,
            Rating: aiResult.Rating,
            Summary: aiResult.Summary,
            Strengths: aiResult.Strengths,
            Risks: aiResult.Risks,
            Recommendations: aiResult.Recommendations,
            StageInsights: aiResult.StageInsights,
            Provider: aiResult.Provider,
            TotalCandidates: totalCandidates,
            TotalJobs: totalJobs,
            OverallHireRate: hireRate,
            AvgAiScore: avgAiScore,
            Funnel: funnel,
            ScoreDistribution: scoreDistribution,
            Coverage: coverage,
            Jobs: jobMetrics,
            StaleCandidates: staleCandidates,
            Outliers: outliers,
            TopCandidates: topCandidates,
            BottomCandidates: bottomCandidates,
            GeneratedAt: DateTime.UtcNow
        );
    }
}
