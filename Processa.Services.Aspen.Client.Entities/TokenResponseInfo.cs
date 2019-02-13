// -----------------------------------------------------------------------
// <copyright file="TokenResponseInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-25 04:05 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System;

    /// <summary>
    /// Representa la infromación de un token transaccional.
    /// </summary>
    /// <seealso cref="ITokenResponseInfo" />
    public class TokenResponseInfo : ITokenResponseInfo
    {
        /// <summary>
        /// Obtiene o establece la cadena que representa el token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora en formato UTC en la que expira el token.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Obtiene o establece el código del canal para el que se generó el token.
        /// </summary>
        public string ChannelKey { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del canal para el que se generó el token.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Obtiene o establece la duración en minutos del token transaccional.
        /// </summary>
        public int ExpirationMinutes { get; set; }
    }
}