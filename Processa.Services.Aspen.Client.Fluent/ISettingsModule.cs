// -----------------------------------------------------------------------
// <copyright file="ISettingsModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 02:32 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;

    /// <summary>
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades de información relacionadas con parametrización del sistema.
    /// </summary>
    public interface ISettingsModule
    {
        /// <summary>
        /// Obtiene la lista de tipos de documento soportados por el servicio Aspen.
        /// </summary>
        /// <returns>Lista de tipos de documento soportados.</returns>
        IList<DocType> GetDocTypes();

        /// <summary>
        /// Obtiene la lista de claims habilitados en el sistema
        /// </summary>
        /// <returns>Lista de claims habilitados.</returns>
        IList<ClaimSettings> GetClaims();
    }
}