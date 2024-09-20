
using SparkTech.Data.Models;
using SparkTech.Data.Repositories;

namespace SparkTech.Services
{
    public class PaymentService(
        IRepository<Payment> repo
    ) : IPaymentService
    {
        public async Task<Payment> CreateAsync(Payment payment)
        {
            Validate(payment);
            await repo.Insert(payment);
            return payment;
        }

        public async Task DeleteAsync(int id, bool? isSoft = false)
        {
            var payment = await repo.GetById(id);
            if (payment == null) throw new KeyNotFoundException("Payment not found");
            await repo.DeleteAsync(id, isSoft: true);
        }

        public async Task<IEnumerable<Payment>> GetAll()
        {
            return await repo.GetAll(p => !p.IsDeleted);
        }

        public async Task<Payment?> GetById(int id)
        {
            return await repo.GetById(id);
        }

        public async Task<Payment?> GetByReference(string reference)
        {
            return (await repo.GetBy(p => !p.IsDeleted && p.Reference.Equals(reference))).FirstOrDefault();
        }

        public async Task UpdateAsync(Payment updatedPayment)
        {
            Validate(updatedPayment);
            await repo.UpdateAsync(updatedPayment);
        }

        public void Validate(Payment payment)
        {
            if (string.IsNullOrEmpty(payment.Reference)) throw new NullReferenceException("Invalid payment reference");
            if (payment == null) throw new NullReferenceException("Null Payment");
            if (payment.Quality == 0) throw new Exception("Quantity can't be $0");
        }
    }
}