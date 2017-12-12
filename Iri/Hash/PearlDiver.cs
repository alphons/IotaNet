using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotaNet.Iri.Hash
{
	/**
	 * (c) 2016 Come-from-Beyond
	 */
	public class PearlDiver
	{

		enum State
		{
			RUNNING,
			CANCELLED,
			COMPLETED
		}

		private static int TRANSACTION_LENGTH = 8019;

		private static int CURL_HASH_LENGTH = 243;
		private static int CURL_STATE_LENGTH = CURL_HASH_LENGTH * 3;

		private static long HIGH_BITS = unchecked((long) 0b11111111_11111111_11111111_11111111_11111111_11111111_11111111_11111111L );
		private static long LOW_BITS = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L;

		private volatile State state;
		private Object syncObj = new Object();

		public void cancel()
		{
			lock (syncObj)
			{
				state = State.CANCELLED;
				//syncObj.notifyAll();
			}
		}

		public bool search(int[] transactionTrits, int minWeightMagnitude, int numberOfThreads)
		{

			if (transactionTrits.Length != TRANSACTION_LENGTH)
			{
				throw new Exception(
					"Invalid transaction trits length: " + transactionTrits.Length);
			}
			if (minWeightMagnitude < 0 || minWeightMagnitude > CURL_HASH_LENGTH)
			{
				throw new Exception("Invalid min weight magnitude: " + minWeightMagnitude);
			}

			lock (syncObj)
			{
				state = State.RUNNING;
			}

			long[] midCurlStateLow = new long[CURL_STATE_LENGTH], midCurlStateHigh = new long[CURL_STATE_LENGTH];

			{
				for (int i = CURL_HASH_LENGTH; i < CURL_STATE_LENGTH; i++)
				{
					midCurlStateLow[i] = HIGH_BITS;
					midCurlStateHigh[i] = HIGH_BITS;
				}

				int offset = 0;
				long[] curlScratchpadLow = new long[CURL_STATE_LENGTH], curlScratchpadHigh = new long[CURL_STATE_LENGTH];
				for (int i = (TRANSACTION_LENGTH - CURL_HASH_LENGTH) / CURL_HASH_LENGTH; i-- > 0;)
				{

					for (int j = 0; j < CURL_HASH_LENGTH; j++)
					{

						switch (transactionTrits[offset++])
						{
							case 0:
								{
									midCurlStateLow[j] = HIGH_BITS;
									midCurlStateHigh[j] = HIGH_BITS;

								}
								break;

							case 1:
								{
									midCurlStateLow[j] = LOW_BITS;
									midCurlStateHigh[j] = HIGH_BITS;
								}
								break;

							default:
								{
									midCurlStateLow[j] = HIGH_BITS;
									midCurlStateHigh[j] = LOW_BITS;
								}
								break;
						}
					}

					transform(midCurlStateLow, midCurlStateHigh, curlScratchpadLow, curlScratchpadHigh);
				}

				for (int i = 0; i < 162; i++)
				{

					switch (transactionTrits[offset++])
					{

						case 0:
							{

								midCurlStateLow[i] = unchecked((long)0b1111111111111111111111111111111111111111111111111111111111111111L);
								midCurlStateHigh[i] = unchecked((long)0b1111111111111111111111111111111111111111111111111111111111111111L);

							}
							break;

						case 1:
							{

								midCurlStateLow[i] = 0b0000000000000000000000000000000000000000000000000000000000000000L;
								midCurlStateHigh[i] = unchecked((long)0b1111111111111111111111111111111111111111111111111111111111111111L);

							}
							break;

						default:
							{

								midCurlStateLow[i] = unchecked((long)0b1111111111111111111111111111111111111111111111111111111111111111L);
								midCurlStateHigh[i] = 0b0000000000000000000000000000000000000000000000000000000000000000L;
							}
							break;
					}
				}

				midCurlStateLow[162 + 0] = unchecked((long)0b1101101101101101101101101101101101101101101101101101101101101101L);
				midCurlStateHigh[162 + 0] = unchecked((long)0b1011011011011011011011011011011011011011011011011011011011011011L);
				midCurlStateLow[162 + 1] = unchecked((long)0b1111000111111000111111000111111000111111000111111000111111000111L);
				midCurlStateHigh[162 + 1] = unchecked((long)0b1000111111000111111000111111000111111000111111000111111000111111L);
				midCurlStateLow[162 + 2] = 0b0111111111111111111000000000111111111111111111000000000111111111L;
				midCurlStateHigh[162 + 2] = unchecked((long)0b1111111111000000000111111111111111111000000000111111111111111111L);
				midCurlStateLow[162 + 3] = unchecked((long)0b1111111111000000000000000000000000000111111111111111111111111111L);
				midCurlStateHigh[162 + 3] = 0b0000000000111111111111111111111111111111111111111111111111111111L;

			}

			if (numberOfThreads <= 0)
			{
				numberOfThreads = Math.Max(Environment.ProcessorCount - 1, 1);
			}

			Thread[] workers = new Thread[numberOfThreads];

			while (numberOfThreads-- > 0)
			{

				int threadIndex = numberOfThreads;
				Thread worker = new Thread(() =>
				{

					long[] midCurlStateCopyLow = new long[CURL_STATE_LENGTH], midCurlStateCopyHigh = new long[CURL_STATE_LENGTH];
					Array.Copy(midCurlStateLow, 0, midCurlStateCopyLow, 0, CURL_STATE_LENGTH);
					Array.Copy(midCurlStateHigh, 0, midCurlStateCopyHigh, 0, CURL_STATE_LENGTH);
					for (int i = threadIndex; i-- > 0;)
					{
						increment(midCurlStateCopyLow, midCurlStateCopyHigh, 162 + CURL_HASH_LENGTH / 9,
								162 + (CURL_HASH_LENGTH / 9) * 2);

					}

					long[] curlStateLow = new long[CURL_STATE_LENGTH], curlStateHigh = new long[CURL_STATE_LENGTH];
					long[] curlScratchpadLow = new long[CURL_STATE_LENGTH], curlScratchpadHigh = new long[CURL_STATE_LENGTH];
					long mask, outMask = 1;
					while (state == State.RUNNING)
					{

						increment(midCurlStateCopyLow, midCurlStateCopyHigh, 162 + (CURL_HASH_LENGTH / 9) * 2,
								CURL_HASH_LENGTH);

						Array.Copy(midCurlStateCopyLow, 0, curlStateLow, 0, CURL_STATE_LENGTH);
						Array.Copy(midCurlStateCopyHigh, 0, curlStateHigh, 0, CURL_STATE_LENGTH);
						transform(curlStateLow, curlStateHigh, curlScratchpadLow, curlScratchpadHigh);

						mask = HIGH_BITS;
						for (int i = minWeightMagnitude; i-- > 0;)
						{
							mask &= ~(curlStateLow[CURL_HASH_LENGTH - 1 - i] ^ curlStateHigh[
								CURL_HASH_LENGTH - 1 - i]);
							if (mask == 0)
							{
								break;
							}
						}
						if (mask == 0)
						{
							continue;
						}

						lock (syncObj)
						{
							if (state == State.RUNNING)
							{
								state = State.COMPLETED;
								while ((outMask & mask) == 0)
								{
									outMask <<= 1;
								}
								for (int i = 0; i < CURL_HASH_LENGTH; i++)
								{
									transactionTrits[TRANSACTION_LENGTH - CURL_HASH_LENGTH + i] =
										(midCurlStateCopyLow[i] & outMask) == 0 ? 1
											: (midCurlStateCopyHigh[i] & outMask) == 0 ? -1 : 0;
								}
								//syncObj.notifyAll();
							}
						}
						break;
					}
				});
				workers[threadIndex] = worker;
				worker.Start();
			}

			try
			{

				lock (syncObj)
				{
					if (state == State.RUNNING)
					{
						//syncObj.wait();
					}
				}
			}
			catch (Exception e)
			{

				lock (syncObj)
				{
					state = State.CANCELLED;
				}
			}

			foreach (Thread worker in workers)
			{
				try
				{
					worker.Join();
				}
				catch (Exception e)
				{

					lock (syncObj)
					{
						state = State.CANCELLED;
					}
				}
			}

			return state == State.COMPLETED;
		}

		private static void transform(long[] curlStateLow, long[] curlStateHigh, long[] curlScratchpadLow, long[] curlScratchpadHigh)
		{

			int curlScratchpadIndex = 0;
			for (int round = 0; round < Curl.NUMBER_OF_ROUNDSP81; round++)
			{
				Array.Copy(curlStateLow, 0, curlScratchpadLow, 0, CURL_STATE_LENGTH);
				Array.Copy(curlStateHigh, 0, curlScratchpadHigh, 0, CURL_STATE_LENGTH);

				for (int curlStateIndex = 0; curlStateIndex < CURL_STATE_LENGTH; curlStateIndex++)
				{
					long alpha = curlScratchpadLow[curlScratchpadIndex];
					long beta = curlScratchpadHigh[curlScratchpadIndex];
					if (curlScratchpadIndex < 365)
					{
						curlScratchpadIndex += 364;
					}
					else
					{
						curlScratchpadIndex += -365;
					}
					long gamma = curlScratchpadHigh[curlScratchpadIndex];
					long delta = (alpha | (~gamma)) & (curlScratchpadLow[curlScratchpadIndex] ^ beta);

					curlStateLow[curlStateIndex] = ~delta;
					curlStateHigh[curlStateIndex] = (alpha ^ gamma) | delta;
				}
			}
		}

		private static void increment(long[] midCurlStateCopyLow,
			 long[] midCurlStateCopyHigh, int fromIndex, int toIndex)
		{

			for (int i = fromIndex; i < toIndex; i++)
			{
				if (midCurlStateCopyLow[i] == LOW_BITS)
				{
					midCurlStateCopyLow[i] = HIGH_BITS;
					midCurlStateCopyHigh[i] = LOW_BITS;
				}
				else
				{
					if (midCurlStateCopyHigh[i] == LOW_BITS)
					{
						midCurlStateCopyHigh[i] = HIGH_BITS;
					}
					else
					{
						midCurlStateCopyLow[i] = LOW_BITS;
					}
					break;
				}
			}
		}
	}
}

