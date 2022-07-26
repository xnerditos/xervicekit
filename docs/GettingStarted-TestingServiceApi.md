# Testing a Service API

Having appropriate tests is an important part of development.  Testing is first class citizen in XerviceKit.  That is, XerviceKit was designed with testing in mind from the start. 

#### Create the test project

```
:~/$ mkdir ServiceName.Tests
:~/$ cd ServiceName.Tests
:~/ServiceName.Tests$ dotnet new mstest
```

Edit the csproj file and add XerviceKit references, as well as a references to our other projects:

```
  <ItemGroup>
    <PackageReference Include="XKit.Lib.Testing" Version="*"/>
    <ProjectReference Include="../ServiceName.Common/ServiceName.Common.csproj"/>
    <ProjectReference Include="../ServiceName.Client/ServiceName.Client.csproj"/>
    <ProjectReference Include="../ServiceName/ServiceName.csproj"/>
  </ItemGroup>
```

#### Add the test class

We recommend a convention of using one test class per service API method, and including all relevant tests in it.  

We also recommend naming the class by the name of the method under test.  Create the file `Method1.cs` in the test project (You can get rid of the file that `dotnet new` put there by default.)  

Make your file look something like this.  Obviously you have to use the name of your Operation class for `ApiOperation`: 

```
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XKit.Lib.Testing;

namespace SomeService.Test;

[TestClass]
public class GetUserTests
{
    private static readonly TestHostHelper testHelper = new();

    [ClassInitialize]
    public static void ClassInit(TestContext _) {
        testHelper.InitializeLocalTestHost();
        testHelper.AddCreateService(
            Constants.ServiceDescriptor,
            typeof(ApiOperation)
        );
        testHelper.StartHost();
    }

    [ClassCleanup]
    public static void ClassTeardown() {
        testHelper.DestroyHost();
    }
}
```

The `ClassInit()` and `ClassTeardown()` set up the testing environment so that you can make service calls from tests and the code will run.  Notice that they use a test helper that provides functionality to reduce friction when writing tests. 

#### Additional setup

Depending on what the test is doing, you might need additional setup.  You can add any setup that needs to be done just once to the `ClassInit()` method.  

If you require some setup and teardown that should be run along with _each test_, then add the following methods and fill in your needed setup. 

```
[TestInitialize]
public void TestInit() {
    // TODO:
}

[TestCleanup]
public void TestTeardown() {
    // TODO:
}
```

#### Add test methods

Generally best practice is to have a test try to verify one case, rather than a test trying to do verify many things at once.  That way, if a test fails, it is much easier to understand the root cause. 

Add a test method to the test class you added above:

```
[TestMethod]
public async Task TestAThing()
{
    // --- arrange:  Set things up for the test

    var client = new Client(testHelper.HostEnvironment.Connector);

    // --- act:  Call the method in question
    var result = await client.Method1(new Method1Request { /* TODO: Fill in the request */ }); 

    // --- assert:  Check for condition that indicate success or failure

    Assert.IsFalse(result.HasError);

    // TODO:  More assertions ...
}
```

The example uses the AAA pattern for tests:  Arrange, Act, Assert.

## What next?

As you can see adding tests is really easy.  However, if you are writing unit tests and your service happens to call other services, how do you handle that?  Ideally, you want to test each service independently.  Take a look at the [guide to testing services with dependencies](./GettingStarted-TestingServiceDependencies.md) in that case. 
