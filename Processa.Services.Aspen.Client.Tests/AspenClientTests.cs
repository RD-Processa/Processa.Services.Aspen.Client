// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 08:49 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;
    using System.Net;
    using System.Security;
    using System.Text.RegularExpressions;
    using CredentialManagement;
    using Fluent;
    using Fluent.Auth;
    using Fluent.Internals;
    using Fluent.Providers;
    using Newtonsoft.Json;
    using NUnit.Framework;

    /// <summary>
    /// Implementa pruebas unitarias de la clase <see cref="AspenClient"/>.
    /// </summary>
    [TestFixture]
    public partial class AspenClientTests
    {
        /// <summary>
        /// Para uso interno. Proporciona la información de conexión para una aplicación autónoma.
        /// </summary>
        private readonly JsonFileProvider autonomousAppInfoProvider = new JsonFileProvider("AppSettingsAutonomous.json");

        /// <summary>
        /// Para uso interno. Proporciona la información de conexión para una aplicación delegada.
        /// </summary>
        private readonly JsonFileProvider delegatedAppInfoProvider = new JsonFileProvider("AppSettingsDelegated.json");

        /// <summary>
        /// Para uso interno. Tiempo de respuesta máximo para una solicitud.
        /// </summary>
        private const int MaximumResponseTime = 2000;

        /// <summary>
        /// Se ejecuta justo antes de llamar a cada método de prueba.
        /// </summary>
        [SetUp]
        public void Init()
        {
            CacheStore.Reset();
        }

        /// <summary>
        /// Lanza una excepción de aserción en caso de que no se cumplan las condiciones.
        /// </summary>
        /// <param name="exception">Isntancia de la excepción que se está evaluando.</param>
        /// <param name="expectedEventId">Identificador del evento que se espera en la excepción.</param>
        /// <param name="expectedStatusCode">HttpStatusCode que se espera en la excepción.</param>
        /// <param name="expectedMessagePattern">Texto o parte del texto, que se espera encontrar en la excepción.</param>
        private static void AssertAspenResponseException(
            AspenResponseException exception, 
            string expectedEventId, 
            HttpStatusCode expectedStatusCode, 
            string expectedMessagePattern)
        {
            Assert.That(exception.StatusCode, Is.EqualTo(expectedStatusCode));
            StringAssert.AreEqualIgnoringCase(expectedEventId ?? exception.EventId, exception.EventId);
            StringAssert.IsMatch(Regex.Escape(expectedMessagePattern ?? exception.Message), exception.Message);
        }

        /// <summary>
        /// Obtiene las credenciales de un usuario en una aplicación delegada.
        /// </summary>
        /// <returns>isntancia de <see cref="DelegatedUserInfo"/> con la información de auteticación del usuario.</returns>
        /// <exception cref="SecurityException">Missing credentials for Aspen:DelegatedUser. See cmdkey utility http://bit.ly/cmdkeyutil</exception>
        private static DelegatedUserInfo GetDelegatedUserCredentials()
        {
            Credential credential = new Credential { Target = "Aspen:DelegatedUser" };
            if (!credential.Exists())
            {
                throw new SecurityException("Missing credentials for Aspen:DelegatedUser. See cmdkey utility http://bit.ly/cmdkeyutil");
            }

            credential.Load();
            const char Separator = ';';
            string docType = credential.Username.Split(Separator)[0];
            string docNumber = credential.Username.Split(Separator)[1];
            return new DelegatedUserInfo(docType, docNumber, credential.Password);
        }

        private static void PrintOutput(string message, params object[] @object)
        {
            Console.WriteLine(PadBoth(message, 50));

            if (@object == null)
            {
                return;
            }

            foreach (object value in @object)
            {
                if (value != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(value, Formatting.Indented));
                }
            }
        }

        private static string PadBoth(string text, int length, char paddingChar = '=')
        {
            string header = $" {(text ?? string.Empty).ToUpperInvariant()} ";
            int spaces = length - header.Length;
            int padLeft = spaces / 2 + header.Length;
            return header.PadLeft(padLeft, paddingChar).PadRight(length, paddingChar);

        }
    }
}