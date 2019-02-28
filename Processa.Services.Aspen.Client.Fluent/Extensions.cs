// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-02-26 11:35 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Internals;
    using System.Collections.Generic;

    /// <summary>
    /// Expone métdos de extensión para varios tipos.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Reemplaza valores en una cadena.
        /// </summary>
        /// <param name="template">Cadena donde se reemplazan los valores</param>
        /// <param name="tokens">Colección de tokens para reemplazar.</param>
        /// <returns>Cadena con los tokens reemplazados.</returns>
        public static string ReplaceTokens(this string template, IDictionary<string, string> tokens)
        {
            Throw.IfNullOrEmpty(template, nameof(template));
            PlaceholderFormatter phf = new PlaceholderFormatter(template);
            foreach (KeyValuePair<string, string> pair in tokens)
            {
                Throw.IfNullOrEmpty(pair.Key, "pair.Key");
                Throw.IfNullOrEmpty(pair.Value, "pair.Value");
                phf.Add(pair.Key, pair.Value);
            }

            return phf.ToString();
        }
    }
}