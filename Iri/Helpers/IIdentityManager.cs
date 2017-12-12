using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Helpers
{
	public interface IIdentityManager
	{

		/**
		 * Verify a previously authenticated account.
		 *
		 * Typical checks could be along the lines of verifying that the account is not now locked or that the password has not been
		 * reset since last verified, also this provides an opportunity for roles to be re-loaded if membership information has
		 * changed.
		 *
		 * @param account - The {@link Account} to verify.
		 * @return An updates {@link Account} if verification is successful, null otherwise.
		 */
		Account verify( Account account);

		/**
		 * Verify a supplied {@link Credential} against a requested ID.
		 *
		 * @param id - The requested ID for the account.
		 * @param credential - The {@link Credential} to verify.
		 * @return The {@link Account} for the user if verification was successful, null otherwise.
		 */
		Account verify( String id,  Credential credential);

		/**
		 * Perform verification when all we have is the Credential, in this case the IdentityManager is also responsible for mapping the Credential to an account.
		 *
		 * The most common scenario for this would be mapping an X509Certificate to the user it is associated with.
		 *
		 * @param credential
		 * @return
		 */
		Account verify( Credential credential);


	}
}
