// -----------------------------------------------------------------------
// <copyright file="MenuItem.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>dmontalvo</author>
// <date>2019-04-11 09:39 AM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de un menú para una aplicación móvil.
    /// </summary>
    public class MenuItem : BaseEntity
    {
        /// <summary>
        /// Obtiene o establece el icono que representa de forma gráfica a la opción de menú en interfaz de usuario.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Obtiene o establece la URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Obtiene o establece la vista de la opción en interfaz de usuario.
        /// </summary>
        public string Screen { get; set; }
    }
}