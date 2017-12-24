using System;
using System.Collections.Generic;

using IotaNet.Iri.Helpers;
using IotaNet.Iri.Model;

namespace IotaNet.Iri.Controllers
{
	/**
	 * Created by paul on 3/14/17 for iri-testnet.
	 */
	public class TipsViewModel
	{
		Logger log = LoggerFactory.getLogger<TipsViewModel>();

		private FifoHashCache tips = new FifoHashCache(5000);
		private FifoHashCache solidTips = new FifoHashCache(5000);

		private SecureRandom seed = new SecureRandom();
		public Object sync = new Object();

		public bool addTipHash(Hash hash)
		{

			synchronized(sync)
		{
				return tips.add(hash);
			}
		}

		public bool removeTipHash(Hash hash) {
			synchronized(sync)
		{
				if (!tips.remove(hash))
				{
					return solidTips.remove(hash);
				}
			}
			return true;
		}

		public void setSolid(Hash tip)
		{
			synchronized(sync) {
				if (tips.remove(tip))
				{
					solidTips.add(tip);
				}
			}
		}

		public HashSet<Hash> getTips()
		{
			HashSet<Hash> hashes = new HashSet<Hash>();
			synchronized(sync) {
				Iterator<Hash> hashIterator;
				hashIterator = tips.iterator();
				while (hashIterator.hasNext())
				{
					hashes.Add(hashIterator.next());
				}

				hashIterator = solidTips.iterator();
				while (hashIterator.hasNext())
				{
					hashes.Add(hashIterator.next());
				}
			}
			return hashes;
		}

		public Hash getRandomSolidTipHash()
		{
			synchronized(sync) {
				int size = solidTips.size();
				if (size == 0)
				{
					return getRandomNonSolidTipHash();
				}
				int index = seed.nextInt(size);
				Iterator<Hash> hashIterator;
				hashIterator = solidTips.iterator();
				Hash hash = null;
				while (index-- >= 0 && hashIterator.hasNext()) { hash = hashIterator.next(); }
				return hash;
				//return solidTips.size() != 0 ? solidTips.get(seed.nextInt(solidTips.size())) : getRandomNonSolidTipHash();
			}
		}

		public Hash getRandomNonSolidTipHash()
		{
			synchronized(sync) {
				int size = tips.size();
				if (size == 0)
				{
					return null;
				}
				int index = seed.nextInt(size);
				Iterator<Hash> hashIterator;
				hashIterator = tips.iterator();
				Hash hash = null;
				while (index-- >= 0 && hashIterator.hasNext()) { hash = hashIterator.next(); }
				return hash;
				//return tips.size() != 0 ? tips.get(seed.nextInt(tips.size())) : null;
			}
		}

		public int nonSolidSize()
		{
			synchronized(sync) {
				return tips.size();
			}
		}

		public int size()
		{
			synchronized(sync) {
				return tips.size() + solidTips.size();
			}
		}

		//    public Hash getRandomTipHash() throws ExecutionException, InterruptedException {
		//        synchronized (sync) {
		//            if(size() == 0) {
		//                return null;
		//            }
		//            int index = seed.nextInt(size());
		//            if(index >= tips.size()) {
		//                return getRandomSolidTipHash();
		//            } else {
		//                return getRandomNonSolidTipHash();
		//            }
		//        }
		//    }


		//    public void loadTipHashes(Tangle tangle) throws Exception {
		//        Set<Indexable> hashes = tangle.keysWithMissingReferences(Transaction.class, Approvee.class);
		//        if(hashes != null) {
		//            synchronized (sync) {
		//                for (Indexable h: hashes) {
		//                    tips.add((Hash) h);
		//                }
		//            }
		//        }
		//    }
		//
		//    public Set<Hash> getTipsHashesFromDB (Tangle tangle) throws Exception {
		//        Set<Hash> tipsFromDB = new HashSet<>();
		//        Set<Indexable> hashes = tangle.keysWithMissingReferences(Transaction.class, Approvee.class);
		//        if(hashes != null) {
		//            tipsFromDB.addAll(hashes.stream().map(h -> (Hash) h).collect(Collectors.toList()));
		//        }
		//        return tipsFromDB;
		//    }

		public class FifoHashCache
		{

			private int capacity;
			private LinkedHashSet<Hash> set;

			public FifoHashCache(int capacity)
			{
				this.capacity = capacity;
				this.set = new LinkedHashSet<>();
			}

			public bool add(Hash key)
			{
				if (this.set.size() == this.capacity)
				{
					Iterator<Hash> it = this.set.iterator();
					it.next();
					it.remove();
				}
				return set.add(key);
			}
			public bool remove(Hash key)
			{
				return this.set.remove(key);
			}
			public int size()
			{
				return this.set.size();
			}
			public bool addAll(Collection c)
			{
				return this.set.addAll(c);
			}
			public Iterator<Hash> iterator()
			{
				return this.set.iterator();
			}
		}

	}
}
