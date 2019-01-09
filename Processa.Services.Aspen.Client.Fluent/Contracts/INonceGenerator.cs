// -----------------------------------------------------------------------
// <copyright file="INonceGenerator.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:08 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    /// <summary>
    /// Define un método para generar un número arbitrario que solo se puede utilizar una vez.
    /// </summary>
    public interface INonceGenerator
    {
        /// <summary>
        /// Obtiene el nombre con el que se agrega esta información a la solicitud.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Obtiene el valor de un número/cadena arbitrario para un único uso.
        /// </summary>
        /// <returns>Cadena con un texto de único uso.</returns>
        string GetNonce();
    }
}