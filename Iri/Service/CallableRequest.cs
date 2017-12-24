using System;

using IotaNet.Iri.Utils;

namespace IotaNet.Iri.Service
{
	public interface CallableRequest<V>
	{
		V call(Pair<String, Object> request);
	}
}
