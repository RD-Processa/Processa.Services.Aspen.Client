// -----------------------------------------------------------------------
// <copyright file="StaticIdentityProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:11 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    using Internals;

    /// <summary>
    /// Representa la información que se utiliza para autenticar la solicitud en el servicio Aspen.
    /// </summary>
    /// <seealso cref="IIdentityProvider" />
    public class StaticIdentityProvider : IIdentityProvider
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="StaticIdentityProvider"/>
        /// </summary>
        /// <param name="apiKey">Valor del ApiKey de conexión.</param>
        /// <param name="apiSecret">Valor del ApiSecret de conexión.</param>
        public StaticIdentityProvider(string apiKey, string apiSecret)
        {
            Throw.IfNullOrEmpty(apiKey, nameof(apiKey));
            Throw.IfNullOrEmpty(apiSecret, nameof(apiSecret));
            this.ApiKey = apiKey;
            this.ApiSecret = apiSecret;
        }

        /// <summary>
        /// Obtiene el valor del ApiKey de conexión.
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// Obtiene el valor del ApiSecret de conexión
        /// </summary>
        public string ApiSecret { get; }
    }
}