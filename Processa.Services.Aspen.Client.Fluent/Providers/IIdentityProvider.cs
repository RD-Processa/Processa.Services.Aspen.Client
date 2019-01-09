// -----------------------------------------------------------------------
// <copyright file="IIdentityProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:05 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    /// <summary>
    /// Define la información que se utiliza para autenticar la solicitud en el servicio Aspen.
    /// </summary>
    /// <remarks>Estos valores son proporcionados por Processa.</remarks>
    public interface IIdentityProvider
    {
        /// <summary>
        /// Obtiene el valor del ApiKey de conexión.
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// Obtiene el valor del ApiSecret de conexión
        /// </summary>
        string ApiSecret { get; }
    }
}