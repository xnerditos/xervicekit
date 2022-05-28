namespace XKit.Lib.Common.Registration
{
	public interface IReadOnlyServiceInstance {
        /// <summary>
        /// Descriptor of the referenced service
        /// </summary>
        /// <value></value>
        IReadOnlyDescriptor Descriptor { get; }
		/// <summary>
		/// Unique identifier for the service instance
		/// </summary>
		/// <value></value>
		string InstanceId { get; }
		/// <summary>
		/// Unique identifier for the host instance
		/// </summary>
		/// <value></value>
		string HostFabricId { get; }
        /// <summary>
        /// Address of the host itself, from which meta service can be accessed
        /// </summary>
        /// <value></value>
        string HostAddress { get; }
		/// <summary>
		/// Key that describes the registration for this instance
		/// </summary>
		/// <value></value>
		string RegistrationKey { get; }
		/// <summary>
		/// Status of the service
		/// </summary>
		/// <value></value>
		IReadOnlyServiceInstanceStatus Status { get; }
        ServiceInstance Clone();
	}
}