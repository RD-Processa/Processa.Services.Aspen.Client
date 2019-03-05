// -----------------------------------------------------------------------
// <copyright file="AspenClientIPush.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-01 02:09 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;
    using Internals;
    using RestSharp;

    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    public partial class AspenClient : IPushModule
    {
        /// <summary>
        /// Obtiene un objeto que permite acceder a las operaciones soportadas por el servicio Aspen para mensajeria push.
        /// </summary>
        public IPushModule Push => this;

        /// <summary>
        /// Obtiene la lista de mensajes Push del usuario actual.
        /// </summary>
        /// <returns>Liata de mensajes push del usuario actual.</returns>
        public List<PushMessageInfo> GetMessages()
        {
            IRestRequest request = new AspenRequest(this, "/push", Method.GET);         
            return this.Execute<List<PushMessageInfo>>(request); 
        }
    }
}