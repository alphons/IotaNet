
using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 5/6/17.
	 */
	public class IntegerIndex : Indexable
	{
		int value;

		public IntegerIndex(int value)
		{
			this.value = value;
		}

		public IntegerIndex() { }

		public int getValue()
		{
			return value;
		}

		public byte[] bytes()
		{
			return Serializer.serialize(value);
		}

		public void read(byte[] bytes)
		{
			this.value = Serializer.getInteger(bytes);
		}

		public Indexable incremented()
		{
			return new IntegerIndex(value + 1);
		}

		public Indexable decremented()
		{
			return new IntegerIndex(value - 1);
		}

		public int CompareTo(Indexable o)
		{
			IntegerIndex i = new IntegerIndex(Serializer.getInteger(o.bytes()));
			return value - ((IntegerIndex)o).value;
		}
	}
}
