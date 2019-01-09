// -----------------------------------------------------------------------
// <copyright file="AspenClient.ISettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 07:20 PM</date>
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
    public partial class AspenClient : ISettingsModule
    {

        /// <summary>
        /// Obtiene una instancia que pemite acceder a recursos de parametrización del servicio Aspen.
        /// </summary>
        public ISettingsModule Settings => this;

        /// <summary>
        /// Obtiene la lista de tipos de documento soportados por el servicio Aspen.
        /// </summary>
        /// <returns>Lista de tipos de documento soportados.</returns>
        public IList<DocType> GetDocTypes()
        {
            IRestRequest request = new AspenRequest(this, Routes.Resources.DocTypes, Method.GET);
            return this.Execute<List<DocType>>(request);
        }
    }
}