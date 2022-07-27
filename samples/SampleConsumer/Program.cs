using XKit.Lib.Consumer;
using Samples.SampleService.V1.Client;
using Samples.SampleService.V1;
using XKit.Lib.Common.Fabric;
using Samples.SampleService.V1.ServiceApiEntities;

var helper = new ConsumerHelper();

helper.CreateInitConsumer(
    dependencies: new[] { Constants.ServiceDescriptor }
);

ISampleServiceClient sampleClient = new SampleServiceClient(
    helper.Log,
    helper.Connector,
    ServiceCallTypeParameters.SyncResult()
);

var result = await sampleClient.SampleCall(new SampleRequest());

Console.WriteLine(result.HasError ? "Failed" : "Succeeded");

