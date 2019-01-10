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
    using NUnit.Framework;

    /// <summary>
    /// Implementa pruebas unitarias de la clase <see cref="AspenClient"/>.
    /// </summary>
    [TestFixture]
    public class AspenClientTests
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
        /// Se produce una excepción si no se llama a Authenticate y se intenta invocar alguna operación que requiera autenticación.
        /// </summary>
        [Test]
        public void GivenAFluentClientWhenForgotInvokeAuthenticateThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                                      .RoutingTo(this.autonomousAppInfoProvider)
                                      .WithIdentity(this.autonomousAppInfoProvider);            

            // Forgot to Invoke method Authenticate()
            void NoneAuthenticatedClient() => client.Settings.GetDocTypes();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NoneAuthenticatedClient);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' should not be empty");
        }

        /// <summary>
        /// Se produce una excepción si falta la cabecera peronalizada que identifica el ApiKey.
        /// </summary>
        [Test]
        public void GivenAMissingApiKeyHeaderWhenInvokeAuthenticateThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(new CustomSettings(true))
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider);

            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);
            AssertAspenResponseException(
                exception,
                "15842",
                HttpStatusCode.BadRequest,
                @"Custom header 'X-PRO-Auth-App' is required");
        }

        /// <summary>
        /// Se produce una excepción si falta la cabecera peronalizada que identifica el Payload.
        /// </summary>
        [Test]
        public void GivenAMissingPayloadHeaderWhenInvokeAuthenticateThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(new CustomSettings(false, true))
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider);

            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);
            AssertAspenResponseException(
                exception,
                "15845",
                HttpStatusCode.BadRequest,
                @"Custom header 'X-PRO-Auth-Payload' is required");
        }

        /// <summary>
        /// Se produce una excepción si no se reconoce el ApiKey.
        /// </summary>
        [Test]
        public void GivenAnInvalidApiKeyWhenInvokeAuthenticateThenAnExceptionIsThrows()
        {
            string randomApiKey = Guid.NewGuid().ToString("D");
            IFluentClient client = AspenClient.Initialize()
                                      .RoutingTo(this.autonomousAppInfoProvider)
                                      .WithIdentity(randomApiKey, this.autonomousAppInfoProvider.ApiSecret);

            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);
            AssertAspenResponseException(
                exception,
                "20005",
                HttpStatusCode.BadRequest,
                @"Invalid AppKey identifier for custom header 'X-PRO-Auth-App'");
        }

        /// <summary>
        /// Se produce una excepción si se reconoce el ApiKey, pero ApiSecret no es el asignado.
        /// </summary>
        [Test]
        public void GivenAnInvalidApiSecretWhenInvokeAuthenticateThenAnExceptionIsThrows()
        {
            string randomApiSecret = Guid.NewGuid().ToString("P");
            IFluentClient client = AspenClient.Initialize()
                                      .RoutingTo(this.autonomousAppInfoProvider)
                                      .WithIdentity(this.autonomousAppInfoProvider.ApiKey, randomApiSecret);

            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);
            AssertAspenResponseException(
                exception,
                "20007",
                HttpStatusCode.BadRequest,
                @"Content for custom header 'X-PRO-Auth-Payload' is not valid");
        }

        /// <summary>
        /// Una identidad reconocida obtiene un token de autenticación.
        /// </summary>
        [Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenAnAuthTokenIsReturned()
        {
            IFluentClient client = AspenClient.Initialize()
                                       .RoutingTo(this.autonomousAppInfoProvider)
                                       .WithIdentity(this.autonomousAppInfoProvider);

            var response = client.Authenticate();
            Assert.IsNotNull(response.AuthToken);
        }

        /// <summary>
        /// Se obtienen la lista de tipos de identificacíon, si la app se puede autenticar.
        /// </summary>
        [Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenGetDocTypesWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var docTypes = client.Settings.GetDocTypes();
            CollectionAssert.IsNotEmpty(docTypes);
        }

        /// <summary>
        /// Una identidad reconocida obtiene un token de autenticación utilizando un llamado en cadena de los métdos Authenticate y GetClient.
        /// </summary>
        [Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenGetClientWorks()
        {            
            IFluentClient client = AspenClient.Initialize()
                                      .RoutingTo(this.autonomousAppInfoProvider)
                                      .WithIdentity(this.autonomousAppInfoProvider)
                                      .Authenticate()
                                      .GetClient();
            
            Assert.IsNotNull(client.AuthToken);
        }

        /// <summary>
        /// Se ejecuta justo antes de llamar a cada método de prueba.
        /// </summary>
        [SetUp]
        public void Init()
        {
            CacheStore.Reset();
        }

        /// <summary>
        /// Se produce una excepción si el scope del ApiKey no corresponde con el esperado por Aspen.
        /// </summary>
        [Test]
        public void GivenARecognizedAutonomousIdentityWhenInvokeAuthenticateForDelegatedScopeThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider);

            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);
            AssertAspenResponseException(
                exception,
                "1000478",
                HttpStatusCode.Forbidden,
                @"Invalid App Scope for this action");
        }

        /// <summary>
        /// Se produce una excepción si no se envían las credenciales del usuario al autenticar una aplicación delegada.
        /// </summary>
        [Test]
        public void GivenARecognizedDelegatedIdentityWhenInvokeAuthenticateWithMissingValuesThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider);

            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' should not be empty");
        }

        /// <summary>
        /// Se emite un token de autenticación de usuarios cuando las credenciales corresponden a unas conocidas.
        /// </summary>
        [Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenAnUserAuthTokenIsGenerated()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            Assert.That(client.AuthToken, Is.TypeOf<UserAuthToken>());
        }

        /// <summary>
        /// Se requiere un valor para PinNumber al establecer pin de usuario.
        /// </summary>
        [Test]
        public void GivenANullPinNumberWhenInvokeSetUserPinThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider);

            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            client.Authenticate(userInfo);
            string randomCode = TestContext.CurrentContext.Random.GetString(6, "0123456789");
            void BadRequest() => ((AspenClient)client.CurrentUser).SetUserPinAvoidingValidation(null, randomCode);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(BadRequest);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"PinNumber");
        }

        /// <summary>
        /// PinNumber debe cumplir con las políticas de Pin (TwinsPolicy).
        /// </summary>
        [Test]
        public void GivenAnInvalidPinNumberWhenInvokeSetUserPinThenATwinsPolicyExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider);

            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            client.Authenticate(userInfo);
            const string InvalidPinNumber = "111111";
            void NotAcceptable() => ((AspenClient)client.CurrentUser).SetUserPinAvoidingValidation(InvalidPinNumber, "123456");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NotAcceptable);
            AssertAspenResponseException(
                exception,
                "15860",
                HttpStatusCode.NotAcceptable,
                @"Por favor utilice caracteres que no sean iguales");
        }

        /// <summary>
        /// PinNumber debe cumplir con las políticas de Pin (ConsecutivePolicy)
        /// </summary>
        [Test]
        public void GivenAnInvalidPinNumberWhenInvokeSetUserPinThenAConsecutivePolicyExceptionIsThrows()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();
           
            const string InvalidPinNumber = "123456";
            void NotAcceptable() => ((AspenClient)client.CurrentUser).SetUserPinAvoidingValidation(InvalidPinNumber, "123456");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NotAcceptable);
            AssertAspenResponseException(
                exception,
                "15860",
                HttpStatusCode.NotAcceptable,
                @"Por favor utilice caracteres que no sean consecutivos");
        }

        /// <summary>
        /// PinNumber debe cumplir con las políticas de Pin (LengthPolicy)
        /// </summary>
        [Test]
        public void GivenATooLongPinNumberWhenInvokeSetUserPinThenALengthPolicyExceptionIsThrows()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            string tooLongPinNumber = TestContext.CurrentContext.Random.GetString(7, "0123456789");
            void NotAcceptable() => ((AspenClient)client.CurrentUser).SetUserPinAvoidingValidation(tooLongPinNumber, "123456");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NotAcceptable);
            AssertAspenResponseException(
                exception,
                "15860",
                HttpStatusCode.NotAcceptable,
                @"Pin es muy largo o muy corto");
        }

        /// <summary>
        /// Se requiere un valor para ActivationCode al establecer pin de usuario.
        /// </summary>
        [Test]
        public void GivenANullActivationCodeWhenInvokeSetUserPinThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            string randomPinNumber = TestContext.CurrentContext.Random.GetString(6, "0123456789");
            void SetUserPin() => ((AspenClient)client.CurrentUser).SetUserPinAvoidingValidation(randomPinNumber, null);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(SetUserPin);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"ActivationCode");
        }

        /// <summary>
        /// Se produce una excepción si el servicio Aspen no está configurado para el envío de mensajes SMS.
        /// </summary>
        [Test]
        public void GivenANonWorkingServiceWhenInvokeRequestActivationCodeThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(new DelegatedAppSettings())
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();
            
            void ServiceUnavailable() => client.CurrentUser.RequestActivacionCode();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(ServiceUnavailable);
            AssertAspenResponseException(
                exception, 
                "20100", 
                HttpStatusCode.ServiceUnavailable, 
                @"No fue posible enviar su código de activación");
        }

        /// <summary>
        /// Se produce una excepción al validar un código de activacion que no está registrado.
        /// </summary>
        [Test]
        public void GivenARandomActivationCodeWhenInvokeValidateCodeThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string randomCode = TestContext.CurrentContext.Random.GetString(6, "0123456789");
            string randomNickname = $"CC|{randomCode}";
            void ValidationFailed() => client.Management.ValidateActivationCode(randomCode, randomNickname);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(ValidationFailed);
            AssertAspenResponseException(
                exception,
                "15868",
                HttpStatusCode.ExpectationFailed,
                "No se encontró una invitación con los valores de búsqueda proporcionados");
        }

        /// <summary>
        /// Códigos de activacion reconocidos funcionan.
        /// </summary>
        /// <param name="nickname">Nickname y código de activación reconocido.</param>
        /// <remarks>Estos códigos se crearon previamente en la bd de activaciones.</remarks>
        [Test]
        public void GivenAReconigzedActivationCodeWhenInvokeValidateCodeThenItWorks(
            [Values("CC|000001", "CC|000002")] string nickname)
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string code = nickname.Split('|')[1];
            void ValidationWorks() => client.Management.ValidateActivationCode(code, nickname);
            Assert.DoesNotThrow(ValidationWorks);
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
            StringAssert.AreEqualIgnoringCase(expectedEventId, exception.EventId);
            StringAssert.IsMatch(Regex.Escape(expectedMessagePattern), exception.Message);
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
    }
}