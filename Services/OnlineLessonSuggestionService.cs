using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeachFlex.Services
{
    public sealed class SuggestedDailyLesson
    {
        public string Title { get; set; } = "";
        public string Objectives { get; set; } = "";
        public string Resources { get; set; } = "";
        public string PreLesson { get; set; } = "";
        public string Activities { get; set; } = "";
        public string Assessment { get; set; } = "";
    }

    // The desktop app never receives the AI provider's API key.
    public sealed class OnlineLessonSuggestionService
    {
        private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(120) };

        public async Task<SuggestedDailyLesson> GenerateAsync(string grade, string subject,
            int term, int week, int day, string contentStandard, string competency,
            string learnerNeeds, bool simplify)
        {
            string endpoint = Environment.GetEnvironmentVariable("TEACHFLEX_ILAW_URL") ?? "";
            string deviceToken = Environment.GetEnvironmentVariable("TEACHFLEX_DEVICE_TOKEN") ?? "";
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(deviceToken))
                throw new InvalidOperationException(
                    "Hindi pa nakakonekta ang online ILAW service. Ipa-configure sa administrator ang TeachFlex server at device access.");
            using var request = new HttpRequestMessage(HttpMethod.Post,
                new Uri(uri, "api/lesson/daily"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deviceToken);
            request.Content = JsonContent.Create(new
            {
                grade, subject, term, week, day, contentStandard, competency,
                learnerNeeds, simplify
            });
            using var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string? serviceMessage = null;
                try
                {
                    using var errorBody = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (errorBody.RootElement.TryGetProperty("error", out var error) &&
                        error.ValueKind == JsonValueKind.String)
                        serviceMessage = error.GetString();
                }
                catch (JsonException) { }
                throw new InvalidOperationException(serviceMessage ?? (response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "Walang access ang device sa online ILAW service.",
                    System.Net.HttpStatusCode.TooManyRequests => "Naabot ang generation limit. Subukan muli mamaya.",
                    _ => $"Hindi nagawa ang online lesson ({(int)response.StatusCode})."
                }));
            }
            return await response.Content.ReadFromJsonAsync<SuggestedDailyLesson>()
                ?? throw new InvalidOperationException("Walang lesson draft na naibalik ang server.");
        }
    }
}
