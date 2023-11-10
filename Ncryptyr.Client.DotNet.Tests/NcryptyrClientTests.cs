namespace Ncryptyr.Client.DotNet.Tests;

public class NcryptyrClientTests
{
    
    [Fact]
    public async Task HappyPath()
    {
        var newAccount = await TestHelper.NewAccount();
        var client = newAccount.Client;
        var account = newAccount.Account;
        try
        {
            var describeAccount = await client.DescribeAccount();
            Assert.Equal(account, describeAccount);

            describeAccount = await client.DescribeAccount(new DescribeAccountCommand(account.Id));
            Assert.Equal(account, describeAccount);

            var listAccounts = await client.ListAccounts();
            Assert.Contains(describeAccount, listAccounts.Accounts);

            listAccounts = await client.ListAccounts(new ListAccountsCommand(account.Id));
            Assert.Contains(describeAccount, listAccounts.Accounts);

            var updateAccount = await client.UpdateAccount(new UpdateAccountCommand(
                Contact: new UpdateContact(
                    Name: "Quality Assurance Updated"
                )
            ));
            Assert.Equal("Quality Assurance Updated", updateAccount.Contact.Name);

            updateAccount = await client.UpdateAccount(new UpdateAccountCommand(
                Contact: new UpdateContact(
                    Name: "Quality Assurance",
                    Email: "qa@ncryptyr.com"
                )
            ));
            Assert.Equal("Quality Assurance", updateAccount.Contact.Name);
            Assert.Equal("qa@ncryptyr.com", updateAccount.Contact.Email);

            updateAccount = await client.UpdateAccount(new UpdateAccountCommand(
                Id: account.Id,
                Contact: new UpdateContact(
                    Name: account.Contact.Name,
                    Email: account.Contact.Email
                )
            ));
            Assert.Equal(account, updateAccount);

            var apiKeyWithSecret = await client.CreateApiKey(new CreateApiKeyCommand(
                Id: "TestKey"
            ));
            Assert.Equal("TestKey", apiKeyWithSecret.Id);
            Assert.Equal(account.Id, apiKeyWithSecret.AccountId);
            Assert.True(apiKeyWithSecret.CreatedDate > 0);
            Assert.NotEmpty(apiKeyWithSecret.Secret);

            var apiKey = new ApiKey(
                Id: apiKeyWithSecret.Id,
                AccountId: apiKeyWithSecret.AccountId,
                CreatedDate: apiKeyWithSecret.CreatedDate
            );

            var listApiKeys = await client.ListApiKeys();
            Assert.Equal(2, listApiKeys.Length);
            Assert.Contains(apiKey, listApiKeys);

            listApiKeys = await client.ListApiKeys(new ListApiKeysCommand(IdBeginsWith: apiKey.Id));
            Assert.Single(listApiKeys);
            Assert.Contains(apiKey, listApiKeys);

            await client.DeleteApiKey(new DeleteApiKeyCommand(Id: apiKey.Id));
            listApiKeys = await client.ListApiKeys();
            Assert.Single(listApiKeys);

            var encryptionKey1 = await client.CreateEncryptionKey(new CreateEncryptionKeyCommand(Id: "TestKey1"));
            Assert.Equal("TestKey1", encryptionKey1.Id);
            Assert.Equal(account.Id, encryptionKey1.AccountId);
            Assert.True(encryptionKey1.CreatedDate > 0);
            Assert.Equal(EncryptionKeyType.AES_128, encryptionKey1.Type);

            var describeEncryptionKey =
                await client.DescribeEncryptionKey(new DescribeEncryptionKeyCommand(Id: "TestKey1"));
            Assert.Equal(encryptionKey1, describeEncryptionKey);

            var encryptionKey2 = await client.CreateEncryptionKey(new CreateEncryptionKeyCommand(Id: "TestKey2"));
            Assert.Equal("TestKey2", encryptionKey2.Id);
            Assert.Equal(account.Id, encryptionKey2.AccountId);
            Assert.True(encryptionKey2.CreatedDate > 0);
            Assert.Equal(EncryptionKeyType.AES_128, encryptionKey2.Type);

            var encryptionKeys = await client.ListEncryptionKeys();
            Assert.Equal(2, encryptionKeys.Length);
            Assert.Contains(encryptionKey1, encryptionKeys);
            Assert.Contains(encryptionKey2, encryptionKeys);

            await client.DeleteEncryptionKey(new DeleteEncryptionKeyCommand(Id: encryptionKey2.Id));
            encryptionKeys = await client.ListEncryptionKeys();
            Assert.Single(encryptionKeys);
            Assert.Contains(encryptionKey1, encryptionKeys);

            var ciphertext = await client.Encrypt("TestKey1", "some awesome text");
            var text = await client.Decrypt(ciphertext);
            Assert.Equal("some awesome text", text);

        }
        finally
        {
            await client.DeleteAccount(new DeleteAccountCommand(Id: account.Id));
        }
    }
}