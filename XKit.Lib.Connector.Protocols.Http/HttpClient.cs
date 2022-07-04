using System.Threading.Tasks;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Fabric;
using RestSharp;
using System;
using XKit.Lib.Common.Log;
using System.Threading;

namespace XKit.Lib.Connector.Protocols.Http {

    internal class HttpClient : IInstanceClient {

        private readonly IRestClient client;
        private readonly ServiceInstance instanceInfo;
        private readonly SemaphoreSlim synchronizer = new(1, 1);

        public HttpClient(
            IReadOnlyServiceInstance instanceInfo,
            IRestClient client
        ) {
            this.instanceInfo = instanceInfo.Clone();
            this.client = client;

            #pragma warning disable CS0618
            // TODO: Upgrade to non-obsolete pattern
            client.AddHandler("application/json", JsonSerializer.Default);
            client.AddHandler("text/json", JsonSerializer.Default);
            client.AddHandler("text/x-json", JsonSerializer.Default);
            client.AddHandler("text/javascript", JsonSerializer.Default);
            client.AddHandler("*+json", JsonSerializer.Default);
            #pragma warning restore CS0618
        }

        // =====================================================================
        // IInstanceClient
        // =====================================================================

        IReadOnlyServiceInstance IInstanceClient.Instance => this.instanceInfo;

        async Task<ServiceCallResult> IInstanceClient.ExecuteCall(
            ServiceCallRequest request
        ) {
            await synchronizer.WaitAsync();
            try {
                IRestRequest restRequest = new RestRequest(GetOperationUrl());
                restRequest.JsonSerializer = JsonSerializer.Default;
                restRequest.AddJsonBody(request.Clone());
                restRequest.Timeout = 1000 * 60 * 5;

                var tcs = new TaskCompletionSource<ServiceCallResult>();
                client.PostAsync<ServiceCallResult>(restRequest, (restResponse, handle) => {
                    var myResult = CreateBlankResultFromRequest(request);
                    try { 
                        var result = restResponse.Data;
                        if (result == null) {
                            myResult.ServiceCallStatus = ServiceCallStatusEnum.ProtocolError;
                            myResult.Message = $"HTTP error.  Message: {restResponse.StatusDescription ?? restResponse.ErrorMessage}.  Code: {restResponse.StatusCode}";
                        } else {
                            UpdateInstance(result);
                            myResult.ServiceCallStatus = ServiceCallStatusEnum.Completed;
                            myResult.OperationStatus = result.OperationStatus;
                            myResult.Service = result.Service;
                            myResult.Code = result.Code;
                            myResult.Payload = result.Payload;
                            myResult.ResponderFabricId = result.ResponderFabricId;
                            myResult.RequestorInstanceId = result.ResponderInstanceId;
                            myResult.Message = result.Message;
                            myResult.OperationId = result.OperationId;
                            myResult.Timestamp = result.Timestamp;
                            myResult.ServiceStatus = result.ServiceStatus;
                        }
                        
                    } catch(Exception ex) { 
                        instanceInfo.Status.TryUpdate(AvailabilityEnum.UnavailableRetryLater, null);
                        myResult.ServiceCallStatus = ServiceCallStatusEnum.UnknownError;
                        myResult.Message = ex.Message;
                    }
                    tcs.SetResult(myResult);
                });

                return await tcs.Task;
            } finally {
                synchronizer.Release();
            }
        }

        // =====================================================================
        // Private utility
        // =====================================================================
        
        private string GetOperationUrl() {
            return "/" + 
                (this.instanceInfo.Descriptor.IsMetaService ? "meta" : "managed") + 
                "/" + this.instanceInfo.Descriptor.Collection +
                "/" + this.instanceInfo.Descriptor.Name +
                "/" + this.instanceInfo.Descriptor.Version;
        }

        private ServiceCallResult UpdateInstance(
            ServiceCallResult resp
        ) {
            if (resp.ServiceStatus != null) {
                instanceInfo.Status.TryUpdate(resp.ServiceStatus);
            } else {
                switch(resp.OperationStatus) {
                    case LogResultStatusEnum.NoAction_ServiceUnavailable:
                    case LogResultStatusEnum.NoAction_Timeout:
                        instanceInfo.Status.TryUpdate(AvailabilityEnum.UnavailableRetryLater, null);
                        break;
                    case LogResultStatusEnum.Success:
                    case LogResultStatusEnum.PartialSuccess:
                        this.instanceInfo.Status.TryUpdate(AvailabilityEnum.Serving5, null);
                        break;
                }
            }

            return resp;
        }

        ServiceCallResult CreateBlankResultFromRequest(
            ServiceCallRequest request
        ) {
            return new ServiceCallResult {
                CorrelationId = request.CorrelationId,
                OperationName = request.OperationName,
                RequestorFabricId = request.RequestorFabricId,
                RequestorInstanceId = request.RequestorInstanceId
            };
        }
    }
}
