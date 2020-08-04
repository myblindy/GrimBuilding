using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public static async Task<DbrParser> FromPathAsync(string path)
        {
            var result = new DbrParser { Path = path };
            using var reader = new StreamReader(path);

            string line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                var sep = line.IndexOf(',');
                result.properties.Add(line[..sep], DecodeArray(line[(sep + 1)..^1].Split(';')));
            }

            return result;
        }

        public IEnumerable<string> GetStringValues(string key) => properties.TryGetValue(key, out var arr) ? arr.OfType<string>() : null;

        public string GetStringValue(string key, int idx = 0) => (string)properties[key][idx];

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