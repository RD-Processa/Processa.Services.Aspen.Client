// -----------------------------------------------------------------------
// <copyright file="TranTypeInfo.cs" company="Processa"> 
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
    public class TranTypeInfo : BaseEntity
    {
        /// <summary>
        /// Obtiene o establece el tipo de transacción.
        /// </summary>
        public string TranType { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo extendido de transacción.
        /// </summary>
        public string ExtendedTranType { get; set; }

        /// <summary>
        /// Obtiene o establece el valor del costo de la transacción.
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// Obtiene o establece el color del fondo para visualizar en presentación.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre corto.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Obtiene o establece la vista de presentación UI.
        /// </summary>
        public string Screen { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de transacción.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Obtiene o establece el patrón de identificador de tipo permitido.
        /// </summary>
        public string AllowedTypeIdPattern { get; set; }
    }
}