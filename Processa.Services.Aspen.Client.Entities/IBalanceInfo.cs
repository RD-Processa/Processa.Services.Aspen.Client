// -----------------------------------------------------------------------
// <copyright file="IBalanceInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-14 05:49 PM</date>
// ----------------------------------------------------------------------

namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Define la información de un tipo de cuenta / Bolsillo en el sistema TUP.
    /// </summary>
    public interface IBalanceInfo
    {
        /// <summary>
        /// Obtiene o establece el color (hexadecimal) que se utiliza para representar el elemento en la UI.
        /// </summary>
        string BackgroundColor { get; set; }

        /// <summary>
        /// Obtiene o establece el saldo actual del tipo de cuenta.
        /// </summary>
        decimal Balance { get; set; }

        /// <summary>
        /// Obtiene o establece el número de cuenta (enmascarado) al que pertenece el tipo de cuenta.
        /// </summary>
        string Number { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre corto (iniciales) que utilizan para representar el elemento en la UI
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la cuenta que se debe utilizar en los procesos transaccionales, o <c>null</c> si la cuenta no lo permite.
        /// </summary>
        string SourceAccountId { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del tipo de cuenta / Bolsillo en el sistema TUP.
        /// </summary>
        /// <example>Un valor como 80, 81, 82, etc.</example>
        string TypeId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del tipo de cuenta / Bolsillo en el sistema TUP.
        /// </summary>
        /// <example>Monedero General, Subsidio familiar, etc.</example>
        string TypeName { get; set; }
    }
}