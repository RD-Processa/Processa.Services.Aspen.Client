// -----------------------------------------------------------------------
// <copyright file="BalanceInfo.cs" company="Processa"> 
// Copyright (c)  Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-14 05:51 PM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de un tipo de cuenta / Bolsillo en el sistema TUP.
    /// </summary>
    public class BalanceInfo : IBalanceInfo
    {
        /// <summary>
        /// Obtiene o establece el color (hexadecimal) que se utiliza para representar el elemento en la UI.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Obtiene o establece el saldo actual del tipo de cuenta.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Obtiene o establece el número de cuenta (enmascarado) al que pertenece el tipo de cuenta.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre corto (iniciales) que utilizan para representar el elemento en la UI
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la cuenta que se debe utilizar en los procesos transaccionales, o <c>null</c> si la cuenta no lo permite.
        /// </summary>
        public string SourceAccountId { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del tipo de cuenta / Bolsillo en el sistema TUP.
        /// </summary>
        /// <example>Un valor como 80, 81, 82, etc.</example>
        public string TypeId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del tipo de cuenta / Bolsillo en el sistema TUP.
        /// </summary>
        /// <example>Monedero General, Subsidio familiar, etc.</example>
        public string TypeName { get; set; }
    }
}