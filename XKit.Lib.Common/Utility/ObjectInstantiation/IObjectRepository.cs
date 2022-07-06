using System;

namespace XKit.Lib.Common.Utility.ObjectRepository {

    /// <summary>
    /// Provides a quick and easy form of dependency injection for "global" objects.
    /// Acronym:  IGOR (pronounced "eye-gor") 
    /// /// </summary>
    public interface IObjectRepository {

        /// <summary>
        /// Gets an object that has been previously registered for a type.  The type
        /// must be interface.
        /// </summary>
        /// <typeparam name="TRegisteredInterface"></typeparam>
        /// <returns>The corresponding object, either created or cached depending on the registration</returns>
        TRegisteredInterface GetObject<TRegisteredInterface>();

        /// <summary>
        /// Gets an object that has been previously registered for a type.  The type
        /// must be interface.
        /// </summary>
        /// <returns>The corresponding object, either created or cached depending on the registration</returns>
        object GetObject(System.Type interfaceType);

        /// <summary>
        /// Indicates of the type has been registered
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        bool HasObject(System.Type interfaceType);

        /// <summary>
        /// Registers an existing object for one or more types
        /// </summary>
        /// <param name="obj">the one object instance that is associated with the type</param>
        /// <param name="forTypes">types for which this object is registered</param>
        void RegisterObjectSingleton<TConcreteType>(TConcreteType obj, params System.Type[] forTypes);

        /// <summary>
        /// Registers a factory method for creating an object instance, associated with one or more types
        /// </summary>
        /// <param name="factoryMethod">the factory method to create the object.  It will only be invoked for each call to 
        /// GetObject.</param>
        /// <param name="forTypes">types for which this factory is registered</param>
        void RegisterObjectFactory<TConcreteType>(Func<TConcreteType> createMethod, params System.Type[] forTypes);

        // Clears all existing registrations
        void Clear();
    }
}
