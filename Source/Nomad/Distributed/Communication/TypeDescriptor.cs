using System;
using System.Runtime.Serialization;
using Version = Nomad.Utils.Version;

namespace Nomad.Distributed.Communication
{
	/// <summary>
	///		Information needed to recreate object which was serialized to the bytes.This class is
	/// sent by WCF alongside with <see cref="byte[]"/> to transfer object.
	/// </summary>
	/// <remarks>
	///		<see cref="TypeDescriptor"/> uses the <see cref="Utils.Version"/> which 
	/// pretty the same class as <see cref="System.Version"/> however the <see cref="Nomad"/>
	/// class supports serialization.
	/// </remarks>
	[DataContract]
	public class TypeDescriptor
	{
		public TypeDescriptor(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			QualifiedName = type.AssemblyQualifiedName;
			Version = new Version(type.Assembly.GetName().Version);
		}

		public TypeDescriptor(string qualifiedName, Version version)
		{
			if (string.IsNullOrEmpty(qualifiedName))
				throw new ArgumentNullException("qualifiedName");

			if (version == null)
				throw new ArgumentNullException("version");

			QualifiedName = qualifiedName;
			Version = version;
		}

		[DataMember]
		public string QualifiedName { get; set; }

		[DataMember]
		public Version Version { get; set; }

		public override string ToString()
		{
			return string.Format("{0}:{1}", QualifiedName, Version);
		}
	}
}