// -----------------------------------------------------------------------
// <copyright file="ISession.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 02:00 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Auth;

    /// <summary>
    /// Define operaciones y propiedades para acceder a los valores actuales de conexión con el servicio Aspen.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Obtiene el token de autenticación emitido para la sesión.
        /// </summary>
        IAuthToken AuthToken { get; }

        /// <summary>
        /// Obtiene la instancia actual configurada para interactuar con el servicio Aspen.
        /// </summary>
        /// <returns>Instancia de <see cref="IFluentClient"/> que permite interactuar con el servicio Aspen.</returns>
        IFluentClient GetClient();
    }
}