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
	public class ApproveeViewModel : HashesViewModel
	{

		private Approvee self;
		private Indexable hash;

		public ApproveeViewModel(Hash hash)
		{
			this.hash = hash;
		}

		private ApproveeViewModel(Approvee hashes, Indexable hash)
		{
			self = hashes == null || hashes.set == null ? new Approvee() : hashes;
			this.hash = hash;
		}

		public static ApproveeViewModel load(Tangle tangle, Indexable hash)
		{
			return new ApproveeViewModel((Approvee)tangle.load<Approvee>(hash), hash);
		}

		public static Pair<Indexable, Persistable> getEntry(Hash hash, Hash hashToMerge) // check if pair is good here
		{
			Approvee hashes = new Approvee();
			hashes.set.Add(hashToMerge);
			return new Pair<Indexable, Persistable>(hash, hashes);
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
			tangle.delete<Approvee>(hash);
		}

		public static ApproveeViewModel first(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.getFirst<Approvee,Hash>();
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new ApproveeViewModel((Approvee)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}

		public ApproveeViewModel next(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.next<Approvee>(hash);
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new ApproveeViewModel((Approvee)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}
	}
}
