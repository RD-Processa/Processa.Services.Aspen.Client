// -----------------------------------------------------------------------
// <copyright file="AppScope.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:48 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    /// <summary>
    /// Define el alcance de la aplicación solicitante.
    /// </summary>
    public enum AppScope
    {
        /// <summary>
        /// Establece el alcance como una aplicación autónoma.
        /// </summary>
        Autonomous,

        /// <summary>
        /// Establece el alcance como una aplicación delegada.
        /// </summary>
        Delegated
    }
}