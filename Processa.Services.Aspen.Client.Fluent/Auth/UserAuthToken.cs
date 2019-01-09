// -----------------------------------------------------------------------
// <copyright file="UserAuthToken.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:32 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Auth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Representa la información de un token de autenticación emitido para un usuario en una aplicación delegada.
    /// </summary>
    /// <seealso cref="AuthToken" />
    public class UserAuthToken : AuthToken
    {
        /// <summary>
        /// Obtiene o establece el identificador del dispositivo para el que se emitió el token de autenticación.
        /// </summary>
        [JsonProperty("sub")]
        public string DeviceId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre de usuario para el que se emitió el token de autenticación
        /// </summary>
        [JsonProperty("aud")]
        public string Username { get; set; }
    }
}