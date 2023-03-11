using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Interfaces;
public interface IPaymentService
{
    MakePaymentResult MakePayment(MakePaymentRequest request);
}