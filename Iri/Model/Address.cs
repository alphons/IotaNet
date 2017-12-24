using System;

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
			set.Add(hash);
		}
	}
}
