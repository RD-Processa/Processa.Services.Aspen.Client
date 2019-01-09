// -----------------------------------------------------------------------
// <copyright file="IAuthSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:29 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Providers;

    /// <summary>
    /// Define las operaciones que permiten establecer la identidad de la aplicación solicitante.
    /// </summary>
    public interface IAuthSettings
    {
        /// <summary>
        /// Establece la identidad de la aplicación solicitante.
        /// </summary>
        /// <param name="identityProvider">Identidad de la aplicación solicitante.</param>
        /// <param name="deviceInfo">Información del dispositivo desde donde se reliza la solicitud o  <see langword="null" /> para utilizar el valor predeterminado.</param>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite conectar con el servicio Aspen.</returns>
        IFluentClient WithIdentity(IIdentityProvider identityProvider, IDeviceInfo deviceInfo = null);

        /// <summary>
        /// Establece la identidad de la aplicación solicitante.
        /// </summary>
        /// <param name="apiKey">ApiKey de la aplicación solicitante.</param>
        /// <param name="apiSecret">ApiSecret de la aplicación solicitante.</param>
        /// <param name="deviceInfo">Información del dispositivo desde donde se reliza la solicitud o  <see langword="null" /> para utilizar el valor predeterminado.</param>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite conectar con el servicio Aspen.</returns>
        IFluentClient WithIdentity(string apiKey, string apiSecret, IDeviceInfo deviceInfo = null);
    }
}