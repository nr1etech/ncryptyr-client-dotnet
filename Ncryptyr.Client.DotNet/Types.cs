namespace Ncryptyr.Client.DotNet;

public record Contact(
  string Name,
  string Email
);

public record Account(
  string Id,
  Contact Contact,
  long CreatedDate
);

public record ApiKey(
  string AccountId,
  string Id,
  long CreatedDate
);

public record ApiKeyWithSecret(
  string AccountId,
  string Id,
  long CreatedDate,
  string Secret
) : ApiKey(
  AccountId,
  Id,
  CreatedDate
);

public class EncryptionKeyType
{
  public static string AES_128 => "AES";
}

public record EncryptionKey(
  string AccountId,
  string Id,
  string Type,
  long CreatedDate
);

public record EncryptionKeyExport(
  string AccountId,
  string Id,
  string Type,
  long CreatedDate,
  int Version,
  string Key,
  string Iv
);

public record UpdateContact(
  string? Name = null,
  string? Email = null
);

public record EnrollCommand(
  string Id,
  Contact Contact
);

public record EnrollCommandOutput(
  Account Account,
  ApiKeyWithSecret ApiKey
);

public record DescribeAccountCommand(
  string? Id = null
);

public record ListAccountsCommand(
  string? IdBeginsWith = null
);

public record ListAccountsCommandOutput(
  List<Account> Accounts
);

public record UpdateAccountCommand(
  string? Id = null,
  UpdateContact? Contact = null
);

public record DeleteAccountCommand(
  string Id
);

public record CreateApiKeyCommand(
  string Id
);

public record ListApiKeysCommand(
  string? IdBeginsWith = null
);

public record DeleteApiKeyCommand(
  string Id
);

public record CreateEncryptionKeyCommand(
  string Id
);

public record DescribeEncryptionKeyCommand(
  string Id
);

public record ExportEncryptionKeyCommand(
  string Id
);

public record ListEncryptionKeysCommand(
  string? IdBeginsWith = null
);

public record DeleteEncryptionKeyCommand(
  string Id
);