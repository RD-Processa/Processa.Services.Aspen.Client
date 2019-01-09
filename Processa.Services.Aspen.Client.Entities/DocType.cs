// -----------------------------------------------------------------------
// <copyright file="DocType.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 03:56 PM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de un tipo de identificación en el sistema.
    /// </summary>
    public class DocType : BaseEntity
    {
        /// <summary>
        /// Obtiene el nombre corto que identifica la entidad en el sistema.
        /// </summary>
        public string ShortName { get; set; }
    }
}