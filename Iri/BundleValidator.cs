using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IotaNet.Iri.Controllers;
using IotaNet.Iri.Storage;

namespace IotaNet.Iri
{
	public class BundleValidator
	{

		public static List<List<TransactionViewModel>> validate(Tangle tangle, Model.Hash hash)
		{
			BundleViewModel bundleViewModel = BundleViewModel.load(tangle, hash);
			List<List<TransactionViewModel>> transactions = new List<List<TransactionViewModel>>();
			Dictionary<Model.Hash, TransactionViewModel> bundleTransactions = loadTransactionsFromTangle(tangle, bundleViewModel);

        foreach (TransactionViewModel transactionViewModel in bundleTransactions.values()) {

            if (transactionViewModel.getCurrentIndex() == 0 && transactionViewModel.getValidity() >= 0) {

					List<TransactionViewModel> instanceTransactionViewModels = new List<TransactionViewModel>();

		 long lastIndex = transactionViewModel.lastIndex();
		long bundleValue = 0;
		int i = 0;
		 Sponge curlInstance = SpongeFactory.create(SpongeFactory.Mode.KERL);
                 Sponge addressInstance = SpongeFactory.create(SpongeFactory.Mode.KERL);

                 int[] addressTrits = new int[TransactionViewModel.ADDRESS_TRINARY_SIZE];
		 int[] bundleHashTrits = new int[TransactionViewModel.BUNDLE_TRINARY_SIZE];

		MAIN_LOOP:
                while (true) {

                    instanceTransactionViewModels.add(transactionViewModel);


                    if (
							transactionViewModel.getCurrentIndex() != i
                            || transactionViewModel.lastIndex() != lastIndex
                            || ((bundleValue = Math.addExact(bundleValue, transactionViewModel.value())) < -TransactionViewModel.SUPPLY
                            || bundleValue > TransactionViewModel.SUPPLY)
                            ) {
                        instanceTransactionViewModels.get(0).setValidity(tangle, -1);
                        break;
                    }

                    if (transactionViewModel.value() != 0 && transactionViewModel.getAddressHash().trits()[Curl.HASH_LENGTH - 1] != 0) {
                        instanceTransactionViewModels.get(0).setValidity(tangle, -1);
                        break;
                    }

                    if (i++ == lastIndex) { // It's supposed to become -3812798742493 after 3812798742493 and to go "down" to -1 but we hope that noone will create such long bundles

                        if (bundleValue == 0) {

                            if (instanceTransactionViewModels.get(0).getValidity() == 0) {
                                curlInstance.reset();
                                for ( TransactionViewModel transactionViewModel2 : instanceTransactionViewModels) {
                                    curlInstance.absorb(transactionViewModel2.trits(), TransactionViewModel.ESSENCE_TRINARY_OFFSET, TransactionViewModel.ESSENCE_TRINARY_SIZE);
                                }
                                curlInstance.squeeze(bundleHashTrits, 0, bundleHashTrits.length);
                                if (instanceTransactionViewModels.get(0).getBundleHash().equals(new Hash(Converter.bytes(bundleHashTrits, 0, TransactionViewModel.BUNDLE_TRINARY_SIZE)))) {

                                     int[] normalizedBundle = ISS.normalizedBundle(bundleHashTrits);

                                    for (int j = 0; j<instanceTransactionViewModels.size(); ) {

                                        transactionViewModel = instanceTransactionViewModels.get(j);
                                        if (transactionViewModel.value() < 0) { // let's recreate the address of the transactionViewModel.
                                            addressInstance.reset();
                                            int offset = 0;
                                            do {
                                                addressInstance.absorb(
														ISS.digest(SpongeFactory.Mode.KERL, Arrays.copyOfRange(normalizedBundle, offset % (Curl.HASH_LENGTH / Converter.NUMBER_OF_TRITS_IN_A_TRYTE), offset = (offset + ISS.NUMBER_OF_FRAGMENT_CHUNKS - 1) % (Curl.HASH_LENGTH / Converter.NUMBER_OF_TRITS_IN_A_TRYTE) + 1),
                                                                Arrays.copyOfRange(instanceTransactionViewModels.get(j).trits(), TransactionViewModel.SIGNATURE_MESSAGE_FRAGMENT_TRINARY_OFFSET,
                                                                        TransactionViewModel.SIGNATURE_MESSAGE_FRAGMENT_TRINARY_OFFSET + TransactionViewModel.SIGNATURE_MESSAGE_FRAGMENT_TRINARY_SIZE)),
                                                        0, Curl.HASH_LENGTH);
                                            } while (++j<instanceTransactionViewModels.size()
                                                    && instanceTransactionViewModels.get(j).getAddressHash().equals(transactionViewModel.getAddressHash())
                                                    && instanceTransactionViewModels.get(j).value() == 0);

                                            addressInstance.squeeze(addressTrits, 0, addressTrits.length);
                                            //if (!Arrays.equals(Converter.bytes(addressTrits, 0, TransactionViewModel.ADDRESS_TRINARY_SIZE), transactionViewModel.getAddress().getHash().bytes())) {
                                            if (! transactionViewModel.getAddressHash().equals(new Hash(Converter.bytes(addressTrits, 0, TransactionViewModel.ADDRESS_TRINARY_SIZE)))) {
                                                instanceTransactionViewModels.get(0).setValidity(tangle, -1);
                                                break MAIN_LOOP;
                                            }
                                        } else {
                                            j++;
                                        }
                                    }

                                    instanceTransactionViewModels.get(0).setValidity(tangle, 1);
transactions.add(instanceTransactionViewModels);
                                } else {
                                    instanceTransactionViewModels.get(0).setValidity(tangle, -1);
                                }
                            } else {
                                transactions.add(instanceTransactionViewModels);
                            }
                        } else {
                            instanceTransactionViewModels.get(0).setValidity(tangle, -1);
                        }
                        break;

                    } else {
                        transactionViewModel = bundleTransactions[transactionViewModel.getTrunkTransactionHash()];
                        if (transactionViewModel == null) {
                            break;
                        }
                    }
                }
            }
        }
        return transactions;
    }

    public static boolean isInconsistent(List<TransactionViewModel> transactionViewModels, boolean milestone)
{
	long value = 0;
	for ( TransactionViewModel bundleTransactionViewModel : transactionViewModels)
	{
		if (bundleTransactionViewModel.value() != 0)
		{
			value += bundleTransactionViewModel.value();
			/*
			if(!milestone && bundleTransactionViewModel.getAddressHash().equals(Hash.NULL_HASH) && bundleTransactionViewModel.snapshotIndex() == 0) {
				return true;
			}
			*/
		}
	}
	return (value != 0 || transactionViewModels.size() == 0);
}

private static Map<Hash, TransactionViewModel> loadTransactionsFromTangle(Tangle tangle, BundleViewModel bundle)
{
	 Map<Hash, TransactionViewModel> bundleTransactions = new HashMap<>();
	try
	{
		for ( Hash transactionViewModel : bundle.getHashes())
		{
			bundleTransactions.put(transactionViewModel, TransactionViewModel.fromHash(tangle, transactionViewModel));
		}
	}
	catch (Exception e)
	{
		e.printStackTrace();
	}
	return bundleTransactions;
}
}
}
