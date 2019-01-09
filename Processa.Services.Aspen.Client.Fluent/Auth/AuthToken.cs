// -----------------------------------------------------------------------
// <copyright file="AuthToken.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:31 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Auth
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Serialization;

    /// <summary>
    /// Representa la información de un token de autenticación emitido por el servicio Aspen.
    /// </summary>
    /// <seealso cref="IAuthToken" />
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class AuthToken : IAuthToken
    {
        /// <summary>
        /// Obtiene o establece una cadena de texto que representa el token de autenticación
        /// </summary>
        [JsonProperty("jti")]
        public string Token { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora local en la que se expira el token de autenticación
        /// </summary>
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        [JsonProperty("exp")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora local en la que se emitió el token de autenticación.
        /// </summary>
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        [JsonProperty("iat")]
        public DateTime IssueAt { get; set; }

        /// <summary>
        /// Obtiene un valor que permite determinar si el token ya expiro.
        /// </summary>
        [JsonIgnore]
        public bool Expired => DateTime.Now >= this.ExpiresAt;

        /// <summary>
        /// Obtiene el texto que se muestra en el depurador para la clase.
        /// </summary>
        private string DebuggerDisplay => $"IssueAt:{this.IssueAt:R} ExpiresAt:{this.ExpiresAt:R}";
    }
}