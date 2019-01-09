// -----------------------------------------------------------------------
// <copyright file="IEndPointSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:30 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Net;
    using Providers;

    /// <summary>
    /// Define las operaciones que permiten establecer la URL del servicio Aspen.
    /// </summary>
    public interface IEndPointSettings
    {
        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="endpointProvider">Instancia con la configuración del servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings"/> que permite la configuración de trazas de seguimiento.</returns>
        ILoggingSettings RoutingTo(IEndpointProvider endpointProvider);

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="endpointProvider">Instancia con la configuración del servicio Aspen.</param>
        /// <param name="proxy">Configuración del servidor Proxy que se debe utilizar para conectar con el servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings"/> que permite la configuración de trazas de seguimiento.</returns>
        ILoggingSettings RoutingTo(IEndpointProvider endpointProvider, IWebProxy proxy);

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="url">URL del servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings"/> que permite la configuración de trazas de seguimiento.</returns>
        ILoggingSettings RoutingTo(string url);

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="url">URL del servicio Aspen.</param>
        /// <param name="proxy">Configuración del servidor Proxy que se debe utilizar para conectar con el servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings"/> que permite la configuración de trazas de seguimiento.</returns>
        ILoggingSettings RoutingTo(string url, IWebProxy proxy);
    }
}