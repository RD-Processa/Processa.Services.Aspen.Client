# Processa.Services.Aspen.Client
Versión alpha del cliente del servicio Aspen


```c#
IFluentClient client = AspenClient.Initialize()
                            .RoutingTo("https://localhost/api")
                            .WithIdentity("MyApyKey", "MyApiSecret")
                            .Authenticate()
                            .GetClient();

var docTypes = client.Settings.GetDocTypes();
```