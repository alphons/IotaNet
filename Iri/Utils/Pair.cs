using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Utils
{
	/**
	 * Created by paul on 4/15/17.
	 */
	public class Pair<S, T>
	{
		public S low;
		public T hi;
		public Pair(S k, T v)
		{
			low = k;
			hi = v;
		}
	}

}
