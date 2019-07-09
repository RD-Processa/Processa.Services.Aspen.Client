// -----------------------------------------------------------------------
// <copyright file="IActivationCodeInfo.cs" company="Processa">
// Copyright (c)  Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-14 09:58 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System.Net;

    public interface IActivationCodeInfo
    {
        string Code { get; set; }
        int TimeLapseMinutes { get; set; }
        string ResponseMessage { get; set; }
        bool Successful { get; set; }
        string Warning { get; set; }

        string Nickname { get; set; }
    }
}