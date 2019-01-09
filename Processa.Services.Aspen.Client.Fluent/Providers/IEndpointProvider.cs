// -----------------------------------------------------------------------
// <copyright file="IEndpointProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:04 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    /// <summary>
    /// Define la ubicación en Internet de la instancia del servicio Aspen con la que se desea conectar.
    /// </summary>
    public interface IEndpointProvider
    {
        /// <summary>
        /// Obtiene la URL del servicio Aspen a donde se desea conectar.
        /// </summary>
        string Url { get; }
    }
}