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

            AspenResponseException are3 =
                Assert.Throws<AspenResponseException>(() => client.Authenticate(useCache: false));
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

            void ValidationWorks() =>
                client.Management.ValidateActivationCode(code, nickname,
                    channelId: "94ED400D-5D26-4160-A3AA-EC45A2B4AD39");

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

                    void GetStatements() => statements = client.Financial.GetStatements(userInfo.DocType,
                        userInfo.DocNumber, account.Id, balance.TypeId);

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

            var result =
                client.Financial.GetBalancesByAlias("51D42557-1D54-4C7A-AB8E-8B70F4BA9635", "CC|000005", "203945");
            PrintOutput("Balances By Alias", result);
            Assert.IsFalse(false);
        }

        [Test]
        public void GetStatementsByAlias()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            var result =
                client.Financial.GetStatementsByAlias("51D42557-1D54-4C7A-AB8E-8B70F4BA9635", "CC|000005", "203945");
            PrintOutput("Statements By Alias", result);
            Assert.IsTrue(true);
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

            void InvokeRequestSingleUseTokenAvoidingValidation(string invalidDocType) =>
                ((AspenClient) client.Financial).RequestSingleUseTokenAvoidingValidation(invalidDocType, "52080323",
                    null, tags);

            void InvokeRequestSingleUseToken(string invalidDocType) =>
                client.Financial.RequestSingleUseToken(invalidDocType, "52080323", tags: tags);

            // Tipo de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseTokenAvoidingValidation("    "));
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

            void InvokeRequestSingleUseTokenAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient) client.Financial).RequestSingleUseTokenAvoidingValidation("CC", invalidDocNumber, null,
                    tags);

            void InvokeRequestSingleUseToken(string invalidDocNumber) =>
                client.Financial.RequestSingleUseToken("CC", invalidDocNumber, tags: tags);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseTokenAvoidingValidation("    "));
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

            void InvokeRequestSingleUseToken(string invalidMetadata) =>
                client.Financial.RequestSingleUseToken("CC", "52080323", invalidMetadata, tags: tags);

            // Metadata vacío...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' no puede ser vacío");

            // Metadata vacío...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRequestSingleUseToken("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' no puede ser vacío");

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

            void InvokeRequestSingleUseToken(TagsInfo tagsInfo) =>
                client.Financial.RequestSingleUseToken("CC", "52080323", tags: tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(
                () => InvokeRequestSingleUseToken(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRequestSingleUseToken(new TagsInfo(pan: "00000")));
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

            // Con metadata.
            Assert.DoesNotThrow(() =>
                client.Financial.RequestSingleUseToken("CC", "52080323", "RANDOM_DATA_BY_ACQUIRER"));

            // Sin metadata y tags.
            TagsInfo tags = new TagsInfo("00000000", "0000000000000", "00", "0000");
            Assert.DoesNotThrow(() => client.Financial.RequestSingleUseToken("CC", "52080323", null, tags));

            // Con metadata y tags.
            Assert.DoesNotThrow(() =>
                client.Financial.RequestSingleUseToken("CC", "52080323", "RANDOM_DATA_BY_ACQUIRER", tags));

            // Sin agregar las propiedades metadata y tags en el body.
            Assert.DoesNotThrow(() =>
                ((AspenClient) client.Financial).RequestSingleUseTokenAvoidingValidation("CC", "52080323",
                    excludeMetadata: true, excludeTags: true));
        }

        [Test, Category("Autonomous-Send-Token"), Author("dmontalvo")]
        public void GivenARecognizedIdentityWhenRequestSingleUseTokenForAnUserButResponseFromSystemKrakenWasUnsuccessfulThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => client.Financial.RequestSingleUseToken("CC", "0000000000"));
            AssertAspenResponseException(
                exception,
                "20102",
                HttpStatusCode.BadGateway,
                @"No fue posible enviar el mensaje. Por favor compruebe la información adicional en el contenido de esta respuesta o vuelva a intentar en unos minutos");

            // La respuesta debe incluir en el body propiedades: NotificationResponseCode y NotificationResponseMessage
            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("notificationResponseCode", exception.Content.Keys);
            Assert.Contains("notificationResponseMessage", exception.Content.Keys);
        }
        #endregion

        #region Payment

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentAvoidingValidation(string invalidDocType) =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation(invalidDocType, "52080323", "585730", "80",
                    10000);

            void InvokePayment(string invalidDocType) =>
                client.Financial.Payment(invalidDocType, "52080323", "585730", "80", 10000);

            // Tipo de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no incluido en el body...
            exception = Assert.Throws<AspenResponseException>(() => ((AspenClient)client.Financial).PaymentAvoidingValidation(
                null,
                "52080323",
                "585730",
                "80",
                10000,
                excludeDocType: true));
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
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("X"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'X' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XXXXX' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'1' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("10"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'10' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("10000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'10000' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-Payment-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation("CC", invalidDocNumber, "585730", "80",
                    10000);

            void InvokePayment(string invalidDocNumber) =>
                client.Financial.Payment("CC", invalidDocNumber, "585730", "80", 10000);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento no agregado al body...
            exception = Assert.Throws<AspenResponseException>(() => ((AspenClient)client.Financial).PaymentAvoidingValidation(
                    "CC", 
                    null,
                    "585730",
                    "80",
                    10000,
                    excludeDocNumber: true));
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

            // Caracteres especiales...
            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("*123456*"));
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

            void InvokePaymentAvoidingValidation(string accountType) =>
                ((AspenClient)client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", accountType, 10000);

            void InvokePayment(string accountType) =>
                client.Financial.Payment("CC", "52080323", "000000", accountType, 10000);

            // Aunque el tipo de cuenta no es obligatorio, no se acepta el vacío.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePayment("8*"));
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

            // No requiere el tipo de cuenta y puede ser nulo.
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // No requiere el tipo de cuenta y no requiere ser incluido en el body.
            exception = Assert.Throws<AspenResponseException>(() => ((AspenClient)client.Financial).PaymentAvoidingValidation(
                "CC", 
                "52080323", 
                "000000", 
                null, 
                10000,
                excludeAccountType: true));
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

            void InvokePaymentAvoidingValidation(string invalidToken) =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation("CC", "52080323", invalidToken, "80", 10000);

            void InvokePayment(string invalidToken) =>
                client.Financial.Payment("CC", "52080323", invalidToken, "80", 10000);

            // Token nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(null));
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

            void InvokePayment(TagsInfo tagsInfo) =>
                client.Financial.Payment("CC", "52080323", "000000", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePayment(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePayment(new TagsInfo(cardAcceptorId: cardAcceptorId)));
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
            void InvokePaymentExcludeTagsFromBody() =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80", 10000,
                    null, excludeTags: true);

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

            void InvokePaymentAvoidingValidation(int invalidAmount) =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80",
                    invalidAmount);

            // Valor menor a cero...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentAvoidingValidation(-1));
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
            void InvokePaymentWithoutAmount() =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80", null, null,
                    excludeAmount: true);

            exception = Assert.Throws<AspenResponseException>(InvokePaymentWithoutAmount);
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            void InvokePaymentNotIntAmount(string invalidAmount) =>
                ((AspenClient) client.Financial).PaymentAvoidingValidation("CC", "52080323", "000000", "80",
                    invalidAmount);

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
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentNotIntAmount("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CFMBQIR78NEIKE0M314Q' => CAFAM (Pagos)
            // 2. Genere un token manualmente:
            // Use Aspen.Autonomous: Send-TranToken -DocNumber '1073688252' -DocType 'CC'
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            Assert.DoesNotThrow(() => client.Financial.Payment("CC", "1073688252", "983862", "80", 10000,
                new TagsInfo(cardAcceptorId: "00000014436950")));
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenTokenIsValidThenFinancialRequestIsUnauthorized()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para establecer un canal que NO autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'COMPMOV60A6GLOB8L013'
            // 2. Genere un token manualmente:
            // Use Aspen.Autonomous: Send-TranToken -DocNumber '52080323' -DocType 'CC'
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "306264", "80", 10000);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                "Operación financiera no autorizada");

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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para que la precisión sea menor o igual.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "428096", "80", int.MaxValue);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para que la precisión sea exacta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "916678", "80", int.MaxValue);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para que la precisión sea exacta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "211124", "80", 9999);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para sea obligatorio del tipo de cuenta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value $true
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "330570", "81", 10000);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para ignorar la validación obligatoria del tipo de cuenta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value $false
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "52080323", "195276", "81", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                "Operación financiera no autorizada");
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenNotRequiredValidateAmountTokenThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CFMBQIR78NEIKE0M314Q' => CAFAM (Pagos)
            // 1. Cambie de la configuración de la aplicación para ignorar la validación del valor del token.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmount' -Value $false
            // 2. Genere un token manualmente:
            // Use Aspen.Autonomous: Send-TranToken -DocNumber '1073688252' -DocType 'CC'
            void InvokePayment() => client.Financial.Payment("CC", "1073688252", "810362", "80", 120000, new TagsInfo(cardAcceptorId: "00000014436950"));
            Assert.DoesNotThrow(InvokePayment);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionEqualAmountTokenIfPrecisionIsEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CFMBQIR78NEIKE0M314Q' => CAFAM (Pagos)
            // 1. Cambie de la configuración de la aplicación para que la precición del valor sea exacta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "1073688252", "142909", "80", 10000, new TagsInfo(cardAcceptorId: "00000014436950"));
            Assert.DoesNotThrow(InvokePayment);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionLessAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CFMBQIR78NEIKE0M314Q' => CAFAM (Pagos)
            // 1. Cambie de la configuración de la aplicación para que la precición del valor sea menor o exacto.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "1073688252", "800280", "80", 10000, new TagsInfo(cardAcceptorId: "00000014436950"));
            Assert.DoesNotThrow(InvokePayment);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenAmountTransactionEqualAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CFMBQIR78NEIKE0M314Q' => CAFAM (Pagos)
            // 1. Cambie de la configuración de la aplicación para que la precición del valor sea menor o exacto.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokePayment() => client.Financial.Payment("CC", "1073688252", "766922", "80", 9000,
                new TagsInfo(cardAcceptorId: "00000014436950"));

            Assert.DoesNotThrow(InvokePayment);
        }

        [Test, Category("Autonomous-Payment"), Author("dmontalvo")]
        public void GivenInvokePaymentWhenServiceResponseWithInternalServerErrorThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Esta prueba es manual y su intención es verificar la respuesta, por lo que se debe cambiar algo en el controlador para que genere excepción no controlada.
            void InvokePayment() => client.Financial.Payment("CC", "1073688252", "774368", "80", 10000);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePayment);
            AssertAspenResponseException(
                exception,
                "87001",
                HttpStatusCode.InternalServerError,
                "Se presentó una excepción general procesando la operación financiera");
        }

        #endregion

        #region PaymentReversal

        [Test, Category("Autonomous-PaymentReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenTransactionIdIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokePaymentReversalAvoidingValidation(string invalidTransactionId) =>
                ((AspenClient) client.Financial).PaymentReversalAvoidingValidation(invalidTransactionId, "CC",
                    "52080323", "80", 10000, tags);

            void InvokePaymentReversal(string invalidTransactionId) =>
                client.Financial.PaymentReversal(invalidTransactionId, "CC", "52080323", "80", 10000, tags);

            // Identificador de transacción nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            // Identificador de transacción vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            // Identificador de transacción espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            // Identificador de transacción con algún caracter inválido...
            string randomTransactionId = $"{Guid.Empty}-{Guid.Empty}-ñÑ+++";
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(randomTransactionId));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' debe coincidir con el patrón ^[a-zA-Z0-9\,\.\-\{\}\\]{1,68}$");

            // Identificador de transacción excede longitud...
            randomTransactionId = $"{Guid.Empty}-{Guid.Empty}-{Guid.Empty}";
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(randomTransactionId));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' debe coincidir con el patrón ^[a-zA-Z0-9\,\.\-\{\}\\]{1,68}$");
        }

        [Test, Category("Autonomous-PaymentReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokePaymentReversalAvoidingValidation(string invalidDocType) =>
                ((AspenClient) client.Financial).PaymentReversalAvoidingValidation(Guid.NewGuid().ToString(),
                    invalidDocType, "52080323", "80", 10000, tags);

            void InvokePaymentReversal(string invalidDocType) =>
                client.Financial.PaymentReversal(Guid.NewGuid().ToString(), invalidDocType, "52080323", "80", 10000,
                    tags);

            // Tipo de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-PaymentReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokePaymentReversalAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient) client.Financial).PaymentReversalAvoidingValidation(Guid.NewGuid().ToString(), "CC",
                    invalidDocNumber, "80", 10000, tags);

            void InvokePaymentReversal(string invalidDocNumber) =>
                client.Financial.PaymentReversal(Guid.NewGuid().ToString(), "CC", invalidDocNumber, "80", 10000, tags);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-PaymentReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokePaymentReversalAvoidingValidation(string invalidAccountType) =>
                ((AspenClient) client.Financial).PaymentReversalAvoidingValidation(Guid.NewGuid().ToString(), "CC",
                    "52080323", invalidAccountType, 10000, tags);

            void InvokePaymentReversal(string invalidAccountType) =>
                client.Financial.PaymentReversal(Guid.NewGuid().ToString(), "CC", "52080323", invalidAccountType, 10000,
                    tags);

            // Tipo de cuenta nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta formato
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // Tipo de cuenta longitud
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");
        }

        [Test, Category("Autonomous-PaymentReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokePaymentReversal(TagsInfo tagsInfo) =>
                client.Financial.PaymentReversal(Guid.NewGuid().ToString(), "CC", "52080323", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() =>
                    InvokePaymentReversal(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversal(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversal(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversal(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversal(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(
                () => InvokePaymentReversal(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversal(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");
        }

        [Test, Category("Autonomous-PaymentReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenAmountIsMissingOrLessThanOrEqualZeroThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            void InvokePaymentReversalAvoidingValidation(object invalidAmount) =>
                ((AspenClient) client.Financial).PaymentReversalAvoidingValidation(
                    Guid.NewGuid().ToString(),
                    "CC",
                    "1073688252",
                    "80",
                    invalidAmount,
                    tags);

            // Valor menor a cero...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor igual a cero...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(0));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Sin valor
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokePaymentReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando el valor es un entero de tipo cadena...
            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("-1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            exception = Assert.Throws<AspenResponseException>(() => InvokePaymentReversalAvoidingValidation("0"));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");
        }

        [Test, Category("Autonomous-PaymentReversal"), Author("dmontalvo")]
        public void GivenAnPaymentAuthorizedWhenInvokePaymentReversalThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            Assert.DoesNotThrow(() => client.Financial.PaymentReversal("2a46bd55-cbaa-4974-b469-255a825c0143", "CC", "1073688252", "80", 10000, tags));
        }

        [Test, Category("Autonomous-PaymentReversal"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWithTransactionIdThatNotExistThenResponseNotFoundTransaction()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomTransactionId = Guid.NewGuid().ToString("D");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.PaymentReversal(randomTransactionId, "CC", "1073688252", "80", 10000, tags));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                $"No se encontró una transacción con el identificador: '{randomTransactionId}'");

            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("financialResponseCode", exception.Content.Keys);
            Assert.Contains("financialResponseMessage", exception.Content.Keys);
        }

        [Test, Category("Autonomous-PaymentReversal"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenButBifrostSystemNotWorkingThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un RoutingKey para el reverso de pago que no existe.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:PaymentReversalRoutingKey' -Value 'Bifrost.PurchaseReversalRequest.NotFound'
            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            void InvokePaymentReversal() => client.Financial.PaymentReversal(Guid.NewGuid().ToString(), "CC", "1073688252", "80", 10000, tags);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePaymentReversal);
            AssertAspenResponseException(
                exception,
                "87002",
                HttpStatusCode.BadGateway,
                "No se ha podido obtener respuesta por el sistema responsable de procesarla");
        }

        [Test, Category("Autonomous-PaymentReversal"), Author("dmontalvo")]
        public void GivenInvokePaymentReversalWhenServiceResponseWithInternalServerErrorThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Esta prueba es manual y su intención es verificar la respuesta, por lo que se debe cambiar algo en el controlador para que genere excepción no controlada.
            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            void InvokePaymentReversal() => client.Financial.PaymentReversal(Guid.NewGuid().ToString(), "CC", "1073688252", "80", 10000, tags);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokePaymentReversal);
            AssertAspenResponseException(
                exception,
                "87001",
                HttpStatusCode.InternalServerError,
                "Se presentó una excepción general procesando la operación financiera");
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
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalNotIntAmount("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CNXRKKE13I015J1FOQCP' => CONEXRED (Retiros)
            // 2. Genere un token manualmente:
            // Use Aspen.Autonomous: Send-TranToken -DocNumber '52080323' -DocType 'CC'
            Assert.DoesNotThrow(() => client.Financial.Withdrawal("CC", "52080323", "073005", "80", 10000));
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenTokenIsValidThenFinancialRequestIsUnauthorized()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para establecer un canal que NO autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'COMPMOV60A6GLOB8L013'
            // 2. Genere un token manualmente:
            // Use Aspen.Autonomous: Send-TranToken -DocNumber '52080323' -DocType 'CC'
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "904085", "80", 10000);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                "Operación financiera no autorizada");

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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para que la precisión sea menor o igual.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "166934", "80", int.MaxValue);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para que la precisión sea exacta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "408468", "80", int.MaxValue);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para que la precisión sea exacta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "122102", "80", 9999);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para sea obligatorio del tipo de cuenta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value $true
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "026676", "81", 10000);
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

            // NOTAS:
            // 1. Cambie de la configuración de la aplicación para ignorar la validación obligatoria del tipo de cuenta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAccountType' -Value $false
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "829628", "81", 10000);

            // La intención es que el supere la validación del token, no importa si la transacción no es autorizada.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawal);
            AssertAspenResponseException(
                exception,
                "87000",
                HttpStatusCode.NotAcceptable,
                "Operación financiera no autorizada");
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenNotRequiredValidateAmountTokenThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CNXRKKE13I015J1FOQCP' => CONEXRED (Retiros)
            // 1. Cambie de la configuración de la aplicación para ignorar la validación del valor del token.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmount' -Value $false
            // 2. Genere un token manualmente:
            // Use Aspen.Autonomous: Send-TranToken -DocNumber '52080323' -DocType 'CC'
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "615203", "80", 120000);
            Assert.DoesNotThrow(InvokeWithdrawal);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionEqualAmountTokenIfPrecisionIsEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CNXRKKE13I015J1FOQCP' => CONEXRED (Retiros)
            // 1. Cambie de la configuración de la aplicación para que la precición del valor sea exacta.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'Equal'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "724269", "80", 10000);
            Assert.DoesNotThrow(InvokeWithdrawal);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionLessAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CNXRKKE13I015J1FOQCP' => CONEXRED (Retiros)
            // 1. Cambie de la configuración de la aplicación para que la precición del valor sea menor o exacto.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 20000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "489928", "80", 19000);
            Assert.DoesNotThrow(InvokeWithdrawal);
        }

        [Test, Category("Autonomous-Withdrawal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalWhenAmountTransactionEqualAmountTokenIfPrecisionIsLessThanOrEqualThenFinancialRequestWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un canal que autorize la transacción.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:Channel' -Value 'CNXRKKE13I015J1FOQCP' => CONEXRED (Retiros)
            // 1. Cambie de la configuración de la aplicación para que la precición del valor sea menor o exacto.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'TokenProvider:ValidateAmountPrecision' -Value 'LessThanOrEqual'
            // 2. Genere un token manualmente:
            // Use Aspen.Delegated: Request-TranToken -Amount 10000 -AccountType '80' -PinNumber 141414
            void InvokeWithdrawal() => client.Financial.Withdrawal("CC", "52080323", "259485", "80", 10000);
            Assert.DoesNotThrow(InvokeWithdrawal);
        }

        #endregion

        #region WithdrawalReversal

        [Test, Category("Autonomous-WithdrawalReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenTransactionIdIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokeWithdrawalReversalAvoidingValidation(string invalidTransactionId) =>
                ((AspenClient)client.Financial).WithdrawalReversalAvoidingValidation(invalidTransactionId, "CC",
                    "52080323", "80", 10000, tags);

            void InvokeWithdrawalReversal(string invalidTransactionId) =>
                client.Financial.WithdrawalReversal(invalidTransactionId, "CC", "52080323", "80", 10000, tags);

            // Identificador de transacción nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            // Identificador de transacción vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            // Identificador de transacción espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            // Identificador de transacción con algún caracter inválido...
            string randomTransactionId = $"{Guid.Empty}-{Guid.Empty}-ñÑ+++";
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(randomTransactionId));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' debe coincidir con el patrón ^[a-zA-Z0-9\,\.\-\{\}\\]{1,68}$");

            // Identificador de transacción excede longitud...
            randomTransactionId = $"{Guid.Empty}-{Guid.Empty}-{Guid.Empty}";
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(randomTransactionId));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' debe coincidir con el patrón ^[a-zA-Z0-9\,\.\-\{\}\\]{1,68}$");
        }

        [Test, Category("Autonomous-WithdrawalReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokeWithdrawalReversalAvoidingValidation(string invalidDocType) =>
                ((AspenClient)client.Financial).WithdrawalReversalAvoidingValidation(Guid.NewGuid().ToString(),
                    invalidDocType, "52080323", "80", 10000, tags);

            void InvokeWithdrawalReversal(string invalidDocType) =>
                client.Financial.WithdrawalReversal(Guid.NewGuid().ToString(), invalidDocType, "52080323", "80", 10000,
                    tags);

            // Tipo de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-WithdrawalReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokeWithdrawalReversalAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient)client.Financial).WithdrawalReversalAvoidingValidation(Guid.NewGuid().ToString(), "CC",
                    invalidDocNumber, "80", 10000, tags);

            void InvokeWithdrawalReversal(string invalidDocNumber) =>
                client.Financial.WithdrawalReversal(Guid.NewGuid().ToString(), "CC", invalidDocNumber, "80", 10000, tags);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-WithdrawalReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");

            void InvokeWithdrawalReversalAvoidingValidation(string invalidAccountType) =>
                ((AspenClient)client.Financial).WithdrawalReversalAvoidingValidation(Guid.NewGuid().ToString(), "CC",
                    "52080323", invalidAccountType, 10000, tags);

            void InvokeWithdrawalReversal(string invalidAccountType) =>
                client.Financial.WithdrawalReversal(Guid.NewGuid().ToString(), "CC", "52080323", invalidAccountType, 10000,
                    tags);

            // Tipo de cuenta nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta formato
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // Tipo de cuenta longitud
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");
        }

        [Test, Category("Autonomous-WithdrawalReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void InvokeWithdrawalReversal(TagsInfo tagsInfo) =>
                client.Financial.WithdrawalReversal(Guid.NewGuid().ToString(), "CC", "52080323", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() =>
                    InvokeWithdrawalReversal(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversal(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversal(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversal(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversal(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(
                () => InvokeWithdrawalReversal(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversal(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");
        }

        [Test, Category("Autonomous-WithdrawalReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenAmountIsMissingOrLessThanOrEqualZeroThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            void InvokeWithdrawalReversalAvoidingValidation(object invalidAmount) =>
                ((AspenClient)client.Financial).WithdrawalReversalAvoidingValidation(
                    Guid.NewGuid().ToString(),
                    "CC",
                    "52080323",
                    "80",
                    invalidAmount,
                    tags);

            // Valor menor a cero...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor igual a cero...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(0));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Sin valor
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeWithdrawalReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando el valor es un entero de tipo cadena...
            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("-1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            exception = Assert.Throws<AspenResponseException>(() => InvokeWithdrawalReversalAvoidingValidation("0"));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");
        }

        [Test, Category("Autonomous-WithdrawalReversal"), Author("dmontalvo")]
        public void GivenAnWithdrawalAuthorizedWhenInvokeWithdrawalReversalThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Genere una transacción exitosa de retiro y reemplace el identificador de la transacción para el reverso.
            Assert.DoesNotThrow(() => client.Financial.WithdrawalReversal("86b3c6a3-14d6-410f-a56e-9643059309aa", "CC", "52080323", "80", 10000));
        }

        [Test, Category("Autonomous-WithdrawalReversal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWithTransactionIdThatNotExistThenResponseNotFoundTransaction()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            string randomTransactionId = Guid.NewGuid().ToString("D");
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.WithdrawalReversal(randomTransactionId, "CC", "52080323", "80", 10000));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                $"No se encontró una transacción con el identificador: '{randomTransactionId}'");

            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("financialResponseCode", exception.Content.Keys);
            Assert.Contains("financialResponseMessage", exception.Content.Keys);
        }

        [Test, Category("Autonomous-WithdrawalReversal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenButBifrostSystemNotWorkingThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 0. Cambie de la configuración de la aplicación para establecer un RoutingKey para el reverso de pago que no existe.
            // Use Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Bifrost:WithdrawalReversalRoutingKey' -Value 'Bifrost.CashWithdrawalReversalRequest.NotFound'
            void InvokeWithdrawalReversal() => client.Financial.WithdrawalReversal(Guid.NewGuid().ToString(), "CC", "52080323", "80", 10000);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawalReversal);
            AssertAspenResponseException(
                exception,
                "87002",
                HttpStatusCode.BadGateway,
                "No se ha podido obtener respuesta por el sistema responsable de procesarla");
        }

        [Test, Category("Autonomous-WithdrawalReversal"), Author("dmontalvo")]
        public void GivenInvokeWithdrawalReversalWhenServiceResponseWithInternalServerErrorThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Esta prueba es manual y su intención es verificar la respuesta, por lo que se debe cambiar algo en el controlador para que genere excepción no controlada.
            void InvokeWithdrawalReversal() => client.Financial.WithdrawalReversal(Guid.NewGuid().ToString(), "CC", "1073688252", "80", 10000);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(InvokeWithdrawalReversal);
            AssertAspenResponseException(
                exception,
                "87001",
                HttpStatusCode.InternalServerError,
                "Se presentó una excepción general procesando la operación financiera");
        }

        #endregion

        #region Refund

        [Test, Category("Autonomous-Refund-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundWhenAuthNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            void InvokeRefundAvoidingValidation(string invalidAuthNumber) =>
                ((AspenClient)client.Financial).RefundAvoidingValidation(invalidAuthNumber, "CC", "1073688252", "80", 10000, tags);

            void InvokeRefund(string invalidAuthNumber) =>
                client.Financial.Refund(invalidAuthNumber, "CC", "1073688252", "80", 10000, tags);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AuthNumber' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AuthNumber' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AuthNumber' no puede ser nulo ni vacío");

            string randomAuthNumber = $"**{new Random().Next()}++";
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(randomAuthNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AuthNumber' debe coincidir con el patrón ^[a-zA-Z0-9]{1,10}$");

            randomAuthNumber = new Random().Next().ToString("00000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(randomAuthNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AuthNumber' debe coincidir con el patrón ^[a-zA-Z0-9]{1,10}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(Guid.NewGuid().ToString()));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AuthNumber' debe coincidir con el patrón ^[a-zA-Z0-9]{1,10}$");
        }

        [Test, Category("Autonomous-Refund-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundAvoidingValidation(string invalidDocType) =>
                ((AspenClient)client.Financial).RefundAvoidingValidation(randomAuthNumber, invalidDocType, "1073688252", "80", 10000, tags);

            void InvokeRefund(string invalidDocType) =>
                client.Financial.Refund(randomAuthNumber, invalidDocType, "52080323", "80", 10000, tags);

            // Tipo de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-Refund-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient)client.Financial).RefundAvoidingValidation(randomAuthNumber, "CC", invalidDocNumber, "80", 10000, tags);

            void InvokeRefund(string invalidDocNumber) =>
                client.Financial.Refund(randomAuthNumber, "CC", invalidDocNumber, "80", 10000, tags);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-Refund-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundAvoidingValidation(string invalidAccountType) =>
                ((AspenClient)client.Financial).RefundAvoidingValidation(randomAuthNumber, "CC", "1073688252", invalidAccountType, 10000, tags);

            void InvokeRefund(string invalidAccountType) =>
                client.Financial.Refund(randomAuthNumber, "CC", "52080323", invalidAccountType, 10000, tags);

            // Tipo de cuenta nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta formato
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // Tipo de cuenta longitud
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");
        }

        [Test, Category("Autonomous-Refund-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundWhenTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefund(TagsInfo tagsInfo) =>
                client.Financial.Refund(randomAuthNumber, "CC", "1073688252", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() =>
                    InvokeRefund(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefund(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefund(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefund(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefund(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(
                () => InvokeRefund(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefund(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");
        }

        [Test, Category("Autonomous-Refund-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundWhenAmountIsMissingOrLessThanOrEqualZeroThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundAvoidingValidation(object invalidAmount) =>
                ((AspenClient)client.Financial).RefundAvoidingValidation(randomAuthNumber, "CC", "1073688252", "80", invalidAmount, tags);

            // Valor menor a cero...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor igual a cero...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(0));
            AssertAspenResponseException(
                exception,
                "15879",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Sin valor
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15879",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando el valor es un entero de tipo cadena...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("-1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundAvoidingValidation("0"));
            AssertAspenResponseException(
                exception,
                "15879",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");
        }

        [Test, Category("Autonomous-Refund"), Author("dmontalvo")]
        public void GivenAnPaymentAuthorizedWhenInvokeRefundThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Genere una transacción exitosa de pago y reemplace el número de autorización para la anulación.
            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            Assert.DoesNotThrow(() => client.Financial.Refund("129130", "CC", "1073688252", "80", 10000, tags));
        }

        [Test, Category("Autonomous-Refund"), Author("dmontalvo")]
        public void GivenInvokeRefundWithAuthNumberThatNotExistThenResponseNotFoundTransaction()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.Refund(randomAuthNumber, "CC", "1073688252", "80", 10000, tags));
            AssertAspenResponseException(
                exception,
                "15879",
                HttpStatusCode.NotFound,
                $"No se encontró una transacción con el número de autorización: '{randomAuthNumber}'");

            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("financialResponseCode", exception.Content.Keys);
            Assert.Contains("financialResponseMessage", exception.Content.Keys);
        }

        #endregion

        #region RefundReversal

        [Test, Category("Autonomous-RefundReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWhenAuthNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            void InvokeRefundReversalAvoidingValidation(string invalidTransactionId) =>
                ((AspenClient)client.Financial).RefundReversalAvoidingValidation(invalidTransactionId, "CC", "1073688252", "80", 10000, tags);

            void InvokeRefundReversal(string invalidTransactionId) =>
                client.Financial.RefundReversal(invalidTransactionId, "CC", "1073688252", "80", 10000, tags);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' no puede ser nulo ni vacío");

            string randomTransactionId = $"**{Guid.Empty}++";
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(randomTransactionId));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' debe coincidir con el patrón ^[a-zA-Z0-9\,\.\-\{\}\\]{1,68}$");

            randomTransactionId = $"{Guid.Empty}-{Guid.Empty}-{Guid.Empty}";
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(randomTransactionId));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TransactionId' debe coincidir con el patrón ^[a-zA-Z0-9\,\.\-\{\}\\]{1,68}$");
        }

        [Test, Category("Autonomous-RefundReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundReversalAvoidingValidation(string invalidDocType) =>
                ((AspenClient)client.Financial).RefundReversalAvoidingValidation(randomAuthNumber, invalidDocType, "1073688252", "80", 10000, tags);

            void InvokeRefundReversal(string invalidDocType) =>
                client.Financial.RefundReversal(randomAuthNumber, invalidDocType, "52080323", "80", 10000, tags);

            // Tipo de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("C"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'C' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("ZZZZZZ"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ZZZZZZ' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-RefundReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundReversalAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient)client.Financial).RefundReversalAvoidingValidation(randomAuthNumber, "CC", invalidDocNumber, "80", 10000, tags);

            void InvokeRefundReversal(string invalidDocNumber) =>
                client.Financial.RefundReversal(randomAuthNumber, "CC", invalidDocNumber, "80", 10000, tags);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-RefundReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundReversalAvoidingValidation(string invalidAccountType) =>
                ((AspenClient)client.Financial).RefundReversalAvoidingValidation(randomAuthNumber, "CC", "1073688252", invalidAccountType, 10000, tags);

            void InvokeRefundReversal(string invalidAccountType) =>
                client.Financial.RefundReversal(randomAuthNumber, "CC", "52080323", invalidAccountType, 10000, tags);

            // Tipo de cuenta nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta vacío...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser nulo ni vacío");

            // Tipo de cuenta formato
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // Tipo de cuenta longitud
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");
        }

        [Test, Category("Autonomous-RefundReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWhenTagsIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundReversal(TagsInfo tagsInfo) =>
                client.Financial.RefundReversal(randomAuthNumber, "CC", "1073688252", "80", 10000, tagsInfo);

            string terminalId = "X".PadRight(9, 'X');
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() =>
                    InvokeRefundReversal(new TagsInfo(terminalId: terminalId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'TerminalId' no tiene un formato valido. Debe tener la forma ^\w{1,8}$");

            string cardAcceptorId = "X".PadRight(16, 'X');
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversal(new TagsInfo(cardAcceptorId: cardAcceptorId)));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CardAcceptorId' no tiene un formato valido. Debe tener la forma ^\w{1,15}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversal(new TagsInfo(customerGroup: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversal(new TagsInfo(customerGroup: "ZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversal(new TagsInfo(customerGroup: "800")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'CustomerGroup' no tiene un formato valido. Debe tener la forma ^\d{1,2}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(new TagsInfo(pan: "XX")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(
                () => InvokeRefundReversal(new TagsInfo(pan: "ZZZZZZZZ")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(new TagsInfo(pan: "000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversal(new TagsInfo(pan: "00000")));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Pan' no tiene un formato valido. Debe tener la forma ^\d{4}$");
        }

        [Test, Category("Autonomous-RefundReversal-BadRequest"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWhenAmountIsMissingOrLessThanOrEqualZeroThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomAuthNumber = (new Random().Next(100000, 999999)).ToString();
            void InvokeRefundReversalAvoidingValidation(object invalidAmount) =>
                ((AspenClient)client.Financial).RefundReversalAvoidingValidation(randomAuthNumber, "CC", "1073688252", "80", invalidAmount, tags);

            // Valor menor a cero...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor igual a cero...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(0));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Sin valor
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() =>
                InvokeRefundReversalAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla la serialización...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando el valor es un entero de tipo cadena...
            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("-1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            exception = Assert.Throws<AspenResponseException>(() => InvokeRefundReversalAvoidingValidation("0"));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                "No se encontró una transacción");
        }

        [Test, Category("Autonomous-RefundReversal"), Author("dmontalvo")]
        public void GivenInvokeRefundReversalWithTransactionIdThatNotExistThenResponseNotFoundTransaction()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            string randomTransactionId = Guid.NewGuid().ToString();
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.RefundReversal(randomTransactionId, "CC", "1073688252", "80", 10000, tags));
            AssertAspenResponseException(
                exception,
                "15880",
                HttpStatusCode.NotFound,
                $"No se encontró una transacción con el identificador: '{randomTransactionId}'");

            Assert.NotNull(exception.Content);
            Assert.IsNotEmpty(exception.Content);
            Assert.Contains("financialResponseCode", exception.Content.Keys);
            Assert.Contains("financialResponseMessage", exception.Content.Keys);
        }

        [Test, Category("Autonomous-RefundReversal"), Author("dmontalvo")]
        public void GivenAnRefundAuthorizedWhenInvokeReversalThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            // NOTAS:
            // 1. Genere una anulación exitosa y reemplaza el identificador de la transacción.
            TagsInfo tags = new TagsInfo(cardAcceptorId: "00000014436950");
            Assert.DoesNotThrow(() => client.Financial.RefundReversal("9f3ee969-7c65-4419-bcab-4dbb6fd2243e", "CC", "1073688252", "80", 10000, tags));
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

        #region TransferAccount


        [Test]
        [Category("Autonomous-TransferAccount")]
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
                string randomDocNumber = new Random().Next().ToString("000000000000");
                client.Management.LinkTransferAccount(doc, randomDocNumber, accountRequestInfo);
            }

            foreach (string invalidDocType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(invalidDocType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("Tipo de documento en la URL no es reconocido", exc.Message);
            }
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
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
                string randomDocNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, randomDocNumber, "Atorres");
                client.Management.LinkTransferAccount("CC", randomDocNumber, accountRequestInfo);
            }

            foreach (string invalidDocType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(invalidDocType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("Tipo de documento en el BODY no es reconocido", exc.Message);
            }
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
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
                string randomDocNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(null, randomDocNumber, "SomeValue");
                client.Management.LinkTransferAccount("CC", randomDocNumber, accountRequestInfo);
            }

            foreach (string invalidDocType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(invalidDocType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'DocType' no puede ser nulo ni vacío", exc.Message);
            }
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
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
                string randomDocNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, null, "SomeValue");
                client.Management.LinkTransferAccount("CC", randomDocNumber, accountRequestInfo);
            }

            foreach (string invalidDocType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(invalidDocType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'DocNumber' no puede ser nulo ni vacío", exc.Message);
            }
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
        [Ignore("Issue:122 Valor de alias ya no es requerido. Si no se envía alias se auto-establece utilizando el tipo y número de documento.")]
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
                string randomDocNumber = new Random().Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, randomDocNumber, string.Empty);
                client.Management.LinkTransferAccount("CC", randomDocNumber, accountRequestInfo);
            }

            foreach (string invalidDocType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(invalidDocType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'Alias' no puede ser nulo ni vacío", exc.Message);
            }
        }

        /// <summary>
        /// Tipo y número de documento no puede ser el mismo en origen y destino.
        /// </summary>
        [Test]
        [Category("Autonomous-TransferAccount")]
        public void LinkTransferInvalidCardholderInfo()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var accountInfo = new TransferAccountRequestRequestInfo("CC", "79483129");
            AspenResponseException exc = Assert.Throws<AspenResponseException>(() => client.Management.LinkTransferAccount("CC", "79483129", accountInfo));
            AssertAspenResponseException(exc, "15884", HttpStatusCode.NotAcceptable, "Tarjetahabiente origen y destino son el mismo");
        }


        /// <summary>
        /// Alias es opcional. Si no se envía se auto-establece utlizando el tipo y número de documento.
        /// </summary>
        [Test]
        [Category("Autonomous-TransferAccount")]
        public void LinkTransferImplictAliasWasUsed()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            var wellKnowCardHolder = "79483129";
            var randomDocNumber = new Random().Next().ToString("000000000");
            var accountInfo = new TransferAccountRequestRequestInfo("CC", wellKnowCardHolder);
            client.Management.LinkTransferAccount("CC", randomDocNumber, accountInfo);
            var accounts = client.Management.GetTransferAccounts("CC", randomDocNumber);
            var wellKnowAccount = accounts.SingleOrDefault(a => a.Alias == $"CC-{wellKnowCardHolder}");
            Assert.IsNotNull(wellKnowAccount);
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
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
                string randomDocNumber1 = new Random(Guid.NewGuid().GetHashCode()).Next().ToString("000000000000");
                string randomDocNumber2 = new Random(Guid.NewGuid().GetHashCode()).Next().ToString("000000000000");
                ITransferAccountRequestInfo accountRequestInfo = new TransferAccountRequestRequestInfo(docType, randomDocNumber1, "áéíóú");
                client.Management.LinkTransferAccount("CC", randomDocNumber2, accountRequestInfo);
            }

            foreach (string invalidDocType in invalidDocTypes)
            {
                AspenResponseException exc = Assert.Throws<AspenResponseException>(() => LinkTransferAccount(invalidDocType));
                Assert.That(exc.StatusCode, Is.EqualTo(HttpStatusCode.NotAcceptable));
                Assert.That(exc.EventId, Is.EqualTo("15867"));
                StringAssert.IsMatch("'Alias' incluye caracteres inválidos", exc.Message);
            }
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
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
            Assert.That(accounts.Count, Is.EqualTo(0));
        }

        [Test]
        [Category("Autonomous-TransferAccount")]
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
        [Category("Autonomous-TransferAccount")]
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
            Assert.That(result.Count, Is.EqualTo(0));
        }

        #endregion
    }
}