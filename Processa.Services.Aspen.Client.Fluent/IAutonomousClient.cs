// -----------------------------------------------------------------------
// <copyright file="IAutonomousClient.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-01 05:40 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using Contracts;

    public interface IAutonomousClient
    {
        IEndPointSettings Initialize();

        IEndPointSettings Initialize(ISettings settings);
    }
}