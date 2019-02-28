// -----------------------------------------------------------------------
// <copyright file="AccountInfo.cs" company="Processa"> 
// Copyright (c)  Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-10 04:47 PM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    using System.Collections.Generic;

    public class AccountInfo : IAccountInfo
    {
        /// <summary>
        ///     Cuando es <c>true</c>, indica a la app que se permite la utilización de la cuenta como origen de un pago.
        /// </summary>
        public bool AllowsPayment { get; set; }

        /// <summary>
        ///     Obtiene o establece el color asociado con la cuenta.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Obtiene o establece el valor del saldo actual de la cuenta.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        ///     Obtiene o establece el identificador univoco de la cuenta.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Cuando es <c>true</c>, indica que la cuenta está bloqueada.
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        ///     Obtiene o establece el número de cuenta enmascarado.
        /// </summary>
        public string MaskedPan { get; set; }

        /// <summary>
        ///     Obtiene o establece el nombre que identifica la cuenta.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Obtiene o establece el orden del elemento en la UI.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     Obtiene o establece el nombre corto de la cuenta.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de cuenta.
        /// </summary>
        public Subsystem Source { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la cuenta que se debe utilizar en los procesos transaccionales, o <c>null</c> si la cuenta no lo permite.
        /// </summary>
        public string SourceAccountId { get; set; }

        /// <summary>
        /// Obtiene o establece la información de atributos extendidos de la cuenta.
        /// </summary>
        public IList<AccountProperty> Properties { get; set; }
    }
}