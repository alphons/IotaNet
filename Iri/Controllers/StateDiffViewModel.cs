using System;
using System.Collections.Generic;

using IotaNet.Iri.Model;
using IotaNet.Iri.Storage;

namespace IotaNet.Iri.Controllers
{
	/**
	 * Created by paul on 5/6/17.
	 */
	public class StateDiffViewModel
	{
		private StateDiff stateDiff;
		private Hash hash;

		public static StateDiffViewModel load(Tangle tangle, Hash hash)
		{
			return new StateDiffViewModel((StateDiff)tangle.load<StateDiff>( hash), hash);
		}

		public StateDiffViewModel(Dictionary<Hash, long> state, Hash hash)
		{
			this.hash = hash;
			this.stateDiff = new StateDiff();
			this.stateDiff.state = state;
		}

		public static bool exists(Tangle tangle, Hash hash)
		{
			return tangle.maybeHas<StateDiff>(hash);
		}

		StateDiffViewModel(StateDiff diff, Hash hash)
		{
			this.hash = hash;
			this.stateDiff = diff == null || diff.state == null ? new StateDiff() : diff;
		}

		public Hash getHash()
		{
			return hash;
		}

		public Dictionary<Hash, long> getDiff()
		{
			return stateDiff.state;
		}

		public bool store(Tangle tangle)
		{
			//return Tangle.instance().save(stateDiff, hash).get();
			return tangle.save(stateDiff, hash);
		}

		public void delete(Tangle tangle)
		{
			tangle.delete<StateDiff>(hash);
		}
	}
}
