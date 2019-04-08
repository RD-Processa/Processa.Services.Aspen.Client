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

        [Test]
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
            var amount = new Random().Next(1, 10000);

            ITokenResponseInfo token1 = this.GetToken();
            this.RedeemToken(token1.Token, amount);

            //var token2 = this.GetToken(metadata);
            //this.ReeemTokenWithMissmatchedMetadata(token2.Token, amount);

            //var token21 = this.GetToken(metadata);
            //this.RedeemToken(token21.Token, amount, metadata);

            //var token22 = this.GetToken(amount: amount);
            //this.ReeemTokenWithMissmatchedAmount(token22.Token, amount + 1);

            //var token23 = this.GetToken(amount: amount);
            //this.RedeemToken(token23.Token, amount - 1);

            //var token3 = this.GetToken(metadata, amount);
            //this.RedeemTokenNoAmount(token3.Token);

            //var token4 = this.GetToken(metadata, amount);
            //this.RedeemTokenNegativeAmount(token4.Token);

            //int accountTypesLength = 6;
            //int index = new Random(Guid.NewGuid().GetHashCode()).Next(0, accountTypesLength);
            //string accountType = Enumerable.Range(80, accountTypesLength).ToList()[index].ToString();

            //var token5 = this.GetToken(metadata, amount, accountType);
            //this.RedeemToken(token5.Token, amount, metadata, accountType);

            //var token6 = this.GetToken(null, amount, accountType);
            //this.ReeemTokenWithMissmatchedAccountType(token6.Token, amount);

            //var token7 = this.GetToken();
            //this.ReeemTokenWithInvalidDocType(token7.Token);
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
            StringAssert.IsMatch("The requested token was not found", ime.Message);
        }

        private void ReeemTokenWithMissmatchedAmount(string token, int amount)
        {
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, amount));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ime.EventId, Is.EqualTo("15875"));
            StringAssert.IsMatch("Transaction value exceeds token value", ime.Message);
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
            StringAssert.IsMatch("AccountType mismatched", ime.Message);
        }

        private void ReeemTokenWithInvalidDocType(string token)
        {
            var ime = Assert.Throws<AspenResponseException>(() => this.RedeemToken(token, 100, docType:"XXX"));
            Assert.That(ime.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(ime.EventId, Is.EqualTo("15852"));
        }
    }
}