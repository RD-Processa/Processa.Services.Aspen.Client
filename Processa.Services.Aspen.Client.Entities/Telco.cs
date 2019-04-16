// -----------------------------------------------------------------------
// <copyright file="DocType.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>dmontalvo</author>
// <date>2019-04-11 09:39 AM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de un operador de telefonía móvil en el sistema.
    /// </summary>
    public class Telco : BaseEntity
    {
        /// <summary>
        /// Obtiene o establece el ícono del operador.
        /// </summary>
        public string Icon { get; set; }
    }
}