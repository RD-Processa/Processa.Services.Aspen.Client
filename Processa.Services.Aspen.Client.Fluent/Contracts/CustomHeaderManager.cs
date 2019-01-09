// -----------------------------------------------------------------------
// <copyright file="CustomHeaderManager.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 02:31 PM</date>
// ----------------------------------------------------------------------

namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    using System.Collections.Generic;
    using Internals;
    using JWT;
    using Providers;
    using RestSharp;

    /// <summary>
    /// Implementa las operaciones necesarias para establecer las cabeceras personalizadas requeridas por el servicio Aspen.
    /// </summary>
    /// <seealso cref="ICustomHeaderManager" />
    public class CustomHeaderManager : ICustomHeaderManager
    {
        /// <summary>
        /// Agrega la cabecera que identifica la aplicación solicitante.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="value">Valor de la cabecera.</param>
        public virtual void AddApiKeyHeader(IRestRequest request, string value)
        {
            Throw.IfNull(request, nameof(request));
            Throw.IfNullOrEmpty(value, nameof(value));
            request.AddHeader("X-PRO-Auth-App", value);

        }

        /// <summary>
        /// Agrega la cabecera que identifica la aplicación solicitante.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="identity">Instancia de <see cref="IIdentityProvider" /> que proporciona la información de autenticación.</param>
        public virtual void AddApiKeyHeader(IRestRequest request, IIdentityProvider identity)
        {
            Throw.IfNull(request, nameof(request));
            Throw.IfNull(identity, nameof(identity));
            this.AddApiKeyHeader(request, identity.ApiKey);
        }

        /// <summary>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="value">Cadena con el Payload para agregar.</param>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        public virtual void AddPayloadHeader(IRestRequest request, string value)
        {
            Throw.IfNull(request, nameof(request));
            Throw.IfNullOrEmpty(value, nameof(value));
            request.AddHeader("X-PRO-Auth-Payload", value);
        }

        /// <summary>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="encoder">The encoder.</param>
        /// <param name="payload">Diccionario que contiene los datos del payload.</param>
        /// <param name="identity">Instancia de <see cref="IIdentityProvider" /> que proporciona la información de autenticación.</param>
        public virtual void AddPayloadHeader(IRestRequest request, IJwtEncoder encoder, IDictionary<string, object> payload, IIdentityProvider identity)
        {
            Throw.IfNull(request, nameof(request));
            Throw.IfNull(encoder, nameof(encoder));
            Throw.IfNull(payload, nameof(payload));
            Throw.IfNull(identity, nameof(identity));
            this.AddPayloadHeader(request, encoder.Encode(payload, identity.ApiSecret));
        }
    }
}