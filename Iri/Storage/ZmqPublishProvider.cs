using System;
using System.Text;
using System.Collections.Generic;

using IotaNet.Iri.Controllers;
using IotaNet.Iri.Helpers;
using IotaNet.Iri.Model;
using IotaNet.Iri.Utils;
using IotaNet.Iri.Zmq;

namespace IotaNet.Iri.Storage
{
	public class ZmqPublishProvider : PersistenceProvider
	{
		private static Logger log = LoggerFactory.getLogger<ZmqPublishProvider>();
		private MessageQ messageQ;

		public ZmqPublishProvider(MessageQ messageQ)
		{
			this.messageQ = messageQ;
		}

		public void init()
		{

		}

		public bool isAvailable()
		{
			return false;
		}

		public void shutdown()
		{

		}

		public bool save(Persistable model, Indexable index)
		{
			return false;
		}

		public void delete<T>(T model, Indexable index)
		{

		}

		public bool update(Persistable model, Indexable index, String item)
		{
			if (model is Transaction)
			{
				Transaction transaction = ((Transaction)model);
				if (item == "sender")
				{
					TransactionViewModel transactionViewModel = new TransactionViewModel(transaction, (Hash)index);
					StringBuilder sb = new StringBuilder(600);
					try
					{
						sb.Append("tx ");
						sb.Append(transactionViewModel.getHash()); sb.Append(" ");
						sb.Append(transactionViewModel.getAddressHash()); sb.Append(" ");
						sb.Append(String.valueOf(transactionViewModel.value())); sb.Append(" ");
						sb.Append(transactionViewModel.getObsoleteTagValue().toString().substring(0, 27)); sb.Append(" ");
						sb.Append(String.valueOf(transactionViewModel.getTimestamp())); sb.Append(" ");
						sb.Append(String.valueOf(transactionViewModel.getCurrentIndex())); sb.Append(" ");
						sb.Append(String.valueOf(transactionViewModel.lastIndex())); sb.Append(" ");
						sb.Append(transactionViewModel.getBundleHash()); sb.Append(" ");
						sb.Append(transactionViewModel.getTrunkTransactionHash()); sb.Append(" ");
						sb.Append(transactionViewModel.getBranchTransactionHash()); sb.Append(" ");
						sb.Append(String.valueOf(transactionViewModel.getArrivalTime()));
						messageQ.publish(sb.ToString());
					}
					catch (Exception e)
					{
						log.error(sb.ToString());
						log.error("Error publishing to zmq.", e);
					}
					return true;
				}
			}
			return false;
		}

		public bool exists<T>(T model, Indexable key)
		{
			return false;
		}

		public Pair<Indexable, Persistable> latest<T, U>()
		{
			return null;
		}

		public HashSet<Indexable> keysWithMissingReferences<T, U>()
		{
			return null;
		}

		public Persistable get<T>(T model, Indexable index)
		{
			return null;
		}

		public bool mayExist<T>(T model, Indexable index)
		{
			return false;
		}

		public long count<T>(T model)
		{
			return 0;
		}

		public HashSet<Indexable> keysStartingWith<T>(T modelClass, byte[] value)
		{
			return null;
		}

		public Persistable seek<T>(T model, byte[] key)
		{
			return null;
		}

		public Pair<Indexable, Persistable> next<T>(T model, Indexable index)
		{
			return null;
		}

		public Pair<Indexable, Persistable> previous<T>(T model, Indexable index)
		{
			return null;
		}

		public Pair<Indexable, Persistable> first<T, U>()
		{
			return null;
		}

		public bool saveBatch(List<Pair<Indexable, Persistable>> models)
		{
			return false;
		}

		public void clear<T>(T column)
		{

		}

		public void clearMetadata<T>(T column)
		{

		}

	}
}
