using System;
using System.Collections.Generic;


namespace IotaNet.Iri.Service.dto
{
	public class GetNeighborsResponse : AbstractResponse
	{


		private Neighbor[] neighbors;

		public Neighbor[] getNeighbors()
		{
			return neighbors;
		}

		public class Neighbor
		{

			private String address;
			public long numberOfAllTransactions, numberOfRandomTransactionRequests, numberOfNewTransactions, numberOfInvalidTransactions, numberOfSentTransactions;
			public String connectionType;

			public String getAddress()
			{
				return address;
			}

			public long getNumberOfAllTransactions()
			{
				return numberOfAllTransactions;
			}

			public long getNumberOfNewTransactions()
			{
				return numberOfNewTransactions;
			}

			public long getNumberOfInvalidTransactions()
			{
				return numberOfInvalidTransactions;
			}

			public long getNumberOfSentTransactions()
			{
				return numberOfSentTransactions;
			}

			public String getConnectionType()
			{
				return connectionType;
			}

			public static Neighbor createFrom(Neighbor n)
			{
				Neighbor ne = new Neighbor();
				int port = n.getPort();
				ne.address = n.getAddress().getHostString() + ":" + port;
				ne.numberOfAllTransactions = n.getNumberOfAllTransactions();
				ne.numberOfInvalidTransactions = n.getNumberOfInvalidTransactions();
				ne.numberOfNewTransactions = n.getNumberOfNewTransactions();
				ne.numberOfRandomTransactionRequests = n.getNumberOfRandomTransactionRequests();
				ne.numberOfSentTransactions = n.getNumberOfSentTransactions();
				ne.connectionType = n.connectionType();
				return ne;
			}
		}

		public static AbstractResponse create(List<Neighbor> elements)
		{
			GetNeighborsResponse res = new GetNeighborsResponse();
			res.neighbors = new Neighbor[elements.size()];
			int i = 0;
			foreach (Neighbor n in elements)
			{
				res.neighbors[i++] = Neighbor.createFrom(n);
			}
			return res;
		}

	}

}
