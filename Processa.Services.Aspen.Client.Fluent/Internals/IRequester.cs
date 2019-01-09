// -----------------------------------------------------------------------
// <copyright file="IRequester.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 07:41 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Internals
{
    using System.Collections.Generic;
    using Providers;
    using RestSharp;

    /// <summary>
    /// Define la información de un solicitante.
    /// </summary>
    internal interface IRequester
    {
        /// <summary>
        /// Obtiene el alcance de la aplicación solicitante.
        /// </summary>
        AppScope AppScope { get; }

        /// <summary>
        /// Obtiene la información del dispositivo solicitante.
        /// </summary>
        IDeviceInfo DeviceInfo { get; }

        /// <summary>
        /// Agrega las cabeceras necesarias para el procesamiento de la solicitud.
        /// </summary>
        /// <param name="request">Instancia de la solicitud a donde se agregan las cabeceras.</param>
        /// <param name="customPayload">Valores que se desean agregar al PayLoad de la solicitud o ; <see langword="null" /> para no agregar valores adicionales.</param>
        void AddRequiredHeaders(IRestRequest request, IDictionary<string, object> customPayload = null);
    }
}