namespace Ncryptyr.Client.DotNet;

public class NcryptyrClient
{
    private const string DEFAULT_BASE_URL = "https://api.ncryptyr.com";
    private const string USER_AGENT = "ncryptyr-client";

    public string? BaseUrl { get; }
    protected NcryptyrHttpClient Client;

    public NcryptyrClient(string? baseUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl))
            Client = new NcryptyrHttpClient(baseUrl);
        else if (Environment.GetEnvironmentVariable("NCRYPTYR_BASE_URL") != null)
            Client = new NcryptyrHttpClient(Environment.GetEnvironmentVariable("NCRYPTYR_BASE_URL")!);
        else
            Client = new NcryptyrHttpClient(DEFAULT_BASE_URL);

        BaseUrl = Client.BaseUrl;

        if (Environment.GetEnvironmentVariable("NCRYPTYR_API_KEY") != null)
            Client.ApiKey(Environment.GetEnvironmentVariable("NCRYPTYR_API_KEY")!);
    }

    public NcryptyrClient ApiKey(string secret)
    {
        Client.ApiKey(secret);
        return this;
    }

    protected async Task ProcessFailure(HttpResponse res)
    {
        var message = res.StatusText;
        try
        {
            var content = await res.Json<Content>();
            if (!string.IsNullOrEmpty(content?.Message))
                message = content.Message;
        }
        catch (Exception) { /* ignored */ }

        if (res.Status == (int)StatusCode.BAD_REQUEST)
            throw new BadRequestException(message);

        if (res.Status == (int)StatusCode.NOT_FOUND)
            throw new NotFoundException(message);

        if (res.Status == (int)StatusCode.FORBIDDEN)
            throw new ForbiddenException(message);

        if (res.Status == (int)StatusCode.INTERNAL_ERROR)
            throw new InternalServerException(message);

        throw new Exception(message);
    }

    protected async Task<TResult> SendCommand<TCommand, TResult>(TCommand command, bool authRequired, string contentType, string? expectedContentType)
        where TResult : class
    {
        var res = await Client.UserAgent(USER_AGENT).Request("/").AuthRequired(authRequired).Post().Json(command, contentType).Send();
        if (!res.Success)
            await ProcessFailure(res);

        if (expectedContentType != null && expectedContentType != res.ContentType)
            throw new Exception($"Expected content type {expectedContentType} and received {res.ContentType}");

        if (res.Status == (int)StatusCode.NO_CONTENT)
            throw new Exception($"Expected response type {typeof(TCommand)} and received no response");
            
        return await res.Json<TResult>() ?? throw new Exception($"Expected response type {typeof(TCommand)} and received an unexpected type");
    }

    protected async Task SendCommand<TCommand>(TCommand command, bool authRequired, string contentType, string? expectedContentType = null)
    {
        var res = await Client.UserAgent(USER_AGENT).Request("/").AuthRequired(authRequired).Post().Json(command, contentType).Send();
        
        if (!res.Success)
            await ProcessFailure(res);
        
        if (expectedContentType != null && expectedContentType != res.ContentType)
            throw new Exception($"Expected content type {expectedContentType} and received {res.ContentType}");
    }

    public async Task<EnrollCommandOutput> Enroll(EnrollCommand command) =>
        await SendCommand<EnrollCommand, EnrollCommandOutput>(command, false, ContentType.ENROLL_V1, ContentType.ENROLL_V1_RESPONSE);

    public async Task<Account> DescribeAccount(DescribeAccountCommand? command = null) =>
        await SendCommand<DescribeAccountCommand, Account>(command ?? new DescribeAccountCommand(), true, ContentType.DESCRIBE_ACCOUNT_V1, ContentType.DESCRIBE_ACCOUNT_V1_RESPONSE);

    public async Task<ListAccountsCommandOutput> ListAccounts(ListAccountsCommand? command = null) =>
        await SendCommand<ListAccountsCommand, ListAccountsCommandOutput>(command ?? new ListAccountsCommand(), true, ContentType.LIST_ACCOUNTS_V1, ContentType.LIST_ACCOUNTS_V1_RESPONSE);

    public async Task<Account> UpdateAccount(UpdateAccountCommand command) =>
        await SendCommand<UpdateAccountCommand, Account>(command, true, ContentType.UPDATE_ACCOUNT_V1, ContentType.UPDATE_ACCOUNT_V1_RESPONSE);

    public async Task DeleteAccount(DeleteAccountCommand command) =>
        await SendCommand(command, true, ContentType.DELETE_ACCOUNT_V1);

    public async Task<ApiKeyWithSecret> CreateApiKey(CreateApiKeyCommand command) =>
        await SendCommand<CreateApiKeyCommand, ApiKeyWithSecret>(command, true, ContentType.CREATE_API_KEY_V1, ContentType.CREATE_API_KEY_V1_RESPONSE);

    public async Task<ApiKey[]> ListApiKeys(ListApiKeysCommand? command = null) =>
        await SendCommand<ListApiKeysCommand, ApiKey[]>(command ?? new ListApiKeysCommand(), true, ContentType.LIST_API_KEYS_V1, ContentType.LIST_API_KEYS_V1_RESPONSE);

    public async Task DeleteApiKey(DeleteApiKeyCommand command) =>
        await SendCommand(command, true, ContentType.DELETE_API_KEY_V1);

    public async Task<EncryptionKey> CreateEncryptionKey(CreateEncryptionKeyCommand command) =>
        await SendCommand<CreateEncryptionKeyCommand, EncryptionKey>(command, true, ContentType.CREATE_ENCRYPTION_KEY_V1, ContentType.CREATE_ENCRYPTION_KEY_V1_RESPONSE);

    public async Task<EncryptionKey> DescribeEncryptionKey(DescribeEncryptionKeyCommand command) =>
        await SendCommand<DescribeEncryptionKeyCommand, EncryptionKey>(command, true, ContentType.DESCRIBE_ENCRYPTION_KEY_V1, ContentType.DESCRIBE_ENCRYPTION_KEY_V1_RESPONSE);

    public async Task<EncryptionKeyExport> ExportEncryptionKey(ExportEncryptionKeyCommand command) =>
        await SendCommand<ExportEncryptionKeyCommand, EncryptionKeyExport>(command, true, ContentType.EXPORT_ENCRYPTION_KEY_V1, ContentType.EXPORT_ENCRYPTION_KEY_V1_RESPONSE);

    public async Task<EncryptionKey[]> ListEncryptionKeys(ListEncryptionKeysCommand? command = null) =>
        await SendCommand<ListEncryptionKeysCommand, EncryptionKey[]>(command ?? new ListEncryptionKeysCommand(), true, ContentType.LIST_ENCRYPTION_KEYS_V1, ContentType.LIST_ENCRYPTION_KEYS_V1_RESPONSE);

    public async Task DeleteEncryptionKey(DeleteEncryptionKeyCommand command) =>
        await SendCommand(command, true, ContentType.DELETE_ENCRYPTION_KEY_V1);

    public async Task<string> Encrypt(string encryptionKeyId, string data)
    {
        var res = await Client.UserAgent(USER_AGENT).Request("/encrypt")
            .Header("Encryption-Key", encryptionKeyId)
            .AuthRequired().Post().Text(data).Send();
        if (!res.Success)
            await ProcessFailure(res);

        return await res.Text();
    }
    
    public async Task<string> Decrypt(string ciphertext)
    {
        var res = await Client.UserAgent(USER_AGENT).Request("/decrypt")
            .AuthRequired().Post().Text(ciphertext).Send();
        if (!res.Success)
            await ProcessFailure(res);
        
        return await res.Text();
    }
    
    
}

internal record Content(string Message);