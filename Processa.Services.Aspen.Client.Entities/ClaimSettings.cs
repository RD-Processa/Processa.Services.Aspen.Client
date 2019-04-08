// -----------------------------------------------------------------------
// <copyright file="ClaimSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-04-01 05:49 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de un permiso de ejecución aplicado a una operación.
    /// </summary>
    public class ClaimSettings
    {
        /// <summary>
        /// Obtiene o establece el identificador del permiso.
        /// </summary>        
        public string ClaimId { get; set; }

        /// <summary>
        /// Obtiene o establece el evento relacionado con el permiso.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Obtiene o establece el alcance de la aplicación relacionado con el permiso.
        /// </summary>
        public Scope Scope { get; set; }
    }
}
