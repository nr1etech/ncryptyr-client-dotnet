using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Ncryptyr.Client.DotNet;

public class Headers : Dictionary<string, string>
{
    public Headers() {}
    public Headers(IDictionary<string, string> headers) : base(headers) {}
}

public class Parameters : Dictionary<string, string>
{
    public Parameters() {}
    public Parameters(IDictionary<string, string> headers) : base(headers) {}
}

public class NcryptyrHttpClient
{
    // https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use
    private static readonly HttpClient Client = new(); // must be singleton to prevent leaks

    public string BaseUrl { get; }
    public Headers CommonHeaders { get; }
    public Headers AuthHeaders { get; }

    public NcryptyrHttpClient(string baseUrl)
    {
        BaseUrl = baseUrl;
        CommonHeaders = new Headers();
        AuthHeaders = new Headers();
    }

    public NcryptyrHttpClient UserAgent(string userAgent)
    {
        CommonHeaders.Add("User-Agent", userAgent);
        return this;
    }

    public NcryptyrHttpClient ApiKey(string secret)
    {
        if (AuthHeaders.Keys.Count > 0)
            throw new InvalidOperationException(" Authentication method is already set");

        CommonHeaders.Add("Api-Key", secret);
        return this;
    }

    public NcryptyrHttpClient AccessToken(string accessToken)
    {
        if (AuthHeaders.Keys.Count > 0)
            throw new InvalidOperationException(" Authentication method is already set");

        CommonHeaders.Add("Authorization", $"Bearer {accessToken}");
        return this;
    }

    public HttpRequest Request(string path) => new(this, path);

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message) => Client.SendAsync(message);
}

public class HttpRequest
{
    public NcryptyrHttpClient Client { get; }
    public string Path { get; set; }
    public Headers Headers { get; set; }

    public HttpRequest(NcryptyrHttpClient client, string path)
    {
        Client = client;
        Path = path;
        Headers = new Headers(client.CommonHeaders);
    }

    public HttpRequest ExpandPath(string paramName, string paramValue)
    {
        Path = Path.Replace($"{{{paramName}}}", paramValue);
        return this;
    }

    public HttpRequest AuthRequired(bool? authRequired = null)
    {
        if (!authRequired.HasValue || authRequired.Value)
            foreach (var (key, value) in Client.AuthHeaders)
                Headers.Add(key, value);

        return this;
    }

    public HttpRequest Header(string name, string value)
    {
        Headers.Add(name, value);
        return this;
    }

    public HttpGetRequest Get() => new(this);

    public HttpPostRequest Post() => new(this);
}

public class HttpGetRequest
{
    public HttpRequest Request { get; }
    public Parameters Parameters { get; set; }

    public HttpGetRequest(HttpRequest request)
    {
        Request = request;
        Parameters = new Parameters();
    }

    public HttpGetRequest Parameter(string? name, string? value)
    {
        if (value != null && name != null)
            Parameters.Add(name, value);
        
        return this;
    }

    public async Task<HttpResponse> Send()
    {
        var url = Request.Client.BaseUrl + Request.Path;

        var uriBuilder = new UriBuilder(url);
        if (Parameters.Count > 0)
        {
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var (key, value) in Parameters)
                query[key] = value.Trim();
            uriBuilder.Query = query.ToString();
        }

        var message = new HttpRequestMessage
        {
            RequestUri = uriBuilder.Uri,
            Method = HttpMethod.Get
        };
        
        foreach(var (name, value) in Request.Headers)
            message.Headers.Add(name, value);

        var res = await Request.Client.SendAsync(message);
        return new HttpResponse(res);
    }
}

public class HttpPostRequest
{
    public HttpRequest Request { get; }
    public Headers Headers { get; set; }
    public Parameters Parameters { get; set; }
    public string Body { get; set; } = string.Empty;

    public HttpPostRequest(HttpRequest request)
    {
        Request = request;
        Headers = new Headers(request.Headers);
    }

    public HttpPostRequest Json<T>(T body, string? contentType) => 
        Json(JsonSerializer.Serialize(body), contentType);

    public HttpPostRequest Json(string body, string? contentType)
    {
        if (Parameters != null)
            throw new Exception("Parameters already set");

        Body = body;
        Headers.Add("Content-Type", contentType ?? "application/json");
        return this;
    }

    public HttpPostRequest Text<T>(T body, string? contentType) => 
        Text(JsonSerializer.Serialize(body), contentType);

    public HttpPostRequest Text(string body, string? contentType = null)
    {
        Body = body;
        Headers["Content-Type"] = contentType ?? "text/plain";
        return this;
    }
    
    public async Task<HttpResponse> Send()
    {
        var url = Request.Client.BaseUrl + Request.Path;
        var body = string.Empty;
        if (Parameters?.Count > 0)
            body = JsonSerializer.Serialize(Parameters);
        else if (Body != string.Empty)
            body = Body;

        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Post
        };

        if (body != string.Empty)
        {
            var contentType = Headers.ContainsKey("Content-Type") ? Headers["Content-Type"] : "application/json";
            message.Content = new StringContent(body, Encoding.UTF8, contentType);
        }
        
        foreach(var (name, value) in Headers.Where(header => header.Key != "Content-Type"))
            message.Headers.Add(name, value);

        var res = await Request.Client.SendAsync(message);
        return new HttpResponse(res);
    }
}

public class HttpResponse
{
    private readonly HttpResponseMessage response;

    public HttpResponse(HttpResponseMessage response) => 
        this.response = response;

    public bool Success => 
        response.StatusCode == HttpStatusCode.OK
           || response.StatusCode == HttpStatusCode.Created
           || response.StatusCode == HttpStatusCode.NoContent;

    public string ContentType => 
        response.Headers.GetValues("Content-Type").FirstOrDefault()
        ?? response.Headers.GetValues("content-type").First();

    public int Status => 
        (int)response.StatusCode;

    public string StatusText => 
        response.ReasonPhrase ?? string.Empty;

    public Task<T?> Json<T>() => 
        response.Content.ReadFromJsonAsync<T>();

    public Task<string> Text() => 
        response.Content.ReadAsStringAsync();
}