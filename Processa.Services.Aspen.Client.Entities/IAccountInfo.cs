// -----------------------------------------------------------------------
// <copyright file="IAccountInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-10 02:03 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// Define la información de una cuenta de un tarjetahabiente.
    /// </summary>
    public interface IAccountInfo
    {
        /// <summary>
        ///     Cuando es <c>true</c>, indica a la app que se permite la utilización de la cuenta como origen de un pago.
        /// </summary>
        bool AllowsPayment { get; set; }

        /// <summary>
        ///     Obtiene o establece el color asociado con la cuenta.
        /// </summary>
        string BackgroundColor { get; set; }

        /// <summary>
        /// Obtiene o establece el valor del saldo actual de la cuenta.
        /// </summary>
        decimal Balance { get; set; }

        /// <summary>
        ///     Obtiene o establece el identificador univoco de la cuenta.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        ///     Cuando es <c>true</c>, indica que la cuenta está bloqueada.
        /// </summary>
        bool IsLockedOut { get; set; }

        /// <summary>
        ///     Obtiene o establece el número de cuenta enmascarado.
        /// </summary>
        string MaskedPan { get; set; }

        /// <summary>
        ///     Obtiene o establece el nombre que identifica la cuenta.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Obtiene o establece el orden del elemento en la UI.
        /// </summary>
        int Order { get; set; }

        /// <summary>
        ///     Obtiene o establece el nombre corto de la cuenta.
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de cuenta.
        /// </summary>
        Subsystem Source { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la cuenta que se debe utilizar en los procesos transaccionales, o <c>null</c> si la cuenta no lo permite.
        /// </summary>
        string SourceAccountId { get; set; }

        /// <summary>
        /// Obtiene o establece la información de atributos extendidos de la cuenta.
        /// </summary>
        IList<AccountProperty> Properties { get; set; }

    }
}