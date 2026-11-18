using Payment.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment.Domain.Repositories
{
    public interface IPaymentGatewayClient
    {
        Task<PaymentResult> ChargeAsync(PaymentRequest request);
    }
}
