using System;
using System.Text;

namespace IotaNet.Iri.Utils
{
	public class Converter
	{
		public static int RADIX = 3;
		public static int BYTE_RADIX = 256;
		public static int MAX_TRIT_VALUE = (RADIX - 1) / 2, MIN_TRIT_VALUE = -MAX_TRIT_VALUE;

		public static int NUMBER_OF_TRITS_IN_A_BYTE = 5;
		public static int NUMBER_OF_TRITS_IN_A_TRYTE = 3;

		static int[][] BYTE_TO_TRITS_MAPPINGS = new int[243][];
		static int[][] TRYTE_TO_TRITS_MAPPINGS = new int[27][];

		public static int HIGH_INTEGER_BITS = unchecked((int)0xFFFFFFFF);
		public static long HIGH_LONG_BITS = unchecked((long)0xFFFFFFFFFFFFFFFF);

		public static String TRYTE_ALPHABET = "9ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static int MIN_TRYTE_VALUE = -13, MAX_TRYTE_VALUE = 13;

		public Converter()
		{
			int[] trits = new int[NUMBER_OF_TRITS_IN_A_BYTE];

			for (int i = 0; i < 243; i++)
			{
				BYTE_TO_TRITS_MAPPINGS[i] = new int[NUMBER_OF_TRITS_IN_A_BYTE];

				Array.Copy(trits, BYTE_TO_TRITS_MAPPINGS[i], NUMBER_OF_TRITS_IN_A_BYTE);

				increment(trits, NUMBER_OF_TRITS_IN_A_BYTE);
			}

			for (int i = 0; i < 27; i++)
			{
				TRYTE_TO_TRITS_MAPPINGS[i] = new int[NUMBER_OF_TRITS_IN_A_TRYTE];

				Array.Copy(trits, TRYTE_TO_TRITS_MAPPINGS[i], NUMBER_OF_TRITS_IN_A_TRYTE);

				increment(trits, NUMBER_OF_TRITS_IN_A_TRYTE);
			}
		}

		public static long longValue(int[] trits, int offset, int size)
		{

			long value = 0;
			for (int i = size; i-- > 0;)
			{
				value = value * RADIX + trits[offset + i];
			}
			return value;
		}

		public static byte[] bytes(int[] trits, int offset, int size)
		{

			byte[] bytes = new byte[(size + NUMBER_OF_TRITS_IN_A_BYTE - 1) / NUMBER_OF_TRITS_IN_A_BYTE];
			for (int i = 0; i < bytes.Length; i++)
			{

				int value = 0;
				for (int j = (size - i * NUMBER_OF_TRITS_IN_A_BYTE) < 5 ? (size - i * NUMBER_OF_TRITS_IN_A_BYTE) : NUMBER_OF_TRITS_IN_A_BYTE; j-- > 0;)
				{
					value = value * RADIX + trits[offset + i * NUMBER_OF_TRITS_IN_A_BYTE + j];
				}
				bytes[i] = (byte)value;
			}

			return bytes;
		}

		public static byte[] bytes(int[] trits)
		{
			return bytes(trits, 0, trits.Length);
		}

		public static void getTrits(byte[] bytes, int[] trits)
		{

			int offset = 0;
			for (int i = 0; i < bytes.Length && offset < trits.Length; i++)
			{
				Array.Copy(BYTE_TO_TRITS_MAPPINGS[bytes[i] < 0 ? (bytes[i] + BYTE_TO_TRITS_MAPPINGS.Length) : bytes[i]], 0, trits, offset, trits.Length - offset < NUMBER_OF_TRITS_IN_A_BYTE ? (trits.Length - offset) : NUMBER_OF_TRITS_IN_A_BYTE);
				offset += NUMBER_OF_TRITS_IN_A_BYTE;
			}
			while (offset < trits.Length)
			{
				trits[offset++] = 0;
			}
		}

		public static int[] trits(String trytes)
		{
			int[] trits = new int[trytes.Length * NUMBER_OF_TRITS_IN_A_TRYTE];
			for (int i = 0; i < trytes.Length; i++)
			{
				Array.Copy(TRYTE_TO_TRITS_MAPPINGS[TRYTE_ALPHABET.IndexOf(trytes[i])], 0, trits, i * NUMBER_OF_TRITS_IN_A_TRYTE, NUMBER_OF_TRITS_IN_A_TRYTE);
			}
			return trits;
		}

		public static void copyTrits(long value, int[] destination, int offset, int size)
		{

			long absoluteValue = value < 0 ? -value : value;
			for (int i = 0; i < size; i++)
			{

				int remainder = (int)(absoluteValue % RADIX);
				absoluteValue /= RADIX;
				if (remainder > MAX_TRIT_VALUE)
				{

					remainder = MIN_TRIT_VALUE;
					absoluteValue++;
				}
				destination[offset + i] = remainder;
			}

			if (value < 0)
			{
				for (int i = 0; i < size; i++)
				{
					destination[offset + i] = -destination[offset + i];
				}
			}
		}


		public static String trytes(int[] trits, int offset, int size)
		{

			var trytes = new StringBuilder();
			for (int i = 0; i < (size + NUMBER_OF_TRITS_IN_A_TRYTE - 1) / NUMBER_OF_TRITS_IN_A_TRYTE; i++)
			{
				int j = trits[offset + i * 3] + trits[offset + i * 3 + 1] * 3 + trits[offset + i * 3 + 2] * 9;
				if (j < 0)
				{
					j += TRYTE_ALPHABET.Length;
				}
				trytes.Append(TRYTE_ALPHABET[j]);
			}
			return trytes.ToString();
		}

		public static String trytes(int[] trits)
		{
			return trytes(trits, 0, trits.Length);
		}

		public static int tryteValue(int[] trits, int offset)
		{
			return trits[offset] + trits[offset + 1] * 3 + trits[offset + 2] * 9;
		}

		public static Tuple<int[], int[]> intPair(int[] trits)
		{
			int[] low = new int[trits.Length];
			int[] hi = new int[trits.Length];
			for (int i = 0; i < trits.Length; i++)
			{
				low[i] = trits[i] != 1 ? HIGH_INTEGER_BITS : 0;
				hi[i] = trits[i] != -1 ? HIGH_INTEGER_BITS : 0;
			}
			return new Tuple<int[], int[]>(low, hi);
		}

		public static Tuple<long[], long[]> longPair(int[] trits)
		{
			long[] low = new long[trits.Length];
			long[] hi = new long[trits.Length];
			for (int i = 0; i < trits.Length; i++)
			{
				low[i] = trits[i] != 1 ? HIGH_LONG_BITS : 0;
				hi[i] = trits[i] != -1 ? HIGH_LONG_BITS : 0;
			}
			return new Tuple<long[], long[]>(low, hi);
		}

		public static void shiftPair(Tuple<long[], long[]> source, Tuple<long[], long[]> dest)
		{
			if (source.Item1.Length == dest.Item1.Length && source.Item2.Length == dest.Item2.Length)
			{
				for (int i = 0; i < dest.Item1.Length; i++)
				{
					dest.Item1[i] <<= 1;
					dest.Item1[i] |= source.Item1[i] & 1;
				}
				for (int i = 0; i < dest.Item2.Length; i++)
				{
					dest.Item2[i] <<= 1;
					dest.Item2[i] |= source.Item2[i] & 1;
				}
			}
		}

		public static int[] trits(Tuple<long[], long[]> pair, int bitIndex)
		{
			int length;
			if (pair.Item1.Length == pair.Item2.Length || pair.Item1.Length < pair.Item2.Length)
			{
				length = pair.Item1.Length;
			}
			else
			{
				length = pair.Item2.Length;
			}
			int[] trits = new int[length];
			long low;
			long hi;
			int mask = 1 << bitIndex;
			for (int i = 0; i < length; i++)
			{
				low = pair.Item1[i] & mask;
				hi = pair.Item2[i] & mask;
				if (hi == low)
				{
					trits[i] = 0;
				}
				else if (low == 0)
				{
					trits[i] = 1;
				}
				else if (hi == 0)
				{
					trits[i] = -1;
				}
			}
			return trits;
		}

		public static int[] trits(long[] low, long[] hi)
		{
			int[] trits = new int[low.Length];
			for (int i = 0; i < trits.Length; i++)
			{
				trits[i] = low[i] == 0 ? 1 : hi[i] == 0 ? -1 : 0;
			}
			return trits;
		}

		public static int[] trits(int[] low, int[] hi)
		{
			int[] trits = new int[low.Length];
			for (int i = 0; i < trits.Length; i++)
			{
				trits[i] = low[i] == 0 ? 1 : hi[i] == 0 ? -1 : 0;
			}
			return trits;
		}

		private static void increment(int[] trits, int size)
		{
			for (int i = 0; i < size; i++)
			{
				if (++trits[i] > Converter.MAX_TRIT_VALUE)
				{
					trits[i] = Converter.MIN_TRIT_VALUE;
				}
				else
				{
					break;
				}
			}
		}

		public static String asciiToTrytes(String input)
		{
			var sb = new StringBuilder(80);
			for (int i = 0; i < input.Length; i++)
			{
				int asciiValue = input[i];
				// If not recognizable ASCII character, return null
				if (asciiValue > 255)
				{
					return null;
				}
				int firstValue = asciiValue % 27;
				int secondValue = (asciiValue - firstValue) / 27;
				sb.Append(TRYTE_ALPHABET[firstValue]);
				sb.Append(TRYTE_ALPHABET[secondValue]);
			}
			return sb.ToString();
		}
	}
}

