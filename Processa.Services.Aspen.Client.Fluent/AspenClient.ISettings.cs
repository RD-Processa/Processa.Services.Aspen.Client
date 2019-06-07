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
        /// Obtiene la lista de operadores de telefonía móvil soportados por el servicio Aspen.
        /// </summary>
        /// <returns>
        /// Lista de operadores de telefonía soportados.
        /// </returns>
        public IList<Telco> GetTelcos()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.Telcos, Method.GET);
            return this.Execute<List<Telco>>(request);
        }

        /// <summary>
        /// Obtiene la lista de los tipos de transacción para una aplicación.
        /// </summary>
        /// <returns>Lista de tipos de transacción soportados.</returns>
        public IList<TranTypeInfo> GetTranTypes()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.TranTypes, Method.GET);
            return this.Execute<List<TranTypeInfo>>(request);
        }

        /// <summary>
        /// Obtiene los tipos de pagos que se pueden realizar a una cuenta.
        /// </summary>
        /// <returns>
        /// Lista de <see cref="PaymentTypeInfo" /> con los tipos de pago para la aplicación solicitante.
        /// </returns>
        public IList<PaymentTypeInfo> GetPaymentTypes()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.PaymentTypes, Method.GET);
            return this.Execute<List<PaymentTypeInfo>>(request);
        }

        /// <summary>
        /// Obtiene los tipos de pagos que se pueden realizar a una cuenta.
        /// </summary>
        /// <returns>Lista de <see cref="TopUpInfo"/> con los valores admitidos de recarga por operador para la aplicación solicitante.</returns>
        public IList<TopUpInfo> GetTopUpValues()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.TopUp, Method.GET);
            return this.Execute<List<TopUpInfo>>(request);
        }

        /// <summary>
        /// Obtiene la lista de opciones que representan el menú de una aplicación móvil.
        /// </summary>
        /// <returns>
        /// Lista de opciones de menú.
        /// </returns>
        public IList<MenuItem> GetMenu()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.Menu, Method.GET);
            return this.Execute<List<MenuItem>>(request);
        }

        /// <summary>
        /// Obtiene los valores misceláneos soportados por el servicio Aspen.
        /// </summary>
        /// <returns>
        /// Colección de valores admitidos.
        /// </returns>
        public IList<KeyValuePair<string, object>> GetMiscellaneousValues()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.Miscellaneous, Method.GET);
            return this.Execute<IList<KeyValuePair<string, object>>>(request);
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

        /// <summary>
        /// Obtiene la lista de canales
        /// </summary>
        /// <returns>Lista de canales.</returns>
        public IList<ChannelInfo> GetChannels()
        {
            IRestRequest request = new AspenRequest(this, Routes.Tokens.Channels, Method.GET);
            return this.Execute<List<ChannelInfo>>(request);
        }
    }
}