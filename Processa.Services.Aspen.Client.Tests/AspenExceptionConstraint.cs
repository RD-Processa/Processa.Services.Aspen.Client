// -----------------------------------------------------------------------
// <copyright file="AspenExceptionConstraint.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-13 04:28 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System.Net;
    using Fluent;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    /// <summary>
    /// Class AspenExceptionConstraint.
    /// </summary>
    public class AspenExceptionConstraint : Constraint
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly string eventId;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly HttpStatusCode httpStatusCode;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly string message;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AspenExceptionConstraint"/>
        /// </summary>
        /// <param name="httpStatusCode">HttpStatus esperado.</param>
        /// <param name="eventId">Identificador del evento esperado.</param>
        /// <param name="message">Mensaje de error esperado.</param>
        public AspenExceptionConstraint(HttpStatusCode httpStatusCode, string eventId, string message)
        {
            this.eventId = eventId;
            this.httpStatusCode = httpStatusCode;
            this.message = message;

        }

        /// <summary>
        /// Aplica las restricciones, devolviendo un ConstraintResult.
        /// </summary>
        /// <typeparam name="TActual">Tipo del valor que se está comprobando.</typeparam>
        /// <param name="actual">Valor para comprobar.</param>
        /// <returns>Instancia de <see cref="ConstraintResult" /> con los resultados de las validaciones.</returns>
        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            AspenException aspenException = actual as AspenException;
            Assert.That(aspenException?.StatusCode, NUnit.Framework.Is.EqualTo(this.httpStatusCode));
            Assert.That(aspenException?.EventId, NUnit.Framework.Is.Not.Null.And.Match(this.eventId));
            Assert.That(aspenException?.Message, NUnit.Framework.Is.Not.Null.And.Match(this.message));
            return new ConstraintResult(this, aspenException, ConstraintStatus.Success);
        }
    }
}