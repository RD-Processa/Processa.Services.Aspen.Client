// -----------------------------------------------------------------------
// <copyright file="UnixEpochGenerator.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-02 04:09 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Contracts
{
    using System;

    /// <summary>
    /// implementa una operación para obtener el número de segundos que han transcurrido desde 1970-01-01T00: 00: 00Z.
    /// </summary>
    /// <seealso cref="Processa.Services.Aspen.Client.Fluent.Contracts.IEpochGenerator" />
    public class UnixEpochGenerator : IEpochGenerator
    {
        /// <summary>
        /// Obtiene el nombre con el que se agrega esta información a la solicitud.
        /// </summary>
        public string Name => "Epoch";

        /// <summary>
        /// Obtiene el número de segundos que han transcurrido desde 1970-01-01T00: 00: 00Z.
        /// </summary>
        /// <returns>Un tiempo de Unix, expresado como el número de segundos que han transcurrido desde 1970-01-01T00: 00: 00Z (1 de enero de 1970, a las 12:00 AM UTC).</returns>
        public double GetSeconds()
        {
            return Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }
    }
}