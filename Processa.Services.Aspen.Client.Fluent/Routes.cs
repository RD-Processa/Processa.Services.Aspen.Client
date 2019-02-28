// -----------------------------------------------------------------------
// <copyright file="Routes.cs" company="Processa"> 
// Copyright (c) 2018 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2018-06-29 07:09 am</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System.Threading.Tasks;

    /// <summary>
    /// Facilita la definición de Endpoints del servicio.
    /// </summary>
    public static class Routes
    {
        /// <summary>
        /// Ruta que se utiliza como raíz al exponer los EndPoint propios del servicio.
        /// </summary>
        public const string BaseRoot = "/api";

        /// <summary>
        /// Endpoint raíz para aplicaciones autónomas.
        /// </summary>
        public static string AutonomousRoot => "/app";

        /// <summary>
        /// Endpoint raíz para aplicaciones delegadas.
        /// </summary>
        public static string DelegatedRoot => "/me";

        /// <summary>
        /// Endpoints para procesos de autenticación.
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// Endpoint raíz para los proceso de autorización.
            /// </summary>
            public static string Root => "/auth";

            /// <summary>
            /// Endpoint para el proceso de autenticación de aplicaciones autónomas y delegadas.
            /// </summary>
            public static string Signin => $"{Root}/signin";

            /// <summary>
            /// Endpoint para el proceso de actualización del secreto de una aplicación.
            /// </summary>
            public static string Secret => $"{Root}/secret";
        }

        /// <summary>
        /// Endpoints para utilidades varias del servicio.
        /// </summary>
        public static class Util
        {
            /// <summary>
            /// Endpoint para operaciones de utilitarios del servicio.
            /// </summary>
            public static string Root => "/utils";

            /// <summary>
            /// Endpoint del utilitario para cifrar cadenas.
            /// </summary>
            public static string Crypto => "/crypto";

            /// <summary>
            /// Endpoint del utilitario para el estado del servicio.
            /// </summary>
            public static string Status => "/status";

            /// <summary>
            /// Endpoint del utilitario para obtener los tipos de documento predeterminados.
            /// </summary>
            public static string DocTypes => "/document-types";

            /// <summary>
            /// Endpoint del utilitario para registrar las excepciones que se produzcan por cierres inesperados de una aplicación móvil.
            /// </summary>
            public static string AppCrash => "/app/crash";
        }

        /// <summary>
        /// Endpoints para recursos de aplicación.
        /// </summary>
        public static class Resources
        {
            /// <summary>
            /// Endpoint raíz para las operaciones de recursos.
            /// </summary>
            public static string Root => "/resx";

            /// <summary>
            /// Endpoint para obtener el menú de una aplicación.
            /// </summary>
            public static string Menu => "/menu";

            /// <summary>
            /// Endpoint para obtener los operadores de telefonía móvil de una aplicación.
            /// </summary>
            public static string Telcos => "/telcos";

            /// <summary>
            /// Endpoint para obtener los tipos de documentos de una aplicación.
            /// </summary>
            public static string DocTypes => $"{Root}/document-types";

            /// <summary>
            /// Endpoint para obtener los tipos de transacción de una aplicación.
            /// </summary>
            public static string TranTypes => "/tran-types";

            /// <summary>
            /// Endpoint para obtener los tipos de pago de una aplicación.
            /// </summary>
            public static string PaymentTypes => "/payment-types";

            /// <summary>
            /// Endpoint para obtener los valores admitidos para los procesos de recargas de celulares de una aplicación.
            /// </summary>
            public static string TopUp => "/topups";

            /// <summary>
            /// Endpoint para obtener los valores misceláneos de la aplicación.
            /// </summary>
            public static string Miscellaneous => "/miscs";
        }

        /// <summary>
        /// Endpoints de consultas.
        /// </summary>
        public static class Inquires
        {
            /// <summary>
            /// Endpoint raíz para las operaciones de consultas.
            /// </summary>
            public static string Root => "/inquires";

            /// <summary>
            /// Endpoints para las operaciones de consultas del usuario actual.
            /// </summary>
            public static class Delegated
            {
                /// <summary>
                /// Endpoint para obtener las cuentas del usuario actual.
                /// </summary>
                public static string Accounts => $"{Inquires.Root}/accounts";

                /// <summary>
                /// Endpoint para obtener los balances de una cuenta.
                /// </summary>
                public static string Balances => $"{Inquires.Root}/accounts/@[AccountId]/balances";

                /// <summary>
                /// Endpoint para obtener los movimientos de una cuenta.
                /// </summary>
                public static string Statements => $"{Inquires.Root}/accounts/@[AccountId]/@[AccountTypeId]/statements";
            }

            /// <summary>
            /// Endpoints para las operaciones de consultas de un usuario para aplicaciones autónomas.
            /// </summary>
            public static class Autonomous
            {
                /// <summary>
                /// Endpoint para obtener las cuentas de un usuario.
                /// </summary>
                public static string Accounts => $"{Inquires.Root}/accounts/@[DocType]/@[DocNumber]";

                /// <summary>
                /// Endpoint para obtener las cuentas de un usuario por alias de registro.
                /// </summary>
                public static string AccountsByAlias => $"{Inquires.Root}/accounts/channel/@[ChannelId]/alias/@[EnrollmentAlias]";

                /// <summary>
                /// Endpoint para obtener los balances de una cuenta.
                /// </summary>
                public static string Balances => $"{Inquires.Root}/accounts/@[DocType]/@[DocNumber]/@[AccountId]/balances";

                /// <summary>
                /// Endpoint para obtener los saldos de una cuenta por alias de registro.
                /// </summary>
                public static string BalancesByAlias => $"{Inquires.Root}/accounts/channel/@[ChannelId]/alias/@[EnrollmentAlias]/id/@[AccountId]/balances";

                /// <summary>
                /// Endpoint para obtener los movimientos de una cuenta.
                /// </summary>
                public static string Statements => $"{Inquires.Root}/accounts/@[DocType]/@[DocNumber]/@[AccountId]/@[AccountTypeId]/statements";

                /// <summary>
                /// Endpoint para obtener los movimientos de una cuenta por alias de registro.
                /// </summary>
                public static string StatementsByAlias => $"{Inquires.Root}/accounts/channel/@[ChannelId]/alias/@[EnrollmentAlias]/id/@[AccountId]/type/@[AccountTypeId]/statements";
            }
        }

        /// <summary>
        /// Endpoints de procesos con tokens.
        /// </summary>
        public static class Tokens
        {
            /// <summary>
            /// Endpoint raíz para las operaciones con tokens.
            /// </summary>
            public static string Root => "/tokens";

            /// <summary>
            /// Endpoint para obtener los canales disponibles para emitir tokens transaccionales.
            /// </summary>
            public static string Channels => "/channels";

            /// <summary>
            /// Endpoint para generar un token transaccional.
            /// </summary>
            public static string GenerateDelegated => "/";

            /// <summary>
            /// Endpoint para redimir un token transaccional.
            /// </summary>
            public static string Redeem => $"{Tokens.Root}/@[Token]";

            /// <summary>
            /// Endpoint para el solicitar el envío de un token transaccional.
            /// </summary>
            public static string RequestToken => $"{Tokens.Root}/send";

            /// <summary>
            /// Endpoint para solicitar la imagen de un token transaccional.
            /// </summary>
            public static string ImageToken => $"{Tokens.Root}/channel/@[ChannelId]/alias/@[EnrollmentAlias]";

            /// <summary>
            /// Endpoint para solicitar el archivo pdf con el resument de los estados de cuenta.
            /// </summary>
            public static string PfdStatements => $"{Inquires.Root}/accounts/channel/@[ChannelId]/alias/@[EnrollmentAlias]/summary";
        }

        /// <summary>
        /// Endpoints para procesos de transferencias.
        /// </summary>
        public static class Transfers
        {
            /// <summary>
            /// Endpoint raíz para las operaciones de transferencias.
            /// </summary>
            public static string Root => "/transfers";

            /// <summary>
            /// Endpoints para administrar cuentas para transferencias de un usuario.
            /// </summary>
            public static class Delegated
            {
                /// <summary>
                /// Endpoint para obtener las cuentas registradas por el usuario para operaciones de transferencia.
                /// </summary>
                public static string Accounts => "/accounts";

                /// <summary>
                /// Endpoint para eliminar una cuenta registrada por el usuario usando el alias de la cuenta.
                /// </summary>
                public static string Unlink => "/accounts/{Alias}";
            }

            /// <summary>
            /// Endpoints para administrar cuentas para transferencias de una aplicación autónoma.
            /// </summary>
            public static class Autonomous
            {
                /// <summary>
                /// Endpoint para obtener las cuentas registradas de un usuario.
                /// </summary>
                public static string Accounts => "/accounts/{DocType}/{DocNumber}";

                /// <summary>
                /// Endpoint para eliminar una cuenta registrada de un usuario y el alias de la cuenta.
                /// </summary>
                public static string Unlink => "/accounts/{DocType}/{DocNumber}/{Alias}";
            }
        }

        /// <summary>
        /// Endpoints para procesos con datos de usuarios.
        /// </summary>
        public static class Users
        {
            /// <summary>
            /// Endpoint raíz para las operaciones con datos de usuarios.
            /// </summary>
            private static string Root => "/user";

            /// <summary>
            /// Endpoint para obtener los datos del perfíl del usuario actual.
            /// </summary>
            public static string Current => "/current";

            /// <summary>
            /// Endpoint para establecer el pin transaccional de usuario actual.
            /// </summary>
            public static string Pin => $"{Root}/pin";

            /// <summary>
            /// Endpoint para solicitar un código de activación.
            /// </summary>
            public static string ActivationCode => $"{Root}/activationcode";
        }

        /// <summary>
        /// Endpoints para procesos con notificaciones push.
        /// </summary>
        public static class Push
        {
            /// <summary>
            /// Endpoint raíz para las operaciones con notificaciones push.
            /// </summary>
            public static string Root => "/push";

            /// <summary>
            /// Endpoint para obtener las notificaciones del usuario.
            /// </summary>
            public static string GetMessages => "/";

            /// <summary>
            /// Endpoint para marcar como leída una notificación.
            /// </summary>
            public static string ToggleRead => "/{TraceId}";

            /// <summary>
            /// Endpoint para actualizar el badge.
            /// </summary>
            public static string ClearBadge => "/badge";

            /// <summary>
            /// Endpoint para registrar el dispositivo del usuario.
            /// </summary>
            public static string RegisterDevice => "/";
        }

        /// <summary>
        /// Endpoints para procesos financieros.
        /// </summary>
        public static class Financial
        {
            /// <summary>
            /// Endpoint raíz para las operaciones financieras.
            /// </summary>
            public static string Root => "/financial";

            /// <summary>
            /// Endpoint de recargas a celular.
            /// </summary>
            public static string Topup => "/topup";

            /// <summary>
            /// Endpoint para la transferencia electrónica de fondos.
            /// </summary>
            public static string Transfer => "/transfers";

            /// <summary>
            /// /// Endpoint para el procesamiento de un retiro.
            /// </summary>
            public static string Withdrawal => $"{Financial.Root}/withdrawal";
        }

        /// <summary>
        /// Endpoints para procesos administrativos.
        /// </summary>
        public static class Management
        {
            /// <summary>
            /// Endpoint raíz para procesos administrativos.
            /// </summary>           
            private static string Root => "/management";

            /// <summary>
            /// Endpoint para validaciones de códigos de activación.
            /// </summary>
            public static string ActivationCode => $@"{Root}/activationcode";

            /// <summary>
            /// Endpoint para el registro de cuentas para transferencias.
            /// </summary>
            public static string LinkTransferAccount => "/transfers/accounts/docType/@[DocType]/docNumber/@[DocNumber]";

            /// <summary>
            /// Endpoint para desvincular cuentas para transferencias.
            /// </summary>
            public static string UnlinkTransferAccount => "/transfers/accounts/docType/@[DocType]/docNumber/@[DocNumber]/Alias/@[Alias]";
        }
    }
}