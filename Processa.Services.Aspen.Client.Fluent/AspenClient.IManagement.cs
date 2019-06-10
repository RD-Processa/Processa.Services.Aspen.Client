// -----------------------------------------------------------------------
// <copyright file="AspenClient.IManagement.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-09 02:31 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Entities;
    using Internals;
    using RestSharp;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    public partial class AspenClient : IManagementModule
    {
        /// <summary>
        /// Valida un código de activación asociado con un usuario.
        /// </summary>
        /// <param name="code">Código de activación que se desea validar.</param>
        /// <param name="nickname">Identificador del usuario para el que se emitió el código de activación.</param>
        /// <param name="alias">Identificador que se desea asociar con el usuario o  <see langword="null" /> para utilizar el valor de <paramref name="nickname" />.</param>
        /// <param name="channelId">Identificador del canal para el que se generó el código de activación.</param>
        public void ValidateActivationCode(string code, string nickname, string alias = null, string channelId = null)
        {
            IRestRequest request = new AspenRequest(this, Routes.Management.ActivationCode, Method.POST);
            request.AddJsonBody(new { Code = code, Nickname = nickname, EnrollmentAlias = alias, ChannelId = channelId });
            this.Execute(request);
        }

        /// <summary>
        /// Vincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="docType">Tipo de documento del cliente al cual se vinculará la cuenta.</param>
        /// <param name="docNumber">Número de documento del cliente al cual se vinculará la cuenta.</param>
        /// <param name="accountInfo">Información de la cuenta a vincular.</param>
        public void LinkTransferAccount(string docType, string docNumber, ITransferAccountRequestInfo accountInfo)
        {
            IDictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "@[DocType]", docType },
                { "@[DocNumber]", docNumber }
            };

            IRestRequest request = new AspenRequest(this, Routes.Management.LinkTransferAccount.ReplaceTokens(tokens), Method.POST);
            request.AddJsonBody(accountInfo);
            this.Execute(request);
        }

        /// <summary>
        /// Obtiene la información de las cuentas vinculadas para transferencias.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario para el que se obtienen las cuentas.</param>
        /// <param name="docNumber">Númerop de documento del usuario para el que se obtienen las cuentas.</param>
        /// <returns>Listado de cuentas inscritas.</returns>
        public IList<TransferAccountResponseInfo> GetTransferAccounts(string docType, string docNumber)
        {
            IDictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "@[DocType]", docType },
                { "@[DocNumber]", docNumber }
            };

            IRestRequest request = new AspenRequest(this, Routes.Management.LinkTransferAccount.ReplaceTokens(tokens), Method.GET);
            var response = this.Execute<IList<TransferAccountResponseInfo>>(request);
            if (response == null)
            {
                return Enumerable.Empty<TransferAccountResponseInfo>().ToList();
            }

            return response;
        }

        /// <summary>
        /// Obtiene la información de las cuentas vinculadas para transferencias del usuario actual.
        /// </summary>
        /// <returns>Listado de cuentas inscritas.</returns>
        public IList<TransferAccountResponseInfo> GetTransferAccounts()
        {
            IRestRequest request = new AspenRequest(this, "/transfers/accounts", Method.GET);
            return this.Execute<IList<TransferAccountResponseInfo>>(request);
        }

        /// <summary>
        /// Desvincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="docType">Tipo de documento del cliente al cual se vinculó la cuenta.</param>
        /// <param name="docNumber">Número de documento del cliente al cual se vinculó la cuenta.</param>
        /// <param name="alias">Nombre o alias con el que se vinculó la cuenta.</param>
        public void UnlinkTransferAccount(string docType, string docNumber, string alias)
        {
            IDictionary<string, string> tokens = new Dictionary<string, string>
            {
                { "@[DocType]", docType },
                { "@[DocNumber]", docNumber },
                { "@[Alias]", alias }
            };

            IRestRequest request = new AspenRequest(this, Routes.Management.UnlinkTransferAccount.ReplaceTokens(tokens), Method.DELETE);
            this.Execute(request);
        }

        /// <summary>
        /// Desvincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="alias">Nombre o alias con el que se vinculó la cuenta.</param>
        public void UnlinkTransferAccount(string alias)
        {
            IRestRequest request = new AspenRequest(this, $"/transfers/accounts/{alias}", Method.DELETE);
            this.Execute(request);
        }

        /// <summary>
        /// Vincula la información de una cuenta a las cuentas habilitadas para transferencias.
        /// </summary>
        /// <param name="accountInfo">Información de la cuenta a vincular.</param>
        public void LinkTransferAccount(ITransferAccountRequestInfo accountInfo)
        {
            IRestRequest request = new AspenRequest(this, $"/transfers/accounts", Method.POST);
            request.AddJsonBody(accountInfo);
            this.Execute(request);
        }

        /// <summary>
        /// Obtiene un objeto que permite acceder a las operaciones soportadas por el servicio Aspen para la administración de entidades variadas en aplicaciones autónomas.
        /// </summary>
        public IManagementModule Management => this;
    }
}