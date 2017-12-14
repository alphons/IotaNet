using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IotaNet.Iri.Hashing
{
	/**
	 * (c) 2016 Come-from-Beyond and Paul Handy
	 *
	 * Curl belongs to the sponge function family.
	 *
	 */
	public class Curl : ISponge
	{
		public static int HASH_LENGTH = 243;
		public static int NUMBER_OF_ROUNDSP81 = 81;
		public static int NUMBER_OF_ROUNDSP27 = 27;
		private int numberOfRounds;
		private static int STATE_LENGTH = 3 * HASH_LENGTH;
		private static int HALF_LENGTH = 364;

		private static int[] TRUTH_TABLE = { 1, 0, -1, 2, 1, -1, 0, 2, -1, 1, 0 };
		/*
		private static final IntPair[] TRANSFORM_INDICES = IntStream.range(0, STATE_LENGTH)
				.mapToObj(i -> new IntPair(i == 0 ? 0 : (((i - 1) % 2) + 1) * HALF_LENGTH - ((i - 1) >> 1),
						((i % 2) + 1) * HALF_LENGTH - ((i) >> 1)))
				.toArray(IntPair[]::new);
				*/

		private int[] state;
		private long[] stateLow;
		private long[] stateHigh;

		private int[] scratchpad = new int[STATE_LENGTH];


		public Curl(SpongeFactory.Mode mode)
		{
			switch (mode)
			{
				case SpongeFactory.Mode.CURLP27:
					{
						numberOfRounds = NUMBER_OF_ROUNDSP27;
					}
					break;
				case SpongeFactory.Mode.CURLP81:
					{
						numberOfRounds = NUMBER_OF_ROUNDSP81;
					}
					break;
				default: throw new Exception("Only Curl-P-27 and Curl-P-81 are supported.");
			}
			state = new int[STATE_LENGTH];
			stateHigh = null;
			stateLow = null;
		}

		public Curl(bool pair, SpongeFactory.Mode mode)
		{
			switch (mode)
			{
				case SpongeFactory.Mode.CURLP27:
					{
						numberOfRounds = NUMBER_OF_ROUNDSP27;
					}
					break;
				case SpongeFactory.Mode.CURLP81:
					{
						numberOfRounds = NUMBER_OF_ROUNDSP81;
					}
					break;
				default: throw new Exception("Only Curl-P-27 and Curl-P-81 are supported.");
			}
			if (pair)
			{
				stateHigh = new long[STATE_LENGTH];
				stateLow = new long[STATE_LENGTH];
				state = null;
				set();
			}
			else
			{
				state = new int[STATE_LENGTH];
				stateHigh = null;
				stateLow = null;
			}
		}

		private void setMode(SpongeFactory.Mode mode)
		{

		}

		public void absorb(int[] trits, int offset, int length)
		{

			do
			{
				Array.Copy(trits, offset, state, 0, length < HASH_LENGTH ? length : HASH_LENGTH);
				transform();
				offset += HASH_LENGTH;
			} while ((length -= HASH_LENGTH) > 0);
		}


		public void squeeze(int[] trits, int offset, int length)
		{

			do
			{
				Array.Copy(state, 0, trits, offset, length < HASH_LENGTH ? length : HASH_LENGTH);
				transform();
				offset += HASH_LENGTH;
			} while ((length -= HASH_LENGTH) > 0);
		}

		private void transform()
		{

			//final int[] scratchpad = new int[STATE_LENGTH];
			int scratchpadIndex = 0;
			int prev_scratchpadIndex = 0;
			for (int round = 0; round < numberOfRounds; round++)
			{
				Array.Copy(state, 0, scratchpad, 0, STATE_LENGTH);
				for (int stateIndex = 0; stateIndex < STATE_LENGTH; stateIndex++)
				{
					prev_scratchpadIndex = scratchpadIndex;
					if (scratchpadIndex < 365)
					{
						scratchpadIndex += 364;
					}
					else
					{
						scratchpadIndex += -365;
					}
					state[stateIndex] = TRUTH_TABLE[scratchpad[prev_scratchpadIndex] + (scratchpad[scratchpadIndex] << 2) + 5];
				}
			}
		}
		public void reset()
		{
			Array.Clear(state, 0, state.Length);
		}

		public void reset(bool pair)
		{
			if (pair)
			{
				set();
			}
			else
			{
				reset();
			}
		}
		private void set()
		{
			stateLow = stateLow.Select(x => Utils.Converter.HIGH_LONG_BITS).ToArray();
			stateHigh = stateHigh.Select(x => Utils.Converter.HIGH_LONG_BITS).ToArray();
		}

		private void pairTransform()
		{
			long[] curlScratchpadLow = new long[STATE_LENGTH];
			long[] curlScratchpadHigh = new long[STATE_LENGTH];
			int curlScratchpadIndex = 0;
			for (int round = numberOfRounds; round-- > 0;)
			{
				Array.Copy(stateLow, 0, curlScratchpadLow, 0, STATE_LENGTH);
				Array.Copy(stateHigh, 0, curlScratchpadHigh, 0, STATE_LENGTH);
				for (int curlStateIndex = 0; curlStateIndex < STATE_LENGTH; curlStateIndex++)
				{
					long alpha = curlScratchpadLow[curlScratchpadIndex];
					long beta = curlScratchpadHigh[curlScratchpadIndex];
					long gamma = curlScratchpadHigh[curlScratchpadIndex += (curlScratchpadIndex < 365 ? 364 : -365)];
					long delta = (alpha | (~gamma)) & (curlScratchpadLow[curlScratchpadIndex] ^ beta);
					stateLow[curlStateIndex] = ~delta;
					stateHigh[curlStateIndex] = (alpha ^ gamma) | delta;
				}
			}
		}

		public void absorb(Tuple<long[], long[]> pair, int offset, int length)
		{
			int o = offset, l = length, i = 0;
			do
			{
				Array.Copy(pair.Item1, o, stateLow, 0, l < HASH_LENGTH ? l : HASH_LENGTH);
				Array.Copy(pair.Item2, o, stateHigh, 0, l < HASH_LENGTH ? l : HASH_LENGTH);
				pairTransform();
				o += HASH_LENGTH;
			} while ((l -= HASH_LENGTH) > 0);
		}

		public Tuple<long[], long[]> squeeze(Tuple<long[], long[]> pair, int offset, int length)
		{
			int o = offset, l = length, i = 0;
			long[] low = pair.Item1;
			long[] hi = pair.Item2;
			do
			{
				Array.Copy(stateLow, 0, low, o, l < HASH_LENGTH ? l : HASH_LENGTH);
				Array.Copy(stateHigh, 0, hi, o, l < HASH_LENGTH ? l : HASH_LENGTH);
				pairTransform();
				o += HASH_LENGTH;
			} while ((l -= HASH_LENGTH) > 0);
			return new Tuple<long[], long[]>(low, hi);
		}
	}
}
