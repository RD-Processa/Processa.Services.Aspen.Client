// -----------------------------------------------------------------------
// <copyright file="CustomSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 02:48 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using Fluent;
    using Fluent.Contracts;

    /// <summary>
    /// Representa una configuración personalizad para esceneraios de pruebas.
    /// </summary>
    /// <seealso cref="DefaultSettings" />
    public class CustomSettings : DefaultSettings
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CustomSettings"/>
        /// </summary>
        /// <param name="bypassApiKeyHeader">Cuando es <see langword="true" /> omite agregar la cabecera con el ApiKey.</param>
        /// <param name="bypassPayloadHeader">Cuando es <see langword="true" /> omite agregar la cabecera con el Payload.</param>
        public CustomSettings(bool bypassApiKeyHeader = false, bool bypassPayloadHeader = false)
        {
            this.CustomHeaderManager = new UnsteadyHeaderManager(bypassApiKeyHeader, bypassPayloadHeader);
        }
    }
}