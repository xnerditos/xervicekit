using System.Threading.Tasks;
using XKit.Lib.Common.Fabric;
using System;
using Samples.SampleService.V1.ServiceApiEntities;
using System.Linq;

namespace Samples.SampleService.V1;

public partial class SampleServiceOperation {

    // NOTE:  This just describes how the service call will run.  The real work
    //        happens in the methods below.
    async Task<ServiceCallResult<SampleResponse>> ISampleServiceApi.SampleCall(
    	SampleRequest request
    ) => await RunServiceCall(
    		requestBody: request,
    		requestValidationAction: ValidateSampleCall,
    		//preCallAction: (req) => PreOperationAction(),
    		//postCallAction: (res) => PostOperationAction(),
    		operationAction: DoSampleCall
    	);

    private bool ValidateSampleCall(SampleRequest request) {
        // NOTE:  Do some validation here
        return request.SomeValue > 0 && request.SomeCollection != null;
    }

    private async Task<SampleResponse> DoSampleCall(SampleRequest request) {
        // NOTE:  Do the operation main logic here

        Log.Info("some log message", code: LogCodes.SampleCall1ComposingResponse);
        
        // Fire off a sample event
        await EventMessenger.RaiseEvent(
            x => x.SampleEvent(null),
            new SampleEventMessage {
                SomeEventData = "Test"
            }
        );

        var newCollection = new System.Collections.Generic.List<string>();
        // NOTE:  We don't have to worry about SomeCollection being null here because
        //        the validation above ensures it will not be null
        newCollection.AddRange(request.SomeCollection);
        newCollection.Add("Some new string");

        return new() {
            AFutureDate = DateTime.Today.AddDays(30),
            RandomValue = Random.Shared.Next().ToString(),
            SomeValue = request.SomeValue,
            SomeCollection = newCollection
                .Select(s => new SampleEntity { Something = s })
                .ToArray()
        };
    }
}
