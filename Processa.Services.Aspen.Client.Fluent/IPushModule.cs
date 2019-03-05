// -----------------------------------------------------------------------
// <copyright file="IPushModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-01 09:11 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;

    /// <summary>
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades de información relacionadas con mensajes Push.
    /// </summary>
    public interface IPushModule
    {
        /// <summary>
        /// Obtiene la lista de mensajes Push del usuario actual.
        /// </summary>
        List<PushMessageInfo> GetMessages();
    }
}