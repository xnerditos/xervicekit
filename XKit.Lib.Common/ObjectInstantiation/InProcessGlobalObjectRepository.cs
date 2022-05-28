using System;
using System.Collections.Generic;

namespace XKit.Lib.Common.ObjectInstantiation {
    internal class InProcessGlobalObjectRepository : IInProcessGlobalObjectRepository {
        
        private Dictionary<Type, Func<object>> factories = new Dictionary<Type, Func<object>>();
        
        public InProcessGlobalObjectRepository() {}

        // =====================================================================
        // IInjectableGlobalObjectRepository
        // =====================================================================
        
        TRegisteredInterface IInProcessGlobalObjectRepository.GetObject<TRegisteredInterface>() {
            Func<object> factory;
            if (!factories.TryGetValue(typeof(TRegisteredInterface), out factory)) {
                throw new Exception("Type not registered " + typeof(TRegisteredInterface).Name);
            }
            return (TRegisteredInterface) factory?.Invoke();
        }

        object IInProcessGlobalObjectRepository.GetObject(Type interfaceType) {
            Func<object> factory;
            if (!factories.TryGetValue(interfaceType, out factory)) {
                throw new Exception("Type not registered " + interfaceType.Name);
            }
            return factory?.Invoke();
        }

        bool IInProcessGlobalObjectRepository.HasObject(Type interfaceType) 
            => factories.ContainsKey(interfaceType);

        void IInProcessGlobalObjectRepository.RegisterFactory<TConcreteType>(Func<TConcreteType> createMethod, params Type[] forTypes) {            
            ValidateType(typeof(TConcreteType), forTypes);

            Func<object> wrapperFactory = () => createMethod();

            foreach(var t in forTypes) {
                factories[t] = wrapperFactory;
            }
        }

        void IInProcessGlobalObjectRepository.RegisterSingleton<TConcreteType>(TConcreteType obj, params Type[] forTypes) {

            ValidateType(typeof(TConcreteType), forTypes);

            Func<object> wrapperFactory = () => obj;        // the magic of closures!
                                                            // Admit it, this is pretty darn elegant.

            foreach(var t in forTypes) {
                factories[t] = wrapperFactory;
            }
        }

        void IInProcessGlobalObjectRepository.Clear() {
            factories.Clear();
        }
        
        // =====================================================================
        // private
        // =====================================================================

        private void ValidateType(
            System.Type concreteTypeToCheck, 
            System.Type[] registeringTypes
        ) {
            if (registeringTypes.Length == 0) {
                throw new ArgumentException("Collection contains zero elements", nameof(registeringTypes));
            }
            
            foreach(var r in registeringTypes) {
                if (!r.IsAssignableFrom(concreteTypeToCheck)) {
                    throw new ArgumentException("Concrete type must implement registered type " + r.Name);
                }
            }
        }
    }
}