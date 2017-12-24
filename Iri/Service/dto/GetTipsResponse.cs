using System;
using System.Collections.Generic;
using System.Linq;

namespace IotaNet.Iri.Service.dto
{
	public class GetTipsResponse : AbstractResponse
	{


		private String[] hashes;

		public static AbstractResponse create(List<String> elements)
		{
			GetTipsResponse res = new GetTipsResponse();
			res.hashes = elements.ToArray(new String[] { });
			return res;
		}

		public String[] getHashes()
		{
			return hashes;
		}

	}
}
