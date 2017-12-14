using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Hashing
{
	public interface ISponge
	{
		void absorb(int[] trits, int offset, int length);
		void squeeze(int[] trits, int offset, int length);
		void reset();
	}

}
