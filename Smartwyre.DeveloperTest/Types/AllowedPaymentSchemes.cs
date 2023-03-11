namespace Smartwyre.DeveloperTest.Types
{
    public enum AllowedPaymentSchemes
    {
        ExpeditedPayments = 1 << 0,
        BankToBankTransfer = 1 << 1,
        AutomatedPaymentSystem = 1 << 2
    }
}
