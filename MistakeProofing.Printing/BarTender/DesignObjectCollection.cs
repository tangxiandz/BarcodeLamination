using System.Collections;
using System.Collections.ObjectModel;

namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Manages the list of objects in a label format.
    /// </summary>
    public sealed class DesignObjectCollection : ComObjectWrapper, IEnumerable<DesignObject>
    {
        private readonly ObjectCollection _items = new();
        private bool _disposed;

        public DesignObjectCollection(object? obj)
            : base(obj)
        {
            int count = (int)GetProperty(nameof(Count));
            for (int index = 1; index <= count; index++)
            {
                _items.Add(new DesignObject(InvokeMethod("Item", index)));
            }
        }

        public int Count => _items.Count;

        public DesignObject this[int index] => _items[index];

        public DesignObject this[string name] => _items[name];

        public bool Contains(string name) => _items.Contains(name);

        public IEnumerator<DesignObject> GetEnumerator() => _items.GetEnumerator();

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


        private class ObjectCollection : KeyedCollection<string, DesignObject>
        {
            public ObjectCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            { }

            protected override string GetKeyForItem(DesignObject item) => item.Name;
        }
    }
}
