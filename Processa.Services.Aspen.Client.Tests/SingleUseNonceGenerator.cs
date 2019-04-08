// -----------------------------------------------------------------------
// <copyright file="SingleUseNonceGenerator.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-15 03:43 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;
    using Fluent.Contracts;

    public class SingleUseNonceGenerator : INonceGenerator
    {
        private readonly string nonce;

        public SingleUseNonceGenerator(string nonce = null)
        {
            this.nonce = nonce ?? Guid.NewGuid().ToString("B"); ;
        }

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
            return this.nonce;
        }
    }
}