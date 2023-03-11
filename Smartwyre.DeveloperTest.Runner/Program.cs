using System;
using Microsoft.Extensions.DependencyInjection;
using Smartwyre.DeveloperTest.Interfaces;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Microsoft.Extensions.Logging;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
        .AddSingleton<IAccountDataStore, AccountDataStore>()
        .AddSingleton<IPaymentService, PaymentService>()
        .AddLogging((loggingBuilder) => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole())
        .BuildServiceProvider();


        var paymentService = serviceProvider.GetRequiredService<IPaymentService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        MakePaymentRequest request = new()
        {
            DebtorAccountNumber = "1",
            Amount = 100.00M,
            PaymentScheme = PaymentScheme.BankToBankTransfer,
            PaymentDate = DateTime.Now,

        };

        var result = paymentService.MakePayment(request);

        logger.LogInformation($"Payment was {(result.Success ? "Successful" : "Unsuccessful")} for account : {request.DebtorAccountNumber}");
    }
}
