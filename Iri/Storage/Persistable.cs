using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotaNet.Iri.Storage
{
	/**
	 * Created by paul on 5/6/17.
	 */
	public interface Persistable
	{
		byte[] bytes();
		void read(byte[] bytes);
		byte[] metadata();
		void readMetadata(byte[] bytes);
		bool merge();
	}
}
