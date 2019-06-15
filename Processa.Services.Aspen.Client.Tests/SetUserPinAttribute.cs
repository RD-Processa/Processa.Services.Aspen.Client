// -----------------------------------------------------------------------
// <copyright file="SetUserPinAttribute.cs" company="Processa">
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-06-13 04:19 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using NUnit.Framework;

    public class SetUserPinAttribute : CategoryAttribute
    {
        public SetUserPinAttribute() : base("SetPin")
        {
        }
    }
}