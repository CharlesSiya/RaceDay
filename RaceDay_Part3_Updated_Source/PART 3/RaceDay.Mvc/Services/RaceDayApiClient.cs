using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

/// <summary>All MVC data access is performed through the Part 2 API; this client never accesses the database.</summary>
public sealed class RaceDayApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private HttpRequestMessage Create(HttpMethod method, string path, HttpContext? context)
    {
        var request = new HttpRequestMessage(method, path);
        var cookie = context?.Session.GetString("ApiAuthCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
        return request;
    }

    private static void SaveAuthCookie(HttpResponseMessage response, HttpContext? context)
    {
        if (context is null || !response.Headers.TryGetValues("Set-Cookie", out var cookies)) return;
        var auth = cookies.FirstOrDefault(x => x.StartsWith(".RaceDay.Auth=", StringComparison.Ordinal));
        if (auth is not null) context.Session.SetString("ApiAuthCookie", auth.Split(';', 2)[0]);
    }

    public async Task<(HttpResponseMessage Response, string Body)> SendAsync(HttpMethod method, string path, HttpContext? context, object? body = null)
    {
        using var request = Create(method, path, context);
        if (body is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        var response = await http.SendAsync(request);
        SaveAuthCookie(response, context);
        return (response, await response.Content.ReadAsStringAsync());
    }

    public async Task<(HttpResponseMessage Response, string Body)> SendFileAsync(string path, HttpContext context, IFormFile file)
    {
        using var request = Create(HttpMethod.Post, path, context);
        using var multipart = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        multipart.Add(content, "file", Path.GetFileName(file.FileName));
        request.Content = multipart;
        var response = await http.SendAsync(request);
        SaveAuthCookie(response, context);
        return (response, await response.Content.ReadAsStringAsync());
    }

    public async Task<JsonDocument?> GetJsonAsync(string path, HttpContext context)
    {
        var (response, body) = await SendAsync(HttpMethod.Get, path, context);
        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body)) return null;
        return JsonDocument.Parse(body);
    }
}
