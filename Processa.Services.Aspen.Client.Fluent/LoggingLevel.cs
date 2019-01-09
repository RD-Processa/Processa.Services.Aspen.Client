// -----------------------------------------------------------------------
// <copyright file="LoggingLevel.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:06 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    /// <summary>
    /// Define los niveles para generación de información de trazas de seguimiento.
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        /// Establece que no envíe información de ningún tipo a las trazas de seguimiento.
        /// </summary>
        None = 0,

        /// <summary>
        /// Establece que solo se envíe información de depuración a las trazas de seguimiento.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Establece que solo se envíe información de objetos relevantes a las trazas de seguimiento.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Establece que solo se envíe información de errores a las trazas de seguimiento.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Establece que se envíe información de cualquier tipo a las trazas de seguimiento.
        /// </summary>
        All = 8
    }
}