using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Helpers
{
	/**
 * Representation of an account, most likely a user account.
 *
 * @author <a href="mailto:darran.lofthouse@jboss.com">Darran Lofthouse</a>
 */
	public interface IAccount
	{

		Principal getPrincipal();

		/**
		 * Returns the users roles.
		 *
		 * @return A set of the users roles
		 */
		List<String> getRoles();

		// TODO - Do we need a way to pass back to IDM that account is logging out? A few scenarios: -
		// 1 - Session expiration so cached account known to be logging out.
		// 2 - API call to logout.
		// 3 - End of HTTP request where account not cached, not strictly logging out but then again no real log-in.

	}
}
