#FunctionDataverseTrial

To local run:
1) first init secrets
> dotnet user-secrets init

2) add secret, replace **username**, **password**, **url** with you actual cred
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" 
"AuthType=OAuth;Username=username;Password=password;Url=url" --project "./FunctionDataverseTrial"

3) check
> dotnet user-secrets list --project "./FunctionDataverseTrial"

To azure run:
> dotnet publish -c Release /p:WebPublishMethod=Package /p:PackageLocation="./"

> az login

> az functionapp deployment source config-local-git -g rent-ready -n FunctionDataverseTrial20220428145733

> git remote add azure https://None@functiondataversetrial20220428145733.scm.azurewebsites.net/FunctionDataverseTrial20220428145733.git



### ps:
login like some@some.onmicrosoft.com  
url like https://contoso.crm4.dynamics.com/

### docs:
https://docs.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-xrm-tooling-create-data  
https://docs.microsoft.com/ru-ru/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux  
https://docs.microsoft.com/en-us/azure/azure-functions/deployment-zip-push  
https://github.com/projectkudu/kudu/wiki/Deploying-from-a-zip-file-or-url  