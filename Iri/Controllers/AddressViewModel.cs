using System;

using System.Collections.Generic;

using IotaNet.Iri.Model;
using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Controllers
{
	/**
	 * Created by paul on 5/15/17.
	 */
	public class AddressViewModel : HashesViewModel
	{

		private Address self;
		private Indexable hash;

		public AddressViewModel(Hash hash)
		{
			this.hash = hash;
		}

		private AddressViewModel(Address hashes, Indexable hash)
		{
			self = hashes == null || hashes.set == null ? new Address() : hashes;
			this.hash = hash;
		}

		public static AddressViewModel load(Tangle tangle, Indexable hash)
		{
			return new AddressViewModel((Address)tangle.load<Address>(hash), hash);
		}

		public bool store(Tangle tangle)
		{
			return tangle.save(self, hash);
		}

		public int size()
		{
			return self.set.Count;
		}

		public bool addHash(Hash theHash)
		{
			return getHashes().Add(theHash);
		}

		public Indexable getIndex()
		{
			return hash;
		}

		public HashSet<Hash> getHashes()
		{
			return self.set;
		}
		public void delete(Tangle tangle)
		{
			tangle.delete<Address>(hash);
		}

		public static AddressViewModel first(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.getFirst<Address, Hash>();
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new AddressViewModel((Address)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}

		public AddressViewModel next(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.next<Address>(hash);
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new AddressViewModel((Address)bundlePair.hi, (Hash)bundlePair.low);
			}
			return default(AddressViewModel);
		}
	}
}

