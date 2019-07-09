// -----------------------------------------------------------------------
// <copyright file="AspenClient.IUser.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 04:49 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Net;
    using System.Threading.Tasks;
    using Entities;
    using Internals;
    using RestSharp;
    using Throw = Internals.Throw;

    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    public partial class AspenClient : IUserModule
    {
        /// <summary>
        /// Obtiene un objeto que permite acceder a las entidades de información del usuario actual que representa la aplicación delegada.
        /// </summary>
        public IUserModule CurrentUser => this;

        /// <summary>
        /// Establece el pin transaccional del usuario actual.
        /// </summary>
        /// <param name="pinNumber">Número de pin que se debe asignar al usuario actual.</param>
        /// <param name="activationCode">Código de activación (SMS) recibido por el usuario.</param>
        public void SetPin(string pinNumber, string activationCode, string nickname = null)
        {
            IRestRequest request = new AspenRequest(this, Routes.Users.Pin, Method.POST);
            request.AddJsonBody(new { PinNumber = pinNumber, ActivationCode = activationCode, Nickname = nickname, EnrollmentAlias = nickname });
            this.Execute(request);
        }

        /// <summary>
        /// Establece el pin transaccional del usuario actual.
        /// </summary>
        /// <param name="pinNumber">Número de pin que se debe asignar al usuario actual.</param>
        /// <param name="activationCode">Código de activación (SMS) recibido por el usuario.</param>
        /// <returns>Instancia de <see cref="Task"/> con la información de la ejecución.</returns>
        public async Task<IRestResponse> SetPinAsync(string pinNumber, string activationCode)
        {
            Throw.IfNullOrEmpty(pinNumber, nameof(pinNumber));
            Throw.IfNullOrEmpty(activationCode, nameof(activationCode));

            IRestRequest request = new AspenRequest(this, Routes.Users.Pin, Method.POST);
            request.AddJsonBody(new { PinNumber = pinNumber, ActivationCode = activationCode });
            return await this.ExecuteAsync(request);            
        }

        /// <summary>
        /// Solicita el envío de un código de activación a través de un mensaje SMS.
        /// </summary>
        /// <param name="nickname">Nombre con el que se registra el usuario. Solo para ambientes de desarrollo.</param>
        /// <returns>En ambientes de desarrollo, una instancia de <see cref="IActivationCodeInfo" /> con la información de los datso generados.</returns>
        public IActivationCodeInfo RequestActivationCode(string nickname = null)
        {
            IRestRequest request = new AspenRequest(this, Routes.Users.ActivationCode, Method.POST);
            request.AddJsonBody(new { Nickname = nickname });
            return this.Execute<ActivationCodeInfo>(request);
        }

        /// <summary>
        /// Solicita el envío de un código de activación a través de un mensaje SMS de forma asíncrona.
        /// </summary>
        /// <returns>Instancia de <see cref="Task"/> con la información de la ejecución.</returns>
        public async Task<IRestResponse> RequestActivationCodeAsync()
        {
            IRestRequest request = new AspenRequest(this, Routes.Users.ActivationCode, Method.POST);
            return await this.ExecuteAsync(request);
        }

        /// <summary>
        /// Solicita el envío de un token transaccional para un usuario autenticado.
        /// </summary>
        public void RequestSingleUseToken()
        {
            IRestRequest request = new AspenRequest(this, Routes.Tokens.RequestToken, Method.POST);
            this.Execute(request);
        }

        /// <summary>
        /// Solicita el envío de un token transaccional por SMS para un usuario autenticado de forma asíncrona.
        /// </summary>
        /// <returns>
        /// Instancia de <see cref="Task" /> con la información de la ejecución.
        /// </returns>
        public async Task<IRestResponse> RequestSingleUseTokenAsync()
        {
            IRestRequest request = new AspenRequest(this, Routes.Tokens.RequestToken, Method.POST);
            return await this.ExecuteAsync(request);
        }

        /// <summary>
        /// Actualiza el pin transaccional del usuario actual a partir del pin o clave actual.
        /// </summary>
        /// <param name="currentPin">Número de pin transaccional o clave de cuenta actual.</param>
        /// <param name="newPin">Nuevo número de pin transaccional.</param>
        public void UpdatePin(string currentPin, string newPin)
        {
            Throw.IfNullOrEmpty(currentPin, nameof(currentPin));
            Throw.IfNullOrEmpty(newPin, nameof(newPin));

            IRestRequest request = new AspenRequest(this, Routes.Users.Pin, Method.PATCH);
            request.AddJsonBody(new { CurrentValue = currentPin, NewValue = newPin });
            this.Execute(request);
        }
    }
}