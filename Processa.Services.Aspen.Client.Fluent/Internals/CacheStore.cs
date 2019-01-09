// -----------------------------------------------------------------------
// <copyright file="CacheStore.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 04:44 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Internals
{
    using System;
    using Auth;
    using Providers;

    /// <summary>
    /// Permite almacenar el último token de autenticación generado.
    /// </summary>
    internal static class CacheStore
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private const string TokenCacheKey = "CURRENT_AUTHTOKEN";

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private const string DeviceCacheKey = "CURRENT_DEVICE";

        /// <summary>
        /// Obtiene el último token de autenticación generado o  <see langword="null" /> si no se ha obtenido ninguno.
        /// </summary>
        /// <returns>Instancia que implementa <see cref="IAuthToken"/> con el valor del último token generado.</returns>
        internal static IAuthToken GetCurrentToken()
        {
            return AppDomain.CurrentDomain.GetData(TokenCacheKey) as IAuthToken;
        }

        /// <summary>
        /// Guarda el último token de autenticación generado.
        /// </summary>
        /// <param name="authToken">Instancia del token que se debe guardar.</param>
        internal static void SetCurrentToken(IAuthToken authToken)
        {
           AppDomain.CurrentDomain.SetData(TokenCacheKey, authToken);
        }

        /// <summary>
        /// Reestablece a <see langword="null" /> todos los valores en la cache.
        /// </summary>
        internal static void Reset()
        {
            AppDomain.CurrentDomain.SetData(TokenCacheKey, null);
            AppDomain.CurrentDomain.SetData(DeviceCacheKey, null);
        }

        /// <summary>
        /// Obtiene la ifnormación del dispositivo solicitante o  <see langword="null" /> si no se ha guardado información.
        /// </summary>
        /// <returns>Instancia que implementa <see cref="IDeviceInfo"/> con al información del dispositivo.</returns>
        internal static IDeviceInfo GetDeviceInfo()
        {
            return AppDomain.CurrentDomain.GetData(DeviceCacheKey) as IDeviceInfo;
        }

        /// <summary>
        /// Guarda la ifnormación del dispositivo actual.
        /// </summary>
        /// <param name="deviceInfo">Instancia del dispositivo que se debe guardar.</param>
        internal static void SetDeviceInfo(IDeviceInfo deviceInfo)
        {
            AppDomain.CurrentDomain.SetData(DeviceCacheKey, deviceInfo);
        }
    }
}