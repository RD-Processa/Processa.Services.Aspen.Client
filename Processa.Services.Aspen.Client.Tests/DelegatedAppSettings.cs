// -----------------------------------------------------------------------
// <copyright file="DelegatedAppSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 05:07 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using Fluent;
    using Fluent.Contracts;

    /// <summary>
    /// Representa la información de configuración de una aplicación delegada.
    /// </summary>
    /// <seealso cref="DefaultSettings" />
    public class DelegatedAppSettings : DefaultSettings
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DelegatedAppSettings"/>
        /// </summary>
        public DelegatedAppSettings()
        {
            this.AppScope = AppScope.Delegated;
        }
    }
}