using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeachFlex.Services
{
    public sealed class TeachFlexIlawDraft
    {
        public string Title { get; set; } = "";
        public string Objectives { get; set; } = "";
        public string Motivation { get; set; } = "";
        public string LessonFlow { get; set; } = "";
        public string FormativeChecks { get; set; } = "";
        public string Resources { get; set; } = "";

        public string Preview => $"LESSON TITLE\n{Title}\n\nOBJECTIVES\n{Objectives}\n\nPRE-LESSON / MOTIVATION\n{Motivation}\n\nLESSON FLOW / ACTIVITIES\n{LessonFlow}\n\nFORMATIVE CHECKS\n{FormativeChecks}\n\nRESOURCES\n{Resources}";
    }

    public sealed class TeachFlexIlawChatService
    {
        private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(130) };

        public async Task<TeachFlexIlawDraft> AskAsync(string grade, string subject, int term,
            int week, int session, string contentStandard, string performanceStandard,
            string competency, string learnerNeeds, string prompt, string previousDraft)
        {
            string endpoint = Environment.GetEnvironmentVariable("TEACHFLEX_ILAW_URL") ?? "";
            string token = Environment.GetEnvironmentVariable("TEACHFLEX_DEVICE_TOKEN") ?? "";
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("Hindi pa nakakonekta ang TeachFlex ILAW AI server. Patakbuhin muna ang pilot launcher o ipa-configure ang server sa administrator.");
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, "api/lesson/chat"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new
            {
                grade, subject, term, week, session, contentStandard,
                performanceStandard, competency, learnerNeeds, prompt, previousDraft
            });
            HttpResponseMessage response;
            try { response = await Client.SendAsync(request); }
            catch (HttpRequestException ex) { throw new InvalidOperationException("Hindi maabot ang TeachFlex AI server. Tiyaking tumatakbo ang pilot launcher at may internet.", ex); }
            catch (TaskCanceledException ex) { throw new InvalidOperationException("Nag-timeout ang TeachFlex AI server. Subukan muli.", ex); }
            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    string? message = null;
                    try
                    {
                        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        if (body.RootElement.TryGetProperty("error", out var error)) message = error.GetString();
                    }
                    catch (JsonException) { }
                    throw new InvalidOperationException(message ?? $"Hindi nagawa ang AI draft (HTTP {(int)response.StatusCode}).");
                }
                return await response.Content.ReadFromJsonAsync<TeachFlexIlawDraft>()
                    ?? throw new InvalidOperationException("Walang draft na naibalik ang TeachFlex AI server.");
            }
        }
    }
}
