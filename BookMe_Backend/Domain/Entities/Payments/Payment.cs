using Domain.Enums.Payments;

namespace Domain.Entities.Payments
{
    public class Payment
    {
        public Guid Id { get; set; }
        public required Guid BookingId { get; set; }

        public decimal Amount { get; set; }
        public required string CurrencyCode { get; set; }

        public PaymentStatus Status { get; set; }
        
        public string ExternalTransactionId { get; set; }
        
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
