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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Entities;
    using Fluent;
    using Fluent.Auth;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Processa.Services.Aspen.Client.Fluent.Contracts;

    /// <summary>
    /// Implementa pruebas unitarias de la clase <see cref="AspenClient"/>.
    /// </summary>
    public partial class AspenClientTests
    {
        #region Signin

        /// <summary>
        /// Se emite un token de autenticación para un usuario cuando la credencial corresponde a una válida por el sistema.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación con valores válidos
        /// Then: Se genera un token de autenticación para el usuario
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenAnUserAuthTokenIsGenerated()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);

            // Then
            Assert.That(client.AuthToken, Is.TypeOf<UserAuthToken>());
        }

        /// <summary>
        /// Se produce una excepción por permisos si se usa una aplicación autonoma.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario
        /// When: Cuando se invoca o solicita la autenticación usando una aplicación autónoma
        /// Then: Entonces se produce una excepción por permisos
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenAUserIdentityWhenInvokeAuthenticateWithUsingApiKeyAutonomousThenAnUserAuthTokenIsGenerated()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.autonomousAppInfoProvider)
                .WithIdentity(this.autonomousAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(userCredentials);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("1000478"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("ApiKey no tiene permisos para realizar la operación. Alcance requerido: 'Delegated'"));
        }

        /// <summary>
        /// Se produce una excepción de autenticación si el usuario no es reconocido en el sistema.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario no reconocida
        /// When: Cuando se invoca o solicita la autenticación
        /// Then: Entonces se produce una excepción de autenticación
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenAUnrecognizedUserIdentityWhenInvokeAuthenticateThenAnExceptionIsThrows()
        {
            // Given
            string fixedDocType = "PAS";
            string randomDocNumber = new Random().Next(1000000000, int.MaxValue).ToString();
            string password = Guid.Empty.ToString();
            DelegatedUserInfo delegatedUserInfo = new DelegatedUserInfo(fixedDocType, randomDocNumber, password);
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(delegatedUserInfo);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("97412"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Combinación de usuario y contraseña invalida. Por favor revise los valores ingresados e intente de nuevo"));
        }

        /// <summary>
        /// Se produce una excepción de autenticación si la credencial del usuario no es válida.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación con una credencial inválida
        /// Then: Entonces se produce una excepción de autenticación
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithInvalidCredentialThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            userCredentials["Password"] = Guid.Empty.ToString();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(userCredentials);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("97414"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Combinación de usuario y contraseña invalida. Por favor revise los valores ingresados e intente de nuevo"));
        }

        /// <summary>
        /// Se produce una excepción de autenticación por bloqueo de intentos inválidos.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación con una credencial inválida y se intenta las veces permitidas
        /// Then: Entonces el usuario será bloqueado y se produce una excepción de autenticación.
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithFailedAttemptsThenUserWillBeLockedOutAndExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            userCredentials["Password"] = Guid.Empty.ToString();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);
            int maxFailedPasswordAttempt = 10;

            // When
            void AuthFails() => client.Authenticate(userCredentials);
            AspenResponseException exception = null;
            for (int index = 1; index <= maxFailedPasswordAttempt - 1; index++)
            {
                exception = Assert.Throws<AspenResponseException>(AuthFails);
                Assert.That(exception.EventId, Is.EqualTo("97414"));
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                Assert.That(exception.Message, Is.Not.Null.And.Matches("Combinación de usuario y contraseña invalida. Por favor revise los valores ingresados e intente de nuevo"));
            }

            // Then
            exception = Assert.Throws<AspenResponseException>(AuthFails);
            Assert.That(exception.EventId, Is.EqualTo("97415"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Usuario ha sido bloqueado por superar el número máximo de intentos de sesión inválidos"));
        }

        /// <summary>
        /// Se produce una excepción de autenticación cuando el usario está bloqueado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida pero bloqueada
        /// When: Cuando se invoca o solicita la autenticación
        /// Then: Entonces se produce una excepción de autenticación.
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithLockoutThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(userCredentials);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("97413"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Usuario está bloqueado por superar el número máximo de intentos de sesión inválidos"));
        }

        /// <summary>
        /// Se produce una excepción de autenticación cuando el usuario existe pero no tiene credenciales establecidas.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocido sin credenciales en el perfil
        /// When: Cuando se invoca o solicita la autenticación
        /// Then: Entonces se produce una excepción de autenticación.
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithMissingCredentialInProfileThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo delegatedUserInfo = new DelegatedUserInfo("CC", "1067888455", "colombia");
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(delegatedUserInfo);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("97416"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("Combinación de usuario y contraseña invalida. Por favor revise los valores ingresados e intente de nuevo"));
        }

        /// <summary>
        /// Se produce una excepción cuando el formato del secreto del usuario es valor inesperado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida con el formato del secreto inválido
        /// When: Cuando se invoca o solicita la autenticación
        /// Then: Entonces se produce una excepción por error interno de servidor.
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithInvalidFormatSecretThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo delegatedUserInfo = new DelegatedUserInfo("CC", "1067888455", "colombia");
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(delegatedUserInfo);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("97417"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("No es posible verificar las credenciales del usuario"));
        }

        /// <summary>
        /// Se produce una excepción cuando se envía un nonce nulo o vacío en la carga de trabajo.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación con un nonce nulo o vacío
        /// Then: Entonces se produce una excepción
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithNullOrEmptyNonceThenAnExceptionIsThrows()
        {
            const AppScope Scope = AppScope.Delegated;
            IEpochGenerator epochGenerator = new FutureEpochGenerator();
            List<ISettings> testSettings = new List<ISettings>()
            {
                new HardCodedSettings(new NullEmptyNonceGenerator(), epochGenerator, Scope),
                new HardCodedSettings(new NullEmptyNonceGenerator(string.Empty), epochGenerator, Scope),
                new HardCodedSettings(new NullEmptyNonceGenerator("   "), epochGenerator, Scope)
            };

            foreach (ISettings settings in testSettings)
            {
                IFluentClient client = AspenClient.Initialize(settings)
                    .RoutingTo(this.delegatedAppInfoProvider)
                    .WithIdentity(this.delegatedAppInfoProvider);

                // When
                DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
                void AuthFails() => client.Authenticate(userCredentials);
                AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

                // Then
                Assert.That(exception.EventId, Is.EqualTo("15852"));
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(exception.Message, Is.Not.Null.And.Matches("'Nonce' no puede ser nulo ni vacío"));
            }
        }

        /// <summary>
        /// Se produce una excepción cuando se envía un nonce que supera la longitud válida en la carga de trabajo.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación con un nonce que excede la longitud
        /// Then: Entonces se produce una excepción
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateWithExceedsLengthNonceThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();

            AppScope scope = AppScope.Delegated;
            IEpochGenerator epochGenerator = new FutureEpochGenerator();
            INonceGenerator nonceGenerator = new SingleUseNonceGenerator($"{Guid.NewGuid()}-{Guid.NewGuid()}");
            IFluentClient client = AspenClient.Initialize(new HardCodedSettings(nonceGenerator, epochGenerator, scope))
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When
            void AuthFails() => client.Authenticate(userCredentials);
            AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

            // Then
            Assert.That(exception.EventId, Is.EqualTo("15852"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("'Nonce' debe coincidir con el patrón"));
        }

        /// <summary>
        /// Se produce una excepción si no se envían las credenciales del usuario.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación con valores faltantes
        /// Then: Entonces se produce una excepción
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenAUserIdentityWhenInvokeAuthenticateWithMissingValuesThenAnExceptionIsThrows()
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
            Assert.That(exception.Message, Is.Not.Null.And.Matches("'DeviceId' no puede ser nulo ni vacío"));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("'DocType' no puede ser nulo ni vacío"));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("'DocNumber' no puede ser nulo ni vacío"));
            Assert.That(exception.Message, Is.Not.Null.And.Matches("'Password' no puede ser nulo ni vacío"));
        }

        /// <summary>
        /// Se produce una excepción si no se envía el valor esperado del tipo de documento en la carga de trabajo.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación sin el valor del tipo de documento
        /// Then: Entonces se produce una excepción
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenAUserIdentityWhenInvokeAuthenticateWithInvalidDocTypeThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            List<string> docTypes = new List<string>()
            {
                null,
                string.Empty,
                "   ",
                "XX",
                "YYYYYY"
            };

            foreach (string dt in docTypes)
            {
                userCredentials["DocType"] = dt;
                IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                    .RoutingTo(this.delegatedAppInfoProvider)
                    .WithIdentity(this.delegatedAppInfoProvider);

                // When
                void AuthFails() => client.Authenticate(userCredentials);
                AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

                // Then
                Assert.That(exception.EventId, Is.EqualTo("15852"));
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                if (string.IsNullOrWhiteSpace(dt))
                {
                    Assert.That(exception.Message, Is.Not.Null.And.Matches("'DocType' no puede ser nulo ni vacío"));
                    continue;
                }

                Assert.That(exception.Message, Is.Not.Null.And.Matches($"'{dt}' no se reconoce como un tipo de identificación"));
            }
        }

        /// <summary>
        /// Se produce una excepción si no se envía el valor esperado del número de documento en la carga de trabajo.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación sin el valor del número de documento
        /// Then: Entonces se produce una excepción
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenAUserIdentityWhenInvokeAuthenticateWithInvalidDocNumberThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            List<string> docNumbers = new List<string>()
            {
                null,
                string.Empty,
                "   ",
                "XXXXX",
                $"{int.MaxValue}{int.MaxValue}"
            };

            foreach (string value in docNumbers)
            {
                userCredentials["docNumber"] = value;
                IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                    .RoutingTo(this.delegatedAppInfoProvider)
                    .WithIdentity(this.delegatedAppInfoProvider);

                // When
                void AuthFails() => client.Authenticate(userCredentials);
                AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

                // Then
                Assert.That(exception.EventId, Is.EqualTo("15852"));
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                if (string.IsNullOrWhiteSpace(value))
                {
                    Assert.That(exception.Message, Is.Not.Null.And.Matches("'DocNumber' no puede ser nulo ni vacío"));
                    continue;
                }

                Assert.That(exception.Message, Is.Not.Null.And.Matches("'DocNumber' debe coincidir con el patrón"));
            }
        }

        /// <summary>
        /// Se produce una excepción si no se envía el valor esperado de la contraseña en la carga de trabajo.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Cuando se invoca o solicita la autenticación sin el valor de la contraseña
        /// Then: Entonces se produce una excepción
        /// </remarks>
        [Category("User-Signin"), Test]
        public void GivenAUserIdentityWhenInvokeAuthenticateWithNullOrEmptyPasswordThenAnExceptionIsThrows()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            List<string> passwords = new List<string>()
            {
                null,
                string.Empty,
                "   "
            };

            foreach (string password in passwords)
            {
                userCredentials["Password"] = password;
                IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                    .RoutingTo(this.delegatedAppInfoProvider)
                    .WithIdentity(this.delegatedAppInfoProvider);

                // When
                void AuthFails() => client.Authenticate(userCredentials);
                AspenResponseException exception = Assert.Throws<AspenResponseException>(AuthFails);

                // Then
                Assert.That(exception.EventId, Is.EqualTo("15852"));
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(exception.Message, Is.Not.Null.And.Matches("'Password' no puede ser nulo ni vacío"));
            }
        }

        #endregion

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
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            // When
            var accounts = client.Financial.GetAccounts();

            // Then
            CollectionAssert.IsNotEmpty(accounts);
        }

        #region RequestActivationCode

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            Assert.DoesNotThrow(() => client.CurrentUser.RequestActivationCode());
        }


        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButMessageTemplateIsNullOrEmptyThenWorkUsingDefaultTemplate()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para la plantilla del mensaje.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'GuessWho:ActivationCodeMessageTemplate' -Value $null
            Assert.DoesNotThrow(() => client.CurrentUser.RequestActivationCode());
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButNotConfiguredToSendMessageThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para establecer en falso el envío de códigos de activación.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'GuessWho:SendActivationCode' -Value 'false'
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20099",
                HttpStatusCode.NotImplemented,
                @"No se ha establecido la configuración para el envío de mensajes de código de activación");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButCampaignGuidIsMissingThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para remover el identificador de la campaña.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Remove-AppSetting -Key 'GuessWho:CampaignGuid'
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20099",
                HttpStatusCode.NotImplemented,
                @"No se ha establecido la configuración para el envío de mensajes de código de activación");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButCampaignGuidIsNullOrEmptyThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para establecer en nula o vacía, la propiedad del identificador de la campaña.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'GuessWho:CampaignGuid' -Value $null
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20099",
                HttpStatusCode.NotImplemented,
                @"No se ha establecido la configuración para el envío de mensajes de código de activación");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButChannelGuidIsMissingThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para remover el identificador del canal.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Remove-AppSetting -Key 'GuessWho:ChannelGuid'
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20099",
                HttpStatusCode.NotImplemented,
                @"No se ha establecido la configuración para el envío de mensajes de código de activación");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButChannelGuidIsNullOrEmptyThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para establecer en nula o vacía, la propiedad del identificador de la campaña.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'GuessWho:ChannelGuid' -Value $null
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20099",
                HttpStatusCode.NotImplemented,
                @"No se ha establecido la configuración para el envío de mensajes de código de activación");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButMessageTemplateIsWhiteSpacesThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración para la plantilla del mensaje.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'GuessWho:ActivationCodeMessageTemplate' -Value '   '
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20099",
                HttpStatusCode.NotImplemented,
                @"No se ha establecido la configuración para el envío de mensajes de código de activación");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButKrakenSystemNotWorkingThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración de KRAKEN para usar una cola que no existe y así imitar un timeout.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Kraken:SendMessageRoutingKey' -Value 'Kraken.NotificationRoutingKey.NotFound'
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
            AssertAspenResponseException(
                exception,
                "20100",
                HttpStatusCode.ServiceUnavailable,
                @"No fue posible enviar el mensaje. Por favor vuelva a intentar en unos minutos");
        }

        [Test, Category("Delegated-Send-Activation-Code"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenRequestActivationCodeButResponseFromKrakenSystemWasUnsuccessfulThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // NOTA: Debe cambiar la configuración de KRAKEN para usar una cola que no existe y así imitar un timeout.
            // Use el comando de Aspen.Core: Get-App -AppKey 'MyAppKey' | Set-AppSetting -Key 'Kraken:SendMessageRoutingKey' -Value 'Kraken.NotificationRoutingKey.NotFound'
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.RequestActivationCode());
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
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
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
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            var balances = client.Financial.GetBalances("203945");
            Console.WriteLine(JsonConvert.SerializeObject(balances, Formatting.Indented));
            CollectionAssert.IsNotEmpty(balances);
        }

        [Test]
        public void GetStatementsWorksForDelegatedApps()
        {
            DelegatedUserInfo delegatedUser = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(delegatedUser)
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

        #region GetSingleUseToken

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // El canal no es requerido y puede ser nulo, por lo que se debe generar un token.
            Assert.DoesNotThrow(() => client.Financial.GetSingleUseToken("141414"));

            // Cuando no se establece canal, la respuesta debe indicar que se usó el predeterminado.
            ITokenResponseInfo tokenResponseInfo = client.Financial.GetSingleUseToken("141414");
            Assert.NotNull(tokenResponseInfo);
            Assert.NotNull(tokenResponseInfo.Token);
            Assert.NotNull(tokenResponseInfo.ChannelKey);
            Assert.AreEqual("*", tokenResponseInfo.ChannelKey);

            // Cuando se establece un canal, en el resultado debe coincidir con el mismo canal.
            IList<ChannelInfo> channels = client.Settings.GetChannels();
            CollectionAssert.IsNotEmpty(channels);

            foreach (ChannelInfo channel in channels)
            {
                tokenResponseInfo = client.Financial.GetSingleUseToken("141414", channelKey: channel.Key);
                Assert.NotNull(tokenResponseInfo);
                Assert.NotNull(tokenResponseInfo.Token);
                Assert.NotNull(tokenResponseInfo.ChannelKey);
                Assert.AreEqual(channel.Key, tokenResponseInfo.ChannelKey);
            }
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButPinNumberIsMissingOrInvalidThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            void GetSingleUseTokenAvoidingValidation(string pinNumber) =>
                ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation(pinNumber);

            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation(null));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'PinNumber' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'PinNumber' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'PinNumber' no puede ser nulo ni vacío");

            exception = Assert.Throws<AspenResponseException>(() => client.Financial.GetSingleUseToken("áéíóú"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'PinNumber' debe coincidir con el patrón ^[ -~]{1,10}$");

            string randomPinNumber = Guid.NewGuid().ToString("N").Substring(0, 12);
            exception = Assert.Throws<AspenResponseException>(() => client.Financial.GetSingleUseToken(randomPinNumber));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'PinNumber' debe coincidir con el patrón ^[ -~]{1,10}$");

            exception = Assert.Throws<AspenResponseException>(() => client.Financial.GetSingleUseToken(randomPinNumber, "áéíóú"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'PinNumber' debe coincidir con el patrón ^[ -~]{1,10}$");
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButCheckPinNumberIsInvalidThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            string[] randomPinNumbers =
            {
                Guid.NewGuid().ToString("N").Substring(0, 10),
                "yyyy",
                "xxxxxx",
                new Random().Next(1000, 9999).ToString(),
                new Random().Next(10000, 99999).ToString(),
                new Random().Next(100000, 999999).ToString(),
                $"++{new Random().Next(999, 9999)}++"
            };

            foreach (string randomPinNumber in randomPinNumbers)
            {
                AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.Financial.GetSingleUseToken(randomPinNumber));
                AssertAspenResponseException(
                    exception,
                    "15862",
                    HttpStatusCode.Unauthorized,
                    @"Pin de usuario/app no es valido. No corresponde con el registrado");
            }
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButAccountTypeIsInvalidThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            void GetSingleUseTokenAvoidingValidation(string accountType) =>
                ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", accountType: accountType);

            ITokenResponseInfo GetSingleUseToken(string accountType) =>
                client.Financial.GetSingleUseToken("141414", accountType: accountType);

            // Aunque el tipo de cuenta no es obligatorio, no se acepta el vacío.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken("XX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken("XXXXX"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken("8*"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken("8000"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'AccountType' debe coincidir con el patrón ^\d{1,3}$");
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButChannelKeyIsInvalidThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            void GetSingleUseTokenAvoidingValidation(string channelKey) =>
                ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", channelKey: channelKey);

            ITokenResponseInfo GetSingleUseToken(string channelKey) =>
                client.Financial.GetSingleUseToken("141414", channelKey: channelKey);

            // Aunque el canal no es obligatorio, no se acepta vacío.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ChannelKey' no puede ser vacío");

            // Aunque el canal no es obligatorio, no se acepta solo espacios en blanco.
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'ChannelKey' no puede ser vacío");

            string invalidChannelKey = "Y";
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
            AssertAspenResponseException(
                exception,
                "15858",
                HttpStatusCode.BadRequest,
                $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

            invalidChannelKey = "XX";
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
            AssertAspenResponseException(
                exception,
                "15858",
                HttpStatusCode.BadRequest,
                $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

            invalidChannelKey = "XXXX";
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
            AssertAspenResponseException(
                exception,
                "15858",
                HttpStatusCode.BadRequest,
                $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

            invalidChannelKey = new Random().Next(99, 99999).ToString();
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
            AssertAspenResponseException(
                exception,
                "15858",
                HttpStatusCode.BadRequest,
                $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

            invalidChannelKey = "****";
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
            AssertAspenResponseException(
                exception,
                "15858",
                HttpStatusCode.BadRequest,
                $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

            invalidChannelKey = Guid.NewGuid().ToString();
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
            AssertAspenResponseException(
                exception,
                "15858",
                HttpStatusCode.BadRequest,
                $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

            // Aunque el canal existe, el nombre debe coincidir exactamente.
            IList<ChannelInfo> channels = client.Settings.GetChannels();
            CollectionAssert.IsNotEmpty(channels);

            foreach (ChannelInfo channel in channels)
            {
                invalidChannelKey = channel.Key.ToLower();
                exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
                AssertAspenResponseException(
                    exception,
                    "15858",
                    HttpStatusCode.BadRequest,
                    $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

                invalidChannelKey = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(channel.Key.ToLower());
                exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
                AssertAspenResponseException(
                    exception,
                    "15858",
                    HttpStatusCode.BadRequest,
                    $"No existe un canal con el código proporcionado '{invalidChannelKey}'");

                invalidChannelKey = $" {channel.Key} ";
                exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(invalidChannelKey));
                AssertAspenResponseException(
                    exception,
                    "15858",
                    HttpStatusCode.BadRequest,
                    $"No existe un canal con el código proporcionado '{invalidChannelKey}'");
            }
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButMetadataIsInvalidThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            void GetSingleUseTokenAvoidingValidation(string metadata) =>
                ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", metadata: metadata);

            ITokenResponseInfo GetSingleUseToken(string metadata) =>
                client.Financial.GetSingleUseToken("141414", metadata: metadata);

            // Aunque el metadata no es obligatorio, no se acepta el vacío.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation(string.Empty));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Metadata' no puede ser vacío");

            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("   "));
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
                exception = Assert.Throws<AspenResponseException>(() => GetSingleUseToken(metadata));
                AssertAspenResponseException(
                    exception,
                    "15852",
                    HttpStatusCode.BadRequest,
                    @"'Metadata' debe coincidir con el patrón ^[ -~]{1,50}$");
            }
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButAmountIsInvalidThenAnExceptionIsThrows()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            void GetSingleUseTokenAvoidingValidation(object amount) =>
                ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", amount: amount);

            // Valor menor a cero...
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation(-1));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Valor menor a cero...
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("-1"));
            AssertAspenResponseException(
                exception,
                "15852",
                HttpStatusCode.BadRequest,
                @"'Amount' debe ser mayor que cero");

            // Cuando no es un valor entero falla por serialización.
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("   "));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla por serialización.
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("XXX"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");

            // Cuando no es un valor entero falla por serialización.
            exception = Assert.Throws<AspenResponseException>(() => GetSingleUseTokenAvoidingValidation("10000000000000000"));
            AssertAspenResponseException(
                exception,
                "15883",
                HttpStatusCode.BadRequest,
                @"Valor inesperado al analizar los datos de solicitud en formato JSON");
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButAccountTypeIsNullOrMissingThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // El tipo de cuenta no es requerido y puede ser nulo, por lo que se debe generar un token.
            Assert.DoesNotThrow(() => client.Financial.GetSingleUseToken("141414"));

            // No se requiere incluir la propiedad en el body, por que debe generar un token.
            Assert.DoesNotThrow(() => ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", excludeAccountType: true));
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButChannelKeyIsNullOrMissingThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // El canal no es requerido y puede ser nulo, por lo que se debe generar un token.
            Assert.DoesNotThrow(() => client.Financial.GetSingleUseToken("141414"));

            // Cuando no se establece canal, se usa el predeterminado
            ITokenResponseInfo tokenResponseInfo1 = client.Financial.GetSingleUseToken("141414");
            Assert.NotNull(tokenResponseInfo1);
            Assert.AreEqual("*", tokenResponseInfo1.ChannelKey);

            // No se requiere incluir la propiedad en el body, por que debe generar un token.
            Assert.DoesNotThrow(() => ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", excludeChannelKey: true));

            // Cuando no se establece canal, se usa el predeterminado
            ITokenResponseInfo tokenResponseInfo2 = ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", excludeAccountType: true);
            Assert.NotNull(tokenResponseInfo2);
            Assert.AreEqual("*", tokenResponseInfo2.ChannelKey);
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButMetadataIsNullOrMissingThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // El tipo de cuenta no es requerido y puede ser nulo, por lo que se debe generar un token.
            Assert.DoesNotThrow(() => client.Financial.GetSingleUseToken("141414"));

            // No se requiere incluir la propiedad en el body, por que debe generar un token.
            Assert.DoesNotThrow(() => ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", excludeMetadata: true));
        }

        [Test, Category("Delegated-Get-Single-Use-Token"), Author("dmontalvo")]
        public void GivenAUserIdentityWhenGetSingleUseTokenButAmountIsNullOrMissingOrZeroThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            void GetSingleUseTokenAvoidingValidation(object amount) =>
                ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", amount: amount);

            // Monto igual a cero puede generar token.
            Assert.DoesNotThrow(() => GetSingleUseTokenAvoidingValidation(0));

            // Cuando se establece vacío igual puede generar token.
            Assert.DoesNotThrow(() => GetSingleUseTokenAvoidingValidation(string.Empty));

            // Monto nulo puede generar token puede generar token.
            Assert.DoesNotThrow(() => GetSingleUseTokenAvoidingValidation(null));

            // Sin monto puede generar token puede generar token.
            Assert.DoesNotThrow(() => ((AspenClient)client.Financial).GetSingleUseTokenAvoidingValidation("141414", excludeAmount: true));

            // Monto establecido como cadena puede generar token.
            Assert.DoesNotThrow(() => GetSingleUseTokenAvoidingValidation("10000"));
        }
        #endregion

        [Test]
        [Category("TransferAccountDelegated")]
        public void GetTransferAccountsDelegated()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            var response = client.Management.GetTransferAccounts();
            PrintOutput("Accounts", response);
        }

        [Test]
        [Category("TransferAccountDelegated")]
        public void UnlinkTransferAccountDelegated()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            var accounts = client.Management.GetTransferAccounts() ?? Enumerable.Empty<TransferAccountResponseInfo>();
            foreach (TransferAccountResponseInfo info in accounts)
            {
                Assert.DoesNotThrow(() => client.Management.UnlinkTransferAccount(info.Alias));
            }
        }

        [Test]
        [Category("TransferAccountDelegated")]
        public void LinkTransferAccountDelegatedInvalidPin()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();


            ITransferAccountRequestInfo account = new TransferAccountRequestRequestInfo("CC", "79483129", "Atorres", "6039590286132628");
            account.PinNumber = new Random().Next(0, 999999).ToString("000000");
            AspenResponseException exc = Assert.Throws<AspenResponseException>(() => client.Management.LinkTransferAccount(account));
            Assert.That(exc.EventId, Is.EqualTo("15862"));
            StringAssert.IsMatch("Pin de usuario/app no es valido", exc.Message);
        }

        [Test]
        [Category("TransferAccountDelegated")]
        public void LinkTransferAccountDelegatedWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                              .RoutingTo(this.delegatedAppInfoProvider)
                                              .WithIdentity(this.delegatedAppInfoProvider)
                                              .Authenticate(userCredentials)
                                              .GetClient();

            var accounts = client.Management.GetTransferAccounts();
            foreach (TransferAccountResponseInfo info in accounts)
            {
                client.Management.UnlinkTransferAccount(info.Alias);
            }

            var alias = $"Alias {new Random().Next(1, 999):000}";
            ITransferAccountRequestInfo account = new TransferAccountRequestRequestInfo("CC", "79483129", alias, "6039590286132628");
            account.PinNumber = "141414";
            Assert.DoesNotThrow(() => client.Management.LinkTransferAccount(account));
        }

        [Test]
        public void PushGetMessages() 
        {
            {
                DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
                IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                                                  .RoutingTo(this.delegatedAppInfoProvider)
                                                  .WithIdentity(this.delegatedAppInfoProvider)
                                                  .Authenticate(userCredentials)
                                                  .GetClient();

                var response = client.Push.GetMessages();
                PrintOutput("Push", response);
            }
        }

        #region Resources

        /// <summary>
        /// Se obtiene la lista de opciones de menú para la aplicación con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar las opciones de menú, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenGetAppMenuWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var menuItems = client.Settings.GetMenu();

            // Then
            CollectionAssert.IsNotEmpty(menuItems);
        }

        /// <summary>
        /// Se obtiene la lista de tipos de identificacíon admitidos para la aplicación con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los tipos de documentos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateGetDocTypesWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var docTypes = client.Settings.GetDocTypes();

            // Then
            CollectionAssert.IsNotEmpty(docTypes);
        }

        /// <summary>
        /// Se obtiene la lista de operadores de telefonía para la aplicación con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los operadores de telefonía, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenGetTelcosWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var telcos = client.Settings.GetTelcos();

            // Then
            CollectionAssert.IsNotEmpty(telcos);
        }

        /// <summary>
        /// Se obtiene los tipos de transacción para la aplicación con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los tipos de transacción, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenGetTranTypesWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var tranTypes = client.Settings.GetTranTypes();

            // Then
            CollectionAssert.IsNotEmpty(tranTypes);
        }

        /// <summary>
        /// Se obtiene los tipos de pagos para la aplicación con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los tipos de pagos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenGetPaymentTypesWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var paymentTypes = client.Settings.GetPaymentTypes();

            // Then
            CollectionAssert.IsNotEmpty(paymentTypes);
        }

        /// <summary>
        /// Se obtiene los valores admitidos para los procesos de recargas de celulares de la aplicación con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los valores admitidos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenGetTopUpValuesWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var topUpValues = client.Settings.GetTopUpValues();

            // Then
            CollectionAssert.IsNotEmpty(topUpValues);
        }

        /// <summary>
        /// Se obtienen los valores miscelaneos del sistema con un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Given: Dada una identidad de usuario reconocida
        /// When: Luego recibir un token de autenticación válido
        /// Then: Al consultar los valores miscelaneos, funciona al recibir una colección de objetos.
        /// </remarks>
        [Category("Delegated-Resources"), Test]
        public void GivenARecognizedUserIdentityWhenInvokeAuthenticateThenGetMiscellaneousValuesWorks()
        {
            // Given
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider);

            // When 
            client.Authenticate(userCredentials);
            var miscValues = client.Settings.GetMiscellaneousValues();

            // Then
            Assert.NotNull(miscValues);
            CollectionAssert.IsNotEmpty(miscValues);
        }

        #endregion

        #region UpdatePin

        /// <summary>
        /// Se produce una excepción si se intenta invocar la operación de reversión sin el identificador de la transacción original.
        /// </summary>
        /// <remarks>
        /// Given: Una solicitud de anulación
        /// When: Con un identificador de transacción original nulo o vacío
        /// Then: Se genera una excepción de tipo <see cref="ArgumentNullException"/> o <see cref="ArgumentException"/>
        /// </remarks>
        [Test]
        [Category("CurrentUser-UpdatePin")]
        public void GivenARecognizedUserIdentityWhenRequestedUpdateTransactionalPinWithCurrentPinThenWorks()
        {
            DelegatedUserInfo userCredentials = GetDelegatedUserCredentials();
            IFluentClient client = AspenClient.Initialize(AppScope.Delegated)
                .RoutingTo(this.delegatedAppInfoProvider)
                .WithIdentity(this.delegatedAppInfoProvider)
                .Authenticate(userCredentials)
                .GetClient();

            // Actualizar el pin funciona.
            Assert.DoesNotThrow(() => client.CurrentUser.UpdatePin("141414", "151515"));

            // Pin anterior ya no es valido para intentar actualizar.
            AspenResponseException exception = Assert.Throws<AspenResponseException>(() => client.CurrentUser.UpdatePin("141414", "161616"));
            AssertAspenResponseException(
                exception,
                "15861",
                HttpStatusCode.NotAcceptable,
                "Para cambiar el Pin se requiere el valor de su Pin actual");
        }

        #endregion
    }
}