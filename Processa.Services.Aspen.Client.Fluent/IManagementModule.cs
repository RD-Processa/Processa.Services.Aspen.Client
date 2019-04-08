// -----------------------------------------------------------------------
// <copyright file="IManagementModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-09 02:27 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;

    /// <summary>
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades de administración varias en aplicaciones autónomas.
    /// </summary>
    public interface IManagementModule
    {
        /// <summary>
        /// Valida un código de activación asociado con un usuario.
        /// </summary>
        /// <param name="code">Código de activación que se desea validar.</param>
        /// <param name="nickname">Identificador del usuario para el que se emitió el código de activación.</param>
        /// <param name="alias">Identificador que se desea asociar con el usuario o  <see langword="null" /> para utilizar el valor de <paramref name="nickname" />.</param>
        /// <param name="channelId">Identificador del canal para el que se generó el código de activación.</param>
        void ValidateActivationCode(string code, string nickname, string alias = null, string channelId = null);

        /// <summary>
        /// Vincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="docType">Tipo de documento del cliente al cual se vinculará la cuenta.</param>
        /// <param name="docNumber">Número de documento del cliente al cual se vinculará la cuenta.</param>
        /// <param name="accountInfo">Información de la cuenta a vincular.</param>
        void LinkTransferAccount(string docType, string docNumber, ITransferAccountRequestInfo accountInfo);

        /// <summary>
        /// Obtiene la información de las cuentas vinculadas para transferencias.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario para el que se obtienen las cuentas.</param>
        /// <param name="docNumber">Númerop de documento del usuario para el que se obtienen las cuentas.</param>
        /// <returns>Listado de cuentas inscritas.</returns>
        IList<TransferAccountResponseInfo> GetTransferAccounts(string docType, string docNumber);

        /// <summary>
        /// Obtiene la información de las cuentas vinculadas para transferencias del usuario actual.
        /// </summary>
        /// <returns>Listado de cuentas inscritas.</returns>
        IList<TransferAccountResponseInfo> GetTransferAccounts();

        /// <summary>
        /// Desvincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="docType">Tipo de documento del cliente al cual se vinculó la cuenta.</param>
        /// <param name="docNumber">Número de documento del cliente al cual se vinculó la cuenta.</param>
        /// <param name="alias">Nombre o alias con el que se vinculó la cuenta.</param>
        void UnlinkTransferAccount(string docType, string docNumber, string alias);

        /// <summary>
        /// Desvincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="alias">Nombre o alias con el que se vinculó la cuenta.</param>
        void UnlinkTransferAccount(string alias);

        /// <summary>
        /// Vincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="accountInfo">Información de la cuenta a vincular.</param>
        void LinkTransferAccount(ITransferAccountRequestInfo accountInfo);
    }
}