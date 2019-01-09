// -----------------------------------------------------------------------
// <copyright file="ILoggingSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:30 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Providers;

    /// <summary>
    /// Define las operaciones que permiten establecer la configuración de trazas de seguimiento.
    /// </summary>
    public interface ILoggingSettings
    {
        /// <summary>
        /// Establece la identidad de la aplicación solicitante.
        /// </summary>
        /// <param name="apiKey">APIKey asignado a la aplicación que se está conectando con el servicio Aspen.</param>
        /// <param name="apiSecret">APISecret asignado a la aplicación que se está conectando con el servicio Aspen.</param>
        /// <param name="deviceInfo">Información del dispositivo desde donde se reliza la solicitud o  <see langword="null" /> para utilizar el valor predeterminado.</param>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite establecer conexión con el servicio Aspen.</returns>
        IFluentClient WithIdentity(string apiKey, string apiSecret, IDeviceInfo deviceInfo = null);

        /// <summary>
        /// Establece la identidad de la aplicación solicitante.
        /// </summary>
        /// <param name="identityProvider">Identidad de la aplicación solicitante.</param>
        /// <param name="deviceInfo">Información del dispositivo desde donde se reliza la solicitud o  <see langword="null" /> para utilizar el valor predeterminado.</param>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite conectar con el servicio Aspen.</returns>
        IFluentClient WithIdentity(IIdentityProvider identityProvider, IDeviceInfo deviceInfo = null);

        /// <summary>
        /// Establece la configuración de trazas de seguimiento.
        /// </summary>
        /// <param name="minimumLoggingLevel">Mínimo nivel requerido para emitir trazas de seguimiento.</param>
        /// <param name="loggingProvider">Instancia de <see cref="ILoggingProvider"/> a donde se escriben las trazas de seguimiento.</param>
        /// <returns>Instancia de <see cref="IAuthSettings"/> que permite establecer la identidad de la aplicación solicitante.</returns>
        IAuthSettings LoggingTo(LoggingLevel minimumLoggingLevel, ILoggingProvider loggingProvider = null);
    }
}