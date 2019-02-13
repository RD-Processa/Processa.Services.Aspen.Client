// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-16 06:10 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using NUnit.Framework.Internal;

    /// <summary>
    /// Expone métidos de extensión para diferentes tipos.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Obtiene una cadena aleatoria compuesta por el número de dígitos [0-9] especificados.
        /// </summary>
        /// <param name="randomizer">Instancia de <see cref="Randomizer"/> que se está extendiendo.</param>
        /// <param name="outputLength">Longitud deseada de la cadena de salida..</param>
        /// <returns>Una cadena aleatoria con la longitud especificada.</returns>
        public static string GetDigits(this Randomizer randomizer, int outputLength)
        {
            return randomizer.GetString(outputLength, "0123456789");
        }
    }
}