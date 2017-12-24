using System;
using System.Collections.Generic;

namespace IotaNet.Iri.Service.dto
{
	public class FindTransactionsResponse : AbstractResponse
	{


		private String[] hashes;

		public static AbstractResponse create(List<String> elements)
		{
			FindTransactionsResponse res = new FindTransactionsResponse();
			res.hashes = elements.toArray(new String[] { });
			return res;
		}

		public String[] getHashes()
		{
			return hashes;
		}
	}

}
