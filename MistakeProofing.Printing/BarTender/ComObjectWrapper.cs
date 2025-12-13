using System.Reflection;
using System.Runtime.InteropServices;

namespace MistakeProofing.Printing.BarTender
{
    public abstract class ComObjectWrapper : IDisposable
    {
        private readonly object _comObject;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComObjectWrapper"/> class with the object being wrapped.
        /// </summary>
        /// <param name="obj">The object to be wrapped.</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not a COM object.</exception>
        protected ComObjectWrapper(object? obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));

            if (!Marshal.IsComObject(obj))
            {
                throw new ArgumentException(Properties.Resources.RequiresComObject, nameof(obj));
            }

            _comObject = obj;
        }

        ~ComObjectWrapper() => Dispose(false);

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources.
            }

            // Dispose unmanaged resources.
            if (_comObject is not null)
            {
                Marshal.ReleaseComObject(_comObject);
            }

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            //ObjectDisposedException.ThrowIf(_disposed, this);
        }

        protected object GetProperty(string name)
        {
            ThrowIfDisposed();
            return _comObject.GetType().InvokeMember(name, BindingFlags.GetProperty, null, _comObject, null)!;
        }

        protected void SetProperty(string name, object value)
        {
            ThrowIfDisposed();
            _comObject.GetType().InvokeMember(name, BindingFlags.SetProperty, null, _comObject, new object?[] { value });
        }

        protected object? InvokeMethod(string name, params object?[] args)
        {
            ThrowIfDisposed();
            return _comObject.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, _comObject, args);
        }
    }
}
