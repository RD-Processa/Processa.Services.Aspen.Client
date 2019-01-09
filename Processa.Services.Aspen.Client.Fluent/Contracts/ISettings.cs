// -----------------------------------------------------------------------
// <copyright file="ISettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 09:10 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    using System.Net;
    using JWT;
    using Providers;

    /// <summary>
    /// Define los valores de configuración que se utilizarán al crear una instancia de <see cref="AspenClient"/>.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para generar los valores Nonce o <see langword="null" /> para utilizar el valor predeterminado.
        /// </summary>
        INonceGenerator NonceGenerator { get; }

        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para generar los valores Epoch o <see langword="null" /> para utilizar el valor predeterminado..
        /// </summary>
        IEpochGenerator EpochGenerator { get; }

        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para serializar en formato Json la información que se envía al servicio Aspen o <see langword="null" /> para utilizar el valor predeterminado.
        /// </summary>
        IJsonSerializer JsonSerializer { get; }

        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para agregar las cabeceras personalizadas requeridas por el servicio Aspen o <see langword="null" /> para utilizar el valor predeterminado.
        /// </summary>
        ICustomHeaderManager CustomHeaderManager { get; }

        /// <summary>
        /// Obtiene el alcance de la aplicación asociada con los valores de identidad <see cref="IIdentityProvider"/> con los que se solicita la información.
        /// </summary>
        AppScope AppScope { get; }

        /// <summary>
        /// Obtiene la información del servidor Proxy que se utiliza para la conexión a Internet o <c>null</c> para no utilizar ninguno.
        /// </summary>
        IWebProxy Proxy { get; }

        /// <summary>
        /// Obtiene el tiempo de espera en milisegundos que se utilizará para la solicitud.
        /// </summary>
        int Timeout { get; }

        /// <summary>
        /// Obtiene la información del dispositivo desde donde se envía la solicitud.
        /// </summary>
        IDeviceInfo DeviceInfo { get; }
    }
}