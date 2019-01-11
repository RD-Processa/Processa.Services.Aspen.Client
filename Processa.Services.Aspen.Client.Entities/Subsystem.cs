// -----------------------------------------------------------------------
// <copyright file="Subsystem.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-10 02:05 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Define los subsistemas que proveen información transaccional.
    /// </summary>
    public enum Subsystem
    {
        /// <summary>
        /// El origen de la información es el sistema de administración de tarjetas débito TUP.
        /// </summary>
        Tup = 0,

        /// <summary>
        /// El origen de la información es sistema de administración de cartera BANCOR.
        /// </summary>
        Bancor = 1,

        /// <summary>
        /// No hay un sistema definido.
        /// </summary>
        /// <remarks>
        /// La información se puede utilizar con la finalidad de comprobar el funcionamiento del servicio,
        /// mientras se finalizan los acuerdos comerciales que permitan a los clientes del API, consumir la información real de los sistemas transaccionales.
        /// </remarks>
        None = 2
    }
}