using System.Collections;
using System.Collections.ObjectModel;

namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Manages the list of named substrings that are assigned to all objects on the label format.
    /// </summary>
    public sealed class SubStringCollection : ComObjectWrapper, IEnumerable<SubString>
    {
        private readonly ObjectCollection _items = new();
        private bool _disposed;

        internal SubStringCollection(object? obj)
            : base(obj)
        {
            int count = (int)GetProperty("Count");
            for (int index = 1; index <= count; index++)
            {
                object? btSubString = InvokeMethod("GetSubString", index);
                _items.Add(new SubString(btSubString));
            }
        }

        public int Count => _items.Count;

        public SubString this[int index] => _items[index];

        public SubString this[string name] => _items[name];

        public bool Contains(string name) => _items.Contains(name);

        /// <summary>
        /// Returns a string that contains the name and value of all substrings separated by a delimiter.
        /// </summary>
        /// <param name="nameValueDelimiter">Delimiter used to separate the name and the value.</param>
        /// <param name="recordDelimiter">Delimiter used to separate multiple substring name/value pairs.</param>
        /// <returns>A string that contains the name and value of all substrings separated by a delimiter.</returns>
        public string GetAll(string nameValueDelimiter, string recordDelimiter)
            => (string)InvokeMethod(nameof(GetAll), nameValueDelimiter, recordDelimiter)!;

        /// <summary>
        /// Sets all substrings using a string that contains the name and value separated by a delimiter.
        /// </summary>
        /// <param name="s">A string containing the value to which all substrings will be set.</param>
        /// <param name="delimiter">A character(s) used to separate the name and value of each name/value record.</param>
        public void SetAll(string s, string delimiter) => InvokeMethod(nameof(SetAll), s, delimiter);

        public IEnumerator<SubString> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var item in _items)
                    {
                        item.Dispose();
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }


        private class ObjectCollection : KeyedCollection<string, SubString>
        {
            public ObjectCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            { }

            protected override string GetKeyForItem(SubString item) => item.Name;
        }
    }
}
