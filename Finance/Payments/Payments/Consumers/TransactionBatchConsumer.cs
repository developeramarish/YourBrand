﻿using MassTransit;

using Microsoft.EntityFrameworkCore;

using YourBrand.Payments.Domain;
using YourBrand.Transactions.Client;
using YourBrand.Transactions.Contracts;

namespace YourBrand.Payments.Consumers;

public class TransactionBatchConsumer : IConsumer<IncomingTransactionBatch>
{
    private readonly IPaymentsContext _context;
    private readonly ITransactionsClient _transactionsClient;

    public TransactionBatchConsumer(IPaymentsContext context, ITransactionsClient transactionsClient) 
    {
        _context = context;
        _transactionsClient = transactionsClient;
    }

    public async Task Consume(ConsumeContext<IncomingTransactionBatch> context)
    {
        var batch = context.Message;

        foreach (var transaction in batch.Transactions)
        {
            await HandleTransaction(transaction, context.CancellationToken);
        }
    }

    private async Task HandleTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Reference == transaction.Reference);

        if(payment is null)
        {
            await _transactionsClient.SetTransactionStatusAsync(transaction.Id, Transactions.Client.TransactionStatus.Unknown);
        }
        else
        {
            payment.RegisterCapture(transaction.Date, transaction.Amount, transaction.Id);

            var amountCaptured = payment.Captures.Sum(c => c.Amount);

            if(amountCaptured == payment.Amount)
            {
                payment.SetStatus(Domain.Enums.PaymentStatus.Captured);
            }
            else if (amountCaptured < payment.Amount)
            {
                payment.SetStatus(Domain.Enums.PaymentStatus.PartiallyCaptured);
            }
            else if (amountCaptured > payment.Amount)
            {
                // TODO: Overcaptured

                //payment.SetStatus(Domain.Enums.PaymentStatus.O);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _transactionsClient.SetTransactionStatusAsync(transaction.Id, Transactions.Client.TransactionStatus.Verified);
        }
    }
}