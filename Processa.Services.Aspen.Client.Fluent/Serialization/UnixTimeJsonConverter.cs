// -----------------------------------------------------------------------
// <copyright file="UnixTimeJsonConverter.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:31 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Serialization
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementa la conversión de tipos <see cref="DateTime"/> a partir de la respuesta del servicio Aspen.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    internal class UnixTimeJsonConverter : JsonConverter
    {
        /// <summary>
        /// Determina si esta instancia puede convertir el tipo de objeto especificado.
        /// </summary>
        /// <param name="objectType">Tipo del objeto a convertir.</param>
        /// <returns><c>true</c> si esta instancia puede convertir el tipo de objeto especificado; de lo contrario, <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        /// <summary>
        /// Lee la representación JSON del objeto.
        /// </summary>
        /// <param name="reader">Instancia de <see cref="T:Newtonsoft.Json.JsonReader" /> desde donde se lee el valor.</param>
        /// <param name="objectType">Tipo del objeto a convertir.</param>
        /// <param name="existingValue">El valor del objeto que se está leyendo.</param>
        /// <param name="serializer">Instanicia del serializador que realiza la llamada.</param>
        /// <returns>El valor del objeto convertido.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(reader.Value)).DateTime.ToLocalTime();
        }

        /// <summary>
        /// Escribe la representación JSON del objeto.
        /// </summary>
        /// <param name="writer">Instancia de <see cref="T:Newtonsoft.Json.JsonWriter" /> a donde se escribe.</param>
        /// <param name="value">valor a convertir.</param>
        /// <param name="serializer">Instanicia del serializador que realiza la llamada.</param>        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}