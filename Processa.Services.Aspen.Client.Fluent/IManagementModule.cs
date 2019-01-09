// -----------------------------------------------------------------------
// <copyright file="IManagementModule.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-09 02:27 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
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
        /// <param name="alias">Identificador que se desea asociar con el usuario o  <see langword="null" /> para utilizar el valor de <paramref name="nickname"/>.</param>
        void ValidateActivationCode(string code, string nickname, string alias = null);
    }
}