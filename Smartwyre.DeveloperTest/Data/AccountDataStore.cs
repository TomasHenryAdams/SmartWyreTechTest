using Microsoft.Extensions.Logging;
using Smartwyre.DeveloperTest.Interfaces;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data;

public class AccountDataStore : IAccountDataStore
{
    private readonly ILogger<AccountDataStore> _logger;

    public AccountDataStore(ILogger<AccountDataStore> logger)
    {
        _logger = logger;
    }

    public Account GetAccount(string accountNumber)
    {
        _logger.LogInformation($"Retrieving account information for account number : {accountNumber}");
        
        if(accountNumber == "1")
            return new()
            {
                AccountNumber = "1",
                Balance = 200.00M,
                Status = AccountStatus.Live,
                AllowedPaymentSchemes = AllowedPaymentSchemes.BankToBankTransfer
            };
        
        return null;
    }

    public void UpdateAccount(Account account)
    {
        _logger.LogInformation($"Updating account : {account.AccountNumber}");
        // Update account in database, code removed for brevity
    }
}