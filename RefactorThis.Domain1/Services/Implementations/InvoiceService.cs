using RefactorThis.Domain1.Enums;
using RefactorThis.Domain1.Models.Entities;
using RefactorThis.Domain1.Repositories.Contracts;
using RefactorThis.Domain1.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain1.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }
        public async Task<string> ProcessPaymentAsync(Payment payment)
        {
            var inv = await _invoiceRepository.GetInvoiceAsync(payment.Reference);

            var responseMessage = string.Empty;

            if (inv is null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            if (inv.Amount == 0 && (inv.Payments == null || !inv.Payments.Any()))
            {
                responseMessage = "no payment needed";
            }
            else
            {
                if (inv.Payments != null && inv.Payments.Any())
                {
                    if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount))
                    {
                        responseMessage = "invoice was already fully paid";
                    }
                    else if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
                    {
                        responseMessage = "the payment is greater than the partial amount remaining";
                    }
                    else
                    {
                        if ((inv.Amount - inv.AmountPaid) == payment.Amount)
                        {
                            switch (inv.Type)
                            {
                                case InvoiceType.Standard:
                                    inv.AmountPaid += payment.Amount;
                                    inv.Payments.Add(payment);
                                    responseMessage = "final partial payment received, invoice is now fully paid";
                                    break;
                                case InvoiceType.Commercial:
                                    inv.AmountPaid += payment.Amount;
                                    inv.TaxAmount += payment.Amount * 0.14m;
                                    inv.Payments.Add(payment);
                                    responseMessage = "final partial payment received, invoice is now fully paid";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                        }
                        else
                        {
                            switch (inv.Type)
                            {
                                case InvoiceType.Standard:
                                    inv.AmountPaid += payment.Amount;
                                    inv.Payments.Add(payment);
                                    responseMessage = "another partial payment received, still not fully paid";
                                    break;
                                case InvoiceType.Commercial:
                                    inv.AmountPaid += payment.Amount;
                                    inv.TaxAmount += payment.Amount * 0.14m;
                                    inv.Payments.Add(payment);
                                    responseMessage = "another partial payment received, still not fully paid";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
                else
                {
                    if (payment.Amount > inv.Amount)
                    {
                        responseMessage = "the payment is greater than the invoice amount";
                    }
                    else if (inv.Amount == payment.Amount)
                    {
                        switch (inv.Type)
                        {
                            case InvoiceType.Standard:
                                inv.AmountPaid = payment.Amount;
                                inv.TaxAmount = payment.Amount * 0.14m;
                                inv.Payments.Add(payment);
                                responseMessage = "invoice is now fully paid";
                                break;
                            case InvoiceType.Commercial:
                                inv.AmountPaid = payment.Amount;
                                inv.TaxAmount = payment.Amount * 0.14m;
                                inv.Payments.Add(payment);
                                responseMessage = "invoice is now fully paid";
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        switch (inv.Type)
                        {
                            case InvoiceType.Standard:
                                inv.AmountPaid = payment.Amount;
                                inv.TaxAmount = payment.Amount * 0.14m;
                                inv.Payments.Add(payment);
                                responseMessage = "invoice is now partially paid";
                                break;
                            case InvoiceType.Commercial:
                                inv.AmountPaid = payment.Amount;
                                inv.TaxAmount = payment.Amount * 0.14m;
                                inv.Payments.Add(payment);
                                responseMessage = "invoice is now partially paid";
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            // await _invoiceRepository.SaveInvoiceAsync(inv);

            return responseMessage;
        }
    }
}
