// -----------------------------------------------------------------------
// <copyright file="StaticEndpointProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:12 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    using Internals;

    /// <summary>
    /// Representa la ubicación en Internet de la instancia del servicio Aspen con la que se desea conectar.
    /// </summary>
    /// <seealso cref="IEndpointProvider" />
    public class StaticEndpointProvider : IEndpointProvider
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="StaticEndpointProvider"/>
        /// </summary>
        /// <param name="url">URL del servicio Aspen a donde se desea conectar.</param>
        public StaticEndpointProvider(string url)
        {
            Throw.IfNullOrEmpty(url, nameof(url));
            this.Url = url;
        }

        /// <summary>
        /// Obtiene la URL del servicio Aspen a donde se desea conectar.
        /// </summary>
        public string Url { get; }
    }
}