using System;

namespace IotaNet.Iri.Service.dto
{

	public class ErrorResponse : AbstractResponse
	{


		private String error;

		public static AbstractResponse create(String error)
		{
			ErrorResponse res = new ErrorResponse();
			res.error = error;
			return res;
		}

		public String getError()
		{
			return error;
		}
	}
}
