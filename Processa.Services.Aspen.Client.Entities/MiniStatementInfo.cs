// -----------------------------------------------------------------------
// <copyright file="MiniStatementInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-18 03:24 PM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    using System;

    /// <summary>
    /// Representa la información de un movimiento/transacción en una cuenta.
    /// </summary>
    public class MiniStatementInfo : IMiniStatementInfo
    {
        /// <summary>
        /// Obtiene o establece el identificador del tipo de cuenta que afectó el movimiento/transacción.
        /// </summary>
        /// <example>Algunos valores reconocidos: 80, 81, 82, etc</example>
        public string AccountTypeId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del tipo de cuenta que afectó el movimiento/transacción.
        /// </summary>
        /// <example>Subsidio Educativo, Subsidio Educativo, Viveres General, etc</example>
        public string AccountTypeName { get; set; }

        /// <summary>
        /// Obtiene o establece el valor por el que se realizó el movimiento/transacción.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del comercio donde se realizó el movimiento/transacción.
        /// </summary>
        public string CardAcceptor { get; set; }

        /// <summary>
        /// Obtiene o establece la naturaleza del movimiento/transacción.
        /// </summary>
        /// <example>Credit, Debit</example>
        public Accounting Category { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora en que se realizó el movimiento/transacción.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre que representa el tipo de transacción.
        /// </summary>
        /// <example>Compra, Retiro, etc.</example>
        public string TranName { get; set; }

        /// <summary>
        /// Obtiene o establece el código que representa el tipo de transacción.
        /// </summary>
        /// <example>00=Compra, 01=Retiro, etc.</example>
        public string TranType { get; set; }
    }
}