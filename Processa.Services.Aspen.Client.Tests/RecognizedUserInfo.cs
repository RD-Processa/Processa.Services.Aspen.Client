// -----------------------------------------------------------------------
// <copyright file="RecognizedUserInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-17 10:38 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;

    /// <summary>
    /// Representa la información de un usuario reconocido por el sistema Aspen.
    /// </summary>
    public class RecognizedUserInfo
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private static RecognizedUserInfo current;

        /// <summary>
        /// Obtiene el tipo de documento del usuario reconocido por el sistema Aspen.
        /// </summary>    
        public string DocType { get; private set; }

        /// <summary>
        /// Obtiene el número de documento del usuario reconocido por el sistema Aspen.
        /// </summary>        
        public string DocNumber { get; private set; }

        /// <summary>
        /// Obtiene el número de cuenta asociado con un usuario reconocido por el sistema Aspen.
        /// </summary>
        public string AccountId { get; private set; }

        /// <summary>
        /// Obtiene la información del usuario configurado en el ambiente.
        /// </summary>
        /// <returns>Instancia de <see cref="RecognizedUserInfo"/> a partir de la información en el ambiente.</returns>
        /// <exception cref="InvalidOperationException">Missing environment variable Aspen:RecognizedUserInfo</exception>
        public static RecognizedUserInfo Current()
        {
            if (current != null)
            {
                return current;
            }

            string userInfo = Environment.GetEnvironmentVariable("Aspen:RecognizedUserInfo");
            const char Separator = '|';

            if (string.IsNullOrWhiteSpace(userInfo))
            {
                throw new InvalidOperationException("Missing info for Aspen:RecognizedUserInfo");
            }

            string[] values = userInfo.Split(Separator);

            current = new RecognizedUserInfo()
            {
                DocType = values[0],
                DocNumber = values?[1],
                AccountId = values?[2]
            };

            return current;
        }
    }
}