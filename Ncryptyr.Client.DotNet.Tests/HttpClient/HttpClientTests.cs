namespace Ncryptyr.Client.DotNet.Tests.HttpClient;

public class HttpClientTests
{
    [Fact]
    public async Task Test_HttpGetRequest()
    {
        var res = await new NcryptyrHttpClient("https://www.google.com").Request("/").Get().Send();
        Assert.Equal((int)StatusCode.OK, res.Status);
        Assert.True(res.Success);
        var text = await res.Text();
        Assert.True(text.Length > 0);
    }
}