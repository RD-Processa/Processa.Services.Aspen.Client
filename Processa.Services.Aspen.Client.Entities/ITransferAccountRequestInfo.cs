// -----------------------------------------------------------------------
// <copyright file="ITransferAccountRequestInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-02-26 11:13 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Define la información necesaria para registrar una cuenta para transferencias.
    /// </summary>
    public interface ITransferAccountRequestInfo
    {
        /// <summary>
        /// Obtiene  el tipo de documento del titular de la cuenta que se desea registrar.
        /// </summary>
        string DocType { get; }

        /// <summary>
        /// Obtiene el número de documento del titular de la cuenta que se desea registrar.
        /// </summary>        
        string DocNumber { get; }

        /// <summary>
        /// Obtiene el nombre con el que se desea identificar la cuenta que se desea registrar.
        /// </summary>        
        string Alias { get; }

        /// <summary>
        /// Obtiene el número de cuenta que se desea registrar.
        /// </summary>
        string AccountNumber { get; }

        /// <summary>
        /// Obtiene o establece el pin transaccional del suuario actual.
        /// </summary>
        string PinNumber { get; set; }

    }
}