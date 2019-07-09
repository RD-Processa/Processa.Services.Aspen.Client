// -----------------------------------------------------------------------
// <copyright file="AspenClientTestsFinancial.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-18 02:01 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Entities;
    using Fluent;
    using Fluent.Auth;
    using NUnit.Framework;

    public partial class AspenClientTests
    {
        /// <summary>
        /// Se permite consultar las cuentas de un usuario desde una aplicación autónoma.
        /// </summary>
        [Test,
         Financial,
         Autonomous,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/7"),
         Author("Atorres")]
        public void GetAccountsAutonomousScope()
        {
            // Given
            RecognizedUserInfo recognizedUserInfo = RecognizedUserInfo.Current();
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            // When            
            List<IAccountInfo> accounts = client.Financial.GetAccounts(recognizedUserInfo.DocType, recognizedUserInfo.DocNumber).ToList();

            PrintOutput("Accounts", accounts);

            // Then
            Assert.That(accounts.Count(), NUnit.Framework.Is.EqualTo(1));
            List<AccountProperty> properties = accounts.First().Properties.ToList();
            Assert.That(properties.Count(), NUnit.Framework.Is.EqualTo(4));
            Assert.That(properties.SingleOrDefault(p => p.Key == "LastTranName"), NUnit.Framework.Is.Not.Null);
            Assert.That(properties.SingleOrDefault(p => p.Key == "LastTranDate"), NUnit.Framework.Is.Not.Null);
            Assert.That(properties.SingleOrDefault(p => p.Key == "LastTranCardAcceptor"), NUnit.Framework.Is.Not.Null);
            Assert.That(properties.SingleOrDefault(p => p.Key == "CardStatusName"), NUnit.Framework.Is.Not.Null);
        }

        /// <summary>
        /// Se permite consultar las cuentas de un usuario desde una aplicación delegada.
        /// </summary>
        [Test,
         Financial,
         Delegated,
         Description("https://github.com/RD-Processa/Processa.Services.Aspen.Client/issues/8"),
         Author("Atorres")]
        public void GetAccountsDelegatedScope()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When            
            List<IAccountInfo> accounts = client.Financial.GetAccounts().ToList();

            // Then
            Assert.That(accounts.Count(), NUnit.Framework.Is.EqualTo(1));
            List<AccountProperty> properties = accounts.First().Properties.ToList();
            Assert.That(properties.Count(), NUnit.Framework.Is.EqualTo(4));
            Assert.That(properties.SingleOrDefault(p => p.Key == "LastTranName"), NUnit.Framework.Is.Not.Null);
            Assert.That(properties.SingleOrDefault(p => p.Key == "LastTranDate"), NUnit.Framework.Is.Not.Null);
            Assert.That(properties.SingleOrDefault(p => p.Key == "LastTranCardAcceptor"), NUnit.Framework.Is.Not.Null);
            Assert.That(properties.SingleOrDefault(p => p.Key == "CardStatusName"), NUnit.Framework.Is.Not.Null);
        }

        /// <summary>
        /// Una aplicación autónoma puede consultar los saldos de cuentas.
        /// </summary>
        [Test,
         Financial,
         Autonomous,
         Author("Atorres")]
        public void GetBalancesAutonomousScope()
        {
            // Given
            RecognizedUserInfo recognizedUserInfo = RecognizedUserInfo.Current();
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            // When
            List<IBalanceInfo> balances = client.Financial.GetBalances(recognizedUserInfo.DocType, recognizedUserInfo.DocNumber, recognizedUserInfo.AccountId).ToList();

            // Then
            Assert.That(balances.Count(), NUnit.Framework.Is.EqualTo(5));
        }


        /// <summary>
        /// Una aplicación delegada puede consultar los saldos de cuentas.
        /// </summary>
        [Test,
         Financial,
         Delegated,
         Author("Atorres")]
        public void GetBalancesDelegatedScope()
        {
            // Given
            RecognizedUserInfo recognizedUserInfo = RecognizedUserInfo.Current();
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            List<IBalanceInfo> balances = client.Financial.GetBalances(recognizedUserInfo.AccountId).ToList();

            // Then
            Assert.That(balances.Count(), NUnit.Framework.Is.EqualTo(5));
        }

        /// <summary>
        /// Una aplicación delegada puede consultar los movimientos de cuentas.
        /// </summary>
        [Test,
         Financial,
         Delegated,
         Author("Atorres")]
        public void GetStatementsDelegatedScope()
        {
            // Given
            RecognizedUserInfo recognizedUserInfo = RecognizedUserInfo.Current();
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            var statements = client.Financial.GetStatements( recognizedUserInfo.AccountId).ToList();

            PrintOutput("Statements", statements);

            // Then
            Assert.That(statements.Count(), NUnit.Framework.Is.EqualTo(5));
            const string AccountTypesPattern = "^80|81|82|83|84|85$";
            foreach (var statement in statements)
            {
                Assert.That(statement.AccountTypeId, NUnit.Framework.Is.Not.Null.And.Match(AccountTypesPattern));
                Assert.That(statement.Amount, NUnit.Framework.Is.AssignableTo(typeof(decimal)));
                Assert.That(statement.CardAcceptor, NUnit.Framework.Is.Not.Null);
                Assert.That(statement.TranName, NUnit.Framework.Is.Not.Null);
                Assert.That(statement.TranType, NUnit.Framework.Is.Not.Null);
            }

        }

        /// <summary>
        /// Una aplicación autónoma puede consultar los movimientos de cuentas.
        /// </summary>
        [Test,
         Financial,
         Autonomous,
         Author("Atorres")]
        public void GetStatementsAutonomousScope()
        {
            // Given
            RecognizedUserInfo recognizedUserInfo = RecognizedUserInfo.Current();
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            // When
            var statements = client.Financial.GetStatements(recognizedUserInfo.DocType, recognizedUserInfo.DocNumber, recognizedUserInfo.AccountId).ToList();

            PrintOutput("Statements", statements);

            // Then
            Assert.That(statements.Count(), NUnit.Framework.Is.EqualTo(5));
            const string AccountTypesPattern = "^80|81|82|83|84|85$";
            foreach (var statement in statements)
            {
                Assert.That(statement.AccountTypeId, NUnit.Framework.Is.Not.Null.And.Match(AccountTypesPattern));
                Assert.That(statement.Amount, NUnit.Framework.Is.AssignableTo(typeof(decimal)));
                Assert.That(statement.CardAcceptor, NUnit.Framework.Is.Not.Null);
                Assert.That(statement.TranName, NUnit.Framework.Is.Not.Null);
                Assert.That(statement.TranType, NUnit.Framework.Is.Not.Null);
            }
        }
    }
}