// -----------------------------------------------------------------------
// <copyright file="HardCodedSettings.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-03-15 03:46 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Tests
{
    using Fluent;
    using Fluent.Contracts;

    public class HardCodedSettings : DefaultSettings
    {
        public HardCodedSettings(INonceGenerator nonceGenerator = null, IEpochGenerator epochGenerator = null, AppScope appScope = AppScope.Autonomous)
        {
            this.AppScope = appScope;
            if (nonceGenerator != null)
            {
                this.NonceGenerator = nonceGenerator;
            }

            if (epochGenerator != null)
            {
                this.EpochGenerator = epochGenerator;
            }
        }
    }
}