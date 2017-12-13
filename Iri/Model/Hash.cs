using System;

using IotaNet.Iri.Hash;
using IotaNet.Iri.Helpers;
using IotaNet.Iri.Utils;
using IotaNet.Iri.Storage;

namespace IotaNet.Iri.Model
{
	public class Hash
	{
		public static int SIZE_IN_TRITS = 243;
		public static int SIZE_IN_BYTES = 49;

		public static Hash NULL_HASH = new Hash(new int[Curl.HASH_LENGTH]);

		private byte[] _bytes;
		private int[] trits;
		private int hashCode;

		// constructors' bill

		public Hash(byte[] bytes, int offset, int size)
		{
			fullRead(bytes, offset, size);
		}

		public Hash() { }

		public Hash(byte[] bytes) : this(bytes, 0, SIZE_IN_BYTES)
		{
		}

		public Hash(int[] trits, int offset)
		{
			this.trits = new int[SIZE_IN_TRITS];
			Array.Copy(trits, offset, this.trits, 0, SIZE_IN_TRITS);
			//this(Converter.bytes(trits, offset, trits.length));
		}

		public Hash(int[] trits) : this(trits, 0)
		{
		}

		public Hash(String trytes) : this(Converter.trits(trytes))
		{
		}

		//
		/*
		public static Hash calculate(byte[] bytes) {
			return calculate(bytes, SIZE_IN_TRITS, new Curl());
		}
		*/
		public static Hash calculate(SpongeFactory.Mode mode, int[] trits)
		{
			return calculate(trits, 0, trits.Length, SpongeFactory.Create(mode));
		}

		public static Hash calculate(byte[] bytes, int tritsLength, ISponge curl)
		{
			int[] trits = new int[tritsLength];
			Converter.getTrits(bytes, trits);
			return calculate(trits, 0, tritsLength, curl);
		}
		public static Hash calculate(int[] tritsToCalculate, int offset, int length, ISponge curl)
		{
			int[] hashTrits = new int[SIZE_IN_TRITS];
			curl.reset();
			curl.absorb(tritsToCalculate, offset, length);
			curl.squeeze(hashTrits, 0, SIZE_IN_TRITS);
			return new Hash(hashTrits);
		}

		public int trailingZeros()
		{
			int index, zeros;
			int[] trits;
			index = SIZE_IN_TRITS;
			zeros = 0;
			trits = Trits();
			while (index-- > 0 && trits[index] == 0)
			{
				zeros++;
			}
			return zeros;
		}

		public int[] Trits()
		{
			if (trits == null)
			{
				trits = new int[Curl.HASH_LENGTH];
				Converter.getTrits(this._bytes, trits);
			}
			return trits;
		}

		public override bool Equals(Object obj)
		{
			//assert obj instanceof Hash;
			if (obj == null) return false;
			return Arrays.equals(bytes(), ((Hash)obj).bytes());
		}

		public override int GetHashCode()
		{
			if (this._bytes == null)
			{
				bytes();
			}
			return hashCode;
		}

		public override String ToString()
		{
			return Converter.trytes(Trits());
		}

		public byte[] bytes()
		{
			if (this._bytes == null)
			{
				this._bytes = Converter.bytes(trits);
				hashCode = Arrays.hashCode(this._bytes);
			}
			return this._bytes;
		}

		private void fullRead(byte[] bytes, int offset, int size)
		{
			this._bytes = new byte[SIZE_IN_BYTES];
			Array.Copy(bytes, offset, this._bytes, 0, size - offset > bytes.Length ? bytes.Length - offset : size);
			hashCode = Arrays.hashCode(this._bytes);

		}

		public void read(byte[] bytes)
		{
			fullRead(bytes, 0, SIZE_IN_BYTES);
		}

		public Indexable incremented()
		{
			return null;
		}

		public Indexable decremented()
		{
			return null;
		}

		public int compareTo(Indexable indexable)
		{
			Hash hash = new Hash(indexable.bytes());
			if (this.Equals(hash))
			{
				return 0;
			}
			long diff = Converter.longValue(hash.Trits(), 0, SIZE_IN_TRITS) - Converter.longValue(Trits(), 0, SIZE_IN_TRITS);
			if (Math.Abs(diff) > int.MaxValue)
			{
				return diff > 0L ? int.MaxValue : int.MinValue + 1;
			}
			return (int)diff;
		}
	}
}

