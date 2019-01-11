// -----------------------------------------------------------------------
// <copyright file="IFinancialModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 02:32 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;

    /// <summary>
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades de información relacionadas con recursos financieros.
    /// </summary>
    public interface IFinancialModule
    {
        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <returns>Listado con la información de las cuentas del usuario actual.</returns>
        IList<IAccountInfo> GetAccounts();

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas del usuario especificado.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <returns>Listado con la información de las cuentas del usuario especificado.</returns>
        IList<IAccountInfo> GetAccounts(string docType, string docNumber);
    }
}