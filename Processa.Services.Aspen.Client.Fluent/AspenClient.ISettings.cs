// -----------------------------------------------------------------------
// <copyright file="AspenClient.ISettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 07:20 PM</date>
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
    public partial class AspenClient : ISettingsModule
    {

        /// <summary>
        /// Obtiene una instancia que pemite acceder a recursos de parametrización del servicio Aspen.
        /// </summary>
        public ISettingsModule Settings => this;

        /// <summary>
        /// Obtiene la lista de tipos de documento soportados por el servicio Aspen.
        /// </summary>
        /// <returns>Lista de tipos de documento soportados.</returns>
        public IList<DocType> GetDocTypes()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.DocTypes, Method.GET);
            return this.Execute<List<DocType>>(request);
        }

        /// <summary>
        /// Obtiene la lista de claims habilitados en el sistema
        /// </summary>
        /// <returns>Lista de claims habilitados.</returns>
        public IList<ClaimSettings> GetClaims()
        {
            RestClient client = new RestClient(this.endpointProvider.Url);
            RestRequest request = new RestRequest("/utils/claims", Method.GET);
            IRestResponse<List<ClaimSettings>> response = client.Execute<List<ClaimSettings>>(request);

            if (!response.IsSuccessful)
            {
                throw new AspenResponseException(response);
            }

            return response.Data;
        }    
    }
}