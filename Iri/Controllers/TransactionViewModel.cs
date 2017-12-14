using System;
using System.Collections.Generic;

using IotaNet.Iri.Model;
using IotaNet.Iri.Storage;
using IotaNet.Iri.Utils;


namespace IotaNet.Iri.Controllers
{
	public class TransactionViewModel
	{

		private Transaction transaction;

		public static int SIZE = 1604;
		private static int TAG_SIZE = 27;

		public static long SUPPLY = 2779530283277761L; // = (3^33 - 1) / 2

		public static int SIGNATURE_MESSAGE_FRAGMENT_TRINARY_OFFSET = 0, SIGNATURE_MESSAGE_FRAGMENT_TRINARY_SIZE = 6561;
		public static int ADDRESS_TRINARY_OFFSET = SIGNATURE_MESSAGE_FRAGMENT_TRINARY_OFFSET + SIGNATURE_MESSAGE_FRAGMENT_TRINARY_SIZE, ADDRESS_TRINARY_SIZE = 243;
		public static int VALUE_TRINARY_OFFSET = ADDRESS_TRINARY_OFFSET + ADDRESS_TRINARY_SIZE, VALUE_TRINARY_SIZE = 81, VALUE_USABLE_TRINARY_SIZE = 33;
		public static int OBSOLETE_TAG_TRINARY_OFFSET = VALUE_TRINARY_OFFSET + VALUE_TRINARY_SIZE, OBSOLETE_TAG_TRINARY_SIZE = 81;
		public static int TIMESTAMP_TRINARY_OFFSET = OBSOLETE_TAG_TRINARY_OFFSET + OBSOLETE_TAG_TRINARY_SIZE, TIMESTAMP_TRINARY_SIZE = 27;
		public static int CURRENT_INDEX_TRINARY_OFFSET = TIMESTAMP_TRINARY_OFFSET + TIMESTAMP_TRINARY_SIZE, CURRENT_INDEX_TRINARY_SIZE = 27;
		public static int LAST_INDEX_TRINARY_OFFSET = CURRENT_INDEX_TRINARY_OFFSET + CURRENT_INDEX_TRINARY_SIZE, LAST_INDEX_TRINARY_SIZE = 27;
		public static int BUNDLE_TRINARY_OFFSET = LAST_INDEX_TRINARY_OFFSET + LAST_INDEX_TRINARY_SIZE, BUNDLE_TRINARY_SIZE = 243;
		public static int TRUNK_TRANSACTION_TRINARY_OFFSET = BUNDLE_TRINARY_OFFSET + BUNDLE_TRINARY_SIZE, TRUNK_TRANSACTION_TRINARY_SIZE = 243;
		public static int BRANCH_TRANSACTION_TRINARY_OFFSET = TRUNK_TRANSACTION_TRINARY_OFFSET + TRUNK_TRANSACTION_TRINARY_SIZE, BRANCH_TRANSACTION_TRINARY_SIZE = 243;

		public static int TAG_TRINARY_OFFSET = BRANCH_TRANSACTION_TRINARY_OFFSET + BRANCH_TRANSACTION_TRINARY_SIZE, TAG_TRINARY_SIZE = 81;
		public static int ATTACHMENT_TIMESTAMP_TRINARY_OFFSET = TAG_TRINARY_OFFSET + TAG_TRINARY_SIZE, ATTACHMENT_TIMESTAMP_TRINARY_SIZE = 27;
		public static int ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_OFFSET = ATTACHMENT_TIMESTAMP_TRINARY_OFFSET + ATTACHMENT_TIMESTAMP_TRINARY_SIZE, ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_SIZE = 27;
		public static int ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_OFFSET = ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_OFFSET + ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_SIZE, ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_SIZE = 27;
		private static int NONCE_TRINARY_OFFSET = ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_OFFSET + ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_SIZE, NONCE_TRINARY_SIZE = 81;

		public static int TRINARY_SIZE = NONCE_TRINARY_OFFSET + NONCE_TRINARY_SIZE;

		public static int ESSENCE_TRINARY_OFFSET = ADDRESS_TRINARY_OFFSET, ESSENCE_TRINARY_SIZE = ADDRESS_TRINARY_SIZE + VALUE_TRINARY_SIZE + OBSOLETE_TAG_TRINARY_SIZE + TIMESTAMP_TRINARY_SIZE + CURRENT_INDEX_TRINARY_SIZE + LAST_INDEX_TRINARY_SIZE;


		private AddressViewModel address;
		private ApproveeViewModel approovers;
		private TransactionViewModel trunk;
		private TransactionViewModel branch;
		private Model.Hash hash;


		public static int GROUP = 0; // transactions GROUP means that's it's a non-leaf node (leafs store transaction value)
		public static int PREFILLED_SLOT = 1; // means that we know only hash of the tx, the rest is unknown yet: only another tx references that hash
		public static int FILLED_SLOT = -1; //  knows the hash only coz another tx references that hash

		private int[] trits;
		public int weightMagnitude;

		public static void fillMetadata(Tangle tangle, TransactionViewModel transactionViewModel)
		{
			if (transactionViewModel.getHash().equals(Model.Hash.NULL_HASH)) { return; }
			if (transactionViewModel.getType() == FILLED_SLOT && !transactionViewModel.transaction.parsed)
			{
				tangle.saveBatch(transactionViewModel.getMetadataSaveBatch());
			}
		}

		public static TransactionViewModel find(Tangle tangle, byte[] hash)
		{
			TransactionViewModel transactionViewModel = new TransactionViewModel((Transaction)tangle.find(new Transaction(), hash), new Model.Hash(hash));

			fillMetadata(tangle, transactionViewModel);
			return transactionViewModel;
		}

		public static TransactionViewModel fromHash(Tangle tangle, Model.Hash hash)
		{
			TransactionViewModel transactionViewModel = new TransactionViewModel((Transaction)tangle.load(new Transaction(), hash), hash);

			fillMetadata(tangle, transactionViewModel);
			return transactionViewModel;
		}

		public static bool mightExist(Tangle tangle, Model.Hash hash)
		{
			return tangle.maybeHas(new Transaction(), hash);
		}

		public TransactionViewModel(Transaction transaction, Model.Hash hash)
		{
			this.transaction = transaction == null || transaction._bytes == null ? new Transaction() : transaction;
			this.hash = hash == null ? Model.Hash.NULL_HASH : hash;
			weightMagnitude = this.hash.trailingZeros();
		}

		public TransactionViewModel(int[] trits, Model.Hash hash)
		{
			transaction = new Transaction();
			this.trits = new int[trits.Length];
			Array.Copy(trits, 0, this.trits, 0, trits.Length);
			transaction._bytes = Converter.bytes(trits);
			this.hash = hash;

			transaction.type = FILLED_SLOT;

			weightMagnitude = this.hash.trailingZeros();
			transaction.validity = 0;
			transaction.arrivalTime = 0;
		}


		public TransactionViewModel(byte[] bytes, Model.Hash hash)
		{
			transaction = new Transaction();
			transaction._bytes = new byte[SIZE];
			Array.Copy(bytes, 0, transaction._bytes, 0, SIZE);

			this.hash = hash;

			weightMagnitude = this.hash.trailingZeros();

			transaction.type = FILLED_SLOT;

		}

		public static int getNumberOfStoredTransactions(Tangle tangle)
		{
			return (int)tangle.getCount(new Transaction());
		}

		public bool update(Tangle tangle, String item)
		{
			getAddressHash();
			getTrunkTransactionHash();
			getBranchTransactionHash();
			getBundleHash();
			getTagValue();
			if (hash.Equals(Model.Hash.NULL_HASH)) {
				return false;
			}
			return tangle.update(transaction, hash, item);
		}

		public TransactionViewModel getBranchTransaction(Tangle tangle)
		{
			if (branch == null) {
				branch = TransactionViewModel.fromHash(tangle, getBranchTransactionHash());
			}
			return branch;
		}

		public TransactionViewModel getTrunkTransaction(Tangle tangle)
		{
			if (trunk == null) {
				trunk = TransactionViewModel.fromHash(tangle, getTrunkTransactionHash());
			}
			return trunk;
		}

		public static int[] Trits(byte[] transactionBytes)
		{
			int[] trits;
			trits = new int[TRINARY_SIZE];
			if (transactionBytes != null)
			{
				Converter.getTrits(transactionBytes, trits);
			}
			return trits;
		}

		public int[] Trits()
		{
			return (trits == null) ? (trits = Trits(transaction._bytes)) : trits;
		}

		public void delete(Tangle tangle)
		{
			tangle.delete(new Transaction(), hash);
		}

		public List<Pair<Indexable, Persistable>> getMetadataSaveBatch()
		{
			List<Pair<Indexable, Persistable>> hashesList = new List<Pair<Indexable, Persistable>>();
			hashesList.add(new Pair<>(getAddressHash(), new Address(hash)));
			hashesList.add(new Pair<>(getBundleHash(), new Bundle(hash)));
			hashesList.add(new Pair<>(getBranchTransactionHash(), new Approvee(hash)));
			hashesList.add(new Pair<>(getTrunkTransactionHash(), new Approvee(hash)));
			hashesList.add(new Pair<>(getObsoleteTagValue(), new Tag(hash)));

			setAttachmentData();

			setMetadata();
			return hashesList;
		}

		public List<Pair<Indexable, Persistable>> getSaveBatch()
		{
			List<Pair<Indexable, Persistable>> hashesList = new List<Pair<Indexable, Persistable>>();
			hashesList.addAll(getMetadataSaveBatch());

			getBytes();
			hashesList.Add(new Pair<Model.Hash, Transaction>(hash, transaction));
			return hashesList;
		}


		public static TransactionViewModel first(Tangle tangle)
		{
			Pair<Indexable, Persistable> transactionPair = tangle.getFirst(Transaction.class, Hash.class);
			if(transactionPair != null && transactionPair.hi != null) 
			{
		        return new TransactionViewModel((Transaction) transactionPair.hi, (Hash) transactionPair.low);
			}
			return null;
		}

		public TransactionViewModel next(Tangle tangle) 
		{
			Pair<Indexable, Persistable> transactionPair = tangle.next(new Transaction(), hash);
			if(transactionPair != null && transactionPair.hi != null)
			{
				return new TransactionViewModel((Transaction) transactionPair.hi, (Hash) transactionPair.low);
			}
			return null;
		}

		public bool store(Tangle tangle)
		{
	if (hash.Equals(Hash.NULL_HASH) || exists(tangle, hash))
	{
		return false;
	}

	List<Pair<Indexable, Persistable>> batch = getSaveBatch();
	if (exists(tangle, hash))
	{
		return false;
	}
	return tangle.saveBatch(batch);
		}

public ApproveeViewModel getApprovers(Tangle tangle)
{
        if(approovers == null)
		{
			approovers = ApproveeViewModel.load(tangle, hash);
		}
			return approovers;
	}

public int getType()
{
	return transaction.type;
}

public void setArrivalTime(long time)
{
	transaction.arrivalTime = time;
}

public long getArrivalTime()
{
	return transaction.arrivalTime;
}

public byte[] getBytes()
{
	if (transaction.bytes == null || transaction.bytes.Length != SIZE)
	{
		transaction.bytes = trits == null ? new byte[SIZE] : Converter.bytes(trits());
	}
	return transaction.bytes;
}

public Hash getHash()
{
	return hash;
}

public AddressViewModel getAddress(Tangle tangle)
{
        if(address == null) {
		address = AddressViewModel.load(tangle, getAddressHash());
	}
        return address;
}

public TagViewModel getTag(Tangle tangle)
{
        return TagViewModel.load(tangle, getTagValue());
}

public Hash getAddressHash()
{
	if (transaction.address == null)
	{
		transaction.address = new Hash(trits(), ADDRESS_TRINARY_OFFSET);
	}
	return transaction.address;
}

public Hash getObsoleteTagValue()
{
	if (transaction.obsoleteTag == null)
	{
		transaction.obsoleteTag = new Hash(Converter.bytes(trits(), OBSOLETE_TAG_TRINARY_OFFSET, OBSOLETE_TAG_TRINARY_SIZE), 0, TAG_SIZE);
	}
	return transaction.obsoleteTag;
}

public Hash getBundleHash()
{
	if (transaction.bundle == null)
	{
		transaction.bundle = new Hash(trits(), BUNDLE_TRINARY_OFFSET);
	}
	return transaction.bundle;
}

public Hash getTrunkTransactionHash()
{
	if (transaction.trunk == null)
	{
		transaction.trunk = new Hash(trits(), TRUNK_TRANSACTION_TRINARY_OFFSET);
	}
	return transaction.trunk;
}

public Hash getBranchTransactionHash()
{
	if (transaction.branch == null)
	{
		transaction.branch = new Hash(trits(), BRANCH_TRANSACTION_TRINARY_OFFSET);
	}
	return transaction.branch;
}

public Hash getTagValue()
{
	if (transaction.tag == null)
	{
		transaction.tag = new Hash(Converter.bytes(trits(), TAG_TRINARY_OFFSET, TAG_TRINARY_SIZE), 0, TAG_SIZE);
	}
	return transaction.tag;
}

public long getAttachmentTimestamp() { return transaction.attachmentTimestamp; }
public long getAttachmentTimestampLowerBound()
{
	return transaction.attachmentTimestampLowerBound;
}
public long getAttachmentTimestampUpperBound()
{
	return transaction.attachmentTimestampUpperBound;
}


public long value()
{
	return transaction.value;
}

public void setValidity( Tangle tangle, int validity)
{
	transaction.validity = validity;
	update(tangle, "validity");
}

public int getValidity()
{
	return transaction.validity;
}

public long getCurrentIndex()
{
	return transaction.currentIndex;
}

public int[] getSignature()
{
	return Arrays.copyOfRange(trits(), SIGNATURE_MESSAGE_FRAGMENT_TRINARY_OFFSET, SIGNATURE_MESSAGE_FRAGMENT_TRINARY_SIZE);
}

public long getTimestamp()
{
	return transaction.timestamp;
}

public byte[] getNonce()
{
	return Converter.bytes(trits(), NONCE_TRINARY_OFFSET, NONCE_TRINARY_SIZE);
}

public long lastIndex()
{
	return transaction.lastIndex;
}

public void setAttachmentData()
{
	getTagValue();
	transaction.attachmentTimestamp = Converter.longValue(trits(), ATTACHMENT_TIMESTAMP_TRINARY_OFFSET, ATTACHMENT_TIMESTAMP_TRINARY_SIZE);
	transaction.attachmentTimestampLowerBound = Converter.longValue(trits(), ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_OFFSET, ATTACHMENT_TIMESTAMP_LOWER_BOUND_TRINARY_SIZE);
	transaction.attachmentTimestampUpperBound = Converter.longValue(trits(), ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_OFFSET, ATTACHMENT_TIMESTAMP_UPPER_BOUND_TRINARY_SIZE);

}
public void setMetadata()
{
	transaction.value = Converter.longValue(trits(), VALUE_TRINARY_OFFSET, VALUE_USABLE_TRINARY_SIZE);
	transaction.timestamp = Converter.longValue(trits(), TIMESTAMP_TRINARY_OFFSET, TIMESTAMP_TRINARY_SIZE);
	//if (transaction.timestamp > 1262304000000L ) transaction.timestamp /= 1000L;  // if > 01.01.2010 in milliseconds
	transaction.currentIndex = Converter.longValue(trits(), CURRENT_INDEX_TRINARY_OFFSET, CURRENT_INDEX_TRINARY_SIZE);
	transaction.lastIndex = Converter.longValue(trits(), LAST_INDEX_TRINARY_OFFSET, LAST_INDEX_TRINARY_SIZE);
	transaction.type = transaction.bytes == null ? TransactionViewModel.PREFILLED_SLOT : TransactionViewModel.FILLED_SLOT;
}

public static bool exists(Tangle tangle, Hash hash) 
	{
        return tangle.exists(new Transaction(), hash);
    }

    public static HashSet<Indexable> getMissingTransactions(Tangle tangle) 
	{
        return tangle.keysWithMissingReferences(new Approvee(), new Transaction());
    }

    public static void updateSolidTransactions(Tangle tangle, HashSet<Hash> analyzedHashes)
{
	Iterator<Hash> hashIterator = analyzedHashes.iterator();
	TransactionViewModel transactionViewModel;
	while (hashIterator.hasNext()) {
		transactionViewModel = TransactionViewModel.fromHash(tangle, hashIterator.next());
		transactionViewModel.updateHeights(tangle);
		transactionViewModel.updateSolid(true);
		transactionViewModel.update(tangle, "solid|height");
	}
}

public bool updateSolid(boolean solid) 
{
        if(solid != transaction.solid) {
		transaction.solid = solid;
		return true;
	}
        return false;
}

public bool isSolid()
{
	return transaction.solid;
}

public int snapshotIndex()
{
	return transaction.snapshot;
}

public void setSnapshot( Tangle tangle,  int index) 
{
        if ( index != transaction.snapshot ) {
		transaction.snapshot = index;
		update(tangle, "snapshot");
	}
}

public long getHeight()
{
	return transaction.height;
}

private void updateHeight(long height) 
{
	transaction.height = height;
}

public void updateHeights( Tangle tangle) 
{
	TransactionViewModel transactionVM = this, trunk = this.getTrunkTransaction(tangle);
	Stack<Hash> transactionViewModels = new Stack<>();
        transactionViewModels.push(transactionVM.getHash());
        while(trunk.getHeight() == 0 && trunk.getType() != PREFILLED_SLOT && !trunk.getHash().equals(Hash.NULL_HASH)) {
            transactionVM = trunk;
            trunk = transactionVM.getTrunkTransaction(tangle);
            transactionViewModels.push(transactionVM.getHash());
        }
        while(transactionViewModels.size() != 0) {
            transactionVM = TransactionViewModel.fromHash(tangle, transactionViewModels.pop());
            if(trunk.getHash().equals(Hash.NULL_HASH) && trunk.getHeight() == 0 && !transactionVM.getHash().equals(Hash.NULL_HASH)) {
                transactionVM.updateHeight(1L);
                transactionVM.update(tangle, "height");
            } else if (trunk.getType() != PREFILLED_SLOT && transactionVM.getHeight() == 0){
                transactionVM.updateHeight(1 + trunk.getHeight());
                transactionVM.update(tangle, "height");
            } else {
                break;
            }
            trunk = transactionVM;
        }
    }

    public void updateSender(String sender)
{
	transaction.sender = sender;
}
public String getSender()
{
	return transaction.sender;
}
}
}

