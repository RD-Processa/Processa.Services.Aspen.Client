// -----------------------------------------------------------------------
// <copyright file="ISettingsModule.cs" company="Processa"> 
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
    /// Define las operaciones soportadas por el servicio Aspen para acceder a entidades de información relacionadas con parametrización del sistema.
    /// </summary>
    public interface ISettingsModule
    {
        /// <summary>
        /// Obtiene la lista de tipos de documento soportados por el servicio Aspen.
        /// </summary>
        /// <returns>Lista de tipos de documento soportados.</returns>
        IList<DocType> GetDocTypes();

        /// <summary>
        /// Obtiene la lista de operadores de telefonía móvil soportados por el servicio Aspen.
        /// </summary>
        /// <returns>Lista de operadores de telefonía soportados.</returns>
        IList<Telco> GetTelcos();

        /// <summary>
        /// Obtiene la lista de opciones que representan el menú de una aplicación móvil.
        /// </summary>
        /// <returns>Lista de opciones de menú.</returns>
        IList<MenuItem> GetMenu();

        /// <summary>
        /// Obtiene la lista de los tipos de transacción para una aplicación.
        /// </summary>
        /// <returns>Lista de tipos de transacción soportados.</returns>
        IList<TranTypeInfo> GetTranTypes();

        /// <summary>
        /// Obtiene los tipos de pagos que se pueden realizar a una cuenta.
        /// </summary>
        /// <returns>Lista de <see cref="PaymentTypeInfo"/> con los tipos de pago para la aplicación solicitante.</returns>
        IList<PaymentTypeInfo> GetPaymentTypes();

        /// <summary>
        /// Obtiene los tipos de pagos que se pueden realizar a una cuenta.
        /// </summary>
        /// <returns>Lista de <see cref="TopUpInfo"/> con los valores admitidos de recarga por operador para la aplicación solicitante.</returns>
        IList<TopUpInfo> GetTopUpValues();

        /// <summary>
        /// Obtiene los valores misceláneos soportados por el servicio Aspen.
        /// </summary>
        /// <returns>Colección de valores admitidos.</returns>
        IList<KeyValuePair<string, object>> GetMiscellaneousValues();

        /// <summary>
        /// Obtiene la lista de claims habilitados en el sistema
        /// </summary>
        /// <returns>Lista de claims habilitados.</returns>
        IList<ClaimSettings> GetClaims();

        /// <summary>
        /// Obtiene la lista de canales
        /// </summary>
        /// <returns>Lista de canales.</returns>
        IList<ChannelInfo> GetChannels();
    }
}