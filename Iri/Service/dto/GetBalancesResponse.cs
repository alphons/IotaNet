using System;
using System.Collections.Generic;

using IotaNet.Iri.Model;

namespace IotaNet.Iri.Service.dto
{
	public class GetBalancesResponse : AbstractResponse
	{
		private List<String> balances;
		private String milestone;
		private int milestoneIndex;

		public static AbstractResponse create(List<String> elements, Hash milestone, int milestoneIndex)
		{
			GetBalancesResponse res = new GetBalancesResponse();
			res.balances = elements;
			res.milestone = milestone.ToString();
			res.milestoneIndex = milestoneIndex;
			return res;
		}

		public String getMilestone()
		{
			return milestone;
		}

		public int getMilestoneIndex()
		{
			return milestoneIndex;
		}

		public List<String> getBalances()
		{
			return balances;
		}
	}
}
