// -----------------------------------------------------------------------
// <copyright file="ReversalInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-04-24 10:51 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    public class ReversalInfo
    {
        public ReversalInfo(
            string transactionId, 
            string docType, 
            string docNumber, 
            string accountType, 
            int amount, 
            TagsInfo tags = null)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));
            Throw.IfNullOrEmpty(transactionId, nameof(transactionId));
            Throw.IfNullOrEmpty(accountType, nameof(accountType));
            Throw.IfEmpty(amount, nameof(amount));

            this.TransactionId = transactionId;
            this.DocType = docType;
            this.DocNumber = docNumber;
            this.AccountType = accountType;
            this.Amount = amount;
            this.Tags = tags;
        }

        public string TransactionId { get; set; }

        public string DocType { get; set; }

        public string DocNumber { get; set; }

        public string AccountType { get; set; }

        public int Amount { get; set; }

        public TagsInfo Tags { get; set; }
    }
}