// -----------------------------------------------------------------------
// <copyright file="ITokenResponseInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-25 04:04 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System;

    /// <summary>
    /// Define la información mínima de un token transaccional.
    /// </summary>
    public interface ITokenResponseInfo
    {
        /// <summary>
        /// Obtiene o establece la cadena que representa el token.
        /// </summary>
        string Token { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora en formato UTC en la que expira el token.
        /// </summary>
        DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Obtiene o establece el código del canal para el que se generó el token.
        /// </summary>
        string ChannelKey { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del canal para el que se generó el token.
        /// </summary>        
        string ChannelName { get; set; }

        /// <summary>
        /// Obtiene o establece la duración en minutos del token transaccional.
        /// </summary>
        int ExpirationMinutes { get; set; }
    }
}