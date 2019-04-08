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
        public void XX()
        {
            IFluentClient client = AspenClient.Initialize()
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
        /// Se obtienen la lista de tipos de identificacíon, si la app se puede autenticar.
        /// </summary>
        [Category("Autonomous-Scope"), Test]
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
        public void yyy()
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

        [Test]
        public void RequestSingleUseTokenAutonomous()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            TagsInfo tags = new TagsInfo("XX", "xx", "zz", "xx");
            client.Financial.RequestSingleUseToken("CC", "52080323");
        }

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

        [Test]
        public void Withdrawal()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            client.Financial.Withdrawal("CC", "52080323", "585730", "80", 10000);
        }

        [Test]
        public void Payment()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            client.Financial.Payment("CC", "52080323", "585730", "80", 10000);
        }


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
                StringAssert.IsMatch("DocType en Url no es reconocido", exc.Message);
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
                StringAssert.IsMatch("DocType en Body no es reconocido", exc.Message);
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
                StringAssert.IsMatch("Alias incluye caracteres inválidos", exc.Message);
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
            StringAssert.IsMatch("La cuenta ya está registrada con otro nombre", exc.Message);
            Assert.That(exc.EventId, Is.EqualTo("15863"));  
            
            // La cuenta debe aparecer en la lista de cuentas registradas
            var accounts = client.Management.GetTransferAccounts(randomDocType, randomDocNumber);
            Assert.That(accounts.Count, Is.EqualTo(1));

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
            string docNumber = new Random().Next().ToString("000000000000");
            AspenResponseException exc = Assert.Throws<AspenResponseException>(() => client.Management.LinkTransferAccount("CC", docNumber, unknownAccountRequestInfo));
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
        public void GivenInvalidTransactionIdWhenRequestRefundThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            Assert.Throws<ArgumentNullException>(() => client.Financial.Refund(null));
            Assert.Throws<ArgumentException>(() => client.Financial.Refund(string.Empty));

            AspenResponseException badRequestException = Assert.Throws<AspenResponseException>(() => client.Financial.Refund($"{Guid.NewGuid()}-{Guid.NewGuid()}"));
            AssertAspenResponseException(
                badRequestException,
                "15852",
                HttpStatusCode.BadRequest,
                "'TransactionId' should be match with pattern");

            AspenResponseException notFoundException = Assert.Throws<AspenResponseException>(() => client.Financial.Refund("  "));
            AssertAspenResponseException(
                notFoundException,
                null,
                HttpStatusCode.NotFound,
                "Not Found");
        }

        [Test]
        [Category("Refund")]
        public void GivenValidTransactionIdWhenRequestRefundThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            string originalTrans = Guid.NewGuid().ToString();
            client.Financial.Refund(originalTrans);
        }

        /// <summary>
        /// Se produce una excepción si se intenta invocar la operación de reversión sin el identificador de la transacción original.
        /// </summary>
        /// <remarks>
        /// Given: Una solicitud de anulación
        /// When: Con un identificador de transacción original nulo o vacío
        /// Then: Se genera una excepción de tipo <see cref="ArgumentNullException"/> o <see cref="ArgumentException"/>
        /// </remarks>
        [Test]
        [Category("Reversal")]
        public void GivenInvalidTransactionIdWhenRequestReversalThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            Assert.Throws<ArgumentNullException>(() => client.Financial.Reversal(null));
            Assert.Throws<ArgumentException>(() => client.Financial.Reversal(string.Empty));

            AspenResponseException badRequestException = Assert.Throws<AspenResponseException>(() => client.Financial.Reversal($"{Guid.NewGuid()}-{Guid.NewGuid()}"));
            AssertAspenResponseException(
                badRequestException,
                "15852",
                HttpStatusCode.BadRequest,
                "'TransactionId' should be match with pattern");

            AspenResponseException notFoundException = Assert.Throws<AspenResponseException>(() => client.Financial.Reversal("  "));
            AssertAspenResponseException(
                notFoundException,
                null,
                HttpStatusCode.NotFound,
                "Not Found");
        }

        [Test]
        [Category("Reversal")]
        public void GivenValidTransactionIdWhenRequestReversalThenWorks()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            string originalTrans = Guid.NewGuid().ToString();
            client.Financial.Reversal(originalTrans);
        }
    }
}