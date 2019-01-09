// -----------------------------------------------------------------------
// <copyright file="DelegatedUserInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 05:50 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Auth
{
    using System;
    using System.Collections.Generic;
    using Internals;

    /// <summary>
    /// Reprsenta la información de autenticación de un usuario en una aplicación delegada.
    /// </summary>
    public class DelegatedUserInfo : Dictionary<string, object>
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DelegatedUserInfo"/>
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario que inicia sesión.</param>
        /// <param name="docNumber">Número de documento del usuario que inicia sesión.</param>
        /// <param name="password">Clave de acceso del usuario que inicia sesión.</param>
        /// <param name="deviceId">Identificador del dispositivo desde el que el usuario que inicia sesión.</param>
        public DelegatedUserInfo(string docType, string docNumber, string password, string deviceId = null)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));
            Throw.IfNullOrEmpty(password, nameof(password));

            this.Add("DocType", docType);
            this.Add("DocNumber", docNumber);
            this.Add("Password", password);
            this.Add("DeviceId", deviceId ?? Environment.MachineName);
        }
    }
}