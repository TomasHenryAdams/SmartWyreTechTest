using System;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Smartwyre.DeveloperTest.Interfaces;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Tests;

public class PaymentServiceTests
{
    private readonly Mock<ILogger<PaymentService>> _mockLogger;
    private readonly Mock<IAccountDataStore> _mockDataStore;

    public PaymentServiceTests()
    {
        _mockLogger = new();
        _mockDataStore = new();
    }

    [Fact]
    public void Account_not_found_will_return_a_false_payment_result()
    {
        _mockDataStore.Setup(d => d.GetAccount(It.IsAny<string>())).Returns<Account>(null);

        var request = new MakePaymentRequest() { DebtorAccountNumber = "1" };

        var paymentService = GetPaymentService();

        var response = paymentService.MakePayment(request);

        Assert.False(response.Success);
        _mockDataStore.Verify(d => d.GetAccount(It.IsAny<string>()), Times.Once);
        _mockDataStore.Verify(d => d.UpdateAccount(It.IsAny<Account>()), Times.Never);
        TestLogger($"No account found for account number : {request.DebtorAccountNumber}");
    }

    [Fact]
    public void If_account_does_not_have_required_funds_will_return_a_false_payment_result()
    {
        var account = CreateAccount(1M, AccountStatus.Live, AllowedPaymentSchemes.AutomatedPaymentSystem);
        var request = CreatePaymentRequest(100M, PaymentScheme.AutomatedPaymentSystem);
        _mockDataStore.Setup(d => d.GetAccount(It.IsAny<string>())).Returns(account);

        var paymentService = GetPaymentService();

        var response = paymentService.MakePayment(request);

        Assert.False(response.Success);
        _mockDataStore.Verify(d => d.GetAccount(It.IsAny<string>()), Times.Once);
        _mockDataStore.Verify(d => d.UpdateAccount(It.IsAny<Account>()), Times.Never);
        TestLogger($"Account : {account.AccountNumber} - does not have the required balance to complete the transaction");
    }

    [Theory]
    [InlineData(AccountStatus.Disabled)]
    [InlineData(AccountStatus.InboundPaymentsOnly)]
    public void If_account_status_is_not_live_will_return_a_false_payment_result(AccountStatus accountStatus)
    {
        var account = CreateAccount(20M, accountStatus, AllowedPaymentSchemes.AutomatedPaymentSystem);
        var request = CreatePaymentRequest(20M, PaymentScheme.AutomatedPaymentSystem);
        _mockDataStore.Setup(d => d.GetAccount(It.IsAny<string>())).Returns(account);

        var paymentService = GetPaymentService();

        var response = paymentService.MakePayment(request);

        Assert.False(response.Success);
        _mockDataStore.Verify(d => d.GetAccount(It.IsAny<string>()), Times.Once);
        _mockDataStore.Verify(d => d.UpdateAccount(It.IsAny<Account>()), Times.Never);
        TestLogger($"Account : {account.AccountNumber} - is not live");
    }

    [Theory]
    [InlineData(AllowedPaymentSchemes.AutomatedPaymentSystem, PaymentScheme.ExpeditedPayments)]
    [InlineData(AllowedPaymentSchemes.ExpeditedPayments, PaymentScheme.BankToBankTransfer)]
    [InlineData(AllowedPaymentSchemes.BankToBankTransfer, PaymentScheme.AutomatedPaymentSystem)]
    public void Payment_type_not_enabled_will_return_a_false_payment_result(AllowedPaymentSchemes allowedPaymentScheme, PaymentScheme requestPaymentScheme)
    {
        var account = CreateAccount(20M, AccountStatus.Live, allowedPaymentScheme);
        var request = CreatePaymentRequest(20M, requestPaymentScheme);
        _mockDataStore.Setup(d => d.GetAccount(It.IsAny<string>())).Returns(account);

        var paymentService = GetPaymentService();

        var response = paymentService.MakePayment(request);

        Assert.False(response.Success);
        _mockDataStore.Verify(d => d.GetAccount(It.IsAny<string>()), Times.Once);
        _mockDataStore.Verify(d => d.UpdateAccount(It.IsAny<Account>()), Times.Never);
        TestLogger($"Payment scheme : {request.PaymentScheme} - is not enabled for account : {account.AccountNumber}");
    }

    [Theory]
    [InlineData(AllowedPaymentSchemes.AutomatedPaymentSystem, PaymentScheme.AutomatedPaymentSystem)]
    [InlineData(AllowedPaymentSchemes.ExpeditedPayments, PaymentScheme.ExpeditedPayments)]
    [InlineData(AllowedPaymentSchemes.BankToBankTransfer, PaymentScheme.BankToBankTransfer)]
    public void Payment_type_enabled_and_live_account_will_update_the_account(AllowedPaymentSchemes allowedPaymentScheme, PaymentScheme requestPaymentScheme)
    {
        var account = CreateAccount(20M, AccountStatus.Live, allowedPaymentScheme);
        var request = CreatePaymentRequest(20M, requestPaymentScheme);
        _mockDataStore.Setup(d => d.GetAccount(It.IsAny<string>())).Returns(account);

        var paymentService = GetPaymentService();

        var response = paymentService.MakePayment(request);

        Assert.True(response.Success);
        _mockDataStore.Verify(d => d.GetAccount(It.IsAny<string>()), Times.Once);
        _mockDataStore.Verify(d => d.UpdateAccount(It.IsAny<Account>()), Times.Once);
    }

    private PaymentService GetPaymentService()
    {
        return new PaymentService(_mockLogger.Object, _mockDataStore.Object);
    }

    private void TestLogger(string logMessage)
    {
        _mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
            It.Is<EventId>(eventId => eventId.Id == 0),
            It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == logMessage && @type.Name == "FormattedLogValues"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private Account CreateAccount(decimal balance, AccountStatus status, AllowedPaymentSchemes allowedPaymentSchemes)
    {
        return new()
        {
            AccountNumber = "1",
            Balance = balance,
            Status = status,
            AllowedPaymentSchemes = allowedPaymentSchemes
        };
    }

    private MakePaymentRequest CreatePaymentRequest(decimal amount, PaymentScheme paymentScheme)
    {
        return new()
        {
            Amount = amount,
            PaymentScheme = paymentScheme
        };
    }
}