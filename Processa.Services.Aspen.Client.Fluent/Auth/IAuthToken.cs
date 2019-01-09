// -----------------------------------------------------------------------
// <copyright file="IAuthToken.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 08:56 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Auth
{
    using System;

    /// <summary>
    /// Define la información de un token de autenticación emitido por el servicio Aspen.
    /// </summary>
    public interface IAuthToken
    {
        /// <summary>
        /// Obtiene o establece la fecha y hora local en la que se emitió el token de autenticación.
        /// </summary>
        DateTime IssueAt { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha y hora local en la que se expira el token de autenticación
        /// </summary>        
        DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Obtiene o establece una cadena de texto que representa el token de autenticación
        /// </summary>        
        string Token { get; set; }
    }
}