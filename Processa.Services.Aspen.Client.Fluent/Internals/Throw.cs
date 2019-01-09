// -----------------------------------------------------------------------
// <copyright file="Throw.cs" company="Processa"> 
// Copyright (c) 2018 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 04:44 PM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Properties;

    /// <summary>
    /// Expone métodos para generar excepciones al validar parámetros.
    /// </summary>
    internal static class Throw
    {
        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException"/> si <paramref name="argumentValue"/> es <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Tipo del argumento a validar</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNull<T>(T argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentException"/> si <paramref name="argumentValue"/> es <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Tipo del argumento a validar</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNull<T>(T? argumentValue, string argumentName) where T : struct
        {
            if (!argumentValue.HasValue)
            {
                throw new ArgumentException(Resources.ArgumentInvalidMessage, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentException" /> si <paramref name="argumentValue" /> es <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Tipo del argumento a validar</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfNull<T>(T? argumentValue, string argumentName, string errorMessage) where T : struct
        {
            if (!argumentValue.HasValue)
            {
                throw new ArgumentException(errorMessage, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException"/> si <paramref name="argumentValue"/> es <c>null</c>, o 
        /// <exception cref="ArgumentException"/> si <paramref name="argumentValue"/> es <see cref="System.String.Empty"/>.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNullOrEmpty(string argumentValue, string argumentName)
        {
            Throw.IfNull(argumentValue, argumentName);

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException(Resources.EmptyStringMessage, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException"/> si <paramref name="argumentValue"/> es <c>null</c>, o 
        /// <exception cref="ArgumentException"/> si <paramref name="argumentValue.Count"/> es cero.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNullOrEmpty(ICollection argumentValue, string argumentName)
        {
            Throw.IfNull(argumentValue, argumentName);

            if (argumentValue.Count == 0)
            {
                throw new ArgumentException(Resources.EmptyCollectionMessage, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException" /> si <paramref name="argumentValue" /> es <c>null</c>, o
        /// <exception cref="ArgumentException" /> si <paramref name="argumentValue" /> contiene algún elemento <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos en la colección</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <exception cref="ArgumentException">La colección contiene algún elemento <c>null</c>.</exception>
        public static void IfHasNull<T>(ICollection<T> argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                return;
            }

            if (argumentValue.Any(item => item == null))
            {
                throw new ArgumentException(Resources.NullItemCollectionMessage, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException" /> si <paramref name="argumentValue" /> es <see cref="Guid.Empty" />.
        /// </summary>
        /// <typeparam name="T">Tipo de <paramref name="argumentValue"/> que se está validando.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfEmpty<T>(T argumentValue, string argumentName)
        {            
            if (argumentValue.Equals(default(T)))
            {
                throw new ArgumentException(Resources.ArgumentInvalidMessage, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="lambda" /> se evalua como <c>false</c>.
        /// </summary>
        /// <param name="lambda">Expresión lambda para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfFalse(Func<bool> lambda, string message = null)
        {
            bool ret = lambda.Invoke();
            if (!ret)
            {
                throw new InvalidConstraintException(message);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="expression" /> se evalua como <c>false</c>.
        /// </summary>
        /// <param name="expression">Expresión para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfFalse(bool expression, string message = null)
        {
            if (!expression)
            {
                throw new InvalidConstraintException(message);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="lambda" /> se evalua como <c>true</c>.
        /// </summary>
        /// <param name="lambda">Expresión lambda para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfTrue(Func<bool> lambda, string message = null)
        {
            bool ret = lambda.Invoke();
            if (ret)
            {
                throw new InvalidConstraintException(message);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="expression" /> se evalua como <c>true</c>.
        /// </summary>
        /// <param name="expression">Expresión para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfTrue(bool expression, string message = null)
        {
            if (expression)
            {
                throw new InvalidConstraintException(message);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentOutOfRangeException"/> cuando el argumento no está dentro del rango inclusivo. 
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="startRange">Valor de inicio del rango. <paramref name="argumentValue"/> debe ser menor que <paramref name="startRange"/></param>
        /// <param name="endRange">Valor final del rango. <paramref name="argumentValue"/> debe ser mayor que <paramref name="endRange"/></param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfOutOfRange(int argumentValue, int startRange, int endRange, string argumentName)
        {
            if (argumentValue < startRange || argumentValue > endRange)
            {
                throw new ArgumentOutOfRangeException(
                    argumentName,
                    argumentValue,
                    string.Format(Resources.ArgumentOutOfRangeMessageFormat, startRange, endRange));
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentOutOfRangeException"/> cuando el argumento no está en el rango.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfOutOfRange(Enum argumentValue, string argumentName)
        {
            Type enumType = argumentValue.GetType();

            if (!Enum.IsDefined(enumType, argumentValue))
            {
                throw new ArgumentOutOfRangeException(
                   argumentName,
                   argumentValue,
                   Resources.EnumIsNotDefinedMessage);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentOutOfRangeException"/> cuando el argumento no está en el rango.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="startRange">Valor de inicio del rango. <paramref name="argumentValue"/> debe ser mayor o igual que <paramref name="startRange"/></param>
        /// <param name="endRange">Valor final del rango. <paramref name="argumentValue"/> debe ser menor o igual que <paramref name="endRange"/></param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfInRange(int argumentValue, int startRange, int endRange, string argumentName)
        {
            if (argumentValue >= startRange && argumentValue <= endRange)
            {
                throw new ArgumentOutOfRangeException(
                    argumentName,
                    argumentValue,
                    string.Format(Resources.ArgumentInOfRangeMessageFormat, startRange, endRange));
            }
        }
    }
}