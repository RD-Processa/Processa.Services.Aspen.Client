// -----------------------------------------------------------------------
// <copyright file="AspenClient.cs" company="Processa"> 
// Copyright (c) 2019 Todos los derechos reservados.
// </copyright>
// <author>atorrest</author>
// <date>2019-01-03 08:43 AM</date>
// ----------------------------------------------------------------------
namespace Processa.Services.Aspen.Client.Fluent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Auth;
    using Contracts;
    using Internals;
    using JWT;
    using JWT.Algorithms;
    using JWT.Serializers;
    using Newtonsoft.Json;
    using Providers;
    using RestSharp;

    /// <summary>
    /// Implementa un cliente que permite la conexión con el servicio Aspen.
    /// </summary>
    /// <seealso cref="Processa.Services.Aspen.Client.Fluent.IFluentClient" />
    /// <seealso cref="IFinancialModule" />
    public partial class AspenClient : IFluentClient, IAuthSettings, ILoggingSettings, IEndPointSettings, IRequester
    {
        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly IJwtAlgorithm algorithm = new HMACSHA256Algorithm();

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly ICustomHeaderManager customHeaderManager;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly IDateTimeProvider datetimeProvider = new UtcDateTimeProvider();

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly IJwtDecoder decoder;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly IJwtEncoder encoder;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly IEpochGenerator epochGenerator;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly INonceGenerator nonceGenerator;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly ISettings settings;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private readonly IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private IEndpointProvider endpointProvider;

        /// <summary>        
        /// Para uso interno.
        /// </summary>
        private IIdentityProvider identityProvider;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private ILoggingProvider loggingProvider = new ConsoleLoggingProvider();

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private LoggingLevel minimumLoggingLevel = LoggingLevel.All;

        /// <summary>        
        /// Para uso interno.
        /// </summary>
        private Dictionary<string, object> payload;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private IWebProxy proxy;

        /// <summary>
        /// Para uso interno.
        /// </summary>
        private RestClient restClient;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AspenClient"/>
        /// </summary>
        /// <param name="settings">Configuración de inicialización.</param>
        private AspenClient(ISettings settings)
        {
            this.nonceGenerator = settings?.NonceGenerator ?? new GuidNonceGenerator();
            this.epochGenerator = settings?.EpochGenerator ?? new UnixEpochGenerator();
            this.customHeaderManager = settings?.CustomHeaderManager ?? new CustomHeaderManager();
            IJsonSerializer serializer = settings?.JsonSerializer ?? new JsonNetSerializer();
            this.settings = settings;
            this.proxy = settings?.Proxy;
            this.encoder = new JwtEncoder(this.algorithm, serializer, this.urlEncoder);
            JwtValidator validator = new JwtValidator(serializer, this.datetimeProvider);
            this.decoder = new JwtDecoder(serializer, validator, this.urlEncoder);
            this.AppScope = settings?.AppScope ?? AppScope.Autonomous;
            this.DeviceInfo = settings?.DeviceInfo ?? new DeviceInfo();
        }

        /// <summary>
        /// Para uso interno.
        /// </summary>
        public AppScope AppScope { get; private set; }
        
        /// <summary>
        /// Obtiene el token de autenticación emitido para la sesión.
        /// </summary>        
        public IAuthToken AuthToken { get; private set; }

        /// <summary>
        /// Para uso interno.
        /// </summary>
        public IDeviceInfo DeviceInfo { get; private set; }
        
        /// <summary>
        /// Inicializa un objeto que permite la conexión con el sistema Aspen.
        /// </summary>        
        /// <param name="settings">Configuración de inicialización, o <see langword="null" /> para utilizar los valores predeterminados.</param>
        /// <returns>Instancia de <see cref="IEndPointSettings" /> que permite establecer la configuración de conexión.</returns>
        public static IEndPointSettings Initialize(ISettings settings)
        {
            Throw.IfNull(settings, nameof(settings));
            return new AspenClient(settings);
        }

        /// <summary>
        /// Inicializa un objeto que permite la conexión con el sistema Aspen.
        /// </summary>
        /// <param name="appScope">Alcance de la aplicación que se está conectando.</param>
        /// <returns>Instancia de <see cref="IEndPointSettings" /> que permite establecer la configuración de conexión.</returns>
        public static IEndPointSettings Initialize(AppScope appScope = AppScope.Autonomous)
        {
            ISettings customSettings = new DefaultSettings(appScope);
            return new AspenClient(customSettings);
        }

        /// <summary>
        /// Agrega las cabeceras necesarias para el procesamiento de la solicitud.
        /// </summary>
        /// <param name="request">Instancia de la solicitud a donde se agregan las cabeceras.</param>
        /// <param name="customPayload">Valores que se desean agregar al PayLoad de la solicitud o ; <see langword="null" /> para no agregar valores adicionales.</param>
        public void AddRequiredHeaders(IRestRequest request, IDictionary<string, object> customPayload = null)
        {
            this.payload = new Dictionary<string, object>
            {
                { this.nonceGenerator.Name, this.nonceGenerator.GetNonce() },
                { this.epochGenerator.Name, this.epochGenerator.GetSeconds() }
            };

            if (customPayload != null)
            {
                foreach (KeyValuePair<string, object> element in customPayload)
                {
                    this.payload.Add(element.Key, element.Value);
                }
            }

            if (this.AuthToken != null)
            {
                this.payload.Add("Token", this.AuthToken.Token);

                if (this.AuthToken is UserAuthToken context)
                {
                    this.payload.Add("DeviceId", context.DeviceId);
                    this.payload.Add("Username", context.Username);
                }
            }

            this.customHeaderManager.AddApiKeyHeader(request, this.identityProvider);
            this.customHeaderManager.AddPayloadHeader(request, this.encoder, this.payload, this.identityProvider);
        }

        /// <summary>
        /// Envía una solicitud de autenticación al servicio Aspen.
        /// </summary>
        /// <param name="customPayload">Información adicional para agregar en el Payload de la solicitud.</param>
        /// <param name="useCache">Cuando es <see langword="true" /> se utiliza el último token de autenticación generado en la sesión.</param>
        /// <returns>Instancia de <see cref="ISession" /> que se puede utilizar para solicitar más recursos de información al servicio Aspen.</returns>
        public ISession Authenticate(IDictionary<string, object> customPayload = null, bool useCache = true)
        {
            IRestRequest request = new AspenRequest(this, Routes.Auth.Signin, Method.POST, false);
            this.AddRequiredHeaders(request, customPayload);

            if (useCache)
            {
                this.AuthToken = CacheStore.GetCurrentToken();
                if (this.AuthToken != null)
                {
                    return this;
                }
            }

            string DecodeJsonResponse(string jwt) => this.decoder.Decode(jwt, this.identityProvider.ApiSecret, true);
            IAuthToken authToken;

            switch (this.AppScope)
            {
                case AppScope.Autonomous:
                    authToken = this.Execute<AuthToken>(request, DecodeJsonResponse);
                    break;
                case AppScope.Delegated:
                    authToken = this.Execute<UserAuthToken>(request, DecodeJsonResponse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.AuthToken = authToken;
            CacheStore.SetCurrentToken(authToken);
            return this;
        }

        /// <summary>
        /// Obtiene la instancia actual configurada para interactuar con el servicio Aspen.
        /// </summary>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite interactuar con el servicio Aspen.</returns>
        public IFluentClient GetClient()
        {
            return this;
        }

        /// <summary>
        /// Establece la configuración de trazas de seguimiento.
        /// </summary>
        /// <param name="minimumLoggingLevel">Mínimo nivel requerido para emitir trazas de seguimiento.</param>
        /// <param name="loggingProvider">Instancia de <see cref="ILoggingProvider" /> a donde se escriben las trazas de seguimiento.</param>
        /// <returns>Instancia de <see cref="IAuthSettings" /> que permite establecer la identidad de la aplicación solicitante.</returns>
        public IAuthSettings LoggingTo(LoggingLevel minimumLoggingLevel, ILoggingProvider loggingProvider = null)
        {
            this.minimumLoggingLevel = minimumLoggingLevel;
            this.loggingProvider = loggingProvider ?? new NullLoggingProvider();
            return this;
        }

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="endpointProvider">Instancia con la configuración del servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings" /> que permite la configuración de trazas de seguimiento.</returns>
        public ILoggingSettings RoutingTo(IEndpointProvider endpointProvider)
        {
            return this.RoutingTo(endpointProvider, null);
        }

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="endpointProvider">Instancia con la configuración del servicio Aspen.</param>
        /// <param name="proxy">Configuración del servidor Proxy que se debe utilizar para conectar con el servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings" /> que permite la configuración de trazas de seguimiento.</returns>
        public ILoggingSettings RoutingTo(IEndpointProvider endpointProvider, IWebProxy proxy)
        {
            Throw.IfNull(endpointProvider, nameof(endpointProvider));
            this.endpointProvider = endpointProvider;
            this.proxy = proxy;
            return this;
        }

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="url">URL del servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings" /> que permite la configuración de trazas de seguimiento.</returns>
        public ILoggingSettings RoutingTo(string url)
        {
            return this.RoutingTo(new StaticEndpointProvider(url), null);
        }

        /// <summary>
        /// Establece la Url del servicio Aspen a donde se envian las solicitudes.
        /// </summary>
        /// <param name="url">URL del servicio Aspen.</param>
        /// <param name="proxy">Configuración del servidor Proxy que se debe utilizar para conectar con el servicio Aspen.</param>
        /// <returns>Instancia de <see cref="ILoggingSettings" /> que permite la configuración de trazas de seguimiento.</returns>
        public ILoggingSettings RoutingTo(string url, IWebProxy proxy)
        {
            return this.RoutingTo(new StaticEndpointProvider(url), proxy);
        }

        /// <summary>
        /// Establece la identidad de la aplicación solicitante.
        /// </summary>
        /// <param name="apiKey">APIKey asignado a la aplicación que se está conectando con el servicio Aspen.</param>
        /// <param name="apiSecret">APISecret asignado a la aplicación que se está conectando con el servicio Aspen.</param>
        /// <param name="deviceInfo">Información del dispositivo desde donde se reliza la solicitud o  <see langword="null" /> para utilizar el valor predeterminado.</param>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite establecer conexión con el servicio Aspen.</returns>
        public IFluentClient WithIdentity(string apiKey, string apiSecret, IDeviceInfo deviceInfo = null)
        {
            return this.WithIdentity(new StaticIdentityProvider(apiKey, apiSecret), deviceInfo);
        }

        /// <summary>
        /// Establece la identidad de la aplicación solicitante.
        /// </summary>
        /// <param name="identityProvider">Identidad de la aplicación solicitante.</param>
        /// <param name="deviceInfo">Información del dispositivo desde donde se reliza la solicitud o  <see langword="null" /> para utilizar el valor predeterminado.</param>
        /// <returns>Instancia de <see cref="IFluentClient" /> que permite conectar con el servicio Aspen.</returns>
        public IFluentClient WithIdentity(IIdentityProvider identityProvider, IDeviceInfo deviceInfo = null)
        {
            Throw.IfNull(identityProvider, nameof(identityProvider));

            if (deviceInfo != null)
            {
                this.DeviceInfo = deviceInfo;
            }

            this.identityProvider = identityProvider;
            return this;
        }

        /// <summary>
        /// Obtiene el cupero que se está enviando con la solicitud (en formato Json).
        /// </summary>
        /// <param name="parameters">Parámetros de la solicitud.</param>
        /// <returns>Cadena en formato JSON con el cuerpo de la solicitud o <see langword="null" /> si no se envian datos en el cuerpo.</returns>
        private static string GetBody(IEnumerable<Parameter> parameters)
        {
            Parameter body = parameters.FirstOrDefault(item => item.Type == ParameterType.RequestBody);
            return body != null ?
                JsonConvert.SerializeObject(body.Value, Formatting.Indented) :
                string.Empty;
        }

        /// <summary>
        /// Obtiene la lista de cabeceras que se envian con la solicitud.
        /// </summary>
        /// <param name="parameters">Parámeros de la solicitud.</param>
        /// <returns>Listado de parámeros que se envian en la cabecera de la solicitud.</returns>
        private static Dictionary<string, object> GetHeaders(IEnumerable<Parameter> parameters)
        {
            return parameters.Where(item => item.Type == ParameterType.HttpHeader)
                                    .ToDictionary((p) => p.Name, (p) => p.Value);
        }

        /// <summary>
        /// Envía la solicitud al servicio Aspen, retornando la respuesta sin procesarla.
        /// </summary>
        /// <param name="request">Información de la solicitud.</param>
        /// <returns>Cadena con la respuesta en crudo del servicio.</returns>
        private string ExecuteRaw(IRestRequest request)
        {
            this.WriteDebugMessage("Uri", $"{this.restClient.BaseUrl}{request.Resource}");
            this.WriteDebugMessage("Method", request.Method.ToString());
            this.WriteDebugMessage("Proxy", $"{(this.restClient.Proxy as WebProxy)?.Address?.ToString() ?? "[NONE]"}");
            this.WriteDebugMessage("Headers", GetHeaders(request.Parameters));
            this.WriteDebugMessage("PayloadInfo", this.payload);
            this.WriteDebugMessage("Body", GetBody(request.Parameters));
            IRestResponse response = this.restClient.Execute(request);
            this.WriteDebugMessage("StatusCode", $"{(int)response.StatusCode} ({response.StatusCode.ToString()})");
            this.WriteDebugMessage("StatusDescription", response.StatusDescription);
            this.WriteInfoMessage("Content-Type", response.ContentType);
            this.WriteInfoMessage("RawResponse", response.Content);            

            if (!response.IsSuccessful)
            {
                throw new AspenResponseException(response);
            }

            return response.Content;
        }

        /// <summary>
        /// Envía la solicitud al servicio Aspen.
        /// </summary>
        /// <typeparam name="TResponse">Tipo al que se convierte la respuesta del servicio Aspen.</typeparam>
        /// <param name="request">Información de la solicitud.</param>
        /// <param name="preprocess">Código a ejecutar antes de enviar la solicitud.</param>
        /// <returns>Instancia de <c>TResponse</c> con la información de respuesta del servicio Aspen.</returns>
        /// <exception cref="AspenResponseException">Se presentó un error al procesar la solicitud. La excepción contiene los detalles del error.</exception>
        private TResponse Execute<TResponse>(IRestRequest request, Func<string, string> preprocess = null)
        {
            try
            {
                this.SetupRestClient(request);
                this.WriteDebugMessage("Uri", $"{this.restClient.BaseUrl}{request.Resource}");
                this.WriteDebugMessage("Method", request.Method.ToString());
                this.WriteDebugMessage("Proxy", $"{(this.restClient.Proxy as WebProxy)?.Address?.ToString() ?? "[NONE]"}");
                this.WriteDebugMessage("Headers", GetHeaders(request.Parameters));
                this.WriteDebugMessage("PayloadInfo", this.payload);
                this.WriteDebugMessage("Body", GetBody(request.Parameters));
                IRestResponse response = this.restClient.Execute(request);
                this.WriteDebugMessage("StatusCode", $"{(int) response.StatusCode} ({response.StatusCode.ToString()})");
                this.WriteDebugMessage("StatusDescription", response.StatusDescription);
                this.WriteInfoMessage("Content-Type", response.ContentType);
                this.WriteInfoMessage("RawResponse", response.Content);


                if (!response.IsSuccessful)
                {
                    throw new AspenResponseException(response);
                }

                string responseContent = response.Content;
                if (preprocess != null)
                {
                    responseContent = preprocess.Invoke(response.Content);
                }

                return JsonConvert.DeserializeObject<TResponse>(responseContent);
            }
            catch (Exception exception)
            {
                this.loggingProvider.WriteError(exception);
                throw;
            }
        }

        /// <summary>
        /// Envía la solicitud al servicio Aspen.
        /// </summary>
        /// <param name="request">Infromación de la solicitud.</param>        
        private void Execute(IRestRequest request)
        {            
            this.SetupRestClient(request);
            this.WriteDebugMessage("Uri", $"{this.restClient.BaseUrl}{request.Resource}");
            this.WriteDebugMessage("Method", request.Method.ToString());
            this.WriteDebugMessage("Proxy", $"{(this.restClient.Proxy as WebProxy)?.Address?.ToString() ?? "[NONE]"}");
            this.WriteDebugMessage("Headers", GetHeaders(request.Parameters));
            this.WriteDebugMessage("PayloadInfo", this.payload);
            this.WriteDebugMessage("Body", GetBody(request.Parameters));
            IRestResponse response = this.restClient.Execute(request);
            this.WriteDebugMessage("StatusCode", $"{(int)response.StatusCode} ({response.StatusCode.ToString()})");
            this.WriteDebugMessage("StatusDescription", response.StatusDescription);
            this.WriteInfoMessage("Content-Type", response.ContentType);
            this.WriteInfoMessage("RawResponse", response.Content);

            if (!response.IsSuccessful)
            {
                throw new AspenResponseException(response);
            }
        }

        /// <summary>
        /// Envía la solicitud al servicio Aspen de forma asíncrona.
        /// </summary>
        /// <param name="request">Infromación de la solicitud.</param>
        /// <returns>Isntancia de Task con el resultado de la ejecución de la tarea.</returns>
        private async Task<IRestResponse> ExecuteAsync(IRestRequest request)
        {
            return await this.restClient.ExecuteTaskAsync(request);
        }

        /// <summary>
        /// Establece la configuración del objeto que establece la comunicación con el servicio Aspen.
        /// </summary>
        /// <param name="request">Información de la solicitud.</param>
        private void SetupRestClient(IRestRequest request)
        {
            if (this.restClient == null)
            {
                this.restClient = new RestClient(this.endpointProvider.Url.TrimEnd('/'));
                if (this.proxy != null)
                {
                    this.restClient.Proxy = this.proxy;
                }
            }

            if (this.settings.Timeout <= 0)
            {
                return;
            }

            this.restClient.Timeout = this.settings.Timeout;
            request.Timeout = this.restClient.Timeout;
        }

        /// <summary>
        /// Escribe un mensaje de depuración en la traza de seguimiento.
        /// </summary>
        /// <param name="key">Identificador del mensaje.</param>
        /// <param name="value">Texto del mensaje.</param>
        private void WriteDebugMessage(string key, object value)
        {
            if (this.minimumLoggingLevel < LoggingLevel.Debug)
            {
                return;
            }

            if (value != null)
            {
                if (!(value is string))
                {
                    value = JsonConvert.SerializeObject(value, Formatting.Indented);
                }
            }

            this.loggingProvider.WriteDebug($"{key} => {value}");
        }

        /// <summary>
        /// Escribe un mensaje de información en la traza de seguimiento.
        /// </summary>
        /// <param name="object">Objeto que se escribe en el mensaje.</param>
        /// <param name="message">Texto que acompaña al mensaje.</param>        
        private void WriteInfoMessage(string message, object @object = null)
        {
            if (this.minimumLoggingLevel >= LoggingLevel.Info)
            {
                this.loggingProvider.WriteInfo(message, @object);
            }
        }
    }
}