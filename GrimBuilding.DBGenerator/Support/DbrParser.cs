using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace GrimBuilding.DBGenerator.Support
{
    class DbrParser
    {
        readonly Dictionary<string, object[]> properties = new Dictionary<string, object[]>();

        public string Path { get; private set; }

        private DbrParser() { }

        static object[] DecodeArray(string[] arr)
        {
            if (arr.All(s => double.TryParse(s, out _)))
                return arr.Select(s => (object)double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            else
                return arr;
        }

        public static async Task<DbrParser> FromPathAsync(string gdDbPath, string offsetPath, string filePath, params string[] navigationProperties)
        {
            var path = System.IO.Path.Combine(gdDbPath, offsetPath, filePath);

            var result = new DbrParser { Path = path };
            using var reader = new StreamReader(path);

            string line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                var sep = line.IndexOf(',');
                result.properties.Add(line[..sep], DecodeArray(line[(sep + 1)..^1].Split(';')));
            }

            var nav = navigationProperties.FirstOrDefault(p => result.properties.ContainsKey(p));
            return nav is null ? result : await FromPathAsync(gdDbPath, offsetPath, (string)result.properties[nav][0], navigationProperties).ConfigureAwait(false);
        }

        public IEnumerable<string> GetStringValues(string key) => properties.TryGetValue(key, out var arr) ? arr.OfType<string>() : null;

        public string GetStringValue(string key, int idx = 0) => (string)properties[key][idx];

        public IEnumerable<(string key, IEnumerable<string> values)> GetAllStringsOfFormat(string keyFormat, int startingIndex = 1)
        {
            while (true)
            {
                var key = string.Format(keyFormat, startingIndex++);
                if (!properties.TryGetValue(key, out var val)) yield break;
                yield return (key, val.OfType<string>());
            }
        }

        public bool TryGetStringValue(string key, int idx, out string val)
        {
            if (properties.TryGetValue(key, out var arr))
            {
                val = (string)arr[idx];
                return true;
            }

            val = null;
            return false;
        }

        public double GetDoubleValue(string key, int idx = 0) => (double)properties[key][idx];

        public bool TryGetDoubleValue(string key, int idx, out double val)
        {
            if (properties.TryGetValue(key, out var arr))
            {
                val = (double)arr[idx];
                return true;
            }

            val = 0;
            return false;
        }

        public bool ContainsKey(string key) => properties.ContainsKey(key);
    }
}