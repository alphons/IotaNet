using System;

using IotaNet.Iri.Model;

namespace IotaNet.Iri.Service.dto
{
	public class GetTransactionsToApproveResponse : AbstractResponse
	{


		private String trunkTransaction;
		private String branchTransaction;

		public static AbstractResponse create(Hash trunkTransactionToApprove, Hash branchTransactionToApprove)
		{
			GetTransactionsToApproveResponse res = new GetTransactionsToApproveResponse();
			res.trunkTransaction = trunkTransactionToApprove.ToString();
			res.branchTransaction = branchTransactionToApprove.ToString();
			return res;
		}

		public String getBranchTransaction()
		{
			return branchTransaction;
		}

		public String getTrunkTransaction()
		{
			return trunkTransaction;
		}
	}
}
