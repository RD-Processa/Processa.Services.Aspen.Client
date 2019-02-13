// -----------------------------------------------------------------------
// <copyright file="AspenClientTests.Delegated.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-16 11:42 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;
    using System.Net;
    using Entities;
    using Fluent;
    using Fluent.Auth;
    using Newtonsoft.Json;
    using NUnit.Framework;

    /// <summary>
    /// Implementa pruebas unitarias de la clase <see cref="AspenClient"/>.
    /// </summary>
    public partial class AspenClientTests
    {
        /// <summary>
        /// Se permite consultar las cuentas de un usuario desde una aplicación delegada.
        /// </summary>
        /// <remarks>A esta prueba como se conecta a varios sistema, se le permite un poco más de tiempo de ejecución.</remarks>
        [Test, 
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/7"),
         Author("Atorres"),
         MaxTime(3000)]
        public void Issue7Delegated()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            // When
            var accounts = client.Financial.GetAccounts();

            // Then
            CollectionAssert.IsNotEmpty(accounts);
        }

        /// <summary>
        /// Se produce una excepción si el servicio Aspen no está configurado para el envío de mensajes SMS.
        /// </summary>
        [Category("Delegated-Scope"), Test]
        public void GivenANonWorkingServiceWhenInvokeRequestActivationCodeThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            // When
            void ServiceUnavailable() => client.CurrentUser.RequestActivacionCode();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(ServiceUnavailable);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("20100"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("No fue posible enviar su código de activación"));
        }

        /// <summary>
        /// Se produce una excepción si no se envían las credenciales del usuario al autenticar una aplicación delegada.
        /// </summary>
        [Category("Delegated-Scope"), Test]
        public void GivenARecognizedDelegatedIdentityWhenInvokeAuthenticateWithMissingValuesThenAnExceptionIsThrows()
        {
            // Given
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15852"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("'DocType' should not be empty"));
        }

        /// <summary>
        /// Se emite un token de autenticación de usuarios cuando las credenciales corresponden a unas conocidas.
        /// </summary>
        [Category("Delegated-Scope"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenAnUserAuthTokenIsGenerated()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider);
            // When 
            client.Authenticate(userInfo);

            // Then
            Assert.That(client.AuthToken, Is.TypeOf<UserAuthToken>());
        }

        /// <summary>
        /// Se requiere un valor para ActivationCode al establecer pin de usuario.
        /// </summary>
        [Test,
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/2"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue2()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();
            // When
            string randomPinNumber = TestContext.CurrentContext.Random.GetDigits(6);
            void SetUserPin() => ((AspenClient)client.CurrentUser).SetPinAvoidingValidation(randomPinNumber, null);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(SetUserPin);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15852"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("ActivationCode"));
        }

        /// <summary>
        /// PinNumber debe cumplir con las políticas de Pin (LengthPolicy)
        /// </summary>
        [Test,
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/3"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue3()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();
            // When
            string tooLongPinNumber = TestContext.CurrentContext.Random.GetDigits(7);
            void NotAcceptable() => client.CurrentUser.SetPin(tooLongPinNumber, "123456");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15860"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Pin es muy largo o muy corto"));
        }

        /// <summary>
        /// PinNumber debe cumplir con las políticas de Pin (ConsecutivePolicy)
        /// </summary>
        [Test,
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/4"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue4()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            // When
            const string InvalidPinNumber = "123456";
            void NotAcceptable() => client.CurrentUser.SetPin(InvalidPinNumber, "123456");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15860"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("caracteres que no sean consecutivos"));
        }

        /// <summary>
        /// PinNumber debe cumplir con las políticas de Pin (TwinsPolicy).
        /// </summary>
        [Test,
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/5"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue5()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();            

            // When            
            const string InvalidPinNumber = "111111";
            void NotAcceptable() => ((AspenClient)client.CurrentUser).SetPinAvoidingValidation(InvalidPinNumber, "123456");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(NotAcceptable);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15860"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Por favor utilice caracteres que no sean iguales"));
        }

        /// <summary>
        /// Se requiere un valor para PinNumber al establecer pin de usuario.
        /// </summary>
        [Test, 
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/6"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue6()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider);

            // When
            client.Authenticate(userInfo);
            string randomCode = TestContext.CurrentContext.Random.GetDigits(6);
            void BadRequest() => ((AspenClient)client.CurrentUser).SetPinAvoidingValidation(null, randomCode);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(BadRequest);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15852"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("PinNumber"));
        }

        /// <summary>
        /// Se produce un error si se utiliza un código de activación no reconocido por el sistema.
        /// </summary>
        [Test,
         Category("Delegated-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/8"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue8()
        {
            // Given
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();
  
            // When
            // PinNumber que cumple con TODAS las políticas de validaciones.
            const string WellFormedPinNumber = "741269";
            string randomCode = TestContext.CurrentContext.Random.GetDigits(6);
            void BadRequest() => client.CurrentUser.SetPin(WellFormedPinNumber, randomCode);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(BadRequest);

            // Then
            const string ResponseText = "Código de activación o identificador es invalido";
            CollectionAssert.IsNotEmpty(exception.Content);
            Assert.That(exception.Content["remainingTimeLapse"], Is.AssignableTo(typeof(long)));
            Assert.That(exception.Content["reason"].ToString(), Is.Not.Null.And.Matches(ResponseText));

            Assert.That(exception.EventId, Is.EqualTo("15868"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.ExpectationFailed));
            Assert.That(exception.Message, Is.Not.Null.And.Matches(ResponseText));
        }

        [Test]
        public void GetBalancesDelegated()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            var balances = client.Financial.GetBalances("203945");
            Console.WriteLine(JsonConvert.SerializeObject(balances, Formatting.Indented));
            CollectionAssert.IsNotEmpty(balances);
        }

        [Test]
        public void xxx()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            var accounts = client.Financial.GetAccounts();
            foreach (IAccountInfo account in accounts)
            {
                var balances = client.Financial.GetBalances(account.Id);
                foreach (IBalanceInfo balance in balances)
                {
                    var statements = client.Financial.GetStatements(account.Id, balance.TypeId);

                    PrintOutput("Account", account);
                    PrintOutput("Balance", balance);
                    PrintOutput("Statements", statements);

                    CollectionAssert.IsNotEmpty(statements);
                }                
            }            
        }

        [Test]
        public void RequestActivacionCode()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            client.CurrentUser.RequestActivacionCode();
            //client.CurrentUser.RequestSingleUseToken("141414");
        }

        [Test]
        public void GetSingleUseToken()
        {
            DelegatedUserInfo userInfo = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userInfo)
                                              .GetClient();

            var response = client.Financial.GetSingleUseToken("141414", "MyData");
            PrintOutput("Token", response);
            Assert.IsTrue(true);
        }
    }
}