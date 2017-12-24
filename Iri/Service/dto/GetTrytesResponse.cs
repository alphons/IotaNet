using System;
using System.Collections.Generic;

namespace IotaNet.Iri.Service.dto
{
	public class GetTrytesResponse : AbstractResponse
	{


		private String[] trytes;

		public static GetTrytesResponse create(List<String> elements)
		{
			GetTrytesResponse res = new GetTrytesResponse();
			res.trytes = elements.toArray(new String[] { });
			return res;
		}

		public String[] getTrytes()
		{
			return trytes;
		}
	}
}
