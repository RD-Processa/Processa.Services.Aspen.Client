// -----------------------------------------------------------------------
// <copyright file="AspenClientTests.Autonomous.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-16 11:39 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Entities;
    using Fluent;
    using Newtonsoft.Json;
    using NUnit.Framework;

    /// <summary>
    /// Implementa pruebas unitarias de la clase <see cref="AspenClient"/>.
    /// </summary>
    public partial class AspenClientTests
    {
        /// <summary>
        /// Se produce una excepción si no se llama a Authenticate y se intenta invocar alguna operación que requiera autenticación.
        /// </summary>
        [Test,
         Category("Autonomous-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/1"),
         Author("Atorres"),
         MaxTime(MaximumResponseTime)]
        public void Issue1()
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
        [Test, Category("Autonomous-Scope")]
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

        [Test]
        public void GetClientWorks()
        {
            AspenClient.Initialize()
                  .RoutingTo(this.autonomousAppInfoProvider)
                  .WithIdentity("29b35be3-3159-4800-807e-cde138439378", "colombia")
                  .Authenticate()
                  .GetClient();
        }

        [Test]
        public void HeadersValidations()
        {            
            IFluentClient client = AspenClient.Initialize(new HardCodedSettings(new SingleUseNonceGenerator()))
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            void DummyCall1() => client.Financial.RequestSingleUseToken("CC", "52080323");
            AspenResponseException are1 = Assert.Throws<AspenResponseException>(DummyCall1);
            AssertAspenResponseException(
                are1, 
                "99003", 
                HttpStatusCode.BadRequest, 
                "Nonce already processed");

            client = AspenClient.Initialize(new HardCodedSettings(null, new FutureEpochGenerator()))
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            void DummyCall2() => client.Financial.RequestSingleUseToken("CC", "52080323");
            AspenResponseException are2 = Assert.Throws<AspenResponseException>(DummyCall2);
            AssertAspenResponseException(
                are2, 
                "15851", 
                HttpStatusCode.RequestedRangeNotSatisfiable, 
                "Verifique que el reloj del dispositivo");

            string apiKey = this.autonomousAppInfoProvider.ApiKey;
            string apiSecret = Guid.NewGuid().ToString("D");
            client = AspenClient.Initialize()
                                .RoutingTo(this.autonomousAppInfoProvider)
                                .WithIdentity(apiKey, apiSecret);
            
            AspenResponseException are3 = Assert.Throws<AspenResponseException>(() => client.Authenticate(useCache:false));
            AssertAspenResponseException(
                are3, 
                "20007", 
                HttpStatusCode.BadRequest, 
                "Content for custom header 'X-PRO-Auth-Payload' is not valid");
        }

        /// <summary>
        /// Se produce una excepción si falta la cabecera peronalizada que identifica el Payload.
        /// </summary>
        [Category("Autonomous-Scope"), Test]
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
        [Category("Autonomous-Scope"), Test]
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
        [Category("Autonomous-Scope"), Test]
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
        [Category("Autonomous-Scope"), Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenAnAuthTokenIsReturned()
        {
            IFluentClient client = AspenClient.Initialize()
                                       .RoutingTo(this.autonomousAppInfoProvider)
                                       .WithIdentity(this.autonomousAppInfoProvider);

            var response = client.Authenticate();
            Assert.IsNotNull(response.AuthToken);
        }

        /// <summary>
        /// Se obtienen la lista de tipos de identificacíon, si la aplicación se puede autenticar.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad autnónoma reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los tipos de documentos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Autonomous-Resources"), Test]
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
        /// Se obtienen la lista de operadores de telefonía, si la aplicación se puede autenticar.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad autnónoma reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los operadores de telefonía, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Autonomous-Resources"), Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenGetTelcosWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            var telcos = client.Settings.GetTelcos();
            CollectionAssert.IsNotEmpty(telcos);
        }

        /// <summary>
        /// Se obtienen los tipos de transacción, si la aplicación se puede autenticar.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad autnónoma reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los tipos de transacción, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Autonomous-Resources"), Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenGetTranTypesWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            var tranTypes = client.Settings.GetPaymentTypes();
            CollectionAssert.IsNotEmpty(tranTypes);
        }

        /// <summary>
        /// Se obtienen los tipos de pagos que se pueden realizar a una cuenta, si la aplicación se puede autenticar.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad autnónoma reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los tipos de pagos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Autonomous-Resources"), Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenGetPaymentTypesWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            var paymentTypes = client.Settings.GetPaymentTypes();
            CollectionAssert.IsNotEmpty(paymentTypes);
        }

        /// <summary>
        /// Se obtienen los valores admitidos para recarga a celular si la aplicación se puede autenticar.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad autnónoma reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los valores admitidos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Autonomous-Resources"), Test]
        public void GivenARecognizedIdentityWhenInvokeAuthenticateThenGetTopUpValuesWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            var topUpValues = client.Settings.GetTopUpValues();
            CollectionAssert.IsNotEmpty(topUpValues);
        }

        /// <summary>
        /// Una identidad reconocida obtiene un token de autenticación utilizando un llamado en cadena de los métdos Authenticate y GetClient.
        /// </summary>
        [Category("Autonomous-Scope"), Test]
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
        /// Se produce una excepción si el scope del ApiKey no corresponde con el esperado por Aspen.
        /// </summary>
        [Category("Autonomous-Scope"), Test]
        public void GivenARecognizedAutonomousIdentityWhenInvokeAuthenticateForDelegatedScopeThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
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
        /// Se produce una excepción al validar un código de activacion que no está registrado.
        /// </summary>
        [Category("Autonomous-Scope"), Test]
        public void GivenARandomActivationCodeWhenInvokeValidateCodeThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string randomCode = TestContext.CurrentContext.Random.GetDigits(6);
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
        [Category("Autonomous-Scope"), Test]
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

        [Test]
        public void GivenAValidActivationCodeWhenInvokeValidateCodeThenItWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string nickname = "70106857";
            string code = nickname.Substring(nickname.Length - 6, 6);
            void ValidationWorks() => client.Management.ValidateActivationCode(code, nickname, channelId: "94ED400D-5D26-4160-A3AA-EC45A2B4AD39");
            Assert.DoesNotThrow(ValidationWorks);
        }

        /// <summary>
        /// Se permite consultar las cuentas de un usuario desde una aplicación autónoma.
        /// </summary>
        /// <remarks>A esta prueba como se conecta a varios sistema, se le permite un poco más de tiempo de ejecución.</remarks>
        [Test,
         Category("Autonomous-Scope"),
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/7"),
         Author("Atorres"),
         MaxTime(3000)]
        public void Issue7Autonomous()
        {
            // Given
            RecognizedUserInfo userInfo = RecognizedUserInfo.Current();
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            // When            
            var accounts = client.Financial.GetAccounts(userInfo.DocType, userInfo.DocNumber);
            PrintOutput("Accounts", accounts);

            // Then
            CollectionAssert.IsNotEmpty(accounts);
        }

        [Test]
        public void GetBalancesAutonomous()
        {
            RecognizedUserInfo userInfo = RecognizedUserInfo.Current();
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var balances = client.Financial.GetBalances(userInfo.DocType, userInfo.DocNumber, userInfo.AccountId);
            Console.WriteLine(JsonConvert.SerializeObject(balances, Formatting.Indented));
            CollectionAssert.IsNotEmpty(balances);
        }

        [Test]
        public void GetStatementsFromAccountINfoWorks()
        {
            RecognizedUserInfo userInfo = RecognizedUserInfo.Current();
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var accounts = client.Financial.GetAccounts(userInfo.DocType, userInfo.DocNumber);
            foreach (IAccountInfo account in accounts)
            {
                var balances = client.Financial.GetBalances(userInfo.DocType, userInfo.DocNumber, userInfo.AccountId);
                foreach (IBalanceInfo balance in balances)
                {
                    IEnumerable<IMiniStatementInfo> statements = null;

                    void GetStatements() => statements = client.Financial.GetStatements(userInfo.DocType, userInfo.DocNumber, account.Id, balance.TypeId);
                    Assert.DoesNotThrow(GetStatements);

                    PrintOutput("Account", account);
                    PrintOutput("Balance", balance);
                    PrintOutput("Statements", statements);
                }
            }
        }

        [Test]
        public void GetAccountsByAlias()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var result = client.Financial.GetAccountsByAlias("51D42557-1D54-4C7A-AB8E-8B70F4BA9635", "CC|000005");
            PrintOutput("Accounts By Alias", result);
            Assert.IsFalse(false);
        }

        [Test]
        public void GetBalancesByAlias()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var result = client.Financial.GetBalancesByAlias("51D42557-1D54-4C7A-AB8E-8B70F4BA9635", "CC|000005", "203945");
            PrintOutput("Balances By Alias", result);
            Assert.IsFalse(false);
        }

        #region SendToken

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo("00000000", "0000000000000", "00", "0000");
            void InvokeRequestSingleUseTokenAvoidingValidation(string invalidDocType) => ((AspenClient)client.Financial).RequestSingleUseTokenAvoidingValidation(invalidDocType, "52080323", null, tags);
            void InvokeRequestSingleUseToken(string invalidDocType) => client.Financial.RequestSingleUseToken(invalidDocType, "52080323", tags: tags);

            // Tipo de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();


            TagsInfo tags = new TagsInfo("00000000", "0000000000000", "00", "0000");
            void InvokeRequestSingleUseTokenAvoidingValidation(string invalidDocNumber) => ((AspenClient)client.Financial).RequestSingleUseTokenAvoidingValidation("CC", invalidDocNumber, null, tags);
            void InvokeRequestSingleUseToken(string invalidDocNumber) => client.Financial.RequestSingleUseToken("CC", invalidDocNumber, tags: tags);

            // Número de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButMetadataIsInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo("00000000", "0000000000000", "00", "0000");
            void InvokeRequestSingleUseToken(string invalidMetadata) => client.Financial.RequestSingleUseToken("CC", "52080323", invalidMetadata, tags: tags);

            // Metadata vacío...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' debe coincidir con el patrón ^[ -~]{1,50}$");

            // Los acentos no son admitidos...
            string[] emphasis = {"á", "é", "í", "ó", "ú"};

            foreach (string value in emphasis)
            {
                string metadata = $"LowercaseLetter-{value}";
                exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(metadata));
                AssertAspenResponseException(
                    exception,
                    "15852",
                    HttpStatusCode.BadRequest,
                    @"'Metadata' debe coincidir con el patrón ^[ -~]{1,50}$");

                metadata = $"UppercaseLetter-{value.ToUpper()}";
                exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(metadata));
                AssertAspenResponseException(
                    exception,
                    "15852",
                    HttpStatusCode.BadRequest,
                    @"'Metadata' debe coincidir con el patrón ^[ -~]{1,50}$");
            }
            
            // Excede longitud...
            string randomMetadata = $"{Guid.NewGuid()}-{Guid.NewGuid()}-{Guid.NewGuid()}";
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(randomMetadata));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' debe coincidir con el patrón ^[ -~]{1,50}$");
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeRequestSingleUseToken(TagsInfo tagsInfo) => client.Financial.RequestSingleUseToken("CC", "52080323", tags: tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // Con las propiedades metadata y tags nulas en el body.
            Assert.DoesNotThrow(() => client.Financial.RequestSingleUseToken("CC", "52080323"));

            // Con las propiedades metadata vacía en el body.
            Assert.DoesNotThrow(() => client.Financial.RequestSingleUseToken("CC", "52080323", "    "));

            // Con metadata.
            Assert.DoesNotThrow(() => client.Financial.RequestSingleUseToken("CC", "52080323", "RANDOM_DATA_BY_ACQUIRER"));

            // Sin metadata y tags.
            TagsInfo tags = new TagsInfo("00000000", "0000000000000", "00", "0000");
            Assert.DoesNotThrow(() => client.Financial.RequestSingleUseToken("CC", "52080323", null, tags));

            // Con metadata y tags.
            Assert.DoesNotThrow(() => client.Financial.RequestSingleUseToken("CC", "52080323", "RANDOM_DATA_BY_ACQUIRER", tags));

            // Sin agregar las propiedades metadata y tags en el body.
            Assert.DoesNotThrow(() => ((AspenClient)client.Financial).RequestSingleUseTokenAvoidingValidation("CC", "52080323", excludeMetadata:true, excludeTags:true));
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButResponseFromSystemKrakenWasUnsuccessfulThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.RequestSingleUseToken("CC", "0000000000"));
            AssertAspenResponseException(
                exception,
                "20102",
                HttpStatusCode.BadGateway,
                @"No fue posible enviar el mensaje. Por favor compruebe la información adicional en el contenido de esta respuesta o vuelva a intentar en unos minutos");

            // La respuesta debe incluir en el body propiedades: NotificationResponseCode y NotificationResponseMessage
            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("notificationResponseCode", exception.Content.Keys);
            Assert.IsNotEmpty(exception.Content["notificationResponseCode"].ToString());
            Assert.Contains("notificationResponseMessage", exception.Content.Keys);
            Assert.IsNotEmpty(exception.Content["notificationResponseMessage"].ToString());
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButTimeoutOnSystemKrakenThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración de KRAKEN para usar una cola que no existe.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Kraken:SendMessageRoutingKey' -Value 'Kraken.NotificationRoutingKey.NotFound'
             */
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.RequestSingleUseToken("CC", "0000000000"));
            AssertAspenResponseException(
                exception,
                "20100",
                HttpStatusCode.ServiceUnavailable,
                @"No fue posible enviar el mensaje. Por favor vuelva a intentar en unos minutos");
        }

        #endregion

        [Test]
        public void GetStatementsByAlias()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var result = client.Financial.GetStatementsByAlias("51D42557-1D54-4C7A-AB8E-8B70F4BA9635", "CC|000005", "203945");
            PrintOutput("Statements By Alias", result);
            Assert.IsTrue(true);
        }

        #region Payment

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentAvoidingValidation(string invalidDocType) => ((AspenClient)client.Financial).PaymentAvoidingValidation(invalidDocType, "52080323", "585730", "80", 10000);
            void InvokePayment(string invalidDocType) => client.Financial.Payment(invalidDocType, "52080323", "585730", "80", 10000);

            // Tipo de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentAvoidingValidation(string invalidDocNumber) => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", invalidDocNumber, "585730", "80", 10000);
            void InvokePayment(string invalidDocNumber) => client.Financial.Payment("CC", invalidDocNumber, "585730", "80", 10000);

            // Número de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePayment(string invalidAccountType) => client.Financial.Payment("CC", "52080323", "000000", invalidAccountType, 10000);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // No requiere el tipo de cuenta y por defecto se usará el tipo de cuenta configurada.
            void InvokePaymentWithoutAccountType() => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", null, 10000, null, excludeAccountType: true, excludeTags: true);
            exception = Assert.Throws<AspenResponseException>(InvokePaymentWithoutAccountType);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenTokenIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentAvoidingValidation(string invalidToken) => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", invalidToken, "80", 10000);
            void InvokePayment(string invalidToken) => client.Financial.Payment("CC", "52080323", invalidToken, "80", 10000);

            // Token nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' no puede ser nulo ni vacío");

            // Token vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' no puede ser nulo ni vacío");

            // Token espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' no puede ser nulo ni vacío");

            // Token solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XXXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token excede longitud...
            string randomToken = new Random().Next().ToString("0000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(randomToken));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token de 6 dígitos debe fallar...
            randomToken = new Random().Next(100000, 999999).ToString();
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(randomToken));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // Token de 9 dígitos debe fallar...
            randomToken = new Random().Next(100000000, 999999999).ToString();
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(randomToken));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePayment(TagsInfo tagsInfo) => client.Financial.Payment("CC", "52080323", "000000", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            // Cuando la propiedad Tags en el body es nula...
            void InvokePaymentWithoutTags() => client.Financial.Payment("CC", "52080323", "000000", "80", 10000);
            exception = Assert.Throws<AspenResponseException>(InvokePaymentWithoutTags);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // Cuando no se incluye la propiedad Tags en el body...
            void InvokePaymentExcludeTagsFromBody() => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80", 10000, null, excludeTags: true);
            exception = Assert.Throws<AspenResponseException>(InvokePaymentExcludeTagsFromBody);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountIsMissingOrLessThanOrEqualZeroThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentAvoidingValidation(int invalidAmount) => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80", invalidAmount);

            // Valor menor a cero...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor menor a cero...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(0));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Sin valor para la transacción...
            void InvokePaymentWithoutAmount() => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80", null, null, excludeAmount: true);
            exception = Assert.Throws<AspenResponseException>(InvokePaymentWithoutAmount);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            void InvokePaymentNotIntAmount(string invalidAmount) => ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80", invalidAmount);

            // Cuando el valor es un entero de tipo cadena...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount("0"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Cuando el valor es nulo, eso equivalente a cero (ya que es el valor predeterminado de un entero)...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount("XXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando el valor es un entero de tipo cadena debe ser exitosa la validación del campo...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount("10000"));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // Cuando el valor es un entero pero no existe token...
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "000000", "80", 10000);
            exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenTokenIsValidThenFinancialRequestIsAuthorized()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Es importtante cambiar de la configuración del ApiKey y establezca un canal que autorize la transacción
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CFMBQIR78NEIKE0M314Q' => CONEXRED (Retiros)
             */

            // TODO: Genere un token manualmente...
            Assert.DoesNotThrow(() => client.Financial.Payment("CC", "1073688252", "217045", "80", 10000, new TagsInfo(cardAcceptorId: "00000014436950")));
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenTokenIsValidThenFinancialRequestIsUnauthorized()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // TODO: Genere un token manualmente...
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "204825", "80", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);

            // La respuesta debe incluir en el body propiedades: FinancialResponseCode y FinancialResponseMessage
            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("financialResponseCode", exception.Content.Keys);
            Assert.Contains("financialResponseMessage", exception.Content.Keys);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionExceedsAmountTokenIfPrecisionIsLessThanOrEqualThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
             */
            // TODO: Genere un token manualmente por $10.000
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "499456", "80", int.MaxValue);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. El valor de la transacción excede el valor del token. Tran ($2,147,483,647) vs. Token ($10,000)");
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionExceedsAmountTokenIfPrecisionIsEqualThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
             */
            // TODO: Genere un token manualmente por $10.000
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "808837", "80", int.MaxValue);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. El valor de la transacción no coincide con el valor del token. Tran ($2,147,483,647) vs. Token ($10,000)");
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionMismatchedAmountTokenIfPrecisionIsEqualThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
             */
            // TODO: Genere un token manualmente por $10.000
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "949693", "80", 9999);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. El valor de la transacción no coincide con el valor del token. Tran ($9,999) vs. Token ($10,000)");
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAccountTypeMismatchedAccountTypeTokenThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value 'true'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "697327", "81", 10000);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. Tipo de cuenta de la transacción no coincide con el tipo de cuenta del token. Tran (81) vs Token (80)");
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenNotRequiredValidateAccountTypeTokenThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value 'false'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "168768", "81", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenNotRequiredValidateAmountTokenThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmount' -Value 'false'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "322775", "80", 120000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionEqualAmountTokenIfPrecisionIsEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountExactly' -Value 'false'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "718064", "80", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionLessAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para usar el tipo de precisión esperado...
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "407738", "80", 9000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionEqualAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para usar el tipo de precisión esperado...
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "614324", "80", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        #endregion

        #region Withdrawal

        [Test, Category("Autonomous-Withdrawal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawalAvoidingValidation(string invalidDocType) => ((AspenClient)client.Financial).WithdrawalAvoidingValidation(invalidDocType, "52080323", "585730", "80", 10000);
            void InvokeWithdrawal(string invalidDocType) => client.Financial.Withdrawal(invalidDocType, "52080323", "585730", "80", 10000);

            // Tipo de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-Withdrawal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawalAvoidingValidation(string invalidDocNumber) => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", invalidDocNumber, "585730", "80", 10000);
            void InvokeWithdrawal(string invalidDocNumber) => client.Financial.Withdrawal("CC", invalidDocNumber, "585730", "80", 10000);

            // Número de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-Withdrawal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawal(string invalidAccountType) => client.Financial.Withdrawal("CC", "52080323", "000000", invalidAccountType, 10000);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // No requiere el tipo de cuenta y por defecto se usará el tipo de cuenta configurada.
            void InvokeWithdrawalWithoutAccountType() => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", "52080323", "000000", null, 10000, null, excludeAccountType: true, excludeTags: true);
            exception = Assert.Throws<AspenResponseException>(InvokeWithdrawalWithoutAccountType);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Withdrawal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenTokenIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawalAvoidingValidation(string invalidToken) => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", "52080323", invalidToken, "80", 10000);
            void InvokeWithdrawal(string invalidToken) => client.Financial.Withdrawal("CC", "52080323", invalidToken, "80", 10000);

            // Token nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' no puede ser nulo ni vacío");

            // Token vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' no puede ser nulo ni vacío");

            // Token espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' no puede ser nulo ni vacío");

            // Token solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("XXXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token excede longitud...
            string randomToken = new Random().Next().ToString("0000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(randomToken));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Token' debe coincidir con el patrón ^\d{1,9}$");

            // Token de 6 dígitos debe fallar...
            randomToken = new Random().Next(100000, 999999).ToString();
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(randomToken));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // Token de 9 dígitos debe fallar...
            randomToken = new Random().Next(100000000, 999999999).ToString();
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(randomToken));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Withdrawal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawal(TagsInfo tagsInfo) => client.Financial.Withdrawal("CC", "52080323", "000000", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawal(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            // Cuando la propiedad Tags en el body es nula...
            void InvokeWithdrawalWithoutTags() => client.Financial.Withdrawal("CC", "52080323", "000000", "80", 10000);
            exception = Assert.Throws<AspenResponseException>(InvokeWithdrawalWithoutTags);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // Cuando no se incluye la propiedad Tags en el body...
            void InvokeWithdrawalExcludeTagsFromBody() => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", "52080323", "000000", "80", 10000, null, excludeTags: true);
            exception = Assert.Throws<AspenResponseException>(InvokeWithdrawalExcludeTagsFromBody);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Withdrawal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountIsMissingOrLessThanOrEqualZeroThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawalAvoidingValidation(int invalidAmount) => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", "52080323", "000000", "80", invalidAmount);

            // Valor menor a cero...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor menor a cero...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalAvoidingValidation(0));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Sin valor para la transacción...
            void InvokeWithdrawalWithoutAmount() => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", "52080323", "000000", "80", null, null, excludeAmount: true);
            exception = Assert.Throws<AspenResponseException>(InvokeWithdrawalWithoutAmount);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            void InvokeWithdrawalNotIntAmount(string invalidAmount) => ((AspenClient)client.Financial).WithdrawalAvoidingValidation("CC", "52080323", "000000", "80", invalidAmount);

            // Cuando el valor es un entero de tipo cadena...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount("0"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Cuando el valor es nulo, eso equivalente a cero (ya que es el valor predeterminado de un entero)...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount("XXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando el valor es un entero de tipo cadena debe ser exitosa la validación del campo...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount("10000"));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // Cuando el valor es un entero pero no existe token...
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "000000", "80", 10000);
            exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenTokenIsValidThenFinancialRequestIsAuthorized()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Es importante cambiar de la configuración del ApiKey y establezca un canal que autorize la transacción...
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CNXRKKE13I015J1FOQCP' => CONEXRED (Retiros)
             */

            // TODO: Genere un token manualmente...
            Assert.DoesNotThrow(() => client.Financial.Withdrawal("CC", "52080323", "112820", "80", 10000));
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenTokenIsValidThenFinancialRequestIsUnauthorized()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // TODO: Genere un token manualmente...
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "144682", "80", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                "Operación financiera no autorizada");

            // La respuesta debe incluir en el body propiedades: FinancialResponseCode y FinancialResponseMessage
            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("financialResponseCode", exception.Content.Keys);
            Assert.Contains("financialResponseMessage", exception.Content.Keys);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionExceedsAmountTokenIfPrecisionIsLessThanOrEqualThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
             */
            // TODO: Genere un token manualmente por $10.000
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "564597", "80", int.MaxValue);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. El valor de la transacción excede el valor del token. Tran ($2,147,483,647) vs. Token ($10,000)");
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionExceedsAmountTokenIfPrecisionIsEqualThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
             */
            // TODO: Genere un token manualmente por $10.000
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "508604", "80", int.MaxValue);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. El valor de la transacción no coincide con el valor del token. Tran ($2,147,483,647) vs. Token ($10,000)");
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionMismatchedAmountTokenIfPrecisionIsEqualThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
             */
            // TODO: Genere un token manualmente por $10.000
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "256721", "80", 9999);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. El valor de la transacción no coincide con el valor del token. Tran ($9,999) vs. Token ($10,000)");
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAccountTypeMismatchedAccountTypeTokenThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value 'true'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "468449", "81", 10000);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. Tipo de cuenta de la transacción no coincide con el tipo de cuenta del token. Tran (81) vs Token (80)");
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenNotRequiredValidateAccountTypeTokenThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value 'false'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "167243", "81", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenNotRequiredValidateAmountTokenThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmount' -Value 'false'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "076514", "80", 120000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionEqualAmountTokenIfPrecisionIsEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para ignorar la validación obligatoria del tipo de cuenta.
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountExactly' -Value 'false'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "136742", "80", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionLessAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para usar el tipo de precisión esperado...
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "981389", "80", 9000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionEqualAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            /*
             * TODO: Debe cambiar la configuración del ApiKey para usar el tipo de precisión esperado...
             * Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
             */
            // TODO: Genere un token manualmente por $10.000 y bolsillo 80
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "273312", "80", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                null);
        }

        #endregion

        [Test]
        public void ValidateSingleUseToken()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            client.Financial.ValidateSingleUseToken("CC", "79483129", "167398", "MyData");
        }

        [Test]
        public void GetClaims()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var response = client.Settings.GetClaims();
            PrintOutput("Claims", response);
            Assert.IsNotNull(response);
        }

        [Test]
        public void GetImageToken()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string channelId = "94ED400D-5D26-4160-A3AA-EC45A2B4AD39";
            string enrollmentAlias = "561625854";
            string response = client.Financial.GetImageToken(channelId, enrollmentAlias);
            byte[] blob = Convert.FromBase64String(response);
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(blob, 0, blob.Length);
                ms.Seek(0, SeekOrigin.Begin);
                ImageConverter imageConverter = new ImageConverter();
                var b = imageConverter.ConvertFrom(blob) as Bitmap;
                var tempPath = Path.ChangeExtension(Path.GetTempFileName(), ".png");
                b.Save(tempPath, ImageFormat.Png);
                string args = $"/e, /select, \"{tempPath}\"";
                Process.Start("explorer.exe", args);
            }
            //PrintOutput("Base 64", response);
        }

        [Test]
        public void GetStatementsFile()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string channelId = "94ED400D-5D26-4160-A3AA-EC45A2B4AD39";
            string enrollmentAlias = "561625854";
            string response = client.Financial.GetStatementsFile(channelId, enrollmentAlias);
            PrintOutput("Resultado", response);
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountNotAcceptableUrl()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string[] invalidDocTypes = { "xd", "yh", "uj", "xxxx", "nhbg" };
            void LinkTransferAccount(string doc)
            {
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo("CC", "79483129", "Atorres");
                string docNumber = new Random().Next().ToString("000000000000");
                client.Management.LinkTransferAccount(doc, docNumber, accountRequestInfo);
            }

            foreach (string docType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(docType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("Tipo de documento en la URL no es reconocido", exc.Message);
            }
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountNotAcceptableBody()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string[] invalidDocTypes = { "xd", "yh", "uj", "xxxx", "nhbg" };
            void LinkTransferAccount(string docType)
            {
                string docNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, docNumber, "Atorres");
                client.Management.LinkTransferAccount("CC", docNumber, accountRequestInfo);
            }

            foreach (string docType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(docType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("Tipo de documento en el BODY no es reconocido", exc.Message);
            }
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountMissingDocType()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string[] invalidDocTypes = { "CC", "NIT", "TI", "CE", "PAS" };
            void LinkTransferAccount(string docType)
            {
                string docNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(null, docNumber, "SomeValue");
                client.Management.LinkTransferAccount("CC", docNumber, accountRequestInfo);
            }

            foreach (string docType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(docType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'DocType' no puede ser nulo ni vacío", exc.Message);
            }
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountMissingDocNumber()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string[] invalidDocTypes = { "CC", "NIT", "TI", "CE", "PAS" };
            void LinkTransferAccount(string docType)
            {
                string docNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, null, "SomeValue");
                client.Management.LinkTransferAccount("CC", docNumber, accountRequestInfo);
            }

            foreach (string docType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(docType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'DocNumber' no puede ser nulo ni vacío", exc.Message);
            }
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountMissingAlias()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string[] invalidDocTypes = { "CC", "NIT", "TI", "CE", "PAS" };
            void LinkTransferAccount(string docType)
            {
                string docNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, docNumber, string.Empty);
                client.Management.LinkTransferAccount("CC", docNumber, accountRequestInfo);
            }

            foreach (string docType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(docType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'Alias' no puede ser nulo ni vacío", exc.Message);
            }
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountInvalidAlias()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string[] invalidDocTypes = { "CC", "NIT", "TI", "CE", "PAS" };
            void LinkTransferAccount(string docType)
            {
                string docNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, docNumber, "áéíóú");
                client.Management.LinkTransferAccount("CC", docNumber, accountRequestInfo);
            }

            foreach (string docType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(docType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'Alias' incluye caracteres inválidos", exc.Message);
            }
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferAccountValidationWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();


            string[] docTypes = { "CC", "NIT", "TI", "CE", "PAS" };
            string randomDocType = docTypes[new Random().Next(0, docTypes.Length)];
            string randomDocNumber = new Random().Next().ToString("000000000000");
            string alias = $"Mi alias {new Random().Next(1, 100):000}";

            const string RecognizedDocType = "CC";
            const string RecognizedDocNumber = "79483129";

            var registeredAccounts = client.Management.GetTransferAccounts(randomDocType, randomDocNumber);

            if (registeredAccounts != null)
            {
                foreach (TransferAccountResponseInfo account in registeredAccounts)
                {
                    client.Management.UnlinkTransferAccount(randomDocType, randomDocNumber, account.Alias);
                }
            }

            ITransferAccountRequestInfo accountInfo = new TransferAccountRequestRequestInfo(RecognizedDocType, RecognizedDocNumber, alias);
            
            // Por primera vez debe funcionar...
            Assert.DoesNotThrow(() => client.Management.LinkTransferAccount(randomDocType, randomDocNumber, accountInfo));

            // Si se intenta registrar de nuevo no debe funcionar..
            AspenResponseException exc = Assert.Throws<AspenResponseException>(() => client.Management.LinkTransferAccount(randomDocType, randomDocNumber, accountInfo));
            StringAssert.IsMatch($"Ya existe una cuenta registrada con el nombre '{alias}'", exc.Message);
            Assert.That(exc.EventId, Is.EqualTo("15853"));

            // Si se intenta registrar de nuevo con otro nombre no debe funcionar.
            string newAlias = $"Otro alias {new Random().Next(1, 100):000}";
            accountInfo = new TransferAccountRequestRequestInfo(RecognizedDocType, RecognizedDocNumber, newAlias);
            exc = Assert.Throws<AspenResponseException>(() => client.Management.LinkTransferAccount(randomDocType, randomDocNumber, accountInfo));
            StringAssert.IsMatch("Cuenta ya está registrada con otro alias", exc.Message);
            Assert.That(exc.EventId, Is.EqualTo("15863"));  
            
            // La cuenta debe aparecer en la lista de cuentas registradas
            var accounts = client.Management.GetTransferAccounts(randomDocType, randomDocNumber);
            Assert.That(accounts.Count, Is.EqualTo(1));

            // La información del titular debe aparecer en el registro de la cuenta.
            TransferAccountResponseInfo accountRequestRequestInfo = accounts.First();
            Assert.AreEqual(accountRequestRequestInfo.CardHolderDocType, accountInfo.DocType);
            Assert.AreEqual(accountRequestRequestInfo.CardHolderDocNumber, accountInfo.DocNumber);

            // Eliminar el registro de la cuenta
            client.Management.UnlinkTransferAccount(randomDocType, randomDocNumber, alias);
            accounts = client.Management.GetTransferAccounts(randomDocType, randomDocNumber);
            Assert.IsNull(accounts);
        }

        [Test]
        [Category("TransferAccount")]
        public void LinkTransferCardHolderValidationWork()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();


            string unrecognizedDocNumber = new Random().Next().ToString("000000000000");
            string alias = $"Mi alias {new Random().Next(1, 100):000}";
            const string RecognizedDocType = "CC";
            ITransferAccountRequestInfo unknownAccountRequestInfo = new TransferAccountRequestRequestInfo(RecognizedDocType, unrecognizedDocNumber, alias);
            string randomDocNumber = new Random().Next().ToString("000000000000");
            AspenResponseException exc = Assert.Throws<AspenResponseException>(() => client.Management.LinkTransferAccount("CC", randomDocNumber, unknownAccountRequestInfo));
            StringAssert.IsMatch("No se encuentra información de la cuenta con los datos suministrados", exc.Message);
            Assert.That(exc.EventId, Is.EqualTo("15856"));
        }

        [Test]
        [Category("TransferAccount")]
        public void GetTransferAccountsWithNoContent()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            const string RecognizedDocType = "CC";
            string unrecognizedDocNumber = new Random().Next().ToString("000000000000");
            var result = client.Management.GetTransferAccounts(RecognizedDocType, unrecognizedDocNumber);        
            Assert.IsNull(result);
        }

        /// <summary>
        /// Se produce una excepción si se intenta invocar la operación de anulación sin el identificador de la transacción original.
        /// </summary>
        /// <remarks>
        /// Given: Una solicitud de anulación
        /// When: Con un identificador de transacción original nulo o vacío
        /// Then: Se genera una excepción de tipo <see cref="ArgumentNullException"/> o <see cref="ArgumentException"/>
        /// </remarks>
        [Test]
        [Category("Refund")]
        public void Refund()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            string randomAuthNumber = TestContext.CurrentContext.Random.GetDigits(6);
            string randomDocNumber = TestContext.CurrentContext.Random.GetDigits(10);
            TagsInfo tags = new TagsInfo("xxxx", "xxxx", "00", "1234");

            AspenResponseException badRequestException = Assert.Throws<AspenResponseException>(() => client.Financial.Refund(randomAuthNumber, "CC", randomDocNumber, "80", 100, tags));
            AssertAspenResponseException(
                badRequestException,
                "15852",
                HttpStatusCode.BadRequest,
                "no tiene un formato valido");

            AspenResponseException notFoundException = Assert.Throws<AspenResponseException>(() => client.Financial.Refund(randomAuthNumber, "CC", randomDocNumber, "80", 100));
            AssertAspenResponseException(
                notFoundException,
                "87001",
                HttpStatusCode.BadGateway,
                "Operación financierá no autorizada");
        }

        [Test]
        [Category("Reversal")]
        public void RefundReversal()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var randomId = Guid.NewGuid().ToString("D");
            var randomDocNumber = new Random().Next().ToString("0000000000");

            AspenResponseException notFoundException = Assert.Throws<AspenResponseException>(() => client.Financial.RefundReversal(randomId, "CC", randomDocNumber, "80", 100));
            AssertAspenResponseException(
                notFoundException,
                "80002",
                HttpStatusCode.BadGateway,
                "No se ha podido obtener respuesta por el sistema responsable de procesarla");
        }

        [Test]
        [Category("Reversal")]
        public void PaymentReversal()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            var randomId = Guid.NewGuid().ToString("D");
            var randomDocNumber = new Random().Next().ToString("0000000000");

            AspenResponseException notFoundException = Assert.Throws<AspenResponseException>(() => client.Financial.PaymentReversal(randomId, "CC", randomDocNumber, "80", 100));
            AssertAspenResponseException(
                notFoundException,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró el TransactionId");
        }

        [Test]
        [Category("Reversal")]
        public void WithdrawalReversal()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var randomId = Guid.NewGuid().ToString("D");
            var randomDocNumber = new Random().Next().ToString("0000000000");
           
            AspenResponseException notFoundException = Assert.Throws<AspenResponseException>(() => client.Financial.WithdrawalReversal(randomId, "CC", randomDocNumber, "80", 100));
            AssertAspenResponseException(
                notFoundException,
                "15880",
                HttpStatusCode.BadGateway,
                "No se encontró el TransactionId");
        }
    }
}