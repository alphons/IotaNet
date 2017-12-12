using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto.Digests;
using IotaNet.Iri.Helpers;

namespace IotaNet.Iri.Hash
{
	public class Kerl : ISponge
	{
		public static int HASH_LENGTH = 243;
		public static int BIT_HASH_LENGTH = 384;
		public static int BYTE_HASH_LENGTH = BIT_HASH_LENGTH / 8;

		private byte[] byte_state;
		private int[] trit_state;
		private KeccakDigest keccak;

		protected Kerl()
		{
			this.keccak = new KeccakDigest(384);
			this.byte_state = new byte[BYTE_HASH_LENGTH];
			this.trit_state = new int[HASH_LENGTH];
		}

		public void reset()
		{

			this.keccak.Reset();
		}


		public void absorb(int[] trits, int offset, int length)
		{

			if (length % 243 != 0) throw new Exception("Illegal length: " + length);

			do
			{
				//copy trits[offset:offset+length]
				Array.Copy(trits, offset, trit_state, 0, HASH_LENGTH);

				//convert to bits
				trit_state[HASH_LENGTH - 1] = 0;
				byte[] bytes = bytesFromBigInt(bigIntFromTrits(trit_state, 0, HASH_LENGTH), BYTE_HASH_LENGTH);

				//run keccak
				keccak.Update(bytes);
				offset += HASH_LENGTH;

			} while ((length -= HASH_LENGTH) > 0);
		}


		public void squeeze(int[] trits, int offset, int length)
		{
			if (length % 243 != 0) throw new Exception("Illegal length: " + length);

			do
			{

				byte_state = this.keccak.digest();
				//convert to trits
				trit_state = tritsFromBigInt(bigIntFromBytes(byte_state, 0, BYTE_HASH_LENGTH), HASH_LENGTH);

				//copy with offset
				trit_state[HASH_LENGTH - 1] = 0;
				Array.Copy(trit_state, 0, trits, offset, HASH_LENGTH);

				//calculate hash again
				for (int i = byte_state.Length; i-- > 0;)
				{

					byte_state[i] = (byte)(byte_state[i] ^ 0xFF);
				}
				keccak.Update(byte_state);
				offset += HASH_LENGTH;

			} while ((length -= HASH_LENGTH) > 0);
		}

		public static BigInteger bigIntFromTrits(int[] trits, int offset, int size)
		{

			BigInteger value = BigInteger.Zero;

			for (int i = size; i-- > 0;)
			{
				value = value * (new BigInteger(Utils.Converter.RADIX)) + new BigInteger(trits[offset + i]);
			}

			return value;
		}

		public static BigInteger bigIntFromBytes(byte[] bytes, int offset, int size)
		{

			return new BigInteger(Arrays.copyOfRange(bytes, offset, offset + size));
		}

		public static int[] tritsFromBigInt(BigInteger value, int size)
		{

			int[] destination = new int[size];
			BigInteger absoluteValue = value.CompareTo(BigInteger.Zero) < 0 ? value.negate() : value;
			for (int i = 0; i < size; i++)
			{

				BigInteger[] divRemainder = absoluteValue.divideAndRemainder(new BigInteger(Utils.Converter.RADIX));
				int remainder = divRemainder[1].intValue();
				absoluteValue = divRemainder[0];

				if (remainder > Utils.Converter.MAX_TRIT_VALUE)
				{
					remainder = Utils.Converter.MIN_TRIT_VALUE;
					absoluteValue = absoluteValue + BigInteger.One;
				}
				destination[i] = remainder;
			}

			if (value.CompareTo(BigInteger.Zero) < 0)
			{
				for (int i = 0; i < size; i++)
				{
					destination[i] = -destination[i];
				}
			}

			return destination;
		}

		public static byte[] bytesFromBigInt(BigInteger value, int size)
		{

			byte[] result = new byte[BYTE_HASH_LENGTH];

			byte[] bytes = value.ToByteArray();
			int i = 0;
			while (i + bytes.Length < BYTE_HASH_LENGTH)
			{

				result[i++] = (byte)(bytes[0] < 0 ? -1 : 0);
			}
			for (int j = bytes.Length; j-- > 0;)
			{

				result[i++] = bytes[bytes.Length - 1 - j];
			}

			return result;
		}
	}
}
