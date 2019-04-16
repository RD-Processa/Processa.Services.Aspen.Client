// -----------------------------------------------------------------------
// <copyright file="TopUpInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>dmontalvo</author>
// <date>2019-04-11 09:39 AM</date>
// ----------------------------------------------------------------------
// ReSharper disable ClassNeverInstantiated.Global
namespace Processa.Services.Aspen.Client.Entities
{
    /// <summary>
    /// Representa la información de valores de recarga admitidos para un operador celular.
    /// </summary>
    public class TopUpInfo
    {
        /// <summary>
        /// Obtiene o establece el nombre que identifica al operador celular.
        /// </summary>
        public string Telco { get; set; }

        /// <summary>
        /// Obtiene o establece la lista de valores de recarga admitidos para el operador celular.
        /// </summary>
        public int[] Allowed { get; set; }
    }
}