using System;

namespace IotaNet.Iri.Service.dto
{
	public class AddedNeighborsResponse : AbstractResponse
	{


		private int addedNeighbors;

		public static AbstractResponse create(int numberOfAddedNeighbors)
		{
			AddedNeighborsResponse res = new AddedNeighborsResponse();
			res.addedNeighbors = numberOfAddedNeighbors;
			return res;
		}

		public int getAddedNeighbors()
		{
			return addedNeighbors;
		}

	}
}
