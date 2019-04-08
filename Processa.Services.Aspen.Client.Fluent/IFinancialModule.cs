// -----------------------------------------------------------------------
// <copyright file="IFinancialModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 02:32 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Collections.Generic;
    using Entities;

    /// <summary>
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades de información relacionadas con recursos financieros.
    /// </summary>
    public interface IFinancialModule
    {
        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <returns>Listado con la información de las cuentas del usuario actual.</returns>
        IEnumerable<IAccountInfo> GetAccounts();

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas del usuario especificado.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <returns>Listado con la información de las cuentas del usuario especificado.</returns>
        IEnumerable<IAccountInfo> GetAccounts(string docType, string docNumber);

        /// <summary>
        /// Obtiene la información resumida de las cuentas asociadas del usuario especificado a partir de su alias utilizado en el registro.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <returns>Listado con la información de las cuentas del usuario especificado.</returns>
        IEnumerable<IAccountInfo> GetAccountsByAlias(string channelId, string enrollmentAlias);

        /// <summary>
        /// Obtiene la información de saldos de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los saldos.</param>
        /// <returns>Listado con la información de saldos de las cuentas del usuario actual.</returns>
        IEnumerable<IBalanceInfo> GetBalances(string accountId);

        /// <summary>
        /// Obtiene la información de saldos de las cuentas asociadas con el usuario actual.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario asociado con la cuenta.</param>
        /// <param name="docNumber">Número de documento del usuario asociado con la cuenta.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los saldos.</param>
        /// <returns>Listado con la información de saldos de las cuentas del usuario actual.</returns>
        IEnumerable<IBalanceInfo> GetBalances(string docType, string docNumber, string accountId);

        /// <summary>
        /// Obtiene la información de saldos de las cuentas asociadas por el alias de registro de un usuario.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los saldos.</param>
        /// <returns>Listado con la información de saldos de las cuentas.</returns>
        IEnumerable<IBalanceInfo> GetBalancesByAlias(string channelId, string enrollmentAlias, string accountId);

        /// <summary>
        /// Obtiene la información de movimientos financieros de una cuenta.
        /// </summary>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los movimientos financieros.</param>
        /// <param name="accountTypeId">Identificador del tipo de cuenta que se desea filtrar o  <see langword="null" /> para omitir el filtro.</param>
        /// <returns>Listado con la información de movimientos financieros de la cuenta especificada para el usuario actual.</returns>
        IEnumerable<IMiniStatementInfo> GetStatements(string accountId, string accountTypeId = null);

        /// <summary>
        /// Obtiene la información de movimientos financieros de una cuenta.
        /// </summary>
        /// <param name="docType">Tipo de documento del propietario de la cuenta.</param>
        /// <param name="docNumber">Número de documento del propietario de la cuenta.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los movimientos financieros.</param>
        /// <param name="accountTypeId">Identificador del tipo de cuenta que se desea filtrar o  <see langword="null" /> para omitir el filtro.</param>
        /// <returns>Listado con la información de movimientos financieros de la cuenta especificada para el usuario.</returns>
        IEnumerable<IMiniStatementInfo> GetStatements(string docType, string docNumber, string accountId, string accountTypeId = null);

        /// <summary>
        /// Obtiene la información de movimientos de las cuentas asociadas por el alias de registro de un usuario.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <param name="accountId">Identificador de la cuenta para la que se obtienen los movimientos financieros.</param>
        /// <param name="accountTypeId">Identificador del tipo de cuenta (bolsillo) que se desea filtrar o  <see langword="null" /> para omitir el filtro.</param>
        /// <returns>Listado con la información de movimientos financieros de la cuenta especificada para el usuario actual.</returns>
        IEnumerable<IMiniStatementInfo> GetStatementsByAlias(string channelId, string enrollmentAlias, string accountId, string accountTypeId = null);

        /// <summary>
        /// Solicita el envío de un token transaccional para un usuario.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <param name="metadata">Metadatos que se desean asociar al token.</param>
        /// <param name="tags">Tags relacionados con la solicitud.</param>
        void RequestSingleUseToken(string docType, string docNumber, string metadata = null, TagsInfo tags = null);

        /// <summary>
        /// Genera la información de un token transaccional de un solo uso.
        /// </summary>
        /// <param name="pinNumber">Pin transaccional del usuario.</param>
        /// <param name="metadata">Metadatos que se desean asociar al token.</param>
        /// <param name="amount">Valor del token.</param>
        /// <param name="accountType">Bolsillo para el que se genera el token.</param>
        /// <returns>Instancia de <see cref="ITokenResponseInfo" /> con la información del token.</returns>
        ITokenResponseInfo GetSingleUseToken(string pinNumber, string metadata = null, int? amount = null, string accountType = null);

        /// <summary>
        /// Obtiene una imagen (representación en formato base64) de un token transaccional de un solo uso.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <returns>Cadena en formato base 64 que representa la información del token transaccional.</returns>
        string GetImageToken(string channelId, string enrollmentAlias);


        /// <summary>
        /// Obtiene un archivo en formato pdf (base 64) con el resumen de los estados de cuenta.
        /// </summary>
        /// <param name="channelId">Identificador del canal por el que se registró el usuario.</param>
        /// <param name="enrollmentAlias">Alias utilizado en el proceso de registro.</param>
        /// <returns>Cadena en formato base 64 que representa la información del resumen con los estados de cuenta.</returns>
        string GetStatementsFile(string channelId, string enrollmentAlias);

        /// <summary>
        /// Comprueba la validez de un token transaccional.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario para el que se generó el token transaccional.</param>
        /// <param name="docNumber">Número de documento del usuario para el que se generó el token transaccional.</param>
        /// <param name="token">Token transaccional que se desea validar.</param>
        /// <param name="metadata">Metadatos que se asociaron al token al momento de su generación.</param>
        /// <param name="amount">Valor para el que se generó el token.</param>
        /// <param name="accountType">Bolsillo para el que se generó el token.</param>
        void ValidateSingleUseToken(string docType, string docNumber, string token, string metadata = null, int? amount = null, string accountType = null);

        /// <summary>
        /// Solicita el procesamiento de una transacción de retiro.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <param name="token">Token transacional asociado con el usuario.</param>
        /// <param name="accountType">Tipo de cuenta de la que se retiran los fondos.</param>
        /// <param name="amount">Valor del retiro.</param>
        /// <param name="metadata">Metadatos que fueron asociado al token en la generación.</param>
        void Withdrawal(string docType, string docNumber, string token, string accountType, int amount, string metadata = null);

        /// <summary>
        /// Solicita el procesamiento de una transacción de pago.
        /// </summary>
        /// <param name="docType">Tipo de documento del usuario.</param>
        /// <param name="docNumber">Número de documento del usuario.</param>
        /// <param name="token">Token transacional asociado con el usuario.</param>
        /// <param name="accountType">Tipo de cuenta de la que se toman los fondos.</param>
        /// <param name="amount">Valor del pago.</param>
        /// <param name="metadata">Metadatos que fueron asociado al token en la generación.</param>
        void Payment(string docType, string docNumber, string token, string accountType, int amount, string metadata = null);
    }
}