// -----------------------------------------------------------------------
// <copyright file="ActivationCodeInfo.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-14 09:55 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    public class ActivationCodeInfo : IActivationCodeInfo
    {
        public string Code { get; set; }

        public int TimeLapseMinutes { get; set; }

        public string ResponseMessage { get; set; }

        public bool Successful { get; set; }

        public string Warning { get; set; }
    }
}