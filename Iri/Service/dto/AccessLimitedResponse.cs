using System;

namespace IotaNet.Iri.Service.dto
{
	/**
	 * Created by Adrian on 07.01.2017.
	 */
	public class AccessLimitedResponse : AbstractResponse
	{


		private String error;

		public static AbstractResponse create(String error)
		{
			AccessLimitedResponse res = new AccessLimitedResponse();
			res.error = error;
			return res;
		}

		public String getError()
		{
			return error;
		}
	}

}
