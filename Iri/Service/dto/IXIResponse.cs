using System;

namespace IotaNet.Iri.Service.dto
{
	/**
	 * Created by paul on 2/10/17.
	 */
	public class IXIResponse : AbstractResponse
	{

		private Object ixi;

		public static IXIResponse create(Object myixi)
		{
			IXIResponse ixiResponse = new IXIResponse();
			ixiResponse.ixi = myixi;
			return ixiResponse;
		}

		public Object getResponse()
		{
			return ixi;
		}
	}

}
