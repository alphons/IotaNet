using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Zmq
{
	public class ZMQ
	{
		public static int PUB = 1;
		public static Context context(int nthreads)
		{
			return new Context();
		}

		public class Socket
		{
			public void bind(string s)
			{
			}
			public void send(string s)
			{
			}
			public void close()
			{
			}
		}

		public class Context
		{
			public Socket socket(int sock)
			{
				return new Socket();
			}

			public void term()
			{

			}
		}

	}

}
