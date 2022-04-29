## Setup

### Prerequisites
- Install [Visual Studio]
- Install [dotnet sdk](https://dotnet.microsoft.com/en-us/download)
- Install [azure cli](https://docs.microsoft.com/ru-ru/cli/azure/install-azure-cli)
- Install [azure-functions-core-tools](https://docs.microsoft.com/ru-ru/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cazurecli%2Cbash#start) i used **scoop install azure-functions-core-tools**

### Setting Up a Env

#### Local
1) first init secrets
> dotnet user-secrets init

1) add secret, replace **username**, **password**, **url** with you actual cred
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" 
"AuthType=OAuth;Username=username;Password=password;Url=url" --project "./FunctionDataverseTrial"

1) check
> dotnet user-secrets list --project "./FunctionDataverseTrial"

#### Azure
1) create resource_group or use existing
1) create azure function
1) set function env **ConnectionStrings:DefaultConnection** with **AuthType=OAuth;Username=username;Password=password;Url=url**
1) use <resource_group> and <app_name> in publish config from prev steps 

### Run App:

- Build: 
```
dotnet build
```
- Test: 
```
dotnet test
```
- Publish:  
may be from VS puplish or cmd
```
dotnet publish -c Release -o publish_output FunctionDataverseTrial\FunctionDataverseTrial.csproj

zip publish_output to publish_output.zip

az login  
az functionapp deployment source config-zip -g <resource_group> -n <app_name> --src publish_output.zip
```

## ps:
login like some@some.onmicrosoft.com  
url like https://contoso.crm4.dynamics.com/

## docs:
https://docs.microsoft.com/ru-ru/azure/azure-functions/functions-run-local?tabs=v4%2Clinux%2Ccsharp%2Cazurecli%2Cbash#start
https://docs.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-xrm-tooling-create-data  
https://docs.microsoft.com/ru-ru/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux  
https://docs.microsoft.com/en-us/azure/azure-functions/deployment-zip-push  
https://github.com/projectkudu/kudu/wiki/Deploying-from-a-zip-file-or-url  