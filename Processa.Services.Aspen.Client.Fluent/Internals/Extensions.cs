// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-04 12:11 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Internals
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Expone métodos de extensión para diferentes tipos.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private const DataProtectionScope ProtectionScope = DataProtectionScope.LocalMachine;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private static readonly Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private static readonly byte[] entropy = null;
        
        /// <summary>
        /// Descifra los datos cifrados especificados.
        /// </summary>
        /// <param name="encryptedData">Texto cifrado.</param>
        /// <returns>Cadena con los datos decifrados.</returns>
        public static string Decrypt(this string encryptedData)
        {
            byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(encryptedData), entropy, ProtectionScope);
            return encoding.GetString(decryptedData);
        }

        /// <summary>
        /// Encripta la entrada especificada. El resultado solo puede ser descifrado en el mismo PC.
        /// </summary>
        /// <param name="input">Texto que se desea encriptar.</param>
        /// <returns>Cadena con los datos cifrados.</returns>
        public static string Encrypt(this string input)
        {
            byte[] encryptedData = ProtectedData.Protect(encoding.GetBytes(input), entropy, ProtectionScope);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Obtiene la ruta de acceso de un assembly.
        /// </summary>
        /// <param name="assembly">Assembly para el que se obtiene la ruta de acceso.</param>
        /// <param name="rootPath">Ruta de acceso de la carpeta raíz donde se crean los archivos de logs.</param>
        /// <param name="childPath">Especifica una ruta (opcional) para agregar a la ruta del assembly.</param>
        /// <returns>Ruta de acceso (carpeta) donde se encuentra ubicado el assembly.</returns>
        internal static string GetAssemblyDirectory(this Assembly assembly, string rootPath = null, string childPath = null)
        {
            if (assembly == null)
            {
                return default(string);
            }

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                rootPath = Path.GetDirectoryName(assembly.Location);
            }

            if (string.IsNullOrWhiteSpace(childPath))
            {
                return rootPath;
            }

            Debug.Assert(rootPath != null, $"{nameof(rootPath)} != null");
            return Path.Combine(rootPath, childPath);
        }
    }
}