using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Network
{
	public abstract class Neighbor
	{

		private InetSocketAddress address;

		private long numberOfAllTransactions;
		private long numberOfNewTransactions;
		private long numberOfInvalidTransactions;
		private long randomTransactionRequests;
		private long numberOfSentTransactions;

		private bool flagged = false;
		public bool isFlagged()
		{
			return flagged;
		}
		public void setFlagged(bool flagged)
		{
			this.flagged = flagged;
		}

		private static AtomicInteger numPeers = new AtomicInteger(0);
		public static int getNumPeers()
		{
			return numPeers.get();
		}
		public static void incNumPeers()
		{
			numPeers.incrementAndGet();
		}
		public static void decNumPeers()
		{
			int v = numPeers.decrementAndGet();
			if (v < 0) numPeers.set(0); ;
		}

		private String hostAddress;

		public String getHostAddress()
		{
			return hostAddress;
		}

		public Neighbor(InetSocketAddress address, bool isConfigured)
		{
			this.address = address;
			this.hostAddress = address.getAddress().getHostAddress();
			this.flagged = isConfigured;
		}

		public abstract void send(DatagramPacket packet);
		public abstract int getPort();
		public abstract String connectionType();
		public abstract bool matches(SocketAddress address);

		public override bool Equals(Object obj)
		{
			return this == obj || !((obj == null) || (obj.getClass() != this.getClass())) && address.equals(((Neighbor)obj).address);
		}

		public int GetHashCode()
		{
			return address.GetHashCode();
		}

		public InetSocketAddress getAddress()
		{
			return address;
		}

		void incAllTransactions()
		{
			numberOfAllTransactions++;
		}

		void incNewTransactions()
		{
			numberOfNewTransactions++;
		}

		void incRandomTransactionRequests()
		{
			randomTransactionRequests++;
		}

		public void incInvalidTransactions()
		{
			numberOfInvalidTransactions++;
		}

		public void incSentTransactions()
		{
			numberOfSentTransactions++;
		}

		public long getNumberOfAllTransactions()
		{
			return numberOfAllTransactions;
		}

		public long getNumberOfInvalidTransactions()
		{
			return numberOfInvalidTransactions;
		}

		public long getNumberOfNewTransactions()
		{
			return numberOfNewTransactions;
		}

		public long getNumberOfRandomTransactionRequests()
		{
			return randomTransactionRequests;
		}

		public long getNumberOfSentTransactions()
		{
			return numberOfSentTransactions;
		}

	}

}
