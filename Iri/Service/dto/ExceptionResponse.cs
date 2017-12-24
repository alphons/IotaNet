using System;

namespace IotaNet.Iri.Service.dto
{
	public class ExceptionResponse : AbstractResponse
	{


		private String exception;

		public static AbstractResponse create(String exception)
		{
			ExceptionResponse res = new ExceptionResponse();
			res.exception = exception;
			return res;
		}

		public String getException()
		{
			return exception;
		}
	}
}
