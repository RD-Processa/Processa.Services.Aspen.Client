// -----------------------------------------------------------------------
// <copyright file="TagsInfo.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-04-02 02:55 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
    using System.Collections.Generic;

    public class TagsInfo : Dictionary<string, string>
    {
        public TagsInfo(string terminalId = null, string cardAcceptorId = null, string customerGroup = null, string pan= null)
        {
            this.AddValue("TerminalId", terminalId);
            this.AddValue("CardAcceptorId", cardAcceptorId);
            this.AddValue("CustomerGroup", customerGroup);
            this.AddValue("Pan", pan);
        }

        private void AddValue(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                this[key] = value;
            }
        }
    }
}