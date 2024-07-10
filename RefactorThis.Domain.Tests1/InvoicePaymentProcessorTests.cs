using RefactorThis.Domain1.Models.Entities;
using RefactorThis.Domain1.Repositories.Contracts;
using RefactorThis.Domain1.Services.Contracts;
using RefactorThis.Domain1.Services.Implementations;
using RefactorThis.Persistence1.Repositories.Implementations;

namespace RefactorThis.Domain.Tests1
{
    [TestClass]
    public class InvoicePaymentProcessorTests
    {
        protected IInvoiceRepository? _invoiceRepository;
        protected IInvoiceService? _invoiceService;

        [TestInitialize]
        public void Initialize()
        {
            _invoiceRepository = new InvoiceRepository();
            _invoiceService = new InvoiceService(_invoiceRepository);
        
        }

        [TestMethod]
        public async Task ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            var payment = new Payment();
            var failureMessage = "";
            try
            {
                if (_invoiceService == null) throw new NullReferenceException();

                var result = await _invoiceService.ProcessPaymentAsync(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment();

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("no payment needed", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10
                    }
                }
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment();

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                }
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);
            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }
    }
}