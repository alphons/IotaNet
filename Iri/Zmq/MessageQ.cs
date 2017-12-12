using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Zmq
{
   /**
	* Created by paul on 6/20/17.
	*/
	public class MessageQ
	{
		private  ZMQ.Context context;
		private  ZMQ.Socket publisher;
		private bool enabled = false;

		public MessageQ(int port, String ipc, int nthreads, bool enabled)
		{
			if (enabled)
			{
				context = ZMQ.context(nthreads);
				publisher = context.socket(ZMQ.PUB);
				publisher.bind(String.Format("tcp://*:{0}", port));
				if (ipc != null)
				{
					publisher.bind(ipc);
				}
				this.enabled = true;
			}
			else
			{
				context = null;
				publisher = null;
			}
		}

		public void publish(String message, params Object[] objects)
		{
			if (enabled)
			{
				publisher.send(String.Format(message, objects));
			}
		}

		public void shutdown()
		{
			publisher.close();
			context.term();
		}
	}
}
