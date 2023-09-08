
# Tyler Cloud Platform Portal Web App Accelerator 2.0

## Features

* [ASP.Net 6.0](http://www.dot.net)
* [Entity Framework Core](https://docs.efproject.net/en/latest/)
* [Angular](https://angular.io/)
* [Angular CLI](https://cli.angular.io/) (for code scaffolding)
* [Typescript](http://www.typescriptlang.org/)
* [SASS](http://sass-lang.com/) support
* [Compodoc](https://compodoc.github.io/compodoc/) for Angular documentation
* [Swagger](http://swagger.io/)
* [Docker](https://www.docker.com/)
* [i18n](https://angular.io/guide/i18n#template-translations)
* [TCP Dynamic Auth](https://github.com/tyler-technologies/tcp-dynamic-auth)

## Prerequisites

* [.NET Core SDK](https://www.microsoft.com/net/download/core)
* [VSCode](https://code.visualstudio.com/#alt-downloads) or Visual Studio 2017
* [Omnisharp C# extension for VSCode](https://github.com/OmniSharp/omnisharp-vscode)
* [NodeJs](https://nodejs.org/en/download/)
* [NuGet cli](https://www.nuget.org/downloads)

## Running a local dev environment in Docker

The only supported way to develop an instance of this template
is to use the Cloud Platform Team's docker-compose environment.
This docker-compose environment emulates the production
environment in which your portal application will ultimately run.

To get a local Portal development environment running,
follow these steps:

1. Clone the following repo:  [platform-dev-environment-compose](https://github.com/tyler-technologies/platform-dev-environment-compose)

2. Run the following command in the directory into which you cloned
the repo in step 1:  docker-compose up -d

```NOTE:``` If you are running this docker-compose on a Windows machine
you will need to free up port 80, because the reverse-proxy service
in the docker-compose needs to listen on it.  The following are the most
common applications in Windows that listen on port 80 and ways
to kill those applications:

* IIS - run 'iisreset /stop' to kill
* World Wide Web Publishing service - run 'net stop http' to kill
* IIS Admin Service - run 'net stop http' or stop the service in 
the service console to kill
* SQL Server Reporting services - stop the service in the services console to kill
* Web Deployment Agent Service - stop the service in the services console to kill

3. Once your development environment is running, check to ensure the
KeyCloak service is running by browsing to the following url:
[http://localhost:6200](http://localhost:6200).  Do not proceed until the KeyCloak website
resolves without error.

4. We now need to license your application with the Cloud Platform, and we'll do this
by browsing to the following url:  [http://admin.localdev.tcpci.com/portal/portal-manager/](http://admin.localdev.tcpci.com/portal/portal-manager/)

5. Log in using the following credentials:
username: tyleridentityadmin@tylertech.com
password: Password@2

1. Click on the list item 'Just another portal' the 'dev' name listed on it.  Select the 'Licensing' tab.

2. Click the checkbox next to the 'dapper' product.  Then click 'Save'. *Note: If an error occurs while licensing the app, please follow the instructions [here](https://tylertech.slack.com/archives/CD57B86RK/p1573844406203100)*

3. Now that your environment is running, you simply run your application
in debug mode using Visual Studio Code.

10. Browse to the following url to test that your application runs:
[http://dev.localdev.tcpci.com:5000/app/new-application](http://dev.localdev.tcpci.com:5000/app/new-application)
You should see the getting-started page of your application,
and you should be able to click the login button in the omnibar
to log into the application.  Use 'bob' and 'secret' as the username
and password, respectively.

### Running with localization

Angular localizes applications at build time.  This means that each language gets
its own spa.  For optimization purposes, `ng serve` only serves 1 localization.
By default, this template uses `en`.

While the language switching is present, and sets the appropriate cookie, it will
not change the language of the application.  In a normal development configuration,
dotnet core sends unresolved requests to ng serve.  In order to setup the environment
for multiple languages, we need to disable the angular app from being served by
ng serve.

#### Setup for disabling ng-serve

1. set the `ignoreSpaDevServerMiddleware` flag in `appsettings.Development.json`
1. run `npm run build` in the `/client` folder to build the client application.  It's output is in `/client/build/`
1. run `npm run copy:build` in the `/311RequestSearch` directory to copy the output of angular (`/client/build/`) into the csproj-level `/spa` directory
1. start the debugger for aspnet core (F5)
1. use the language switcher to set the `.User.Locale` cookie, or make sure the cookie is deleted and add `es-MX` to the top of your languages in your browser setting.

### Updating base href

Due to how the client is statically built, there is not an easy button for setting the base href.
When the project is ready to update the base href in the form of `/product-name/application-name`,
it must be updated in the client app and the bff-server. 

* `client/src/index.html` update, the `<base href="/product-name/application-name"/>`
* `startup.cs` at the top of the `Startup.Configure()` method.  Modify the `rootPath` variable

```csharp
var rootPath = new PathString("/product-name/application-name");
```

## CI/CD

This project is configured to push to and pull from Artifactory. Before building you will need to perform the local developer setup [here](https://github.com/tyler-technologies/artifactory-github-migration/blob/master/01-local-dev-setup/README.md).

If your team has not used Artifactory for any projects and you intend to build with TeamCity, you will also need to perform the [team setup](https://github.com/tyler-technologies/artifactory-github-migration/blob/master/02-team-setup/README.md).

### Create GitHub Repository

GitHub projects are configured to use [GitHub Actions](https://docs.github.com/en/actions) instead of TeamCity. This script requires [jq](https://stedolan.github.io/jq/download/).

```bash
1_create_github_repository.sh
```

The GitHub Actions build and versioning process differs from how it is done in TeamCity. See [this](https://github.com/tyler-technologies/tcpweb-accelerator-core/blob/master/docs/continuous-integration/README.md) for details.

## Other commands

### Scaffold Angular components using Angular CLI

Scaffold  | Usage
---       | ---
Component | `ng g component my-new-component`
Directive | `ng g directive my-new-directive`
Pipe      | `ng g pipe my-new-pipe`
Service   | `ng g service my-new-service`
Class     | `ng g class my-new-class`
Guard     | `ng g guard my-new-guard`
Interface | `ng g interface my-new-interface`
Enum      | `ng g enum my-new-enum`
Module    | `ng g module my-module`

### Compodoc
* `npm run compodoc:watch`

### Swagger
* Visit http://localhost:5000/swagger when debugging to view the swagger api information.  If running in a production mode, then replace the port with the appropriate port. For example, if running with the docker steps below, the uri becomes http://localhost:85/swagger.

### Docker

This section discusses how to create a Docker container out of your
application and how to run that container locally.

## Creating a local docker container out of your application and running it
To create a local docker container, build the docker image with the following command:

```bash
npm run publish:release
```

Once your docker container is built, once you have your
local docker-compose development environment running,
and once you can reach KeyCloak at [http://localhost:6200](http://localhost:6200),
run your docker container using the following command:

```bash
docker run --network platform -p 5000:80 -e ConfigurationServer__uri=http://consul:8500 -l "traefik.frontend.rule=PathPrefix: /app/new-application" <containerName>:<containerVersion>
```

An example of the above command would be something like:

```bash
docker run --network platform -p 5000:80 -e ConfigurationServer__uri=http://consul:8500 -l "traefik.frontend.rule=PathPrefix: /app/new-application" my-fake-app:1.0.0
```

Ensure you have licensed your application in the dev tenant using the
instructions above for getting your local development environment running.
Then, navigate to the following url to test that your container is running:

[http://dev.localdev.tcpci.com/app/new-application](http://dev.localdev.tcpci.com/app/new-application)

### Consul

Consul is a configuration server that stores key/value pairs applications can request as
application configuration. Consul allows an application's configuration to live
in a central place available to all microservices which are part of a given application.
It also propogates updated configuration items to connected applications without
having to restart or cycle those applications.

In order to start using Consul, you simply need to add a Consul uri in the appsettings.json
file in this project under the `ConfigurationServer` configuration section.
Consul runs on port 8500 by default.

```json
"ConfigurationServer": {
    "uri": "http://localhost:8500",
    "retryCount": 3,
    "retryIntervalSec": 5
  }
```

If the uri property of this configuration section is specified, the application
will attempt to connect to Consul while bootstrapping. If the Consul server
cannot be reached, a fatal error will result. If the uri property of this
of this configuration section is not specified, the application will
not attempt to connect to a Consul configuration server.

## [AOT - Ahead of time](https://angular.io/docs/ts/latest/cookbook/aot-compiler.html) compilation DON'TS

### The following are some things that will make AOT compile fail.

* Don’t use `require()` statements for your templates or styles, use styleUrls and templateUrls, the angular2-template-loader plugin will change it to require at build time.
* Don’t use default exports.
* Don’t use form.controls.controlName, use form.get(‘controlName’)
* Don’t use control.errors?.someError, use control.hasError(‘someError’)
* Don’t use functions in your providers, routes or declarations, export a function and then reference that function name
* Inputs, Outputs, View or Content Child(ren), Hostbindings, and any field you use from the template or annotate for Angular should be public
* Don't dynamically load, non-AOT angular components.  AOT-compiled apps do not have the angular-compiler loaded on the client.


# Adding a rolling log file during local development

When in production, your application will log to console output (stdout), which will be picked up by a DataDog agent and shipped off to DataDogHQ.

There may be some situations where you wish to add a rolling log file to help you when working locally. This can be done by updating the Serilog configuration in app settings.

Update `appSettings.Development.json`

* Add `Serilog.Sinks.RollingFile` to the `Using` section
* Add rolling file config to the `WriteTo` section 


Example config - _additional configuration removed for brevity_

```JSON
"Serilog": {
    "Using": [
        "Serilog.Sinks.RollingFile"
    ],

    "WriteTo": [
        {
            "Name": "RollingFile",
            "Args": {
                "pathFormat": "logs/log-{Date}.txt",
                "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] [{RequestId}] {Message:lj}{NewLine}{Exception}"
            }
        }
    ],
}
```

Additional configuration and formatting can be found at the [Serilog repo](https://github.com/serilog/serilog) with specific formatting details found in the [Serilog wiki](https://github.com/serilog/serilog/wiki/Formatting-Output).

