// -----------------------------------------------------------------------
// <copyright file="Throw.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-02-26 11:22 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Entities
{
#pragma warning disable 1710
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Expone métodos para generar excepciones al validar parámetros.
    /// </summary>
    public static class Throw
    {
        /// <summary>
        /// Genera una excepción del tipo <typeparam name="TException" /> si <paramref name="argumentValue" /> es <c>null</c>.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfNull<TException>(object argumentValue, string argumentName, string errorMessage = "cannot be a null value.")
            where TException : Exception
        {
            if (argumentValue != null)
            {
                return;
            }

            ThrowException<TException>($"{argumentName} {errorMessage}");
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentException"/> si <paramref name="argumentValue"/> es <c>null</c>.
        /// </summary>
        /// <typeparam name="TValue">Tipo del argumento a validar</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNull<TValue>(TValue? argumentValue, string argumentName) where TValue : struct
        {
            if (!argumentValue.HasValue)
            {
                IfNull<ArgumentNullException>(null, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción del tipo <typeparam name="TException" /> si <paramref name="argumentValue" /> es <c>null</c>.
        /// </summary>
        /// <typeparam name="TValue">Tipo del argumento a validar</typeparam>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNull<TValue, TException>(TValue? argumentValue, string argumentName)
            where TValue : struct
            where TException : Exception
        {
            if (!argumentValue.HasValue)
            {
                IfNull<TException>(null, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException"/> si <paramref name="argumentValue"/> es <c>null</c>.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNull(object argumentValue, string argumentName)
        {
            IfNull<ArgumentNullException>(argumentValue, argumentName);
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
                IfNull<ArgumentNullException>(null, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException" /> si <paramref name="argumentValue" /> es <c>null</c>, o si <paramref name="argumentValue" /> es <see cref="String.Empty" />.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfNullOrEmpty(string argumentValue, string argumentName, string errorMessage = "cannot be an empty string.")
        {
            IfNull<ArgumentNullException>(argumentValue, argumentName);
            IfNullOrEmpty<ArgumentException>(argumentValue, argumentName, errorMessage);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> si <paramref name="argumentValue" /> es <c>null</c>, o si <paramref name="argumentValue" /> es <see cref="String.Empty" />.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfNullOrEmpty<TException>(string argumentValue, string argumentName, string errorMessage = "cannot be an empty string.")
            where TException : Exception
        {
            IfNull<ArgumentNullException>(argumentValue, argumentName);

            if (argumentValue.Length > 0)
            {
                return;
            }

            ThrowException<TException>($"{argumentName} {errorMessage}");
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException"/> si <paramref name="argumentValue"/> es <c>null</c>, o 
        /// <exception cref="ArgumentException"/> si <paramref name="argumentValue.Count"/> es cero.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IfNullOrEmpty(ICollection argumentValue, string argumentName)
        {
            IfNull<ArgumentNullException>(argumentValue, argumentName);
            IfNullOrEmpty<ArgumentException>(argumentValue, argumentName);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> si <paramref name="argumentValue" /> es <c>null</c>, o si <paramref name="argumentValue.Count" /> es cero.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfNullOrEmpty<TException>(ICollection argumentValue, string argumentName, string errorMessage = "cannot be an empty collection.")
            where TException : Exception
        {
            IfNull<ArgumentNullException>(argumentValue, argumentName);

            if (argumentValue.Count > 0)
            {
                return;
            }

            ThrowException<TException>($"{argumentName} {errorMessage}");
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException" /> si <paramref name="argumentValue" /> es <c>null</c>, o
        /// <exception cref="ArgumentException" /> si <paramref name="argumentValue" /> contiene algún elemento <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos en la colección</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>        
        /// <exception cref="ArgumentException">La colección contiene algún elemento <c>null</c>.</exception>
        public static void IfHasNull<T>(IEnumerable<T> argumentValue, string argumentName, string errorMessage = "cannot contains a null item in the collection.")
        {
            IfNull<ArgumentNullException>(argumentValue, argumentName);

            if (argumentValue.Any(item => item == null))
            {
                IfNull<ArgumentNullException>(null, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> si <paramref name="argumentValue" /> es <c>null</c>, o si <paramref name="argumentValue" /> contiene algún elemento <c>null</c>.
        /// </summary>
        /// <typeparam name="TValue">Tipo de los elementos en la colección</typeparam>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <exception cref="ArgumentException">La colección contiene algún elemento <c>null</c>.</exception>
        public static void IfHasNull<TValue, TException>(IEnumerable<TValue> argumentValue, string argumentName)
            where TException : Exception
        {
            IfNull<TException>(argumentValue, argumentName);

            if (argumentValue.Any(item => item == null))
            {
                IfNull<TException>(null, argumentName);
            }
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentNullException" /> si <paramref name="argumentValue" /> es <see cref="Guid.Empty" />.
        /// </summary>
        /// <typeparam name="T">Tipo de <paramref name="argumentValue"/> que se está validando.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>        
        public static void IfEmpty<T>(T argumentValue, string argumentName, string errorMessage = "cannot be an invalid value")
        {
            IfEmpty<T, ArgumentException>(argumentValue, argumentName, errorMessage);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> si <paramref name="argumentValue" /> es <see cref="Guid.Empty" />.
        /// </summary>
        /// <typeparam name="TValue">Tipo de <paramref name="argumentValue" /> que se está validando.</typeparam>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfEmpty<TValue, TException>(TValue argumentValue, string argumentName, string errorMessage = "cannot be an invalid value")
            where TException : Exception
        {
            if (argumentValue == null)
            {
                ThrowException<TException>($"{argumentName} {errorMessage}");
            }

            if (typeof(TValue) == typeof(string))
            {
                string s = Convert.ToString(argumentValue);
                if (string.IsNullOrWhiteSpace(s))
                {
                    ThrowException<TException>($"{argumentName} {errorMessage}");
                }
            }

            Debug.Assert(argumentValue != null, nameof(argumentValue) + " != null");
            if (!argumentValue.Equals(default(TValue)))
            {
                return;
            }

            ThrowException<TException>($"{argumentName} {errorMessage}");
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="lambda" /> se evalua como <c>false</c>.
        /// </summary>
        /// <param name="lambda">Expresión lambda para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfFalse(Func<bool> lambda, string message = null)
        {
            bool ret = lambda.Invoke();
            IfFalse<InvalidConstraintException>(ret, message);
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="expression" /> se evalua como <c>false</c>.
        /// </summary>
        /// <param name="expression">Expresión para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfFalse(bool expression, string message = null)
        {
            IfFalse<InvalidConstraintException>(expression, message);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> si <paramref name="expression" /> se evalua como <c>false</c>.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="expression">Expresión para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfFalse<TException>(bool expression, string message = null)
            where TException : Exception
        {
            if (expression)
            {
                return;
            }

            ThrowException<TException>($"{message ?? "null message"}");
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="lambda" /> se evalua como <c>true</c>.
        /// </summary>
        /// <param name="lambda">Expresión lambda para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfTrue(Func<bool> lambda, string message = null)
        {
            bool ret = lambda.Invoke();
            IfTrue<InvalidConstraintException>(ret, message);
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidConstraintException" /> si <paramref name="expression" /> se evalua como <c>true</c>.
        /// </summary>
        /// <param name="expression">Expresión para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfTrue(bool expression, string message = null)
        {
            IfTrue<InvalidConstraintException>(expression, message);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> si <paramref name="expression" /> se evalua como <c>true</c>.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="expression">Expresión para evaluar.</param>
        /// <param name="message">Texto que describe la excepción.</param>
        public static void IfTrue<TException>(bool expression, string message = null)
            where TException : Exception
        {
            if (!expression)
            {
                return;
            }

            ThrowException<TException>($"{message ?? "null message"}");
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
            IfOutOfRange<ArgumentOutOfRangeException>(argumentValue, startRange, endRange, argumentName);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException"/> cuando el argumento no está dentro del rango inclusivo.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción que se debe generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="startRange">Valor de inicio del rango. <paramref name="argumentValue" /> debe ser menor que <paramref name="startRange" /></param>
        /// <param name="endRange">Valor final del rango. <paramref name="argumentValue" /> debe ser mayor que <paramref name="endRange" /></param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto para la excepción.</param>
        public static void IfOutOfRange<TException>(int argumentValue, int startRange, int endRange, string argumentName, string errorMessage = "cannot be outside the range")
            where TException : Exception
        {
            if (argumentValue.Between(startRange, endRange))
            {
                return;
            }

            ThrowException<TException>($"{argumentValue} {errorMessage} ({startRange})-({endRange})");
        }

        /// <summary>
        /// Genera una excepción <exception cref="InvalidEnumArgumentException" /> cuando el argumento no está en el rango.
        /// </summary>
        /// <typeparam name="TENum">Tipo de la enumeración a evaluar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        public static void IsUndefined<TENum>(object argumentValue, string argumentName)
            where TENum : struct, IConvertible
        {
            Type enumType = typeof(TENum);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            try
            {
                if (!Enum.IsDefined(enumType, argumentValue))
                {
                    throw new InvalidEnumArgumentException(argumentName, Convert.ToInt32(argumentValue), enumType);
                }
            }
            catch (FormatException exception)
            {
                string message = $"{argumentValue} is not valid for Enum {enumType.Name}. ({exception.Message})";
                throw new InvalidEnumArgumentException(message, exception);
            }
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException" /> cuando el valor de <paramref name="argumentValue" /> no está incluido en la enumeración.
        /// </summary>
        /// <typeparam name="TEnum">Tipo de la enumeración.</typeparam>
        /// <typeparam name="TException">Tipo de la excepción que se desde generar.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto del error.</param>
        public static void IsUndefined<TEnum, TException>(object argumentValue, string argumentName, string errorMessage = "is invalid for enum")
            where TEnum : struct, IConvertible
            where TException : Exception
        {
            Type enumType = typeof(TEnum);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            ThrowException<TException>($"{argumentValue} {errorMessage ?? "null message"} {enumType.Name}");
        }

        /// <summary>
        /// Genera una excepción <exception cref="ArgumentOutOfRangeException" /> cuando <paramref name="argumentValue"/> está en el rango.
        /// </summary>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="startRange">Valor de inicio del rango. <paramref name="argumentValue" /> debe ser mayor o igual que <paramref name="startRange" /></param>
        /// <param name="endRange">Valor final del rango. <paramref name="argumentValue" /> debe ser menor o igual que <paramref name="endRange" /></param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto del error.</param>
        public static void IfInRange(int argumentValue, int startRange, int endRange, string argumentName, string errorMessage = "cannot be inside the range")
        {
            IfInRange<ArgumentOutOfRangeException>(argumentValue, startRange, endRange, errorMessage);
        }

        /// <summary>
        /// Genera una excepción <typeparam name="TException"/> cuando <paramref name="argumentValue"/> está en el rango.
        /// </summary>
        /// <typeparam name="TException">The type of the t exception.</typeparam>
        /// <param name="argumentValue">Valor del argumento que se valida.</param>
        /// <param name="startRange">Valor de inicio del rango. <paramref name="argumentValue" /> debe ser mayor o igual que <paramref name="startRange" /></param>
        /// <param name="endRange">Valor final del rango. <paramref name="argumentValue" /> debe ser menor o igual que <paramref name="endRange" /></param>
        /// <param name="argumentName">Nombre del argumento que se valida.</param>
        /// <param name="errorMessage">Texto del error.</param>
        public static void IfInRange<TException>(int argumentValue, int startRange, int endRange, string argumentName, string errorMessage = "cannot be inside the range") where TException : Exception
        {
            if (!argumentValue.Between(startRange, endRange))
            {
                return;
            }

            ThrowException<TException>($"{errorMessage} ({startRange})-({endRange})");
        }

        /// <summary>
        /// Genera una excepción del tipo especificado.
        /// </summary>
        /// <typeparam name="TException">Tipo de la excepción a generar.</typeparam>
        /// <param name="message">Texto del mensaje.</param>
        private static void ThrowException<TException>(string message)
        {
            Type exceptionType = typeof(TException);
            try
            {
                Exception exception = (Exception)Activator.CreateInstance(exceptionType, message);
                throw exception;
            }
            catch (MissingMethodException)
            {
                string notSupportedMessage = $"ctor with parameter string not found in type: {exceptionType.Name}";
                throw new NotSupportedException(notSupportedMessage);
            }
        }
    }
#pragma warning restore 1710
}