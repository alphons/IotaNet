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
	public class BundleViewModel : HashesViewModel
	{

		private Bundle self;
		private Indexable hash;

		public BundleViewModel(Hash hash)
		{
			this.hash = hash;
		}

		private BundleViewModel(Bundle hashes, Indexable hash)
		{
			self = hashes == null || hashes.set == null ? new Bundle() : hashes;
			this.hash = hash;
		}

		public static BundleViewModel load(Tangle tangle, Indexable hash)
		{
			return new BundleViewModel((Bundle)tangle.load<Bundle>(hash), hash);
		}

		public static Pair<Indexable, Persistable> getEntry(Hash hash, Hash hashToMerge)
		{
			Bundle hashes = new Bundle();
			hashes.set.Add(hashToMerge);
			return new Pair<Indexable, Persistable>(hash, hashes);
		}

		/*
		public static boolean merge(Hash hash, Hash hashToMerge) throws Exception {
			Bundle hashes = new Bundle();
			hashes.set = new HashSet<>(Collections.singleton(hashToMerge));
			return Tangle.instance().merge(hashes, hash);
		}
		*/

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
			tangle.delete<Bundle>(hash);
		}

		public static BundleViewModel first(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.getFirst<Bundle, Hash>();
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new BundleViewModel((Bundle)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}

		public BundleViewModel next(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.next<Bundle>(hash);
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new BundleViewModel((Bundle)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}
	}

}
