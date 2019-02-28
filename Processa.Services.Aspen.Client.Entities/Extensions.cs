// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-02-26 11:24 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System;

    /// <summary>
    /// Expone métodos de extensión para varios tipos.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Valida si un valor se encuentra entre dos límites.
        /// </summary>
        /// <typeparam name="T">Tipo del valor a comparar.</typeparam>
        /// <param name="actual">Valor a comparar.</param>
        /// <param name="lower">Límite inferior.</param>
        /// <param name="upper">Límite superior.</param>
        /// <returns><c>true</c> si <paramref name="actual"/> es menor que <paramref name="lower"/> y mayyor que <paramref name="upper"/>, de lo contrario <c>false</c>.</returns>
        public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T>
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) <= 0;
        }
    }
}