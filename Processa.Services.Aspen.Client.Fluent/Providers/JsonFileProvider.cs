// -----------------------------------------------------------------------
// <copyright file="JsonFileProvider.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 11:57 AM</date>
// ----------------------------------------------------------------------
// ReSharper disable MemberCanBePrivate.Global
namespace Processa.Services.Aspen.Client.Fluent.Providers
{
    using System;
    using System.IO;
    using System.Reflection;
    using Internals;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Extrae los valores de configuración del archivo appsettings.json.
    /// </summary>
    public class JsonFileProvider : IEndpointProvider, IIdentityProvider
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="JsonFileProvider" />.
        /// </summary>
        /// <param name="filename">Nombre del archivo que contiene la información de conexión.</param>
        /// <exception cref="InvalidOperationException">ASPEN_ENVIRONMENT variable not found.</exception>
        public JsonFileProvider(string filename = "appsettings.json")
        {
            string basePath = Assembly.GetExecutingAssembly().GetAssemblyDirectory();
            string fallbackName = filename;

            // Como la operación cifrar/decifrar solo funciona en el mismo PC,
            // se busca primero un archivo asociado al usuario para que cada desarrollador pueda tener su propio archivo.
            // De esta forma, se evita guadar en el CVS archivos que funcionan en un PC si y en otro no.            
            string userfileName = $"{Path.GetFileNameWithoutExtension(filename)}-{Environment.UserName}{Path.GetExtension(filename)}";
            string sourceFilename = File.Exists(Path.Combine(basePath, userfileName)) ? userfileName : fallbackName;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(sourceFilename, false, false);

            this.Section = Environment.GetEnvironmentVariable("ASPEN_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(this.Section))
            {
                string message = "You need to set an environment variable with name ASPEN_ENVIRONMENT and one of this values: Dev, QA, Prod";
                throw new InvalidOperationException(message);
            }

            IConfigurationRoot configuration = builder.Build();
            IConfigurationSection section = configuration.GetSection(this.Section);
            this.Url = section["Url"];
            this.ApiKey = section["ApiKey"];
            this.ApiSecret = section["ApiSecret"].Decrypt();
        }

        /// <summary>
        /// Obtiene el nombre de la sección que se leyo en el archivo de configuración.
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// Obtiene el valor de la configuración en appsettings.json para la clave Url.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Obtiene el valor de la configuración en appsettings.json para la clave ApiKey.
        /// </summary>        
        public string ApiKey { get; private set; }

        /// <summary>
        /// Obtiene el valor de la configuración en appsettings.json para la clave ApiSecret.
        /// </summary>       
        public string ApiSecret { get; private set; }
    }
}