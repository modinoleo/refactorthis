using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RefactorThis.Domain1.Enums;
using RefactorThis.Domain1.Models.Entities;
using RefactorThis.Domain1.Repositories.Contracts;
using RefactorThis.Domain1.Services.Contracts;
using RefactorThis.Domain1.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain1.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private const decimal TaxRate = 0.14m;
        public InvoiceService(IInvoiceRepository invoiceRepository, ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }
     
        public async Task<string> ProcessPaymentAsync(Payment payment)
        {
            var inv = await _invoiceRepository.GetInvoiceAsync(payment.Reference);

            var responseMessage = string.Empty;

            _logger.LogInformation($"InvoiceService | ProcessPaymentAsync -  {JsonConvert.SerializeObject(payment)}");

            if (inv is null)
            {
                _logger.LogError($"InvoiceService | ProcessPaymentAsync -  {JsonConvert.SerializeObject(inv)}");

                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            var validationMessage = PaymentValidators.CheckPaymentValidity(inv,payment);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                responseMessage = validationMessage;
            }
            else
            {
                if (inv.Payments != null && inv.Payments.Any())
                {
                    responseMessage = PaymentValidators.CheckIfFinalPartialPayment(inv, payment);
                    inv.AmountPaid += payment.Amount;
                    if (inv.Type == InvoiceType.Commercial)
                    {
                        inv.TaxAmount += payment.Amount * TaxRate;
                    }
               
                }
                else
                {
                    responseMessage = PaymentValidators.CheckIfInvoiceIsFullyPaid(inv, payment);
                    inv.AmountPaid = payment.Amount;
                    inv.TaxAmount += payment.Amount * TaxRate;
                }
                inv.Payments?.Add(payment);
            }

            await _invoiceRepository.SaveInvoiceAsync(inv);
           
            return responseMessage;
        }
    }
}
