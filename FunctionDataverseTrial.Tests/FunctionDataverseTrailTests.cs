using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace FunctionDataverseTrial.Tests;

public class FunctionDataverseTrailTests
{
    public static IEnumerable GetSimpleTestData()
    {
        yield return new TestCaseData(new DateTime(2022, 01, 04), new DateTime(2023, 03, 01));
    }

    public static IEnumerable GetSimpleWithCountTestData()
    {
        yield return new TestCaseData(new DateTime(2022, 01, 04), new DateTime(2022, 01, 04), 1);
        yield return new TestCaseData(new DateTime(2022, 01, 04), new DateTime(2022, 01, 05), 2);
        yield return new TestCaseData(new DateTime(2022, 01, 01), new DateTime(2022, 01, 10), 10);
    }

    [Test, TestCaseSource(nameof(GetSimpleTestData))]
    public void ShouldThrowLogicException(DateTime from, DateTime to)
    {
        // arrange
        var mock = new Mock<IOrganizationService>();
        mock.Setup(e => e.RetrieveMultiple(It.IsAny<QueryBase>()))
            .Returns(() => new EntityCollection(new List<Entity>()));
        var sc = new ServiceCollection()
            .AddLogging()
            .AddTransient<FunctionDataverseTrail>()
            .AddTransient(_ => mock.Object)
            .BuildServiceProvider();

        var request = new FunctionDataverseTrail.Model(from, to);

        // act
        sc.GetRequiredService<FunctionDataverseTrail>().Run(request);

        // assert
        mock.Verify(f => f.Execute(It.IsAny<ExecuteMultipleRequest>()), Times.Once);
    }

    [Test, TestCaseSource(nameof(GetSimpleWithCountTestData))]
    public void ShouldAddExactlyCountItems(DateTime from, DateTime to, int count)
    {
        // arrange
        var mock = new Mock<IOrganizationService>();
        mock.Setup(e => e.RetrieveMultiple(It.IsAny<QueryBase>()))
            .Returns(() => new EntityCollection(new List<Entity>()));

        var sc = new ServiceCollection()
            .AddLogging()
            .AddTransient<FunctionDataverseTrail>()
            .AddTransient(_ => mock.Object)
            .BuildServiceProvider();

        var request = new FunctionDataverseTrail.Model(from, to);

        // act
        sc.GetRequiredService<FunctionDataverseTrail>().Run(request);

        // assert
        mock.Verify(f => f.Execute(It.Is<ExecuteMultipleRequest>(p => p.Requests.Count == count)), Times.Once);
    }

    [Test]
    public void ShouldNotAddDuplicates()
    {
        // arrange
        var requests = new List<OrganizationRequest>();
        var convert = requests
            .OfType<ExecuteMultipleRequest>()
            .SelectMany(s => s.Requests)
            .OfType<CreateRequest>()
            .Select(s => s.Target);

        var mock = new Mock<IOrganizationService>();
        mock.Setup(e => e.Execute(It.IsAny<ExecuteMultipleRequest>()))
            .Callback<OrganizationRequest>(request => requests.Add(request));
        mock.Setup(e => e.RetrieveMultiple(It.IsAny<QueryBase>()))
            .Returns(() => new EntityCollection(convert.ToList()));

        var sc = new ServiceCollection()
            .AddLogging()
            .AddTransient<FunctionDataverseTrail>()
            .AddTransient(_ => mock.Object)
            .BuildServiceProvider();
        var func = sc.GetRequiredService<FunctionDataverseTrail>();

        // act
        func.Run(new FunctionDataverseTrail.Model(new DateTime(2022, 01, 01), new DateTime(2022, 01, 10)));
        func.Run(new FunctionDataverseTrail.Model(new DateTime(2022, 01, 5), new DateTime(2022, 01, 15)));

        // assert
        Assert.AreEqual(15, convert.Count());
    }
}