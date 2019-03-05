// -----------------------------------------------------------------------
// <copyright file="IFluentClient.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 08:38 AM</date>
// ----------------------------------------------------------------------
// ReSharper disable PossibleInterfaceMemberAmbiguity
namespace Processa.Services.Aspen.Client.Fluent
{
    /// <summary>
    /// Define las operaciones necesarias para establecer una conexión con el servicio Aspen y enviar solicitudes de procesamiento de información.
    /// </summary>
    public interface IFluentClient : ISessionManager, ISession
    {
        /// <summary>
        /// Obtiene un objeto que permite acceder a la información relacionada con la parametrización del sistema Aspen.
        /// </summary>
        ISettingsModule Settings { get; }

        /// <summary>
        /// Obtiene un objeto que permite acceder a las entidades de información relacionadas con recursos financieros en el sistema Aspen.
        /// </summary>
        IFinancialModule Financial { get; }

        /// <summary>
        /// Obtiene un objeto que permite acceder a las entidades de información del usuario actual que representa la aplicación delegada.
        /// </summary>
        IUserModule CurrentUser { get; }

        /// <summary>
        /// Obtiene un objeto que permite acceder a las operaciones soportadas por el servicio Aspen para la administración de entidades variadas en aplicaciones autónomas.
        /// </summary>
        IManagementModule Management { get; }

        /// <summary>
        /// Obtiene un objeto que permite acceder a las operaciones soportadas por el servicio Aspen para mensajeria push.
        /// </summary>
        IPushModule Push { get; }
    }
}