using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml.Tokenization;

namespace MonoGameHtml {
	public static class EnumerableUtil {
		// splits IEnumerable on elements where predicate is true, removing said elements in the process
		public static T[][] SplitOn<T>(this IEnumerable<T> enumerable, Func<T, bool> shouldSplit) {
			var outputList = new List<T[]>();
			var currentList = new List<T>();

			foreach (var elem in enumerable) {
				if (shouldSplit(elem)) {
					outputList.Add(currentList.ToArray());
					currentList = new List<T>();
					continue;
				}

				currentList.Add(elem);
			}

			if (currentList.Any()) { 
				outputList.Add(currentList.ToArray());
			}

			return outputList.ToArray();
		}
	}
}