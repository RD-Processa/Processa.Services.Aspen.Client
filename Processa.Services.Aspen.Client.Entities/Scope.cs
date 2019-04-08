// -----------------------------------------------------------------------
// <copyright file="Scope.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-04-01 05:49 PM</date>
// ----------------------------------------------------------------------

namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Define el acceso permitido para una aplicación.
    /// </summary>    
    public enum Scope
    {
        /// <summary>
        /// La aplicación puede solicitar datos de cualquier tarjetahabiente.
        /// </summary>
        Autonomous = 0,

        /// <summary>
        /// La aplicación solo puede solicitar datos del tarjetahabiente logueado (actualmente).
        /// </summary>
        Delegated = 1,

        /// <summary>
        /// El acceso está permitido para cualquier tipo de aplicación.
        /// </summary>
        Any
    }
}