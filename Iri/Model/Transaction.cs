using System;
using System.IO;
using System.Text;

using IotaNet.Iri.Helpers;
using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;
using IotaNet.Iri.Controllers;

namespace IotaNet.Iri.Model
{
	/**
	 * Created by paul on 3/2/17 for iri.
	 */
	public class Transaction : Persistable
	{

		public static int SIZE = 1604;

		public byte[] bytes;

		public Hash address;
		public Hash bundle;
		public Hash trunk;
		public Hash branch;
		public Hash obsoleteTag;
		public long value;
		public long currentIndex;
		public long lastIndex;
		public long timestamp;

		public Hash tag;
		public long attachmentTimestamp;
		public long attachmentTimestampLowerBound;
		public long attachmentTimestampUpperBound;

		public int validity = 0;
		public int type = TransactionViewModel.PREFILLED_SLOT;
		public long arrivalTime = 0;

		//public boolean confirmed = false;
		public bool parsed = false;
		public bool solid = false;
		public long height = 0;
		public String sender = "";
		public int snapshot;

		public byte[] Bytes()
		{
			return bytes;
		}

		public void read(byte[] bytes)
		{
			if (bytes != null)
			{
				this.bytes = new byte[SIZE];
				Array.Copy(bytes, 0, this.bytes, 0, SIZE);
				this.type = TransactionViewModel.FILLED_SLOT;
			}
		}

		public byte[] metadata()
		{
			int allocateSize =
					Hash.SIZE_IN_BYTES * 6 + //address,bundle,trunk,branch,obsoleteTag,tag
							sizeof(long) * 9 + //value,currentIndex,lastIndex,timestamp,attachmentTimestampLowerBound,attachmentTimestampUpperBound,arrivalTime,height
							sizeof(int) * 3 + //validity,type,snapshot
							1 + //solid
							sender.Length; //sender
			var buffer = new MemoryStream(allocateSize);
			buffer.put(address.bytes());
			buffer.put(bundle.bytes());
			buffer.put(trunk.bytes());
			buffer.put(branch.bytes());
			buffer.put(obsoleteTag.bytes());
			buffer.put(Serializer.serialize(value));
			buffer.put(Serializer.serialize(currentIndex));
			buffer.put(Serializer.serialize(lastIndex));
			buffer.put(Serializer.serialize(timestamp));

			buffer.put(tag.bytes());
			buffer.put(Serializer.serialize(attachmentTimestamp));
			buffer.put(Serializer.serialize(attachmentTimestampLowerBound));
			buffer.put(Serializer.serialize(attachmentTimestampUpperBound));

			buffer.put(Serializer.serialize(validity));
			buffer.put(Serializer.serialize(type));
			buffer.put(Serializer.serialize(arrivalTime));
			buffer.put(Serializer.serialize(height));
			//buffer.put((byte) (confirmed ? 1:0));
			buffer.put(new byte[] { (byte)(solid ? 1 : 0) });
			buffer.put(Serializer.serialize(snapshot));
			buffer.put(Encoding.ASCII.GetBytes(sender));
			return buffer.GetBuffer();
		}

		public void readMetadata(byte[] bytes)
		{
			int i = 0;
			if (bytes != null)
			{
				address = new Hash(bytes, i, Hash.SIZE_IN_BYTES);
				i += Hash.SIZE_IN_BYTES;
				bundle = new Hash(bytes, i, Hash.SIZE_IN_BYTES);
				i += Hash.SIZE_IN_BYTES;
				trunk = new Hash(bytes, i, Hash.SIZE_IN_BYTES);
				i += Hash.SIZE_IN_BYTES;
				branch = new Hash(bytes, i, Hash.SIZE_IN_BYTES);
				i += Hash.SIZE_IN_BYTES;
				obsoleteTag = new Hash(bytes, i, Hash.SIZE_IN_BYTES);
				i += Hash.SIZE_IN_BYTES;
				value = Serializer.getLong(bytes, i);
				i += sizeof(long);
				currentIndex = Serializer.getLong(bytes, i);
				i += sizeof(long);
				lastIndex = Serializer.getLong(bytes, i);
				i += sizeof(long);
				timestamp = Serializer.getLong(bytes, i);
				i += sizeof(long);

				tag = new Hash(bytes, i, Hash.SIZE_IN_BYTES);
				i += Hash.SIZE_IN_BYTES;
				attachmentTimestamp = Serializer.getLong(bytes, i);
				i += sizeof(long);
				attachmentTimestampLowerBound = Serializer.getLong(bytes, i);
				i += sizeof(long);
				attachmentTimestampUpperBound = Serializer.getLong(bytes, i);
				i += sizeof(long);

				validity = Serializer.getInteger(bytes, i);
				i += sizeof(int);
				type = Serializer.getInteger(bytes, i);
				i += sizeof(int);
				arrivalTime = Serializer.getLong(bytes, i);
				i += sizeof(long);
				height = Serializer.getLong(bytes, i);
				i += i += sizeof(long);
				;
				/*
				confirmed = bytes[i] == 1;
				i++;
				*/
				solid = bytes[i] == 1;
				i++;
				snapshot = Serializer.getInteger(bytes, i);
				i += sizeof(int);
				byte[] senderBytes = new byte[bytes.Length - i];
				if (senderBytes.Length != 0)
				{
					Array.Copy(bytes, i, senderBytes, 0, senderBytes.Length);
				}
				sender = Encoding.ASCII.GetString(senderBytes);
				parsed = true;
			}
		}

		public bool merge()
		{
			return false;
		}
	}
}
