using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Interfaces;

public interface IAccountDataStore
{
    Account GetAccount(string accountNumber);

    void UpdateAccount(Account account);
}