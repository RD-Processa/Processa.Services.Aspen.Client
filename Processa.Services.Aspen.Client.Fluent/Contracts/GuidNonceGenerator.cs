// -----------------------------------------------------------------------
// <copyright file="GuidNonceGenerator.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:08 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    using System;

    /// <summary>
    /// Implementa un método para generar un número arbitrario que solo se puede utilizar una vez a partir de la estructura <see cref="Guid"/>.
    /// </summary>
    /// <seealso cref="INonceGenerator" />
    public class GuidNonceGenerator : INonceGenerator
    {
        /// <summary>
        /// Obtiene el nombre con el que se agrega esta información a la solicitud.
        /// </summary>
        public string Name => "Nonce";

        /// <summary>
        /// Obtiene el valor de un número/cadena arbitrario para un único uso.
        /// </summary>
        /// <returns>Cadena con un texto de único uso.</returns>
        public string GetNonce()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}