// -----------------------------------------------------------------------
// <copyright file="AspenClient.IFinancial.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 04:20 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    public partial class AspenClient : IFinancialModule
    {
        /// <summary>
        /// Obtiene una instancia que pemite acceder a recursos financieros del servicio Aspen.
        /// </summary>
        public IFinancialModule Financial => this;
    }
}