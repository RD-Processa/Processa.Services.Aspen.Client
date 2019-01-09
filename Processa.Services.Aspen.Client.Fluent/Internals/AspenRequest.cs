// -----------------------------------------------------------------------
// <copyright file="AspenRequest.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 07:49 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Internals
{
    using System;
    using Providers;
    using RestSharp;

    /// <summary>
    /// Representa la información de una solicitud HTTP al servicio Aspen.
    /// </summary>
    /// <seealso cref="RestSharp.RestRequest" />
    internal class AspenRequest : RestRequest
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AspenRequest" />.
        /// </summary>
        /// <param name="requester">Instancia del solicitante.</param>
        /// <param name="url">URL del recurso solicitado.</param>
        /// <param name="method">Método o verbo HTTP para invocar el recurso.</param>
        /// <param name="addRequiredHeaders">Cuando es <see langword="true" /> se llama al método AddRequiredHeaders.</param>
        internal AspenRequest(IRequester requester, string url, Method method, bool addRequiredHeaders = true) : this(requester.AppScope, url, method, requester.DeviceInfo)
        {
            if (addRequiredHeaders)
            {
                requester.AddRequiredHeaders(this, null);
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AspenRequest" />.
        /// </summary>
        /// <param name="appScope">Alcance de la aplicación solicitante.</param>
        /// <param name="url">URL del recurso solicitado.</param>
        /// <param name="method">Método o verbo HTTP para invocar el recurso.</param>
        /// <param name="deviceInfo">Información del dispositivo que envía la petición.</param>
        private AspenRequest(AppScope appScope, string url, Method method, IDeviceInfo deviceInfo) :
            base($"{(appScope == AppScope.Autonomous ? Routes.AutonomousRoot : Routes.DelegatedRoot)}{url}", method, DataFormat.Json)
        {
            Throw.IfNullOrEmpty(url, nameof(url));

            const string ContentType = "application/json; charset=utf-8";

            this.AddHeader("Accept", "application/json");
            this.AddHeader("Content-Type", ContentType);
            this.Timeout = 15000;
            this.JsonSerializer = new RestSharp.Serialization.Json.JsonSerializer
            {
                ContentType = ContentType
            };

            if (deviceInfo == null)
            {
                deviceInfo = CacheStore.GetDeviceInfo() ?? new DeviceInfo();
            }

            switch (appScope)
            {
                case AppScope.Delegated:
                    this.AddHeader("X-PRO-Request-DeviceInfo", deviceInfo.ToJson());
                    CacheStore.SetDeviceInfo(deviceInfo);
                    break;
                case AppScope.Autonomous:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(appScope), appScope, null);
            }  
            
            
        }
    }
}