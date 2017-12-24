using System;
using System.Collections.Concurrent;

using IotaNet.Iri.Model;
using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Controllers
{
	/**
	 * Created by paul on 4/11/17.
	 */
	public class MilestoneViewModel
	{
		private Milestone milestone;
		private static ConcurrentDictionary<int, MilestoneViewModel> milestones = new ConcurrentDictionary<int, MilestoneViewModel>();

		private MilestoneViewModel(Milestone milestone)
		{
			this.milestone = milestone;
		}

		public static void clear()
		{
			milestones.Clear();
		}

		public MilestoneViewModel(int index, Hash milestoneHash)
		{
			this.milestone = new Milestone();
			this.milestone.index = new IntegerIndex(index);
			milestone.hash = milestoneHash;
		}

		public static MilestoneViewModel get(Tangle tangle, int index)
		{
			MilestoneViewModel milestoneViewModel = milestones.get(index);
			if (milestoneViewModel == null && load(tangle, index))
			{
				milestoneViewModel = milestones.get(index);
			}
			return milestoneViewModel;
		}

		public static bool load(Tangle tangle, int index)
		{
			Milestone milestone = (Milestone)tangle.load<Milestone>(new IntegerIndex(index));
			if (milestone != null && milestone.hash != null)
			{
				milestones.put(index, new MilestoneViewModel(milestone));
				return true;
			}
			return false;
		}

		public static MilestoneViewModel first(Tangle tangle)
		{
			Pair<Indexable, Persistable> milestonePair = tangle.getFirst<Milestone, IntegerIndex>();
			if (milestonePair != null && milestonePair.hi != null)
			{
				Milestone milestone = (Milestone)milestonePair.hi;
				return new MilestoneViewModel(milestone);
			}
			return null;
		}

		public static MilestoneViewModel latest(Tangle tangle)
		{
			Pair<Indexable, Persistable> milestonePair = tangle.getLatest<Milestone, IntegerIndex>();
			if (milestonePair != null && milestonePair.hi != null)
			{
				Milestone milestone = (Milestone)milestonePair.hi;
				return new MilestoneViewModel(milestone);
			}
			return null;
		}

		public MilestoneViewModel previous(Tangle tangle)
		{
			Pair<Indexable, Persistable> milestonePair = tangle.previous<Milestone>(this.milestone.index);
			if (milestonePair != null && milestonePair.hi != null)
			{
				Milestone milestone = (Milestone)milestonePair.hi;
				return new MilestoneViewModel((Milestone)milestone);
			}
			return null;
		}

		public MilestoneViewModel next(Tangle tangle)
		{
			Pair<Indexable, Persistable> milestonePair = tangle.next<Milestone>(this.milestone.index);
			if (milestonePair != null && milestonePair.hi != null)
			{
				Milestone milestone = (Milestone)milestonePair.hi;
				return new MilestoneViewModel((Milestone)milestone);
			}
			return null;
		}

		public MilestoneViewModel nextWithSnapshot(Tangle tangle)
		{
			MilestoneViewModel milestoneViewModel = next(tangle);
			while (milestoneViewModel != null && !StateDiffViewModel.exists(tangle, milestoneViewModel.getHash()))
			{
				milestoneViewModel = milestoneViewModel.next(tangle);
			}
			return milestoneViewModel;
		}

		public static MilestoneViewModel firstWithSnapshot(Tangle tangle)
		{
			MilestoneViewModel milestoneViewModel = first(tangle);
			while (milestoneViewModel != null && !StateDiffViewModel.exists(tangle, milestoneViewModel.getHash()))
			{
				milestoneViewModel = milestoneViewModel.next(tangle);
			}
			return milestoneViewModel;
		}

		public static MilestoneViewModel findClosestPrevMilestone(Tangle tangle, int index)
		{
			Pair<Indexable, Persistable> milestonePair = tangle.previous<Milestone>(new IntegerIndex(index));
			if (milestonePair != null && milestonePair.hi != null)
			{
				return new MilestoneViewModel((Milestone)milestonePair.hi);
			}
			return null;
		}

		public static MilestoneViewModel findClosestNextMilestone(Tangle tangle, int index)
		{
			if (index <= Milestone.MILESTONE_START_INDEX)
			{
				return first(tangle);
			}
			Pair<Indexable, Persistable> milestonePair = tangle.next<Milestone>(new IntegerIndex(index));
			if (milestonePair != null && milestonePair.hi != null)
			{
				return new MilestoneViewModel((Milestone)milestonePair.hi);
			}
			return null;
		}

		public static MilestoneViewModel latestWithSnapshot(Tangle tangle)
		{
			MilestoneViewModel milestoneViewModel = latest(tangle);
			while (milestoneViewModel != null && !StateDiffViewModel.exists(tangle, milestoneViewModel.getHash()))
			{
				milestoneViewModel = milestoneViewModel.previous(tangle);
			}
			return milestoneViewModel;
		}

		public bool store(Tangle tangle)
		{
			return tangle.save(milestone, milestone.index);
		}

		public Hash getHash()
		{
			return milestone.hash;
		}
		public int index()
		{
			return milestone.index.getValue();
		}

		public void delete(Tangle tangle)
		{
			tangle.delete<Milestone>(milestone.index);
		}

	}
}
