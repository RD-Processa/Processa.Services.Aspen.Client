// -----------------------------------------------------------------------
// <copyright file="IDelegatedClient.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-01 05:49 PM</date>
// ----------------------------------------------------------------------

namespace Processa.Services.Aspen.Client.Fluent
{
    using Contracts;

    public interface IDelegatedClient
    {
        IEndPointSettings Initialize();

        IEndPointSettings Initialize(ISettings settings);
    }
}