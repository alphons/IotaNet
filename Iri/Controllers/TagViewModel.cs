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
	public class TagViewModel : HashesViewModel
	{

		private Tag self;
		private Indexable hash;

		public TagViewModel(Hash hash)
		{
			this.hash = hash;
		}

		private TagViewModel(Tag hashes, Indexable hash)
		{
			self = hashes == null || hashes.set == null ? new Tag() : hashes;
			this.hash = hash;
		}

		public static TagViewModel load(Tangle tangle, Indexable hash)
		{
			return new TagViewModel((Tag)tangle.load<Tag>( hash), hash);
		}

		public static Pair<Indexable, Persistable> getEntry(Hash hash, Hash hashToMerge)
		{
			Tag hashes = new Tag();
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
			tangle.delete<Tag>(hash);
		}
		public static TagViewModel first(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.getFirst<Tag, Hash>();
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new TagViewModel((Tag)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}

		public TagViewModel next(Tangle tangle)
		{
			Pair<Indexable, Persistable> bundlePair = tangle.next<Tag>(hash);
			if (bundlePair != null && bundlePair.hi != null)
			{
				return new TagViewModel((Tag)bundlePair.hi, (Hash)bundlePair.low);
			}
			return null;
		}
	}
}
