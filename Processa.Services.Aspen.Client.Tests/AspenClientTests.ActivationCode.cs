// -----------------------------------------------------------------------
// <copyright file="AspenClientTestsActivationCode.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-14 10:01 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;
    using System.Net;
    using System.ServiceProcess;
    using Fluent;
    using Fluent.Auth;
    using NUnit.Framework;

    public partial class AspenClientTests
    {
        /// <summary>
        /// Un código reconocido por el sistema funciona.
        /// </summary>
        [Test, ActivationCode, Author("dmontalvo")]
        public void ARecognizedCodeWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient userClient = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            var randomNickname = TestContext.CurrentContext.Random.GetString(10);
            var response = userClient.CurrentUser.RequestActivationCode(randomNickname);

            IFluentClient appClient = AspenClient.Initialize()
                                                       .RoutingTo(this.autonomousAppInfoProvider)
                                                       .WithIdentity(this.autonomousAppInfoProvider)
                                                       .Authenticate(useCache:false)
                                                       .GetClient();

            // Estas pruebas solo sirven en el ambiente de desarrollo. 
            // En otros ambientes esta operación no retorna el código generado.
            Assert.That(response.Code, NUnit.Framework.Is.Not.Null);
            void CodeValidatedSuccessfully() => appClient.Management.ValidateActivationCode(response.Code, randomNickname);
            Assert.DoesNotThrow(CodeValidatedSuccessfully);
        }

        /// <summary>
        /// Código de activación invalido no funciona.
        /// </summary>
        [Test, ActivationCode, Author("Atorres")]
        public void InvalidCodeDoesNotWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient userClient = AspenClient.Initialize(AppScope.Delegated)
                                                  .RoutingTo(this.delegatedAppInfoProvider)
                                                  .WithIdentity(this.delegatedAppInfoProvider)
                                                  .Authenticate(userCredentials)
                                                  .GetClient();

            IFluentClient appClient = AspenClient.Initialize()
                                                 .RoutingTo(this.autonomousAppInfoProvider)
                                                 .WithIdentity(this.autonomousAppInfoProvider)
                                                 .Authenticate(useCache: false)
                                                 .GetClient();

            // When
            var randomNickname = TestContext.CurrentContext.Random.GetString(10);

            // Esta prueba puede funcionar o no, dependiendo del sistema para envió de SMS.
            // Por eso se hacen aserciones en diferentes escenarios.
            try
            {
                var response = userClient.CurrentUser.RequestActivationCode(randomNickname);
                // Estas pruebas solo sirven en el ambiente de desarrollo. 
                // En otros ambientes esta operación no retorna el código generado.
                Assert.That(response.Code, NUnit.Framework.Is.Not.Null);
                var randomCode = TestContext.CurrentContext.Random.GetDigits(6);
                appClient.Management.ValidateActivationCode(randomCode, randomNickname);

            }
            catch (AspenResponseException exception) when (exception.EventId == "20100")
            {
                Assert.That(exception, Is.ExpectedException(HttpStatusCode.ServiceUnavailable,  message: "No fue posible enviar el mensaje"));
            }
            catch (AspenResponseException exception) when (exception.EventId == "15868")
            {
                Assert.That(exception, Is.ExpectedException(HttpStatusCode.ExpectationFailed, message: "Código de activación o identificador es invalido"));
                Assert.That(exception.Content["currentAttempt"], NUnit.Framework.Is.GreaterThan(0));
                Assert.That(exception.Content["successful"], NUnit.Framework.Is.EqualTo(false));
            }
        }

        /// <summary>
        /// Solicitar un código de activación funciona.
        /// </summary>
        [Test, ActivationCode, Author("Atorres")]
        public void RequestAnActivationCodeWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient userClient = AspenClient.Initialize(AppScope.Delegated)
                                                  .RoutingTo(this.delegatedAppInfoProvider)
                                                  .WithIdentity(this.delegatedAppInfoProvider)
                                                  .Authenticate(userCredentials)
                                                  .GetClient();

            ServiceController sc = new ServiceController("Processa.Services.Aspen.Host.Dev");
            sc.ExecuteCommand(129);

            // When

            // Estas pruebas solo sirven en el ambiente de desarrollo. 
            // En otros ambientes esta operación no retorna el código generado.
            var randomNickname = TestContext.CurrentContext.Random.GetString(10);

            // Esta prueba puede funcionar o no, dependiendo del sistema para envió de SMS.
            // Por eso se hacen aserciones en diferentes escenarios.
            try
            {
                var activationCode = userClient.CurrentUser.RequestActivationCode(randomNickname);
                Assert.That(activationCode.Code, NUnit.Framework.Is.Not.Null);
                Assert.That(activationCode.Successful, NUnit.Framework.Is.EqualTo(true));
                Assert.That(activationCode.TimeLapseMinutes, NUnit.Framework.Is.GreaterThanOrEqualTo(0));
            }
            catch (AspenResponseException exception)
            {
                Assert.That(exception, Is.ExpectedException(HttpStatusCode.ServiceUnavailable, "20100", "No fue posible enviar el mensaje"));
            }
        }

        /// <summary>
        /// Se validan los intentos de políticas al solicitar un código de activación.
        /// </summary>
        [Test, ActivationCode, Author("Atorres")]
        public void PolicyAttemptsIsValidated()
        {
            // Este valor debe corresponder con la política definida para el ApiKey de la solicitud.
            int policyAttempts = 3;

            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient userClient = AspenClient.Initialize(AppScope.Delegated)
                                                  .RoutingTo(this.delegatedAppInfoProvider)
                                                  .WithIdentity(this.delegatedAppInfoProvider)
                                                  .Authenticate(userCredentials)
                                                  .GetClient();

            // When
            string randomNickname = TestContext.CurrentContext.Random.GetString(10);
            
            // Agotar los intentos definidos en la política.
            for (int index = 1; index < policyAttempts; index++)
            {
                userClient.CurrentUser.RequestActivationCode(randomNickname);
            }

            void RequestCodeFailed() =>  userClient.CurrentUser.RequestActivationCode(randomNickname);
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(RequestCodeFailed);

            //Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.ExpectationFailed, "15870", "Solicitud de código de activación inválida"));
            Assert.That(responseException.Content["attempt"], NUnit.Framework.Is.EqualTo(policyAttempts));
            Assert.That(responseException.Content["remainingTimeLapse"], NUnit.Framework.Is.GreaterThan(0));
        }

        [Test, ActivationCode, Author("Atorres")]
        public void XyZ()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient userClient = AspenClient.Initialize(AppScope.Delegated)
                                                  .RoutingTo(this.delegatedAppInfoProvider)
                                                  .WithIdentity(this.delegatedAppInfoProvider)
                                                  .Authenticate(userCredentials)
                                                  .GetClient();

            string randomNickname = TestContext.CurrentContext.Random.GetString(10);
            var response = userClient.CurrentUser.RequestActivationCode();
            userClient.CurrentUser.SetPin("178965", response.Code);
        }

    }
}