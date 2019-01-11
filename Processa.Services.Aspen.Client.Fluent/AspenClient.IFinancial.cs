// -----------------------------------------------------------------------
// <copyright file="AspenClient.IFinancial.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 04:20 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;
    using Internals;
    using RestSharp;

    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    public partial class AspenClient : IFinancialModule
    {
        /// <summary>
        /// Obtiene una instancia que pemite acceder a recursos financieros del servicio Aspen.
        /// </summary>
        public IFinancialModule Financial => this;

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <returns>Listado con la información de las cuentas del usuario actual.</returns>
        public IList<IAccountInfo> GetAccounts()
        {
            IRestRequest request = new AspenRequest(this, Routes.Inquires.Delegated.Accounts, Method.GET);
            return this.Execute<List<AccountInfo>>(request).ConvertAll(item => (IAccountInfo)item);
        }

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas del usuario especificado.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <returns>Listado con la información de las cuentas del usuario especificado.</returns>
        public IList<IAccountInfo> GetAccounts(string docType, string docNumber)
        {
            string url = $"{Routes.Inquires.Autonomous.Accounts}/{docType}/{docNumber}";
            IRestRequest request = new AspenRequest(this, url, Method.GET);
            return this.Execute<List<AccountInfo>>(request).ConvertAll(item => (IAccountInfo)item);
        }
    }
}