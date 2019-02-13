// -----------------------------------------------------------------------
// <copyright file="IMiniStatementInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-18 03:22 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System;

    /// <summary>
    /// Define la información de un movimiento/transacción en una cuenta.
    /// </summary>
    public interface IMiniStatementInfo
    {
        /// <summary>
        /// Obtiene o establece el identificador del tipo de cuenta que afectó el movimiento/transacción.
        /// </summary>
        /// <example>80, 81, 82, etc</example>
        string AccountTypeId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del tipo de cuenta que afectó el movimiento/transacción.
        /// </summary>
        /// <example>Subsidio Educativo, Subsidio Educativo, Viveres General, etc</example>
        string AccountTypeName { get; set; }

        /// <summary>
        /// Obtiene o establece el valor por el que se realizó el movimiento/transacción.
        /// </summary>
        decimal Amount { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del comercio donde se realizó el movimiento/transacción.
        /// </summary>
        string CardAcceptor { get; set; }

        /// <summary>
        /// Obtiene o establece la naturaleza del movimiento/transacción.
        /// </summary>
        /// <example>Credit, Debit</example>
        Accounting Category { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora en que se realizó el movimiento/transacción.
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre que representa el tipo de transacción.
        /// </summary>
        /// <example>Compra, Retiro, etc.</example>
        string TranName { get; set; }

        /// <summary>
        /// Obtiene o establece el código que representa el tipo de transacción.
        /// </summary>
        /// <example>00=Compra, 01=Retiro, etc.</example>
        string TranType { get; set; }
    }
}