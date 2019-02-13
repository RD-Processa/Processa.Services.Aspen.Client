// -----------------------------------------------------------------------
// <copyright file="Accounting.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-18 03:23 PM</date>
// ----------------------------------------------------------------------

namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Define la naturaleza contable de una operación.
    /// </summary>
    public enum Accounting
    {
        /// <summary>
        /// La operación es de tipo débito.
        /// </summary>
        Debit = 0,

        /// <summary>
        /// La operación es de tipo crédito.
        /// </summary>
        Credit = 1
    }
}