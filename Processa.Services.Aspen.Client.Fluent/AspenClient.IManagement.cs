// -----------------------------------------------------------------------
// <copyright file="AspenClient.IManagement.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-09 02:31 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Internals;
    using RestSharp;

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
        public void ValidateActivationCode(string code, string nickname, string alias = null)
        {
            IRestRequest request = new AspenRequest(this, Routes.Management.ActivationCode, Method.POST);
            request.AddJsonBody(new { Code = code, Nickname = nickname, EnrollmentAlias = alias });
            this.Execute(request);
        }

        /// <summary>
        /// Obtiene un objeto que permite acceder a las operaciones soportadas por el servicio Aspen para la administración de entidades variadas en aplicaciones autónomas.
        /// </summary>
        public IManagementModule Management => this;
    }
}