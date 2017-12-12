﻿using System;

namespace IotaNet.Iri.Storage
{
/**
 * Created by paul on 5/6/17.
 */
	public interface Indexable : IComparable<Indexable>
	{
		byte[] bytes();
		void read(byte[] bytes);
		Indexable incremented();
		Indexable decremented();
	}
}
