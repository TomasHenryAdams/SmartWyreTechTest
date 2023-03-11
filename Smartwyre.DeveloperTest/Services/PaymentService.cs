using Microsoft.Extensions.Logging;
using Smartwyre.DeveloperTest.Interfaces;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;
public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IAccountDataStore _accountData;

    public PaymentService(ILogger<PaymentService> logger, IAccountDataStore accountData)
    {
        _logger = logger;
        _accountData = accountData;
    }

    public MakePaymentResult MakePayment(MakePaymentRequest request)
    {
        var account = _accountData.GetAccount(request.DebtorAccountNumber);

        MakePaymentResult result = new();

        if (account == null)
        {
            _logger.LogInformation($"No account found for account number : {request.DebtorAccountNumber}");

            return result;
        }

        if (!PaymentTypeEnabled(account, request))
        {
            _logger.LogInformation($"Payment scheme : {request.PaymentScheme} - is not enabled for account : {account.AccountNumber}");

            return result;
        }

        if (account.Balance < request.Amount)
        {
            _logger.LogInformation($"Account : {account.AccountNumber} - does not have the required balance to complete the transaction");

            return result;
        }

        if (account.Status != AccountStatus.Live)
        {
            _logger.LogInformation($"Account : {account.AccountNumber} - is not live");

            return result;
        }

        account.Balance -= request.Amount;

        _accountData.UpdateAccount(account);

        result.Success = true;

        return result;
    }

    private bool PaymentTypeEnabled(Account account, MakePaymentRequest request)
    {
        return account.AllowedPaymentSchemes.HasFlag((AllowedPaymentSchemes)(1 << (int)request.PaymentScheme));
    }
}