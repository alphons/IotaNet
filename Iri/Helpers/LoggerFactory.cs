using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Helpers
{
	public class LoggerFactory
	{
		public static Logger getLogger<T>()
		{
			return new Logger(typeof(T));
		}
	}

	public class Logger
	{
		public Logger(Type t)
		{
		}
		public void debug(string format, params object[] list)
		{

		}
		public void info(string format, params object[] list)
		{

		}
	}
}
