using System.Collections;

namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Manages the list of open formats in BarTender, and is used to create and open label formats.
    /// </summary>
    public sealed class FormatCollection : ComObjectWrapper, IEnumerable<Format>
    {
        private readonly List<Format> _items = new List<Format>();
        private bool _disposed;

        internal FormatCollection(object? obj)
            : base(obj)
        {
            int count = (int)GetProperty("Count");
            for (int index = 1; index <= count; index++)
            {
                object? btFormat = InvokeMethod("GetFormat", index);
                Format format = new(btFormat);
                format.Closed += Format_Closed;
                _items.Add(format);
            }
        }

        /// <summary>
        /// Opens a label format.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="closeOutFirstFormat"></param>
        /// <param name="usePrinter"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">The specified file was not found.</exception>
        public Format Open(string filename, bool closeOutFirstFormat = false, string usePrinter = "")
        {
            string path = Path.GetFullPath(filename);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format(Properties.Resources.FileNotFound, path), filename);
            }

            object? btFormat = InvokeMethod(nameof(Open), filename, closeOutFirstFormat, usePrinter);
            Format format = new(btFormat);
            format.Closed += Format_Closed;
            _items.Add(format);
            return format;
        }

        /// <summary>
        /// Returns the number of opened label formats.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Returns a reference to a specified <see cref="Format"/> object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Format this[int index] => _items[index];

        public IEnumerator<Format> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Format_Closed(object? sender, EventArgs e)
        {
            if (sender is Format format)
            {
                format.Closed -= Format_Closed;
                _items.Remove(format);
            }
        }

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
    }
}
