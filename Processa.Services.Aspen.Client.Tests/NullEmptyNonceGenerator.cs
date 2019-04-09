// -----------------------------------------------------------------------
// <copyright file="NullEmptyNonceGenerator.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>dmontalvo</author>
// <date>2019-03-15 03:43 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using Fluent.Contracts;

    public class NullEmptyNonceGenerator : INonceGenerator
    {
        private readonly string nonce;

        public NullEmptyNonceGenerator(string nonce = null)
        {
            this.nonce = nonce;
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