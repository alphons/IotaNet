using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IotaNet.Iri.Helpers;

namespace IotaNet.Iri.Hashing
{
	/**
	 * (c) 2016 Come-from-Beyond
	 */
	public class ISS
	{
		public static int NUMBER_OF_FRAGMENT_CHUNKS = 27;
		public static int FRAGMENT_LENGTH = Curl.HASH_LENGTH * NUMBER_OF_FRAGMENT_CHUNKS;
		private static int NUMBER_OF_SECURITY_LEVELS = 3;

		private static int MIN_TRIT_VALUE = -1, MAX_TRIT_VALUE = 1;
		public static int TRYTE_WIDTH = 3;
		private static int MIN_TRYTE_VALUE = -13, MAX_TRYTE_VALUE = 13;
		public static int NORMALIZED_FRAGMENT_LENGTH = Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS;

		public static int[] subseed(SpongeFactory.Mode mode, int[] seed, int index)
		{
			if (index < 0)
			{
				throw new Exception("Invalid subseed index: " + index);
			}

			int[] subseedPreimage = Arrays.copyOf(seed, seed.Length);

			while (index-- > 0)
			{

				for (int i = 0; i < subseedPreimage.Length; i++)
				{

					if (++subseedPreimage[i] > MAX_TRIT_VALUE)
					{
						subseedPreimage[i] = MIN_TRIT_VALUE;
					}
					else
					{
						break;
					}
				}
			}

			int[] subseed = new int[Curl.HASH_LENGTH];

			var hash = SpongeFactory.Create(mode);
			hash.absorb(subseedPreimage, 0, subseedPreimage.Length);
			hash.squeeze(subseed, 0, subseed.Length);
			return subseed;
		}

		public static int[] key(SpongeFactory.Mode mode, int[] subseed, int numberOfFragments)
		{

			if (subseed.Length != Curl.HASH_LENGTH)
			{
				throw new Exception("Invalid subseed length: " + subseed.Length);
			}
			if (numberOfFragments <= 0)
			{
				throw new Exception("Invalid number of key fragments: " + numberOfFragments);
			}

			int[] key = new int[FRAGMENT_LENGTH * numberOfFragments];

			var hash = SpongeFactory.Create(mode);
			hash.absorb(subseed, 0, subseed.Length);
			hash.squeeze(key, 0, key.Length);
			return key;
		}

		public static int[] digests(SpongeFactory.Mode mode, int[] key)
		{

			if (key.Length == 0 || key.Length % FRAGMENT_LENGTH != 0)
			{
				throw new Exception("Invalid key length: " + key.Length);
			}


			int[] digests = new int[key.Length / FRAGMENT_LENGTH * Curl.HASH_LENGTH];
			var hash = SpongeFactory.Create(mode);

			for (int i = 0; i < key.Length / FRAGMENT_LENGTH; i++)
			{

				int[] buffer = Arrays.copyOfRange(key, i * FRAGMENT_LENGTH, (i + 1) * FRAGMENT_LENGTH);
				for (int j = 0; j < NUMBER_OF_FRAGMENT_CHUNKS; j++)
				{

					for (int k = MAX_TRYTE_VALUE - MIN_TRYTE_VALUE; k-- > 0;)
					{
						hash.reset();
						hash.absorb(buffer, j * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
						hash.squeeze(buffer, j * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
					}
				}
				hash.reset();
				hash.absorb(buffer, 0, buffer.Length);
				hash.squeeze(digests, i * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
			}

			return digests;
		}

		public static int[] address(SpongeFactory.Mode mode, int[] digests)
		{

			if (digests.Length == 0 || digests.Length % Curl.HASH_LENGTH != 0)
			{
				throw new Exception("Invalid digests length: " + digests.Length);
			}

			int[] address = new int[Curl.HASH_LENGTH];

			var hash = SpongeFactory.Create(mode);
			hash.absorb(digests, 0, digests.Length);
			hash.squeeze(address, 0, address.Length);

			return address;
		}

		public static int[] normalizedBundle(int[] bundle)
		{

			if (bundle.Length != Curl.HASH_LENGTH)
			{
				throw new Exception("Invalid bundleValidator length: " + bundle.Length);
			}

			int[] normalizedBundle = new int[Curl.HASH_LENGTH / TRYTE_WIDTH];

			for (int i = 0; i < NUMBER_OF_SECURITY_LEVELS; i++)
			{

				int sum = 0;
				for (int j = i * (Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS); j < (i + 1) * (Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS); j++)
				{

					normalizedBundle[j] = bundle[j * TRYTE_WIDTH] + bundle[j * TRYTE_WIDTH + 1] * 3 + bundle[j * TRYTE_WIDTH + 2] * 9;
					sum += normalizedBundle[j];
				}
				if (sum > 0)
				{

					while (sum-- > 0)
					{

						for (int j = i * (Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS); j < (i + 1) * (Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS); j++)
						{

							if (normalizedBundle[j] > MIN_TRYTE_VALUE)
							{
								normalizedBundle[j]--;
								break;
							}
						}
					}

				}
				else
				{

					while (sum++ < 0)
					{

						for (int j = i * (Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS); j < (i + 1) * (Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS); j++)
						{

							if (normalizedBundle[j] < MAX_TRYTE_VALUE)
							{
								normalizedBundle[j]++;
								break;
							}
						}
					}
				}
			}

			return normalizedBundle;
		}

		public static int[] signatureFragment(SpongeFactory.Mode mode, int[] normalizedBundleFragment, int[] keyFragment)
		{

			if (normalizedBundleFragment.Length != NORMALIZED_FRAGMENT_LENGTH)
			{
				throw new Exception("Invalid normalized bundleValidator fragment length: " + normalizedBundleFragment.Length);
			}
			if (keyFragment.Length != FRAGMENT_LENGTH)
			{
				throw new Exception("Invalid key fragment length: " + keyFragment.Length);
			}

			var signatureFragment = Arrays.copyOf(keyFragment, keyFragment.Length);

			var hash = SpongeFactory.Create(mode);

			for (int j = 0; j < NUMBER_OF_FRAGMENT_CHUNKS; j++)
			{

				for (int k = MAX_TRYTE_VALUE - normalizedBundleFragment[j]; k-- > 0;)
				{
					hash.reset();
					hash.absorb(signatureFragment, j * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
					hash.squeeze(signatureFragment, j * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
				}
			}

			return signatureFragment;
		}

		public static int[] digest(SpongeFactory.Mode mode, int[] normalizedBundleFragment, int[] signatureFragment)
		{

			if (normalizedBundleFragment.Length != Curl.HASH_LENGTH / TRYTE_WIDTH / NUMBER_OF_SECURITY_LEVELS)
			{
				throw new Exception("Invalid normalized bundleValidator fragment length: " + normalizedBundleFragment.Length);
			}
			if (signatureFragment.Length != FRAGMENT_LENGTH)
			{
				throw new Exception("Invalid signature fragment length: " + signatureFragment.Length);
			}

			int[] digest = new int[Curl.HASH_LENGTH];
			int[] buffer = Arrays.copyOf(signatureFragment, FRAGMENT_LENGTH);
			var hash = SpongeFactory.Create(mode);
			for (int j = 0; j < NUMBER_OF_FRAGMENT_CHUNKS; j++)
			{

				for (int k = normalizedBundleFragment[j] - MIN_TRYTE_VALUE; k-- > 0;)
				{
					hash.reset();
					hash.absorb(buffer, j * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
					hash.squeeze(buffer, j * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
				}
			}
			hash.reset();
			hash.absorb(buffer, 0, buffer.Length);
			hash.squeeze(digest, 0, digest.Length);

			return digest;
		}

		public static int[] getMerkleRoot(SpongeFactory.Mode mode, int[] hash, int[] trits, int offset, int indexIn, int size)
		{
			int index = indexIn;
			var curl = SpongeFactory.Create(mode);
			for (int i = 0; i < size; i++)
			{
				curl.reset();
				if ((index & 1) == 0)
				{
					curl.absorb(hash, 0, hash.Length);
					curl.absorb(trits, offset + i * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
				}
				else
				{
					curl.absorb(trits, offset + i * Curl.HASH_LENGTH, Curl.HASH_LENGTH);
					curl.absorb(hash, 0, hash.Length);
				}
				curl.squeeze(hash, 0, hash.Length);

				index >>= 1;
			}
			if (index != 0)
			{
				return Model.Hash.NULL_HASH.trits();
			}
			return hash;
		}
	}
}
