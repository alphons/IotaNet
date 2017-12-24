using System;

namespace IotaNet.Iri.Service.dto
{
	public class GetInclusionStatesResponse : AbstractResponse
	{


		private bool[] states;

		public static AbstractResponse create(bool[] inclusionStates)
		{
			GetInclusionStatesResponse res = new GetInclusionStatesResponse();
			res.states = inclusionStates;
			return res;
		}

		public bool[] getStates()
		{
			return states;
		}

	}
}
