// -----------------------------------------------------------------------
// <copyright file="NullLoggingProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 06:34 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    using System;

    /// <summary>
    /// Implementa un tipo que puede omite la escritura de información de seguimiento.
    /// </summary>
    /// <seealso cref="ILoggingProvider" />
    public class NullLoggingProvider : ILoggingProvider
    {
        /// <summary>
        /// Omite escribir información de depuración.
        /// </summary>
        /// <param name="message">Texto del mensaje de depuración.</param>
        public void WriteDebug(string message)
        {
        }

        /// <summary>
        /// mite escribir información de un objeto.
        /// </summary>
        /// <param name="object">Instancia del objeto a escribir.</param>
        /// <param name="message">Texto del mensaje de información.</param>
        public void WriteInfo(object @object, string message = null)
        {
        }

        /// <summary>
        /// mite escribir información de un error.
        /// </summary>
        /// <param name="exception">Excepción que ocasion+o el error.</param>
        /// <param name="message">Texto del mensaje de error.</param>
        public void WriteError(Exception exception, string message = null)
        {
        }
    }
}