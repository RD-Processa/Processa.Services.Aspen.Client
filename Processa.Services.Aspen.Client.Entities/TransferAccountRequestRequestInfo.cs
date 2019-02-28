// -----------------------------------------------------------------------
// <copyright file="TransferAccountRequestRequestInfo.cs" company="Processa"> 
// Copyright (c)  Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-02-26 11:17 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información necesaria para registrar una cuenta para transferencias.
    /// </summary>    
    public class TransferAccountRequestRequestInfo : ITransferAccountRequestInfo
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TransferAccountRequestRequestInfo"/>
        /// </summary>
        /// <param name="docType">Tipo de documento del titular de la cuenta que se desea registrar.</param>
        /// <param name="docNumber">Número de documento del titular de la cuenta que se desea registrar.</param>
        /// <param name="alias">Nombre con el que se desea identificar la cuenta que se desea registrar.</param>
        /// <param name="accountNumber">Número de cuenta que se desea registrar o <c>null</c> para que el sistema busque el número de cuenta asociado con el tarjehabiente.</param>
        public TransferAccountRequestRequestInfo(string docType, string docNumber, string @alias, string accountNumber = null)
        {

#if !DEBUG
{
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));
            Throw.IfNullOrEmpty(@alias, nameof(@alias));
}
#endif

            this.DocType = docType;
            this.DocNumber = docNumber;
            this.Alias = alias;
            this.AccountNumber = accountNumber;
            this.PinNumber = null;
        }

        /// <summary>
        /// Obtiene o establece el tipo de documento del titular de la cuenta que se desea registrar.
        /// </summary>
        public string DocType { get; }

        /// <summary>
        /// Obtiene o establece el número de documento del titular de la cuenta que se desea registrar.
        /// </summary>        
        public string DocNumber { get; }

        /// <summary>
        /// Obtiene o establece el nombre con el que se desea identificar la cuenta que se desea registrar.
        /// </summary>        
        public string Alias { get; }

        /// <summary>
        /// Obtiene o establece el número de cuenta que se desea registrar.
        /// </summary>
        public string AccountNumber { get; }

        /// <summary>
        /// Obtiene o establece el pin transaccional del suuario actual.
        /// </summary>
        public string PinNumber { get; set; }
    }
}