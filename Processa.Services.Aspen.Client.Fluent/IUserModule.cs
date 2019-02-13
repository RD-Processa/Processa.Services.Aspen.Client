// -----------------------------------------------------------------------
// <copyright file="IUserModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 04:47 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Threading.Tasks;
    using RestSharp;

    /// <summary>
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades que almacenan la información de usuarios en aplicaciones delegadas.
    /// </summary>
    public interface IUserModule
    {
        /// <summary>
        /// Establece el pin transaccional del usuario actual.
        /// </summary>
        /// <param name="pinNumber">Número de pin que se debe asignar al usuario actual.</param>
        /// <param name="activationCode">Código de activación (SMS) recibido por el usuario.</param>
        void SetPin(string pinNumber, string activationCode);

        /// <summary>
        /// Establece el pin transaccional del usuario actual.
        /// </summary>
        /// <param name="pinNumber">Número de pin que se debe asignar al usuario actual.</param>
        /// <param name="activationCode">Código de activación (SMS) recibido por el usuario.</param>
        /// <returns>Instancia de <see cref="Task"/> con la información de la ejecución.</returns>
        Task<IRestResponse> SetPinAsync(string pinNumber, string activationCode);

        /// <summary>
        /// Solicita el envío de un código de activación a través de un mensaje SMS.
        /// </summary>
        void RequestActivacionCode();

        /// <summary>
        /// Solicita el envío de un código de activación a través de un mensaje SMS de forma asíncrona.
        /// </summary>
        /// <returns>Instancia de <see cref="Task"/> con la información de la ejecución.</returns>
        Task<IRestResponse> RequestActivacionCodeAsync();
    }
}