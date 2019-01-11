// -----------------------------------------------------------------------
// <copyright file="DefaultSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 09:13 AM</date>
// ----------------------------------------------------------------------
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    using System;
    using System.Net;
    using JWT;
    using JWT.Serializers;
    using Providers;

    /// <summary>
    /// Representa los valores de configuración predeterminados del sistema, que se utilizarán al crear una instancia de <see cref="AspenClient"/>.
    /// </summary>
    /// <seealso cref="ISettings" />
    public class DefaultSettings : ISettings
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DefaultSettings"/>
        /// </summary>
        public DefaultSettings(AppScope appScope = AppScope.Autonomous)
        {
            this.AppScope = appScope;
            this.NonceGenerator = new GuidNonceGenerator();
            this.EpochGenerator = new UnixEpochGenerator();
            this.JsonSerializer = new JsonNetSerializer();
            this.CustomHeaderManager = new CustomHeaderManager();
            this.DeviceInfo = new DeviceInfo();
            this.Proxy = null;
            this.Timeout = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;
        }

        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para generar los valores Nonce o <see langword="null" /> para utilizar el valor predeterminado.
        /// </summary>
        public INonceGenerator NonceGenerator { get; protected set; }

        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para generar los valores Epoch o <see langword="null" /> para utilizar el valor predeterminado..
        /// </summary>
        public IEpochGenerator EpochGenerator { get; protected set; }
        
        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para serializar en formato Json la información que se envía al servicio Aspen o <see langword="null" /> para utilizar el valor predeterminado.
        /// </summary>
        public IJsonSerializer JsonSerializer { get; protected set; }

        /// <summary>
        /// Obtiene la instancia de la clase que se utiliza para agregar las cabeceras personalizadas requeridas por el servicio Aspen o <see langword="null" /> para utilizar el valor predeterminado.
        /// </summary>
        public ICustomHeaderManager CustomHeaderManager { get; protected set; }

        /// <summary>
        /// Obtiene el alcance de la aplicación asociada con los valores de identidad <see cref="T:Processa.Services.Aspen.Client.Fluent.Providers.IIdentityProvider" /> con los que se solicita la información.
        /// </summary>
        public AppScope AppScope { get; protected set; }

        /// <summary>
        /// Obtiene la información del servidor Proxy que se utiliza para la conexión a Internet o <c>null</c> para no utilizar ninguno.
        /// </summary>
        public IWebProxy Proxy { get; protected set; }
        
        /// <summary>
        /// Obtiene el tiempo de espera en milisegundos que se utilizará para la solicitud.
        /// </summary>
        public int Timeout { get; protected set; }

        /// <summary>
        /// Obtiene la información del dispositivo desde donde se envía la solicitud.
        /// </summary>
        public IDeviceInfo DeviceInfo { get; }
    }
}