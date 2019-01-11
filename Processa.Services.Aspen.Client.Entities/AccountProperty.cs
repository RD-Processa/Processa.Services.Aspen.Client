// -----------------------------------------------------------------------
// <copyright file="AccountProperty.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-10 02:06 PM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Processa.Services.Aspen.Client.Entities
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Representa la información de un atributo extendido de una cuenta.
    /// </summary>
    public class AccountProperty
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private const string DefaultCultureName = "es-US";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AccountProperty"/>
        /// </summary>
        public AccountProperty()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AccountProperty"/> con los valores especificados.
        /// </summary>
        /// <param name="label">Nombre que se muestra para el atributo extendido.</param>
        /// <param name="key">Identificador interno del atributo extendido.</param>
        /// <param name="value">Valor asociado con el atributo extendido.</param>
        public AccountProperty(string label, string key, string value)
        {
            this.Label = label;
            this.Key = key;
            this.Value = Regex.Replace(CultureInfo.CreateSpecificCulture(DefaultCultureName).TextInfo.ToTitleCase(value.ToLowerInvariant()), @"\s+", " ");
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AccountProperty"/> con los valores especificados.
        /// </summary>
        /// <param name="label">Nombre que se muestra para el atributo extendido.</param>
        /// <param name="key">Identificador interno del atributo extendido.</param>
        /// <param name="value">Valor asociado con el atributo extendido.</param>
        /// <param name="format">El formato del valor.</param>
        public AccountProperty(string label, string key, DateTime value, string format = null)
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(DefaultCultureName);
            string customFormat = format ?? "MMM d 'de' yyyy 'a las' h:mm tt";
            this.Label = label;
            this.Key = key;
            this.Value = value.ToString(customFormat, customCulture);
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AccountProperty"/> con los valores especificados.
        /// </summary>
        /// <param name="label">Nombre que se muestra para el atributo extendido.</param>
        /// <param name="key">Identificador interno del atributo extendido.</param>
        /// <param name="value">Valor asociado con el atributo extendido.</param>
        public AccountProperty(string label, string key, long value)
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(DefaultCultureName);
            customCulture.NumberFormat.CurrencyGroupSeparator = ".";
            this.Label = label;
            this.Key = key;
            this.Value = value.ToString("C0", customCulture);
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AccountProperty" /> con los valores especificados.
        /// </summary>
        /// <param name="label">Nombre que se muestra para el atributo extendido.</param>
        /// <param name="key">Identificador interno del atributo extendido.</param>
        /// <param name="value">Valor asociado con el atributo extendido.</param>
        /// <param name="amount">Valor de la transacción.</param>
        public AccountProperty(string label, string key, string value, decimal amount)
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(DefaultCultureName);
            customCulture.NumberFormat.CurrencyGroupSeparator = ".";
            this.Label = label;
            this.Key = key;
            if (amount > 0)
            {
                this.Value = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLowerInvariant())} por {amount.ToString("C0", customCulture)}";
                return;
            }

            this.Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
        }

        /// <summary>
        /// Obtiene o establece el nombre que se muestra para el atributo extendido.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador interno del atributo extendido.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Obtiene o establece el valor asociado con el atributo extendido.
        /// </summary>
        public string Value { get; set; }
    }
}