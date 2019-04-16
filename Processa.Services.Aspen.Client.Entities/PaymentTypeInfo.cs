// -----------------------------------------------------------------------
// <copyright file="PaymentTypeInfo.cs" company="Processa"> 
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
    public class PaymentTypeInfo : BaseEntity
    {
        /// <summary>
        /// Obtiene o establece el identificador padre del elemento.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre abreviado del elemento.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Obtiene o establece el color del fondo para representación del tipo de token en presentación UI.
        /// </summary>
        public string BackgroundColor { get; set; }
    }
}