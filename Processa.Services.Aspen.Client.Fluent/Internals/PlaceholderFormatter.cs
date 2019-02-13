// -----------------------------------------------------------------------
// <copyright file="PlaceholderFormatter.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-18 02:56 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Internals
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Expone operaciones para el reemplazo de marcadores de posición nombrados en un <see cref="string"/>.
    /// </summary>
    public class PlaceholderFormatter
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly string format;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly Dictionary<string, object> placeholders;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="PlaceholderFormatter"/>
        /// </summary>
        /// <param name="format">Cadena de formato con los marcadores nombrados para reemplazar.</param>
        public PlaceholderFormatter(string format)
        {
            Throw.IfNullOrEmpty(format, nameof(format));
            this.format = format;
            this.placeholders = new Dictionary<string, object>();
        }

        /// <summary>
        /// Agrega el valor del marcador a la lista de valores para reemplazar.
        /// </summary>
        /// <param name="key">Texto del marcador.</param>
        /// <param name="value">Valor del marcador.</param>
        public void Add(string key, object value)
        {
            Throw.IfNullOrEmpty(key, nameof(key));
            Throw.IfNull(value, nameof(value));
            this.placeholders.Add(key, value);
        }

        /// <summary>
        /// Retorna una instancia de <see cref="System.String" /> que representa el valor de la instacia actual.
        /// </summary>
        /// <returns>Un <see cref="System.String" /> que representa esta instancia.</returns>
        public override string ToString()
        {
            return this.placeholders.Aggregate(this.format, (current, parameter) => current.Replace(parameter.Key, parameter.Value.ToString()));
        }
    }
}