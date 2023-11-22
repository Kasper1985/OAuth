using System.Collections.Generic;

namespace OAuthServer.Extensions
{
    public static class TypesConverterExtensions
    {
        public static void Deconstruct<K,V>(this KeyValuePair<K,V> keyValuePair, out K key, out V value)
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
    }
}