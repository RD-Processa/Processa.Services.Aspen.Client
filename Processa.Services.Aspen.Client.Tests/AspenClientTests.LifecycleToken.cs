// -----------------------------------------------------------------------
// <copyright file="AspenClientTests.LifecycleToken.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-13 06:45 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using Entities;
    using Fluent;
    using Fluent.Auth;
    using NUnit.Framework;

    public partial class AspenClientTests
    {
        private IFluentClient delegatedClient;

        private IFluentClient autonomousClient;

        private DelegatedUserInfo userInfo;

        private string docType;

        private string docNumber;

        private const string WellKnownPin = "141414";

        [Test, Category("Autonomous-Redeem-Token")]
        public void TokenLifeCycleWorks()
        {
            this.userInfo = GetDelegatedUserCredentials();
            this.delegatedClient = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(this.userInfo)
                                              .GetClient();

            this.autonomousClient = AspenClient.Initialize()
                                                .RoutingTo(this.autonomousAppInfoProvider)
                                                .WithIdentity(this.autonomousAppInfoProvider)
                                                .Authenticate(useCache: false)
                                                .GetClient();

            this.docType = this.userInfo["DocType"].ToString();
            this.docNumber = this.userInfo["DocNumber"].ToString();           
            string metadata = Guid.NewGuid().ToString("B");
            int amount = new Random().Next(1, 10000);

            ITokenResponseInfo token1 = this.GetToken();
            this.RedeemToken(token1.Token, amount);

            ITokenResponseInfo token3 = this.GetToken(metadata, amount);
            this.RedeemTokenNoAmount(token3.Token);

            ITokenResponseInfo token4 = this.GetToken(metadata, amount);
            this.RedeemTokenNegativeAmount(token4.Token);

            int accountTypesLength = 6;
            int index = new Random(Guid.NewGuid().GetHashCode()).Next(0, accountTypesLength);
            string accountType = Enumerable.Range(80, accountTypesLength).ToList()[index].ToString();

            ITokenResponseInfo token5 = this.GetToken(metadata, amount, accountType);
            this.RedeemToken(token5.Token, amount, metadata, accountType);

            ITokenResponseInfo token6 = this.GetToken(null, amount, accountType);
            this.ReeemTokenWithMissmatchedAccountType(token6.Token, amount);

            ITokenResponseInfo token7 = this.GetToken();
            this.ReeemTokenWithInvalidDocType(token7.Token);
        }

        [Test, Category("Autonomous-Redeem-Token")]
        public void ReedemTokenWithMissmatchedMetadata()
        {
            this.userInfo = GetDelegatedUserCredentials();
            this.delegatedClient = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(this.userInfo)
                                              .GetClient();

            this.autonomousClient = AspenClient.Initialize()
                                                .RoutingTo(this.autonomousAppInfoProvider)
                                                .WithIdentity(this.autonomousAppInfoProvider)
                                                .Authenticate(useCache: false)
                                                .GetClient();

            this.docType = this.userInfo["DocType"].ToString();
            this.docNumber = this.userInfo["DocNumber"].ToString();
            string metadata = Guid.NewGuid().ToString("B");
            var amount = new Random().Next(1, 10000);

            ITokenResponseInfo token = this.GetToken(metadata);
            this.ReeemTokenWithMissmatchedMetadata(token.Token, amount);
        }

        [Test, Category("Autonomous-Redeem-Token")]
        public void ReedemTokenWithMissmatchedAmount()
        {
            this.userInfo = GetDelegatedUserCredentials();
            this.delegatedClient = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(this.userInfo)
                .GetClient();

            this.autonomousClient = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate(useCache: false)
                .GetClient();

            this.docType = this.userInfo["DocType"].ToString();
            this.docNumber = this.userInfo["DocNumber"].ToString();
            int amount = new Random().Next(1, 10000);

            ITokenResponseInfo token1 = this.GetToken(amount: amount);
            this.ReeemTokenWithMissmatchedAmount(token1.Token, amount + 1);

            ITokenResponseInfo token2 = this.GetToken(amount: amount);
            this.ReeemTokenWithMissmatchedAmount(token2.Token, amount - 1);
        }

        [Test, Category("Autonomous-Redeem-Token"), Author("dmontalvo")]
        public void GivenReedemTokenWhenDocTypeIsMissingOrNotRecognizedThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void ValidateSingleUseTokenAvoidingValidation(string invalidDocType) =>
                ((AspenClient)client.Financial).ValidateSingleUseTokenAvoidingValidation(invalidDocType, "52080323", "000000", accountType: "80", amount: 10000);

            void ValidateSingleUseToken(string invalidDocType) =>
                client.Financial.ValidateSingleUseToken(invalidDocType, "52080323", "000000", accountType: "80", amount: 10000);

            // Tipo de documento nulo...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");


            // Tipo de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocType' no puede ser nulo ni vacío");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("X"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'X' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XX' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'XXXXX' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'1' no se reconoce como un tipo de identificación");

            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("10"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'10' no se reconoce como un tipo de identificación");


            // Tipo de documento no reconocido...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("10000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'10000' no se reconoce como un tipo de identificación");
        }

        [Test, Category("Autonomous-Redeem-Token"), Author("dmontalvo")]
        public void GivenReedemTokenWhenDocNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void ValidateSingleUseTokenAvoidingValidation(string invalidDocNumber) =>
                ((AspenClient)client.Financial).ValidateSingleUseTokenAvoidingValidation("CC", invalidDocNumber, "000000", accountType: "80", amount: 10000);

            void ValidateSingleUseToken(string invalidDocNumber) =>
                client.Financial.ValidateSingleUseToken("CC", invalidDocNumber, "000000", accountType: "80", amount: 10000);

            // Número de documento nulo...
            AspenResponseException exception =
                Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento vacío...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento espacios en blanco...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("    "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' no puede ser nulo ni vacío");

            // Número de documento solo letras...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento letras y números...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("X1X2X3X4X5"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento números y espacios...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken(" 123 456 "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Caracteres especiales...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("*123456*"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");

            // Número de documento excede longitud...
            string randomDocNumber = new Random().Next().ToString("00000000000000000000");
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken(randomDocNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'DocNumber' debe coincidir con el patrón ^\d{1,18}$");
        }

        [Test, Category("Autonomous-Redeem-Token"), Author("dmontalvo")]
        public void GivenReedemTokenWhenAccountTypeIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void ValidateSingleUseTokenAvoidingValidation(string accountType) =>
                ((AspenClient)client.Financial).ValidateSingleUseTokenAvoidingValidation("CC", "52080323", "000000", accountType: accountType, amount: 10000);

            void ValidateSingleUseToken(string accountType) =>
                client.Financial.ValidateSingleUseToken("CC", "52080323", "000000", accountType: accountType, amount: 10000);

            // Aunque el tipo de cuenta no es obligatorio, no se acepta el vacío.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("8*"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            // No requiere el tipo de cuenta y puede ser nulo.
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15875",
                HttpStatusCode.NotFound,
                @"Falló la redención del token. No se encontró un token con los valores proporcionados");

            // No requiere el tipo de cuenta y no requiere ser incluido en el body.
            exception = Assert.Throws<AspenResponseException>(() => ((AspenClient)client.Financial).ValidateSingleUseTokenAvoidingValidation(
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

        [Test, Category("Autonomous-Redeem-Token"), Author("dmontalvo")]
        public void GivenReedemTokenWhenMetadataIsInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void ValidateSingleUseTokenAvoidingValidation(string metadata) =>
                ((AspenClient)client.Financial).ValidateSingleUseTokenAvoidingValidation("CC", "52080323", "000000", accountType: "80", amount: 10000, metadata: metadata);

            void ValidateSingleUseToken(string metadata) =>
                client.Financial.ValidateSingleUseToken("CC", "52080323", "000000", accountType: "80", amount: 10000, metadata: metadata);

            // Aunque el metadata no es obligatorio, no se acepta el vacío.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' no puede ser vacío");

            string[] metadatas =
            {
                "áéíóú",
                "ÁÉÍÓÚ",
                $"{Guid.NewGuid()}-{Guid.NewGuid()}"
            };

            foreach (string metadata in metadatas)
            {
                exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseToken(metadata));
                AssertAspenResponseException(
                    exception,
                    "15852",
                    HttpStatusCode.BadRequest,
                    @"'Metadata' debe coincidir con el patrón ^[ -~]{1,50}$");
            }
        }

        [Test, Category("Autonomous-Redeem-Token"), Author("dmontalvo")]
        public void GivenReedemTokenWhenAmountIsInvalidThenAnExceptionIsThrows()
        {
            IFluentClient client = AspenClient.Initialize()
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider)
                .Authenticate()
                .GetClient();

            void ValidateSingleUseTokenAvoidingValidation(object amount) =>
                ((AspenClient)client.Financial).ValidateSingleUseTokenAvoidingValidation("CC", "52080323", "000000", accountType: "80", amount: amount);

            // Cuando se establece vacío, la serialización lo interpreta como nulo
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' no puede ser nulo ni vacío");

            // Valor menor a cero...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor menor a cero...
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("-1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Cuando no es un valor entero falla por serialización.
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla por serialización.
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla por serialización.
            exception = Assert.Throws<AspenResponseException>(() => ValidateSingleUseTokenAvoidingValidation("10000000000000000"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");
        }

        private ITokenResponseInfo GetToken(string metadata = null, int? amount = null, string accountType = null)
        {            
            ITokenResponseInfo tokenInfo = this.delegatedClient.Financial.GetSingleUseToken(WellKnownPin, metadata, amount, accountType);
            Assert.IsNotEmpty(tokenInfo.Token);
            Assert.IsTrue(tokenInfo.ExpirationMinutes > 0);
            Assert.IsTrue(tokenInfo.ExpiresAt > DateTimeOffset.Now);
            return tokenInfo;
        }

        private void RedeemToken(string token, int? amount, string metadata = null, string accountType = null, string docType = null, string docNumber = null)
        {
            this.autonomousClient.Financial.ValidateSingleUseToken(
                docType ?? this.docType, 
                docNumber ?? this.docNumber,
                token,
                metadata,
                amount,
                accountType);
        }

        private void ReeemTokenWithMissmatchedMetadata(string token, int amount)
        {
            string randomMetadata = Guid.NewGuid().ToString("P");
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, amount, randomMetadata));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ime.EventId, Is.EqualTo("15875"));
            StringAssert.IsMatch("No se encontró un token con los valores proporcionados", ime.Message);
        }

        private void ReeemTokenWithMissmatchedAmount(string token, int amount)
        {
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, amount));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ime.EventId, Is.EqualTo("15875"));
            StringAssert.IsMatch("El valor de la transacción no coincide con el valor del token", ime.Message);
        }

        private void RedeemTokenNoAmount(string token)
        {
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, null));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(ime.EventId, Is.EqualTo("15852"));
            StringAssert.IsMatch("'Amount'", ime.Message);
        }

        private void RedeemTokenNegativeAmount(string token)
        {
            int randomAmount = new Random().Next(int.MinValue, -1);
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, randomAmount));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(ime.EventId, Is.EqualTo("15852"));
            StringAssert.IsMatch("'Amount'", ime.Message);
        }

        private void ReeemTokenWithMissmatchedAccountType(string token, int amount)
        {
            string randomAccountType = new Random().Next(10, 80).ToString("00");
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, amount, accountType: randomAccountType));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ime.EventId, Is.EqualTo("15875"));
            StringAssert.IsMatch("Tipo de cuenta de la transacción no coincide con el tipo de cuenta del token", ime.Message);
        }

        private void ReeemTokenWithInvalidDocType(string token)
        {
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, 100, docType:"XXX"));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(ime.EventId, Is.EqualTo("15852"));
        }
    }
}