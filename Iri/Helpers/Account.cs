using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Helpers
{
	public class Account : IAccount
	{
		public Principal getPrincipal()
		{
			return new Principal();
		}

		public List<string> getRoles()
		{
			return new List<string>();
		}
	}
}
