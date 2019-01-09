// -----------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 03:57 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Sirve como clase base para algunas entidades del sistema.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador univoco de la entidad.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Obtiene o establece el orden en que se debe mostrar la entidad en el sistema.
        /// </summary>        
        public int Order { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre que identifica la entidad en el sistema.
        /// </summary>
        public string Name { get; set; }
    }
}