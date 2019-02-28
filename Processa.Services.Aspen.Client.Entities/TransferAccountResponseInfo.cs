// -----------------------------------------------------------------------
// <copyright file="TransferAccountResponseInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-02-27 05:37 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de una cuenta inscrita para transferencias.
    /// </summary>
    public class TransferAccountResponseInfo
    {
        /// <summary>
        /// Obtiene o establece el alias con el que se registró la cuenta.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del titular de la cuenta inscrita.
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// Obtiene el numéro (enmascarado) de la cuenta inscrita.
        /// </summary>
        public string MaskedPan { get; set; }
    }
}