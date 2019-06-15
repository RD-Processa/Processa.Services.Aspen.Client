// -----------------------------------------------------------------------
// <copyright file="AspenClientTestsSetUserPin.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-13 04:20 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System.Net;
    using Fluent;
    using Fluent.Auth;
    using NUnit.Framework;

    public partial class AspenClientTests
    {
        /// <summary>
        /// PinNumber debe cumplir con las políticas longitud (LengthPolicy).
        /// </summary>
        [Test,
         SetUserPin,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/3"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void PinNumberLengthMustBeRecognized()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();
            // When
            string tooLongPinNumber = TestContext.CurrentContext.Random.GetDigits(7);
            void PinNotAcceptable() => client.CurrentUser.SetPin(tooLongPinNumber, "123456");
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(PinNotAcceptable);

            // Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.NotAcceptable, "15860", "Pin es muy largo o muy corto"));
        }

        /// <summary>
        /// PinNumber no puede ser una consecución ascendente de dígitos (ConsecutivePolicy)
        /// </summary>
        [Test,
         SetUserPin,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/4"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void PinNumberCannotBeConsecutiveAscending()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            const string InvalidPinNumber = "123456";
            string randomActivationCode = TestContext.CurrentContext.Random.GetDigits(6);
            void NotAcceptable() => client.CurrentUser.SetPin(InvalidPinNumber, randomActivationCode);
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.NotAcceptable, "15860", "Utilice caracteres que no sean consecutivos"));
        }

        /// <summary>
        /// PinNumber no puede ser una consecución descendente de dígitos (ConsecutivePolicy)
        /// </summary>
        [Test,
         SetUserPin,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/4"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void PinNumberCannotBeConsecutiveDescending()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            const string InvalidPinNumber = "654321";
            string randomActivationCode = TestContext.CurrentContext.Random.GetDigits(6);
            void NotAcceptable() => client.CurrentUser.SetPin(InvalidPinNumber, randomActivationCode);
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.NotAcceptable, "15860", "Utilice caracteres que no sean consecutivos"));
        }

        /// <summary>
        /// PinNumber no puede ser una consecución sucesiva de dígitos (TwinsPolicy)
        /// </summary>
        [Test,
         SetUserPin,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/5"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void PinNumberCannotBeTwins()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            const string InvalidPinNumber = "999999";
            string randomActivationCode = TestContext.CurrentContext.Random.GetDigits(6);
            void NotAcceptable() => client.CurrentUser.SetPin(InvalidPinNumber, randomActivationCode);
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.NotAcceptable, "15860", "Utilice caracteres que no sean iguales y consecutivos"));
        }

        /// <summary>
        /// PinNumber no puede ser una consecución sucesiva de dígitos (TwinsPolicy)
        /// </summary>
        [Test,
         SetUserPin,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/6"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void PinNumberRequiresAllInputs()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            void NotAcceptable() => client.CurrentUser.SetPin(null, null);
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.NotAcceptable, "15860", "no puede ser nulo ni vacío"));
        }

        /// <summary>
        /// PinNumber debe corresponder al asociado con el usuario.
        /// </summary>
        [Test,
         SetUserPin,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/8"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void PinNumberUnknown()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            // PinNumber que cumple con TODAS las políticas de validaciones.
            const string WellFormedPinNumber = "741269";
            string randomActivationCode = TestContext.CurrentContext.Random.GetDigits(6);
            void ExpectationFailed() => client.CurrentUser.SetPin(WellFormedPinNumber, randomActivationCode);
            AspenResponseException responseException = Assert.Throws<AspenResponseException>(ExpectationFailed);

            // Then
            Assert.That(responseException, Is.ExpectedException(HttpStatusCode.ExpectationFailed, "15868", "No se encontró una invitación"));
            Assert.That(responseException.Content["remainingTimeLapse"], NUnit.Framework.Is.Null);
        }
    }
}