namespace Ncryptyr.Client.DotNet.Tests;

public record NewAccountOutput(
    NcryptyrClient Client,
    Account Account,
    ApiKey ApiKey
);

public static class TestHelper
{
    public static NcryptyrClient Client => 
        Environment.GetEnvironmentVariable("NCRYPTYR_BASE_URL") != null 
            ? new NcryptyrClient() 
            : new NcryptyrClient("https://api-stage.ncryptyr.com");

    public static string AccountId => $"NcryptyrClientTest{DateTime.UtcNow.Ticks}";

    public static Contact Contact => new("Quality Assurance", "ncryptyr-client-test@ncryptyr.com");

    public static async Task<NewAccountOutput> NewAccount()
    {
        var client = Client;
        var accountId = AccountId;
        var contact = Contact;
        var (account, apiKeyWithSecret) = await client.Enroll(new EnrollCommand(
            Id: accountId, 
            Contact: contact
        ));
        
        Assert.Equal(contact, account.Contact);
        Assert.NotEmpty(apiKeyWithSecret.Secret);
        Assert.True(apiKeyWithSecret.CreatedDate > 0);
        Assert.Equal("master", apiKeyWithSecret.Id);
        Assert.Equal(accountId, apiKeyWithSecret.AccountId);
        client.ApiKey(apiKeyWithSecret.Secret);
        return new NewAccountOutput(
            Client: client,
            Account: account,
            ApiKey: apiKeyWithSecret
        );
    }
}