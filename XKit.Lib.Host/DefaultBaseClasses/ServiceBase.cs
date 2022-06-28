using System.Collections.Generic;
using XKit.Lib.Common.Host;
using XKit.Lib.Common.Registration;
using XKit.Lib.Common.Log;
using XKit.Lib.Common.Fabric;
using XKit.Lib.Common.Config;
using XKit.Lib.Host.Config;
using XKit.Lib.Common.Services;
using System;
using XKit.Lib.Common.Utility;
using System.Collections.Concurrent;
using System.Reflection;
using XKit.Lib.Common.Utility.Invocation;
using System.Linq;
using XKit.Lib.Common.Utility.Extensions;
using System.Threading.Tasks;
using XKit.Lib.Common.Services.MessageBroker;
using XKit.Lib.Common.Client;
using System.Linq.Expressions;

namespace XKit.Lib.Host.DefaultBaseClasses {
    
    public abstract class ServiceBase<TOperation> 
        : IServiceOperationOwner
        where TOperation : IServiceOperation {

        protected class CallInvokerInfo { 
            public Func<object, object[], object> MethodInvoker { get; }
            public Type RequestType { get; }
            public Type DeclaringType { get; }
            public CallInvokerInfo(
                Func<object, object[], object> methodInvoker,
                Type requestType,
                Type declaringType
            ) {
                this.MethodInvoker = methodInvoker;
                this.RequestType = requestType;
                this.DeclaringType = declaringType;
            }
        }

        // =====================================================================
        // const, static and readonly
        // =====================================================================
        protected const int DefaultTimeoutMs = 10000;
        protected static readonly IEnumerable<IReadOnlyDescriptor> DefaultDependencies = Array.Empty<IReadOnlyDescriptor>();
        protected static readonly IEnumerable<IReadOnlySubscription> NoEventSubscriptions = Array.Empty<Subscription>();
        protected static readonly IEnumerable<IReadOnlySubscription> NoCommandsSubscriptions = Array.Empty<Subscription>();
        protected static readonly IReadOnlyServiceCallPolicy DefaultPolicy = new ServiceCallPolicy {
            CallPattern = ServiceCallPatternEnum.FirstChance
        };

        // =====================================================================
        // private
        // =====================================================================

        private readonly HashSet<string> features = new();
        private readonly List<IServiceDaemon> daemons = new();
        private readonly ConcurrentDictionary<string, CallInvokerInfo> invokerCache = new();
        private Func<ServiceOperationContext, object> callOperationConstructor = null;
        private readonly Dictionary<string, MethodInfo> callMethodInfoCache = new();
        private readonly List<Subscription> eventSubscriptions;
        private readonly List<Subscription> commandSubscriptions;

        // =====================================================================
        // protected
        // =====================================================================
                
        protected ILocalEnvironment LocalEnvironment { get; }
        protected IHostEnvironment HostEnvironment => LocalEnvironment.HostEnvironment;

        protected IDependencyConnector DependencyConnector => LocalEnvironment.DependencyConnector;

        protected RunStateEnum RunState { get; private set; }  = RunStateEnum.Inactive; 

        protected void SetRunStateActive() 
            => RunState = RunStateEnum.Active;

        protected void SetRunStatePaused() 
            => RunState = RunStateEnum.Paused;

        protected void SetRunStateBeginShutdown() 
            => RunState = RunStateEnum.ShuttingDown;

        protected void SetRunStateFinishShutdown() 
            => RunState = RunStateEnum.Inactive;


        protected void AddDaemon<TMessage>(
            IServiceDaemon<TMessage> daemon
        ) where TMessage : class {
            this.daemons.Add(daemon);
            this.OnServiceStartingEvent += () => daemon.Start();
            this.OnServiceStoppingEvent += () => daemon.Stop();
            this.OnServicePauseEvent += () => daemon.Pause();
            this.OnServiceResumeEvent += () => daemon.Resume();
            this.OnEnvironmentChangeEvent += () => daemon.SignalEnvironmentChange();
            daemon.AddToService(this);
        }

        protected CallInvokerInfo GetCallInvoker(string name) {

            return invokerCache.GetOrAdd(
                name, 
                (k) => {
                    var method = GetServiceCallTargetMethod(k);
                    if (method == null) {
                        return null;
                    }
                    return new CallInvokerInfo(
                        MethodInvokerFactory.ForMethod(method),
                        method.GetParameters().FirstOrDefault()?.ParameterType,
                        method.DeclaringType
                    );
                }
            );
        }

        // =====================================================================
        // abstract and virtual members
        // =====================================================================
        protected abstract IReadOnlyDescriptor Descriptor { get; }
        protected virtual IEnumerable<IReadOnlyDescriptor> Dependencies => DefaultDependencies;
        protected virtual IEnumerable<IReadOnlySubscription> EventSubscriptions => eventSubscriptions;
        protected virtual IEnumerable<IReadOnlySubscription> CommandsSubscriptions => commandSubscriptions;
        protected virtual IReadOnlyServiceCallPolicy CallPolicy => DefaultPolicy;
        protected virtual string ConfigurationDocumentIdentifier => Identifiers.GetServiceVersionLevelKey(Descriptor);

        protected virtual IServiceOperation CreateOperation(
            ServiceOperationContext context
        ) {

            if (callOperationConstructor == null) {
                
                var constructorWithOperationInvoker = typeof(TOperation).GetConstructor(new[] { 
                    typeof(ServiceOperationContext)
                });
                if (constructorWithOperationInvoker == null) {
                    throw new Exception("Cannot instantiate service api operation");
                }
                var constructorInvoker = MethodInvokerFactory.ForConstructor(constructorWithOperationInvoker);
                this.callOperationConstructor = 
                    (ServiceOperationContext c) => constructorInvoker(new object[] { c });
            }

            return (TOperation)callOperationConstructor(context);
        }
        
        protected virtual MethodInfo GetServiceCallTargetMethod(string methodName) {

            callMethodInfoCache.TryGetValue(methodName, out var method);
            return method;
        }
        
        protected abstract bool CanStartNewOperation();

        protected virtual async Task<ServiceCallResult> ExecuteCall(
            ServiceCallRequest request
        ) {
            
            var operationContext = new ServiceOperationContext(
                service: this,
                localEnv: this.LocalEnvironment,
                callTypeParameters: request.CallTypeParameters,
                requestorInstanceId: request.RequestorInstanceId,
                requestorFabricId: request.RequestorFabricId,
                correlationId: request.CorrelationId
            );

            var target = CreateOperation(
                operationContext
            );

            var invokerInfo = GetCallInvoker(request.OperationName);
            if (invokerInfo == null) {
                return CreateResult(
                    request,
                    operationContext.OperationId,
                    "Operation requested is not present in the operation interface",
                    ServiceCallStatusEnum.OperationNotFound
                );
            }

            Task operationTask = null;
            ServiceCallResult result = null;
            Exception operationException = null;
            try {
                if (invokerInfo.RequestType != null) {
                    operationTask = (Task)invokerInfo.MethodInvoker(target, new[] { request.GetBody(invokerInfo.RequestType) });
                } else {
                    operationTask = (Task)invokerInfo.MethodInvoker(target, Array.Empty<object>());
                }
                if (!operationTask.IsCompleted) {
                    await operationTask; //.ConfigureAwait(false);
                }

                // HACK:  I wish there was a better way to do this.  But using a dynamic, while
                //        not as fast as a direct call, is still better than reflection. 
                //        In order to properly access Result, we would otherwise have to jump through
                //        hoops.  So this will work for now.  This can be improved in the future.
                if (operationTask != null) {
                    result = (ServiceCallResult)((dynamic) operationTask).Result;
                } else { 
                    result = CreateResult(
                        request,
                        operationContext.OperationId,
                        "Operation async task was null",
                        ServiceCallStatusEnum.UndefinedOperationMethod
                    );
                }
            } catch(Exception ex) { 
                operationException = ex; 
            } finally {
                operationTask?.Dispose();
            }
            
            if (result == null) {
                result = CreateResult(
                    request,
                    operationContext.OperationId,
                    operationTask.Exception?.Message ?? "Operation returned no result",
                    ServiceCallStatusEnum.OperationReturnedNoResult
                );
            } 

            return result;
        }

        protected virtual void StartService() {  
            OnServiceStartingEvent?.Invoke();
            SetRunStateActive();
        }
        protected virtual void StopService() {
            SetRunStateBeginShutdown();
            OnServiceStoppingEvent?.Invoke();
            SetRunStateFinishShutdown();
        }

        protected virtual void PauseService() {
            SetRunStatePaused();
            OnServicePauseEvent?.Invoke();
        }

        protected virtual void ResumeService() {
            OnServiceResumeEvent?.Invoke();
            SetRunStateActive();
        }

        protected virtual void SignalEnvironmentChange() {
            OnEnvironmentBeforeChangeEvent?.Invoke();
            OnEnvironmentChangeEvent?.Invoke(); 
        }

        protected virtual IReadOnlyList<MethodInfo> GetServiceCallMethodsInfo() {

            var serviceCallableInterface = typeof(IServiceCallable);
            var apiInterfaces = 
                (from interfaceFromConcreteClass in typeof(TOperation).GetInterfaces()
                //where interfaceFromConcreteClass .GetInterfaces().Any(i => i == serviceApiInterface)
                where serviceCallableInterface.IsAssignableFrom(interfaceFromConcreteClass)
                select interfaceFromConcreteClass).Distinct();

            var taskType = typeof(Task);
            var resultType = typeof(ServiceCallResult);
            var publicMethodsOnConcreteType =
                from method in typeof(TOperation).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where 
                    taskType.IsAssignableFrom(method.ReturnType) && 
                    method.ReturnType.IsGenericType &&
                    !method.IsConstructor
                let methodReturnTypeInner = method.ReturnType.GetGenericArguments().FirstOrDefault()
                where 
                    methodReturnTypeInner != null && 
                    resultType.IsAssignableFrom(methodReturnTypeInner)
                let parameters = method.GetParameters()
                where 
                    parameters.Length <= 1
                select method;
            
            return 
                apiInterfaces.SelectMany(i => i.GetMethods())
                .Union(publicMethodsOnConcreteType)
                .ToArray();
        }


        // =====================================================================
        // events
        // =====================================================================

        protected event OnServiceStartDelegate OnServiceStartingEvent;
        protected event OnServiceStopDelegate OnServiceStoppingEvent;
        protected event OnEnvironmentChangeDelegate OnEnvironmentBeforeChangeEvent;
        protected event OnEnvironmentChangeDelegate OnEnvironmentChangeEvent;
        protected event OnServicePauseDelegate OnServicePauseEvent;
        protected event OnServiceResumeDelegate OnServiceResumeEvent;
        protected event OnHostStartCompleteDelegate OnHostStartCompleteEvent;

        // =====================================================================
        // Construction
        // =====================================================================

        public ServiceBase(
            ILocalEnvironment localEnvironment
        ) {
            LocalEnvironment = localEnvironment ?? throw new ArgumentNullException(nameof(localEnvironment));
            if (localEnvironment?.HostEnvironment == null) { 
                throw new Exception(
                    message: "Local environment is not a host!"
                ); 
            }
            var serviceCallMethods = GetServiceCallMethodsInfo();
            serviceCallMethods.ForEach(m => {
                // FUTURE:  In the future we might perhaps enforce inheritance for operation name.  That is,
                //          verify it which "type" it is based on which interface it inherits from 
                if (m.DeclaringType.IsInterface) {
                    this.callMethodInfoCache[$"{m.DeclaringType.Name}.{m.Name}"] = m;
                } else {
                    this.callMethodInfoCache[m.Name] = m;
                }
            });
            if (this.EventSubscriptions == null) {
                this.eventSubscriptions = new List<Subscription>();
            }
            if (this.CommandsSubscriptions == null) {
                this.commandSubscriptions = new List<Subscription>();
            }
        }

        // =====================================================================
        // IServiceBase
        // =====================================================================

        IEnumerable<IReadOnlyDescriptor> IServiceBase.Dependencies => Dependencies;
        IEnumerable<IReadOnlySubscription> IServiceBase.EventSubscriptions => EventSubscriptions;
        IEnumerable<IReadOnlySubscription> IServiceBase.CommandSubscriptions => CommandsSubscriptions;
        IReadOnlyServiceCallPolicy IServiceBase.CallPolicy => CallPolicy;        
        IHostEnvironment IServiceBase.HostEnvironment => HostEnvironment;
        ILocalEnvironment IServiceBase.LocalEnvironment => LocalEnvironment;

        void IServiceBase.SignalEnvironmentChange()
            => SignalEnvironmentChange();        
        protected void SignalHostStartupComplete() {
            OnHostStartCompleteEvent?.Invoke();
        }
        protected void SignalHostShutdownComplete() {
            OnHostStartCompleteEvent?.Invoke();
        }
        
        void IServiceBase.StartService() 
            => StartService();

        void IServiceBase.StopService()
            => StopService();
        
        void IServiceBase.SignalHostStartupComplete() 
            => SignalHostStartupComplete();

        void IServiceBase.SignalHostShutdownComplete() 
            => SignalHostShutdownComplete();

        bool IServiceBase.HasFeature(string featureName) 
            => features.Contains(featureName);

        string IServiceBase.ConfigurationDocumentIdentifier => ConfigurationDocumentIdentifier;

        bool IServiceBase.CanStartNewOperation() => this.CanStartNewOperation();

        ServiceInstanceStatus IServiceBase.GetServiceStatus() {
            return new ServiceInstanceStatus {
                Health = HostEnvironment.GetHealth(),
                InstanceId = this.InstanceId,
                RunState = HostEnvironment.HostRunState,
                Availability = AvailabilityEnum.Serving5
            };
        }

        IReadOnlyDescriptor IServiceBase.Descriptor => this.Descriptor;

        RunStateEnum IServiceBase.ServiceState => RunState;

        string IServiceBase.InstanceId => this.InstanceId;

        Task<ServiceCallResult> IServiceBase.ExecuteCall(
            ServiceCallRequest request
        ) => ExecuteCall(request);

        IServiceDaemon IServiceBase.GetDaemon(
            string daemonName
        ) => daemons.Where(d => d.Name == daemonName).FirstOrDefault();

        IServiceDaemon[] IServiceBase.GetDaemons() => daemons.ToArray();

        // =====================================================================
        // Features
        // =====================================================================

        /// <summary>
        /// Called by the implementing / derived class to add configurability and obtain 
        /// a config reader.
        /// </summary>
        /// <typeparam name="TConfigType"></typeparam>
        protected IConfigReader<TConfigType> AddFeatureConfigurable<TConfigType>() where TConfigType : class, new() {
            var configReader = ConfigReaderFactory.CreateForService<TConfigType>(Descriptor, HostEnvironment.LocalConfigSessionFactory);
            this.OnEnvironmentBeforeChangeEvent += () => configReader.InvalidateCache();
            AddFeatureFlag(StandardServiceFeatures.Configurable);
            return configReader;
        }

        // =====================================================================
        // Other protected
        // =====================================================================

        protected string InstanceId { get; } = Common.Utility.Identifiers.GenerateIdentifier();

        protected void AddFeatureFlag(string featureName) {
            this.features.Add(featureName);
        }

        protected Subscription AddEventSubscription<TCallInterface>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> method,
            ServiceCallPolicy policy = null,
            ServiceClientErrorHandling? errorHandling = null,
            int? maxDeliveryRetries = null,
            int? maxConsecutiveFailures = null,
            int[] failureDelaysToRetryMs = null
        ) where TCallInterface : IServiceEvents 
            => AddEventSubscription<TCallInterface>(
                ((MethodCallExpression)method.Body).Method.Name,
                policy,
                errorHandling,
                maxDeliveryRetries,
                maxConsecutiveFailures,
                failureDelaysToRetryMs
            );

        protected Subscription AddEventSubscription<TCallInterface>(
            string method,
            ServiceCallPolicy policy = null,
            ServiceClientErrorHandling? errorHandling = null,
            int? maxDeliveryRetries = null,
            int? maxConsecutiveFailures = null,
            int[] failureDelaysToRetryMs = null
        ) where TCallInterface : IServiceEvents {
            if (this.eventSubscriptions == null) {
                throw new Exception("Event subscriptions are being handled by a derived class");
            }
            var subscription = new Subscription {
                Recipient = this.Descriptor.Clone(),
                Policy = policy,
                RecipientHostId = 
                    policy?.CallPattern.GetValueOrDefault() == ServiceCallPatternEnum.SpecificHost ? 
                        LocalEnvironment.FabricId :
                        null,
                MessageTypeName = $"{typeof(TCallInterface).Name}.{method}",
                ErrorHandling = errorHandling,
                MaxDeliveryRetries = maxDeliveryRetries,
                MaxConsecutiveFailures = maxConsecutiveFailures,
                FailureDelaysToRetryMs = failureDelaysToRetryMs
            };
            this.eventSubscriptions.Add(subscription);
            return subscription;
        }

         protected Subscription AddCommandSubscription<TCallInterface>(
            Expression<Func<TCallInterface, Task<ServiceCallResult>>> method,
            ServiceCallPolicy policy = null,
            ServiceClientErrorHandling? errorHandling = null,
            int? maxDeliveryRetries = null,
            int? maxConsecutiveFailures = null,
            int[] failureDelaysToRetryMs = null
        ) where TCallInterface : IServiceCommands 
            => AddCommandSubscription<TCallInterface>(
                ((MethodCallExpression)method.Body).Method.Name,
                policy,
                errorHandling,
                maxDeliveryRetries,
                maxConsecutiveFailures,
                failureDelaysToRetryMs
            );

        protected Subscription AddCommandSubscription<TCallInterface>(
            string method,
            ServiceCallPolicy policy = null,
            ServiceClientErrorHandling? errorHandling = null,
            int? maxDeliveryRetries = null,
            int? maxConsecutiveFailures = null,
            int[] failureDelaysToRetryMs = null
        ) where TCallInterface : IServiceCommands {
            if (this.commandSubscriptions == null) {
                throw new Exception("Command subscriptions are being handled by a derived class");
            }
            var subscription = new Subscription {
                Recipient = this.Descriptor.Clone(),
                Policy = policy,
                RecipientHostId = 
                    policy?.CallPattern.GetValueOrDefault() == ServiceCallPatternEnum.SpecificHost ? 
                        this.LocalEnvironment.FabricId :
                        null,
                MessageTypeName = $"{typeof(TCallInterface).Name}.{method}",
                ErrorHandling = errorHandling,
                MaxDeliveryRetries = maxDeliveryRetries,
                MaxConsecutiveFailures = maxConsecutiveFailures,
                FailureDelaysToRetryMs = failureDelaysToRetryMs
            };
            this.commandSubscriptions.Add(subscription);
            return subscription;
        }

        // =====================================================================
        // private utility
        // =====================================================================

        ServiceCallResult CreateResult(
            ServiceCallRequest request,
            Guid operationId,
            string message,
            ServiceCallStatusEnum callStatus
        ) => new ServiceCallResult {
                ServiceCallStatus = callStatus,
                Service = this.Descriptor.Clone(),
                OperationId = operationId,
                Timestamp = DateTime.UtcNow,
                ResponderFabricId = LocalEnvironment.FabricId,
                ResponderInstanceId = this.InstanceId,
                Message = message,
                OperationStatus = LogResultStatusEnum.Fault,
                OperationName = request.OperationName,
                CorrelationId = request.CorrelationId,
                RequestorFabricId = request.RequestorFabricId,
                RequestorInstanceId = request.RequestorInstanceId,
                ServiceStatus = new ServiceInstanceStatus {
                    Health = HostEnvironment.GetHealth(),
                    InstanceId = this.InstanceId,
                    RunState = HostEnvironment.HostRunState,
                    Availability = AvailabilityEnum.Serving5
                }
        };

    }
}
