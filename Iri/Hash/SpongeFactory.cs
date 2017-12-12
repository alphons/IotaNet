using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Hash
{
	public abstract class SpongeFactory
	{
		public enum Mode
		{
			CURLP81,
			CURLP27,
			KERL,
			//BCURLT
		}

		public static ISponge Create(Mode mode)
		{
			switch (mode)
			{
				case Mode.CURLP81:
					return new Curl(mode);
				case Mode.CURLP27:
					return new Curl(mode);
				case Mode.KERL:
					return new Kerl();
				//case Mode.BCURLT: 
				//	return new Curl(true, mode);
				default:
					return null;
			}
		}
	}
}
