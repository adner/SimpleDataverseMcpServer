using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<IOrganizationService>(provider =>
{
    // TODO Enter your Dataverse environment's URL and logon info.
    string url = "[URL to your environment]";
    string connectionString = $@"
    AuthType = ClientSecret;
    Url = {url};
    ClientId = [ClientId];
    Secret = [ClientSecret]";

    return new ServiceClient(connectionString);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync(); 

[McpServerToolType]
public static class DataverseTool
{
    [McpServerTool, Description("Executes an WhoAmI request aginst Dataverse and returns the result as a JSON string.")]
    public static string WhoAmI(IOrganizationService orgService)
    {
        try
        {
            WhoAmIRequest req = new WhoAmIRequest();

            var whoAmIResult = orgService.Execute(req);

            return Newtonsoft.Json.JsonConvert.SerializeObject(whoAmIResult);
        }
        catch (Exception err)
        {
            Console.Error.WriteLine(err.ToString());

            return err.ToString();
        }
    }

    [McpServerTool, Description("Executes an FetchXML request using the supplied expression that needs to be a valid FetchXml expression. Returns the result as a JSON string. If the request fails, the response will be prepended with [ERROR] and the error should be presented to the user.")]
    public static string ExecuteFetch(string fetchXmlRequest, IOrganizationService orgService)
    {
        try
        {
            FetchExpression fetchExpression = new FetchExpression(fetchXmlRequest);
            EntityCollection result = orgService.RetrieveMultiple(fetchExpression);

            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }
        catch (Exception err)
        { 
            var errorString = "[ERROR] " + err.ToString();
            Console.Error.WriteLine(err.ToString());

            return errorString;
        }

    }
}






