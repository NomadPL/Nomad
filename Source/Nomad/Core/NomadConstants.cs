using log4net;

namespace Nomad.Core
{
	/// <summary>
	///		Holds on the information about the 
	/// constants in the whole <see cref="Nomad"/> project. <c>To readonly use</c>
	/// </summary>
	public static class NomadConstants
	{
		/// <summary>
		///		All <see cref="Nomad"/> classes if they are using <see cref="ILog"/> should use
		/// this value as the default for method <see cref="LogManager.GetRepository()"/> and all
		/// other methods, accesing logger.
		/// </summary>
		public const string NOMAD_LOGGER_REPOSITORY = "Nomad";
	}
}