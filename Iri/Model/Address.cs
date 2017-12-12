using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 5/15/17.
	 */
	public class Address : Hashes
	{

	public Address() { }
	public Address(Hash hash)
	{
		set.add(hash);
	}
}
}
