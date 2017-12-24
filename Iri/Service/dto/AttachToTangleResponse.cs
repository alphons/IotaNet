using System;
using System.Collections.Generic;

namespace IotaNet.Iri.Service.dto
{
	public class AttachToTangleResponse : AbstractResponse
	{

		private List<String> trytes;

		public static AbstractResponse create(List<String> elements)
		{
			AttachToTangleResponse res = new AttachToTangleResponse();
			res.trytes = elements;
			return res;
		}

		public List<String> getTrytes()
		{
			return trytes;
		}
	}
}
