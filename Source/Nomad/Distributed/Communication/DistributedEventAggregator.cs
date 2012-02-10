using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using log4net;
using Nomad.Communication.EventAggregation;
using Nomad.Messages;
using Nomad.Messages.Distributed;
using Version = Nomad.Utils.Version;

namespace Nomad.Distributed.Communication
{
	/// <summary>
	///     Distributed version of <see cref="IEventAggregator"/> which enables Nomad application
	/// communicate over WCF links.
	/// </summary>
	/// <remarks>
	///     This class is visible to the modules loaded to Nomad as simple <see cref="IEventAggregator"/>. 
	/// </remarks>
	public class DistributedEventAggregator : MarshalByRefObject,
	                                          IEventAggregator, IDistributedEventAggregator, IDisposable
	{
		// TODO: make this value injectable ?
		private const int MESSAGE_SIZE = 2048;
		private static readonly ILog Loggger = LogManager.GetLogger(typeof (DistributedEventAggregator));

		private static IEventAggregator _localEventAggregator;


		private static readonly object LockObject = new object();
		private IList<IDistributedEventAggregator> _deas;

		/// <summary>
		///     This constructor is needed by the <see cref="ServiceHost"/>.
		/// </summary>
		public DistributedEventAggregator()
		{
		}

		/// <summary>
		///		This constructor is used by container during initalization. Used by <see cref="NomadDistributedEventAggregatorInstaller"/>
		/// </summary>
		/// <param name="localEventAggrgator"></param>
		public DistributedEventAggregator(IEventAggregator localEventAggrgator)
		{
			LocalEventAggregator = localEventAggrgator;
		}

		private static IEventAggregator LocalEventAggregator
		{
			get { return _localEventAggregator; }
			set
			{
				lock (LockObject)
				{
					if (_localEventAggregator != null)
						throw new InvalidOperationException("The local event aggregator can be set only once");

					_localEventAggregator = value;
				}
			}
		}

		/// <summary>
		///     Changes the list of the registerd remote site for the DEA.
		/// </summary>
		public IList<IDistributedEventAggregator> RemoteDistributedEventAggregator
		{
			set { _deas = value; }
			get { return _deas; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			// onDispose method or sending Dispos message throu communication normal services
			var msg = new NomadDetachingMessage("Sample Dispatching Message");
			SendToAllControl(msg);
		}

		#endregion

		#region IDistributedEventAggregator Members

		public void OnPublishControl(NomadDistributedMessage message)
		{
			Loggger.Debug(string.Format("Acquired message {0}", message));

			// propagate message to the local subscribers
			LocalEventAggregator.Publish(message);

			// do not propagete to the other DEA
			// at least in this implementation
		}

		public void OnPublish(byte[] byteStream, TypeDescriptor typeDescriptor)
		{
			Loggger.Debug(string.Format("Acquired message of type {0}", typeDescriptor));


			try
			{
				// try recreating this type 
				Type type = Type.GetType(typeDescriptor.QualifiedName);
				if (type != null)
				{
					var nomadVersion = new Version(type.Assembly.GetName().Version);

					if (!nomadVersion.Equals(typeDescriptor.Version))
					{
						throw new InvalidCastException("The version of the assembly does not match");
					}
				}

				// try deserializing object
				Object sendObject = Deserialize(byteStream);
				
				// check if o is assignable
				if (type != null && !type.IsInstanceOfType(sendObject))
				{
					throw new InvalidCastException("The sent object cannot be casted to sent type");
				}

				// invoke this generic method with type t
				// TODO: this is totaly not refactor aware use expression tree to get this publish thing
				var methodInfo = LocalEventAggregator.GetType().GetMethod("Publish");
				var goodMethodInfo = methodInfo.MakeGenericMethod(type);

				goodMethodInfo.Invoke(LocalEventAggregator, new[]{sendObject});
			}
			catch (Exception e)
			{
				Loggger.Warn("The type not be recreated", e);
			}

			
		}

		#endregion

		#region IEventAggregator Members

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action) where T : class
		{
			return Subscribe(action, DeliveryMethod.AnyThread);
		}

		public IEventAggregatorTicket<T> Subscribe<T>(Action<T> action, DeliveryMethod deliveryMethod) where T : class
		{
			// subscribe on local event
			return _localEventAggregator.Subscribe(action, deliveryMethod);

			// subscribe on remote or not by now
		}

		public void Publish<T>(T message) where T : class
		{
			// try publishing message in the local system on this machine
			_localEventAggregator.Publish(message);

			// filter local NomadMessage
			if (message is NomadMessage)
			{
				return;
			}
			// try publishing message in the remote system
			SendToAll(message);
		}

		#endregion

		private object Deserialize(byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				try
				{
					IFormatter formatter = new BinaryFormatter();
					return formatter.Deserialize(stream);
				}
				catch (Exception e)
				{
					Loggger.Warn("DeSerialization warning: ", e);
					throw;
				}
			}
		}

		private byte[] Serialize(Object obj)
		{
			MemoryStream stream = null;
			byte[] bytes;
			try
			{
				IFormatter formatter = new BinaryFormatter();
				stream = new MemoryStream();
				formatter.Serialize(stream, obj);

				if (stream.Length > MESSAGE_SIZE)
				{
					throw new InvalidOperationException("Object is to large for serialization");
				}

				bytes = stream.ToArray();
			}
			catch (Exception e)
			{
				Loggger.Warn("Serialization warning: ", e);

				// further sending is not possible
				return null;
			}
			finally
			{
				if (null != stream)
					stream.Close();
			}

			return bytes;
		}

		private void SendToAll<T>(T message)
		{
			byte[] bytes = Serialize(message);
			if (bytes == null)
				return;

			var descriptor = new TypeDescriptor(message.GetType());

			foreach (IDistributedEventAggregator dea in _deas)
			{
				try
				{
					dea.OnPublish(bytes, descriptor);
				}
				catch (Exception e)
				{
					Loggger.Warn(string.Format("Could not sent message '{0}' to DEA: {1}", message, dea), e);
					throw;
				}
			}
		}

		/// <summary>
		///		Sends the message using control type of invocation.
		/// </summary>
		private void SendToAllControl<T>(T message)
		{
			// NOTE: this code should be parralelized
			foreach (IDistributedEventAggregator dea in _deas)
			{
				try
				{
					dea.OnPublishControl(message as NomadDistributedMessage);
				}
				catch (Exception e)
				{
					Loggger.Warn("Exception during sending to DEA", e);
					throw;
				}
			}
		}
	}
}