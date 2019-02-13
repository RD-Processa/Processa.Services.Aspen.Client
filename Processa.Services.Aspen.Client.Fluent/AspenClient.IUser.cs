// -----------------------------------------------------------------------
// <copyright file="AspenClient.IUser.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 04:49 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Threading.Tasks;
    using Internals;
    using RestSharp;

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
        public void SetPin(string pinNumber, string activationCode)
        {
            Throw.IfNullOrEmpty(pinNumber, nameof(pinNumber));
            Throw.IfNullOrEmpty(activationCode, nameof(activationCode));

            IRestRequest request = new AspenRequest(this, Routes.Users.Pin, Method.POST);
            request.AddJsonBody(new { PinNumber = pinNumber, ActivationCode = activationCode });
            this.Execute(request);
        }

        /// <summary>
        /// Establece el pin transaccional del usuario actual sin validar localmente. Se expone como internal con el fin de validar el comportamiento del servicio Aspen.
        /// </summary>
        /// <param name="pinNumber">Número de pin que se debe asignar al usuario actual.</param>
        /// <param name="activationCode">Código de activación (SMS) recibido por el usuario.</param>
        internal void SetPinAvoidingValidation(string pinNumber, string activationCode)
        {
            IRestRequest request = new AspenRequest(this, Routes.Users.Pin, Method.POST);
            request.AddJsonBody(new {PinNumber = pinNumber, ActivationCode = activationCode});
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
        public void RequestActivacionCode()
        {
            IRestRequest request = new AspenRequest(this, Routes.Users.ActivationCode, Method.POST);
            this.Execute(request);
        }

        /// <summary>
        /// Solicita el envío de un código de activación a través de un mensaje SMS de forma asíncrona.
        /// </summary>
        /// <returns>Instancia de <see cref="Task"/> con la información de la ejecución.</returns>
        public async Task<IRestResponse> RequestActivacionCodeAsync()
        {
            IRestRequest request = new AspenRequest(this, Routes.Users.ActivationCode, Method.POST);
            return await this.ExecuteAsync(request);
        }

        /// <summary>
        /// Requests the single use token.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        public void RequestSingleUseToken(string pinNumber)
        {
            IRestRequest request = new AspenRequest(this, Routes.Tokens.Root, Method.POST);
            request.AddJsonBody(new { PinNumber = pinNumber, Metadata = "xxxx"});
            this.Execute(request);
        }
    }
}