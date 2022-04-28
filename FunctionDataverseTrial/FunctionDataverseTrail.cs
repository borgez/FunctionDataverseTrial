using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace FunctionDataverseTrial;

public class FunctionDataverseTrail
{
    private readonly ILogger _logger;
    private readonly IOrganizationService _organizationService;

    public FunctionDataverseTrail(ILogger<FunctionDataverseTrail> log, IOrganizationService organizationService)
    {
        _logger = log;
        _organizationService = organizationService;
    }

    [FunctionName("FunctionDataverseTrail")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(Model))]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent)]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "test/dataverse/trail")]
        Model request)
    {
        _logger.LogTrace("C# HTTP trigger function processed a request {@body}.", request);

        // next: move to middleware
        if (!Validate(request, out var validationResults))
            return new BadRequestObjectResult(
                $"{nameof(Model)} is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}");

        Handler(request);

        return new NoContentResult();
    }

    // next: use FluentValidation
    private bool Validate(Model request, out List<ValidationResult> validationResults)
    {
        validationResults = new();

        return Validator.TryValidateObject(request, new ValidationContext(request, null, null), validationResults);
    }

    // next: R&D add polly
    // next: R&D add transaction
    private void Handler(Model model)
    {
        var excluded = new HashSet<DateTime>();
        var query = new QueryExpression("msdyn_timeentry")
        {
            ColumnSet = new ColumnSet("msdyn_start", "msdyn_end"),
            Criteria = new FilterExpression()
        };

        var results = _organizationService.RetrieveMultiple(query);

        foreach (var entity in results.Entities)
            excluded.Add(entity.GetAttributeValue<DateTime>("msdyn_start"));

        var batch = new ExecuteMultipleRequest()
        {
            Settings = new ExecuteMultipleSettings()
            {
                ContinueOnError = false
            },
            Requests = new OrganizationRequestCollection()
        };

        // The logic should create an msdyn_timeentry record for every date in the date range from StartOn to EndOn.
        // The logic should also ensure that there are no duplicate time entry records created per date.
        // fixme: i not exactly understand witch field must be checked, now check "msdyn_start"
        var days = model.EndOn - model.StartOn;
        for (var i = 0; i <= days.TotalDays; i++)
        {
            var date = model.StartOn.AddDays(i).Date;

            // skip existing
            if (excluded.Contains(date))
                continue;

            var createRequest = new CreateRequest
            {
                Target = new Entity("msdyn_timeentry")
                {
                    Attributes = new AttributeCollection
                    {
                        { "msdyn_start", date },
                        { "msdyn_end", date.AddDays(1) }
                    }
                }
            };

            batch.Requests.Add(createRequest);
        }

        _organizationService.Execute(batch);
    }

    public record Model(DateTime StartOn, DateTime EndOn) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndOn.Date < StartOn.Date)
                yield return new ValidationResult("some wrong with dates, start < end",
                    new[] { nameof(StartOn), nameof(EndOn) });
        }
    }
}