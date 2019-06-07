// -----------------------------------------------------------------------
// <copyright file="ChannelInfo.cs" company="Processa"> 
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
    public class ChannelInfo : BaseEntity
    {
        /// <summary>
        /// Obtiene o establece el código que identifica el canal.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que identifica el color de fondo en que se muestran los canales en una UI.
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Obtiene o establece la descripción asociada con el canal.
        /// </summary>
        public string Description { get; set; }
    }
}