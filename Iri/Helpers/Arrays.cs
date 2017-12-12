using System;

namespace IotaNet.Iri.Helpers
{
	public class Arrays
	{
		public static T[] copyOfRange<T>(T[] original, int from, int to)
		{
			var len = to - from;
			var destination = new T[len];
			Array.Copy(original, destination, len);
			return destination;
		}

		public static T[] copyOf<T>(T[] original, int newLength)
		{
			var destination = new T[newLength];
			Array.Copy(original, destination, newLength);
			return destination;
		}

		public static bool equals<T>(T a, T b)
		{
			return false;
		}

		public static int hashCode(byte[] a)
		{
			return 0;
		}
	}
}
