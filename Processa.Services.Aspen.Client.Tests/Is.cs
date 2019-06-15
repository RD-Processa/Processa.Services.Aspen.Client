// -----------------------------------------------------------------------
// <copyright file="Is.cs" company="Processa">
// Copyright (c)  Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-13 05:14 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System.Net;

    /// <summary>
    /// Extiende la clase base Is.
    /// </summary>
    public class Is : NUnit.Framework.Is
    {
        /// <summary>
        /// Verifica si la información de la excepción corresponde con la esperada.
        /// </summary>
        /// <param name="httpStatusCode">HttpStatus esperado.</param>
        /// <param name="eventId">Identificador del evento esperado.</param>
        /// <param name="message">Mensaje de error esperado.</param>
        /// <returns>Instancia de <see cref="AspenExceptionConstraint"/> que permite llevar a cabo las validaciones.</returns>
        public static AspenExceptionConstraint ExpectedException(
            HttpStatusCode httpStatusCode, 
            string eventId = ".*", 
            string message = ".*")
        {
            return new AspenExceptionConstraint(httpStatusCode, eventId, message);
        }
    }
}