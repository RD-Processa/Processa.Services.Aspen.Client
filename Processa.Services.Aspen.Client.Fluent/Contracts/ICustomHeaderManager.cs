// -----------------------------------------------------------------------
// <copyright file="ICustomHeaderManager.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 02:23 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    using System.Collections.Generic;
    using JWT;
    using Providers;
    using RestSharp;

    /// <summary>
    /// Define las operaciones necesarias para establecer las cabeceras personalizadas requeridas por el servicio Aspen.
    /// </summary>
    public interface ICustomHeaderManager
    {
        /// <summary>
        /// Agrega la cabecera que identifica la aplicación solicitante.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="value">Valor de la cabecera.</param>
        void AddApiKeyHeader(IRestRequest request, string value);

        /// <summary>
        /// Agrega la cabecera que identifica la aplicación solicitante.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="identity">Instancia de <see cref="IIdentityProvider"/> que proporciona la ifnormación de autenticación.</param>
        void AddApiKeyHeader(IRestRequest request, IIdentityProvider identity);

        /// <summary>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="value">Cadena con el Payload para agregar.</param>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        void AddPayloadHeader(IRestRequest request, string value);

        /// <summary>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="encoder">The encoder.</param>
        /// <param name="payload">Diccionario que contiene los datos del payload.</param>
        /// <param name="identity">Instancia de <see cref="IIdentityProvider" /> que proporciona la información de autenticación.</param>
        void AddPayloadHeader(IRestRequest request, IJwtEncoder encoder, IDictionary<string, object> payload, IIdentityProvider identity);
    }
}