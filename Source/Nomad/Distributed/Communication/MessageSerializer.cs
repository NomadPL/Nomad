using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;
using Nomad.Core;

namespace Nomad.Distributed.Communication
{
	/// <summary>
	///		Helper class for performing serialization and deserialization
	/// of messages sent by <see cref="DistributedEventAggregator"/>.
	/// </summary>
	public static class MessageSerializer
	{
		private static readonly ILog Logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY,
		                                                           typeof (MessageSerializer));

		private static decimal MESSAGE_SIZE = 65000;

		public static object Deserialize(byte[] bytes)
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
					Logger.Warn("DeSerialization warning: ", e);
					throw;
				}
			}
		}

		public static byte[] Serialize(Object obj)
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
				Logger.Warn("Serialization warning: ", e);

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
	}
}