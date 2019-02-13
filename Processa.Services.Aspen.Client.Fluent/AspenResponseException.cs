// -----------------------------------------------------------------------
// <copyright file="AspenResponseException.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:56 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Se produce cuando el servicio Aspen genera una respuesta de error.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public sealed class AspenResponseException : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AspenResponseException"/>
        /// </summary>
        /// <param name="response">Respuesta generada por el servicio Aspen.</param>
        public AspenResponseException(IRestResponse response) : base(response?.StatusDescription ?? response?.ErrorMessage ?? "General Exception", response?.ErrorException)
        {
            if (response == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                this.Content = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            }

            this.StatusCode = response.StatusCode;
            this.StatusDescription = response.StatusDescription ?? string.Empty;
            this.ResponseStatus = response.ResponseStatus;
            this.Message = this.StatusDescription;
            this.HelpLink = GetResponseHeader(response, "X-PRO-Response-Help");
            this.Data["Uri"] = response.ResponseUri?.ToString();
            this.Data["ResponseTime"] = GetResponseHeader(response, "X-PRO-Response-Time");
            this.Data["ResponseContentType"] = response.ContentType;
            Match match = Regex.Match(response.StatusDescription ?? string.Empty, @".*EventId\:?\s+\((?<EventId>\d+)\).*");
            {
                if (match.Success)
                {
                    this.EventId = match.Groups["EventId"].Value;
                }
            }

            if (!response.IsSuccessful && !string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                this.Message = response.ErrorMessage;
            }
        }

        /// <summary>
        /// Obtiene el identificador del evento emitido para el error por el servicio Aspen.
        /// </summary>        
        public string EventId { get; private set; }

        /// <summary>
        /// Obtiene o establece un enlace al archivo de ayuda en línea asociado con esta excepción.
        /// </summary>
        public override string HelpLink { get; set; }

        /// <summary>
        /// Obtiene un mensaje que describe la excepción actual.
        /// </summary>
        public override string Message { get; }

        /// <summary>
        /// Obtiene el estado de la solicitud devuelto por el servicio Aspen o Error por errores de transporte. Los errores HTTP seguirán devolviendo ResponseStatus.Completed, verifique StatusCode en su lugar.
        /// </summary>
        public ResponseStatus ResponseStatus { get; private set; }

        /// <summary>
        /// Obtiene el código de estado de respuesta HTTP devuelto por el servicio Aspen.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
        
        /// <summary>
        /// Obtiene la descripción del estado HTTP devuelto por el servicio Aspen.
        /// </summary>
        public string StatusDescription { get; private set; }

        /// <summary>
        /// Obtiene la información de los datos incluidos en la respuesta generada por el servicio Aspen.
        /// </summary>
        public Dictionary<string, object> Content { get; private set; }

        /// <summary>
        /// Obtiene el valor de una cabecera de la respuesta.
        /// </summary>
        /// <param name="response">Instancia con la información de la respuesta.</param>
        /// <param name="name">Nombre de la cabecera a buscar.</param>
        /// <returns>Valor de la cabecera si se encuentra o <see langword="null" /> si no se encuentra la cabecera en <paramref name="name"/>.</returns>
        private static string GetResponseHeader(IRestResponse response, string name)
        {
            return response
                   .Headers
                   .SingleOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        }
    }
}