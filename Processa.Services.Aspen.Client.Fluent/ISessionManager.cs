// -----------------------------------------------------------------------
// <copyright file="ISessionManager.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:07 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;

    /// <summary>
    /// Define operaciones para establecer una conexión con el servicio Aspen.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Envía una solicitud de autenticación al servicio Aspen.
        /// </summary>
        /// <param name="customPayload">Información adicional para agregar en el Payload de la solicitud.</param>
        /// <param name="cacheIfAvailable">Cuando es <see langword="true" /> se utiliza el último token de autenticación generado en la sesión.</param>
        /// <returns>Instancia de <see cref="ISession" /> que se puede utilizar para solicitar más recursos de información al servicio Aspen.</returns>
        ISession Authenticate(IDictionary<string, object> customPayload = null, bool cacheIfAvailable = true);
    }
}