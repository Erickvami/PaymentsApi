
using SparkTech.Data.Models;

namespace SparkTech.Services {
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAll();
        Task<Payment?> GetById(int id);
        Task<Payment?> GetByReference(string reference);
        Task<Payment> CreateAsync(Payment payment);
        void Validate(Payment payment);
        Task DeleteAsync(int id, bool? isSoft = false);
        Task UpdateAsync(Payment updatedPayment);

    }
}