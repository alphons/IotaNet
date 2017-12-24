using System;

namespace IotaNet.Iri.Service.dto
{
	public class RemoveNeighborsResponse : AbstractResponse
	{
		private int removedNeighbors;

		public static AbstractResponse create(int numberOfRemovedNeighbors)
		{
			RemoveNeighborsResponse res = new RemoveNeighborsResponse();
			res.removedNeighbors = numberOfRemovedNeighbors;
			return res;
		}

		public int getRemovedNeighbors()
		{
			return removedNeighbors;
		}

	}
}
