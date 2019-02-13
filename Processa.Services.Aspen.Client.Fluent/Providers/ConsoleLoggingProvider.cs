// -----------------------------------------------------------------------
// <copyright file="ConsoleLoggingProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:09 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// implementa un tipo que puede escribir información de seguimiento en la salida estandar (Consola).
    /// </summary>
    /// <seealso cref="ILoggingProvider" />
    public class ConsoleLoggingProvider : ILoggingProvider
    {
        /// <summary>
        /// Escribe información de depuración en la salida estandar.
        /// </summary>
        /// <param name="message">Texto del mensaje de depuración.</param>
        public void WriteDebug(string message)
        {
            Console.WriteLine($@"[DEBUG] {message}");
        }

        /// <summary>
        /// Escribe información de un error en la salida estandar.
        /// </summary>
        /// <param name="exception">Excepción que ocasion+o el error.</param>
        /// <param name="message">Texto del mensaje de error.</param>
        public void WriteError(Exception exception, string message = null)
        {
            Console.WriteLine($@"[ERROR] {message ?? exception?.Message}");

            if (exception != null)
            {
                Console.WriteLine($@"[ERROR] {JsonConvert.SerializeObject(exception, Formatting.Indented)}");
            }
        }

        /// <summary>
        /// Escribe información de un objeto en la salida estandar.
        /// </summary>
        /// <param name="object">Instancia del objeto a escribir.</param>
        /// <param name="message">Texto del mensaje de información.</param>
        public void WriteInfo(string message, object @object)
        {
            StringBuilder sb = new StringBuilder(message).Append(" ");

            if (!(@object is string))
            {
                @object = JsonConvert.SerializeObject(@object, Formatting.Indented);
            }

            sb.Append(@object.ToString());
            Console.WriteLine($@"[INFO] {sb}");
        }
    }
}