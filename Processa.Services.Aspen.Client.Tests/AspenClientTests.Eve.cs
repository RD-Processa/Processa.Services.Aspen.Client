// -----------------------------------------------------------------------
// <copyright file="AspenClientTestsEve.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-25 10:05 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using System.Collections.Generic;
    using System.Net;
    using Entities;
    using Fluent;
    using NUnit.Framework;

    public partial class AspenClientTests
    {
        /// <summary>
        /// No se pueden consultar las cuentas desde un chatbot, con un usuario que no se ha registrado.
        /// </summary>
        [Test,
         Eve,
         Author("Atorres")]
        public void GetAccountsByAliasUnrecognizedUser()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string channelId = "94ED400D-5D26-4160-A3AA-EC45A2B4AD39";
            string enrollmentAlias = "70511510";
            void UnrecognizedUser() => client.Financial.GetAccountsByAlias(channelId, enrollmentAlias);
            AspenException exception = Assert.Throws<AspenException>(UnrecognizedUser);
            Assert.That(exception, Is.ExpectedException(
                HttpStatusCode.NotFound, 
                "15881",
                "No se ha encontrado información de enrolamiento con los valores suministrados"));
        }

        /// <summary>
        /// No se puede activar un usuario si no se proporciona un código de activación valido.
        /// </summary>
        [Eve, Test, Author("Atorres")]
        public void ValidateCodeDoesWorksIfInvalidCodeIsProvided()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string randomCode = "899129";
            string randomNickname = "70446311550";
            void ValidationFailed() => client.Management.ValidateActivationCode(randomCode, randomNickname);
            AspenException exception = Assert.Throws<AspenException>(ValidationFailed);
            Assert.That(exception, Is.ExpectedException(
                            HttpStatusCode.ExpectationFailed, 
                            "15868", 
                            "No se encontró una invitación con los valores de búsqueda proporcionados"));
        }

        /// <summary>
        /// No se pueden consultar las cuentas desde un chatbot, con un usuario que no se ha registrado.
        /// </summary>
        [Test,
         Eve,
         Author("Atorres")]
        public void GetAccountsByAliasRecognizedUser()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string channelId = "94ED400D-5D26-4160-A3AA-EC45A2B4AD39";
            string enrollmentAlias = "386034126";
            IEnumerable<IAccountInfo> response = client.Financial.GetAccountsByAlias(channelId, enrollmentAlias);
            CollectionAssert.IsEmpty(response);
        }

        [Test,
         Eve,
         Author("Atorres")]
        public void GetStatementsByAliasRecognizedUser()
        {
            IFluentClient client = AspenClient.Initialize()
                                              .RoutingTo(this.autonomousAppInfoProvider)
                                              .WithIdentity(this.autonomousAppInfoProvider)
                                              .Authenticate()
                                              .GetClient();

            string channelId = "AB621914-0355-41AF-A69E-E5C4F282AF4A";
            string enrollmentAlias = "39301076";
            var response = client.Financial.GetStatementsByAlias(channelId, enrollmentAlias, "5259838650470180");
            CollectionAssert.IsEmpty(response);
        }
    }
}