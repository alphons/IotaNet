using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Helpers
{
	public class PasswordCredential : Credential
	{
		public char[] getPassword()
		{
			return "secret".ToArray();
		}
	}
}
