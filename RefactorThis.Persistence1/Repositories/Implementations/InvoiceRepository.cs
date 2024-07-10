using RefactorThis.Domain1.Models.Entities;
using RefactorThis.Domain1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence1.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ICollection<Invoice> _invoices = new HashSet<Invoice>();
        private Invoice _Invoice { get; set; }
        public async Task AddAsync(Invoice invoice)
        {
          await Task.Run(() => _invoices.Add(invoice));
        }

        public Task<Invoice?> GetInvoiceAsync(string reference)
        {
            //return Task.FromResult(_invoices.FirstOrDefault(_ => _.Payments == null
            //|| _.Payments.Any(a => a.Reference.Equals(reference, StringComparison.CurrentCultureIgnoreCase))));

            var result = _invoices.FirstOrDefault(_ => _.Payments == null
            || _.Payments.Any(a => a.Reference.Equals(reference, StringComparison.CurrentCultureIgnoreCase)));

            if (result is null)
            {
                return Task.FromResult(_invoices.FirstOrDefault());
            }
            else
            {
                return Task.FromResult(result);
            }
         
        }

        public Task SaveInvoiceAsync(Invoice invoice)
        {
            throw new NotImplementedException();
        }
    }
}
