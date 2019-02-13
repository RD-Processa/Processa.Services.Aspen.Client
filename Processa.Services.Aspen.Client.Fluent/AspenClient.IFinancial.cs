// -----------------------------------------------------------------------
// <copyright file="AspenClient.IFinancial.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 04:20 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System;
    using System.Collections.Generic;
    using Entities;
    using Internals;
    using RestSharp;

    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    public partial class AspenClient : IFinancialModule
    {
        /// <summary>
        /// Obtiene una instancia que pemite acceder a recursos financieros del servicio Aspen.
        /// </summary>
        public IFinancialModule Financial => this;

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <returns>Listado con la información de las cuentas del usuario actual.</returns>
        public IEnumerable<IAccountInfo> GetAccounts()
        {
            IRestRequest request = new AspenRequest(this, Routes.Inquires.Delegated.Accounts, Method.GET);
            return this.Execute<List<AccountInfo>>(request).ConvertAll(item => (IAccountInfo)item);
        }

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas del usuario especificado.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <returns>Listado con la información de las cuentas del usuario especificado.</returns>
        public IEnumerable<IAccountInfo> GetAccounts(string docType, string docNumber)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Autonomous.Accounts);
            formatter.Add("@[DocType]", docType);
            formatter.Add("@[DocNumber]", docNumber);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<AccountInfo>>(request).ConvertAll(item => (IAccountInfo)item);
        }

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas del usuario especificado a partir de su alias utilizado en el registro.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <returns>Listado con la información de las cuentas del usuario especificado.</returns>
        public IEnumerable<IAccountInfo> GetAccountsByAlias(string channelId, string enrollmentAlias)
        {
            Throw.IfNullOrEmpty(channelId, nameof(channelId));
            Throw.IfNullOrEmpty(enrollmentAlias, nameof(enrollmentAlias));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Autonomous.AccountsByAlias);
            formatter.Add("@[ChannelId]", channelId);
            formatter.Add("@[EnrollmentAlias]", enrollmentAlias);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<AccountInfo>>(request).ConvertAll(item => (IAccountInfo)item);
        }

        /// <summary>
        /// Obtiene la información de saldos de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los saldos.</param>
        /// <returns>Listado con la información de saldos de las cuentas del usuario actual.</returns>
        public IEnumerable<IBalanceInfo> GetBalances(string accountId)
        {
            Throw.IfNullOrEmpty(accountId, nameof(accountId));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Delegated.Balances);
            formatter.Add("@[AccountId]", accountId);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<BalanceInfo>>(request).ConvertAll(item => (IBalanceInfo)item);

        }

        /// <summary>
        /// Obtiene la información de saldos de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los saldos.</param>
        /// <returns>Listado con la información de saldos de las cuentas del usuario actual.</returns>
        public IEnumerable<IBalanceInfo> GetBalances(string docType, string docNumber, string accountId)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));
            Throw.IfNullOrEmpty(accountId, nameof(accountId));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Autonomous.Balances);
            formatter.Add("@[DocType]", docType);
            formatter.Add("@[DocNumber]", docNumber);
            formatter.Add("@[AccountId]", accountId);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<BalanceInfo>>(request).ConvertAll(item => (IBalanceInfo)item);
        }

        /// <summary>
        /// Obtiene la información de saldos de las cuentas asociadas.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los saldos.</param>
        /// <returns>Listado con la información de saldos de las cuentas.</returns>
        public IEnumerable<IBalanceInfo> GetBalancesByAlias(string channelId, string enrollmentAlias, string accountId)
        {
            Throw.IfNullOrEmpty(channelId, nameof(channelId));
            Throw.IfNullOrEmpty(enrollmentAlias, nameof(enrollmentAlias));
            Throw.IfNullOrEmpty(accountId, nameof(accountId));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Autonomous.BalancesByAlias);
            formatter.Add("@[ChannelId]", channelId);
            formatter.Add("@[EnrollmentAlias]", enrollmentAlias);
            formatter.Add("@[AccountId]", accountId);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<BalanceInfo>>(request).ConvertAll(item => (IBalanceInfo)item);
        }

        /// <summary>
        /// Obtiene la información de movimientos financieros de una cuenta.
        /// </summary>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los movimientos financieros.</param>
        /// <param name="accountTypeId">Identificador del tipo de cuenta que se desea filtrar o  <see langword="null" /> para omitir el filtro.</param>
        /// <returns>Listado con la información de movimientos financieros de la cuenta especificada para el usuario actual.</returns>
        public IEnumerable<IMiniStatementInfo> GetStatements(string accountId, string accountTypeId = null)
        {
            Throw.IfNullOrEmpty(accountId, nameof(accountId));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Delegated.Statements);
            formatter.Add("@[AccountId]", accountId);
            formatter.Add("@[AccountTypeId]", string.IsNullOrWhiteSpace(accountTypeId) ? "*" : accountTypeId);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<MiniStatementInfo>>(request).ConvertAll(item => (IMiniStatementInfo)item);
        }

        /// <summary>
        /// Obtiene la información de movimientos financieros de una cuenta.
        /// </summary>
        /// <param name="docType">Tipo de documento del propietario de la cuenta.</param>
        /// <param name="docNumber">Número de documento del propietario de la cuenta.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los movimientos financieros.</param>
        /// <param name="accountTypeId">Identificador del tipo de cuenta que se desea filtrar o  <see langword="null" /> para omitir el filtro.</param>
        /// <returns>Listado con la información de movimientos financieros de la cuenta especificada para el usuario.</returns>
        public IEnumerable<IMiniStatementInfo> GetStatements(string docType, string docNumber, string accountId, string accountTypeId = null)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));
            Throw.IfNullOrEmpty(accountId, nameof(accountId));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Autonomous.Statements);
            formatter.Add("@[DocType]", docType);
            formatter.Add("@[DocNumber]", docNumber);
            formatter.Add("@[AccountId]", accountId);
            formatter.Add("@[AccountTypeId]", string.IsNullOrWhiteSpace(accountTypeId) ? "*" : accountTypeId);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<MiniStatementInfo>>(request).ConvertAll(item => (IMiniStatementInfo)item);
        }

        /// <summary>
        /// Obtiene la información de movimientos de las cuentas asociadas por el alias de registro de un usuario.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los movimientos financieros.</param>
        /// <param name="accountTypeId">Identificador del tipo de cuenta (bolsillo) que se desea filtrar o  <see langword="null" /> para omitir el filtro.</param>
        /// <returns>Listado con la información de movimientos financieros de la cuenta especificada para el usuario actual.</returns>
        public IEnumerable<IMiniStatementInfo> GetStatementsByAlias(string channelId, string enrollmentAlias, string accountId, string accountTypeId = null)
        {
            Throw.IfNullOrEmpty(channelId, nameof(channelId));
            Throw.IfNullOrEmpty(enrollmentAlias, nameof(enrollmentAlias));
            Throw.IfNullOrEmpty(accountId, nameof(accountId));

            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Inquires.Autonomous.StatementsByAlias);
            formatter.Add("@[ChannelId]", channelId);
            formatter.Add("@[EnrollmentAlias]", enrollmentAlias);
            formatter.Add("@[AccountId]", accountId);
            formatter.Add("@[AccountTypeId]", string.IsNullOrWhiteSpace(accountTypeId) ? "*" : accountTypeId);

            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.Execute<List<MiniStatementInfo>>(request).ConvertAll(item => (IMiniStatementInfo)item);
        }

        /// <summary>
        /// Solicita el envío de un token transaccional para un usuario.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <param name="metadata">Metadatos que se desean asociar al token.</param>
        public void RequestSingleUseToken(string docType, string docNumber, string metadata = null)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));

            IRestRequest request = new AspenRequest(this, Routes.Tokens.RequestToken, Method.POST);
            request.AddJsonBody(new { DocType = docType, DocNumber = docNumber, Metadata = metadata });
            this.Execute(request);
        }

        /// <summary>
        /// Genera la información de un token transaccional de un solo uso.
        /// </summary>
        /// <param name="pinNumber">Pin transaccional del usuario.</param>
        /// <param name="metadata">Metadatos que se desean asociar al token.</param>
        /// <returns>Instancia de <see cref="ITokenResponseInfo" /> con la información del token.</returns>
        public ITokenResponseInfo GetSingleUseToken(string pinNumber, string metadata = null)
        {
            Throw.IfNullOrEmpty(pinNumber, nameof(pinNumber));
            IRestRequest request = new AspenRequest(this, Routes.Tokens.Root, Method.POST);
            request.AddJsonBody(new { Metadata = metadata, PinNumber = pinNumber });
            return this.Execute<TokenResponseInfo>(request);
        }

        /// <summary>
        /// Obtiene una imagen (representación en formato base64) de un token transaccional de un solo uso.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <returns>Cadena en formato base 64 que representa la información del token transaccional.</returns>
        public string GetImageToken(string channelId, string enrollmentAlias)
        {
            Throw.IfNullOrEmpty(channelId, nameof(channelId));
            Throw.IfNullOrEmpty(enrollmentAlias, nameof(enrollmentAlias));
            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Tokens.ImageToken);
            formatter.Add("@[ChannelId]", channelId);
            formatter.Add("@[EnrollmentAlias]", enrollmentAlias);
            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.GET);
            return this.ExecuteRaw(request);
        }

        /// <summary>
        /// Comprueba la validez de un token transaccional.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario para el que se generó el token transaccional.</param>
        /// <param name="docNumber">Número de documento del usuario para el que se generó el token transaccional.</param>
        /// <param name="token">Token que se desea validar.</param>
        /// <param name="metadata">Metadatos que se asociaron al token al momento de su generación.</param>
        public void ValidateSingleUseToken(string docType, string docNumber, string token, string metadata = null)
        {
            Throw.IfNullOrEmpty(token, nameof(token));
            PlaceholderFormatter formatter = new PlaceholderFormatter(Routes.Tokens.Redeem);
            formatter.Add("@[Token]", token);
            IRestRequest request = new AspenRequest(this, formatter.ToString(), Method.PUT);
            request.AddJsonBody(new { Metadata = metadata, DocType = docType, DocNumber = docNumber });
            this.Execute(request);
        }

        /// <summary>
        /// Solicita el procesamiento de una transacción de retiro.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <param name="token">Token transacional asociado con el usuario.</param>
        /// <param name="accountType">Tipo de cuenta de la que se retiran los fondos.</param>
        /// <param name="amount">Valor del retiro.</param>
        /// <param name="metadata">Metadatos que fueron asociado al token en la generación.</param>
        public void Withdrawal(string docType, string docNumber, string token, string accountType, int amount, string metadata = null)
        {
            Throw.IfNullOrEmpty(docType, nameof(docType));
            Throw.IfNullOrEmpty(docNumber, nameof(docNumber));
            Throw.IfNullOrEmpty(token, nameof(token));
            Throw.IfNullOrEmpty(accountType, nameof(accountType));
            Throw.IfEmpty(amount, nameof(amount));

            IRestRequest request = new AspenRequest(this, Routes.Financial.Withdrawal, Method.POST);
            request.AddJsonBody(new { DocType = docType, DocNumber = docNumber, Token = token, AccountType = accountType, Amount = amount, Metadata = metadata });
            this.Execute(request);

        }
    }
}
