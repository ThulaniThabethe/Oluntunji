using System;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<bool> RefundPaymentAsync(string transactionId, decimal amount);
        string GenerateTransactionId();
    }

    public class PaymentService : IPaymentService
    {
        private readonly IEmailService _emailService;

        public PaymentService()
        {
            _emailService = new EmailService();
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Simulate payment processing delay
                await Task.Delay(2000);

                // Generate transaction ID
                var transactionId = GenerateTransactionId();
                var paymentReference = $"PAY-{DateTime.Now:yyyyMMdd}-{transactionId.Substring(0, 8)}";

                // Simulate payment success/failure (90% success rate)
                var random = new Random();
                var isSuccess = random.Next(1, 101) <= 90;

                var result = new PaymentResult
                {
                    IsSuccess = isSuccess,
                    TransactionId = transactionId,
                    PaymentReference = paymentReference,
                    Amount = request.Amount,
                    Currency = "ZAR",
                    ProcessedAt = DateTime.Now,
                    Message = isSuccess ? "Payment processed successfully" : "Payment failed - insufficient funds"
                };

                if (isSuccess)
                {
                    // Send payment confirmation email
                    try
                    {
                        await _emailService.SendPaymentConfirmationAsync(request.PrintRequestId, result);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to send payment confirmation email: {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = $"Payment processing error: {ex.Message}",
                    ProcessedAt = DateTime.Now
                };
            }
        }

        public async Task<bool> RefundPaymentAsync(string transactionId, decimal amount)
        {
            try
            {
                // Simulate refund processing
                await Task.Delay(1000);
                
                // Simulate 95% success rate for refunds
                var random = new Random();
                return random.Next(1, 101) <= 95;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateTransactionId()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
    }

    public class PaymentRequest
    {
        public int PrintRequestId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZAR";
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public string Description { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CVV { get; set; }
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string PaymentReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        EFT,
        Cash
    }
}