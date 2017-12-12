using System;
using System.Linq;
using System.Collections.Generic;

using IotaNet.Iri.Hash;
using IotaNet.Iri.Helpers;

namespace IotaNet.Iri.Utils
{
	public class MapIdentityManager : IIdentityManager
	{
		private Dictionary<String, char[]> users;

		public MapIdentityManager(Dictionary<String, char[]> users)
		{
			this.users = users;
		}

		public Account verify(Account account)
		{
			// An existing account so for testing assume still valid.
			return account;
		}

		public Account verify(String id, Credential credential)
		{
			Account account = getAccount(id);
			if (account != null && verifyCredential(account, credential))
			{
				return account;
			}

			return null;
		}

		public Account verify(Credential credential)
		{
			// TODO Auto-generated method stub
			return null;
		}

		private bool verifyCredential(Account account, Credential credential)
		{
			if (credential is PasswordCredential)
			{
				char[] givenPassword = ((PasswordCredential)credential).getPassword();
				String trytes = Converter.asciiToTrytes(new String(givenPassword));
				int[] in_trits = Converter.trits(trytes);
				int[] hash_trits = new int[Curl.HASH_LENGTH];
				ISponge curl;
				curl = SpongeFactory.Create(SpongeFactory.Mode.CURLP81);
				curl.absorb(in_trits, 0, in_trits.Length);
				curl.squeeze(hash_trits, 0, Curl.HASH_LENGTH);
				String out_trytes = Converter.trytes(hash_trits);
				char[] char_out_trytes = out_trytes.ToArray();
				char[] expectedPassword = users[account.getPrincipal().getName()];
				bool verified = Arrays.equals(givenPassword, expectedPassword);
				// Password can either be clear text or the hash of the password
				if (!verified)
				{
					verified = Arrays.equals(char_out_trytes, expectedPassword);
				}
				return verified;
			}
			return false;
		}

		private Account getAccount(String id)
		{
			if (users.ContainsKey(id))
			{
				return new Account();
			}
			return null;
		}
	}
}

