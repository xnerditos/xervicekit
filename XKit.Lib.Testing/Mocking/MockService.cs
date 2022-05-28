using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Host.DefaultBaseClasses;

namespace XKit.Lib.Testing.Mocking {

    public class MockService<TCallInterface> : 
        ManagedService<MockOperation<TCallInterface>>, IMockService<TCallInterface>
        where TCallInterface : class, IServiceCallable {
        
        private readonly IReadOnlyDescriptor descriptor;
        private readonly Mock<TCallInterface> apiMock;

        public MockService(
            IReadOnlyDescriptor descriptor,
            Mock<TCallInterface> apiMock,
            ILocalEnvironment localEnvironment = null
        ) : base(
            localEnvironment
            ) { 
                this.descriptor = descriptor;
                this.apiMock = apiMock;
            }

        public MockService(
            IReadOnlyDescriptor descriptor,
            MockBehavior mockBehavior = MockBehavior.Loose,
            ILocalEnvironment localEnvironment = null 
        ) : this(
            descriptor,
            new Mock<TCallInterface>(mockBehavior),
            localEnvironment
            ) { }

        public Mock<TCallInterface> ApiMock => this.apiMock;

        protected override IReadOnlyDescriptor Descriptor => descriptor;

        protected override IServiceOperation CreateOperation(ServiceOperationContext context) 
            => new MockOperation<TCallInterface>(context, apiMock); 

        protected override IReadOnlyList<MethodInfo> GetServiceCallMethodsInfo() {
            return typeof(TCallInterface).GetMethods();
        }

        protected override async Task<ServiceCallResult> ExecuteCall(ServiceCallRequest request) {

            var method = GetServiceCallTargetMethod(request.OperationName);
            if (method != null) {
                if (method.DeclaringType == typeof(TCallInterface)) {
                    Task task;
                    var requestType = method.GetParameters().FirstOrDefault()?.ParameterType;
                    if (requestType != null) {
                        task = (Task)method.Invoke(ApiMock.Object, new object[] { request.GetBody(requestType) });
                    } else {
                        task = (Task)method.Invoke(ApiMock.Object, new object[0]);
                    }
                    if (!task.IsCompleted) {
                        await task.ConfigureAwait(false);
                    }
                    if (task == null) {
                        throw new Exception("Operation returned null task!");
                    }
                    return (ServiceCallResult)((dynamic) task).Result;
                }
            }

            throw new Exception("Method not found");
        }
    }
}
