using System;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using Nomad.Core;

namespace Nomad.Utils
{
	/// <summary>
	///		Manages the creation of loggers from the 
	/// one place.
	/// </summary>
	public class LoggingHelper
	{
		private ILog _logger;
		private ILoggerRepository _repository;

		public ILoggerRepository Repository
		{
			get { return _repository; }
		}

		public ILog Logger
		{
			get { return _logger; }
		}

		/// <summary>
		///		Registers loggers within the <see cref="AppDomain"/>
		/// and creates the <see cref="ILoggerRepository"/> using 
		/// provided <paramref name="loggerConfiguration"/>.
		/// 
		/// Sample logging welcome message is done using <paramref name="type"/>
		/// </summary>
		/// <param name="loggerConfiguration"></param>
		/// <param name="type"></param>
		public void RegisterLogging(string loggerConfiguration, Type type)
		{
			try
			{
				_repository = LogManager.CreateRepository(NomadConstants.NOMAD_LOGGER_REPOSITORY);
			}
			catch (LogException)
			{
				// the repository is already defined 
				// this situation should not happen in real life situation
				// but can occur in syntetic testing
				_logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY, type);
				return;
			}


			try
			{
				var file = new FileInfo(loggerConfiguration);
				XmlConfigurator.Configure(_repository, file);
			}
			catch (Exception)
			{
				// NOTE: eat the exception here - fallback mechanism if user not specified other ways
				BasicConfigurator.Configure(_repository);
			}

			_logger = LogManager.GetLogger(NomadConstants.NOMAD_LOGGER_REPOSITORY, type);
			_logger.Info("The modules logger is enabled at levels:");


			_logger.Info(string.Format("Debug: {0}", _logger.IsDebugEnabled));
			_logger.Info(string.Format("Info: {0}", _logger.IsInfoEnabled));
			_logger.Info(string.Format("Warn: {0}", _logger.IsWarnEnabled));
			_logger.Info(string.Format("Error: {0}", _logger.IsErrorEnabled));
			_logger.Info(string.Format("Fatal: {0}", _logger.IsFatalEnabled));
		}
	}
}