// -----------------------------------------------------------------------
// <copyright file="UnsteadyHeaderManager.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 02:54 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using Fluent.Contracts;
    using RestSharp;

    /// <summary>
    /// Implementa un administrador de cabeceras que permite omitir sus valores a conveniencia (para escenarios de pruebas).
    /// </summary>
    /// <seealso cref="CustomHeaderManager" />
    public class UnsteadyHeaderManager : CustomHeaderManager
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly bool skipApiKeyHeader;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly bool skipPayloadHeader;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UnsteadyHeaderManager"/>
        /// </summary>
        /// <param name="skipApiKeyHeader">Cuando es <see langword="true" /> omite agregar la cabecera con el ApiKey.</param>
        /// <param name="skipPayloadHeader">Cuando es <see langword="true" /> omite agregar la cabecera con el Payload.</param>
        public UnsteadyHeaderManager(bool skipApiKeyHeader = false, bool skipPayloadHeader = false)
        {
            this.skipApiKeyHeader = skipApiKeyHeader;
            this.skipPayloadHeader = skipPayloadHeader;
        }

        /// <summary>
        /// Agrega la cabecera que identifica la aplicación solicitante.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="value">Valor de la cabecera.</param>
        public override void AddApiKeyHeader(IRestRequest request, string value)
        {
            if (!this.skipApiKeyHeader)
            {
                base.AddApiKeyHeader(request, value);
            }
        }

        /// <summary>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        /// </summary>
        /// <param name="request">Solicitud a donde se agrega la cabecera.</param>
        /// <param name="value">Cadena con el Payload para agregar.</param>
        /// Agrega la cabecera con el payload requerido por el servicio Aspen.
        public override void AddPayloadHeader(IRestRequest request, string value)
        {
            if (!this.skipPayloadHeader)
            {
                base.AddPayloadHeader(request, value);
            }
        }
    }
}
