using System;
using System.Collections.Generic;

namespace XKit.Lib.Common.Utility.ObjectRepository {
    internal class ObjectRepository : IObjectRepository {
        
        private readonly Dictionary<Type, Func<object>> factories = new();
        
        public ObjectRepository() {}

        // =====================================================================
        // IInjectableGlobalObjectRepository
        // =====================================================================
        
        TRegisteredInterface IObjectRepository.GetObject<TRegisteredInterface>() {
            if (!factories.TryGetValue(typeof(TRegisteredInterface), out var factory)) {
                throw new Exception("Type not registered " + typeof(TRegisteredInterface).Name);
            }
            return (TRegisteredInterface) factory?.Invoke();
        }

        object IObjectRepository.GetObject(Type interfaceType) {
            if (!factories.TryGetValue(interfaceType, out var factory)) {
                throw new Exception("Type not registered " + interfaceType.Name);
            }
            return factory?.Invoke();
        }

        bool IObjectRepository.HasObject(Type interfaceType) 
            => factories.ContainsKey(interfaceType);

        void IObjectRepository.RegisterObjectFactory<TConcreteType>(Func<TConcreteType> createMethod, params Type[] forTypes) {
            ValidateType(typeof(TConcreteType), forTypes);

            foreach (var t in forTypes) {
                factories[t] = () => createMethod();
            }
        }

        void IObjectRepository.RegisterObjectSingleton<TConcreteType>(TConcreteType obj, params Type[] forTypes) {

            ValidateType(typeof(TConcreteType), forTypes);

            foreach (var t in forTypes) {
                // the magic of closures!
                // Admit it, this is pretty darn elegant.

                factories[t] = () => obj;
            }
        }

        void IObjectRepository.Clear() {
            factories.Clear();
        }
        
        // =====================================================================
        // private
        // =====================================================================

        private static void ValidateType(
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
