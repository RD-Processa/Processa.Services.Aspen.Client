// -----------------------------------------------------------------------
// <copyright file="PushMessageInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-01 02:18 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System;

    public class PushMessageInfo
    {
        public int Id { get; set; }
        public string DocType { get; set; }
        public string DocNumber { get; set; }
        public int AppId { get; set; }
        public string Message { get; set; }
        public DateTime SendAt { get; set; }
        public bool Read { get; set; }
        public DateTime? ReadAt { get; set; }
        public string HiddenData { get; set; }
        public string Devices { get; set; }
        public string TraceId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }
}