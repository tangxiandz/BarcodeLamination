using System.Reflection;
using System.Runtime.InteropServices;

namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Top level object for BarTender.
    /// </summary>
    public sealed class Application : IDisposable
    {
        public const string BARTENDER_PROGID = "BarTender.Application";

        private readonly object _btApp;
        private LicenseServer? _licenseServer = null;
        private FormatCollection? _formats = null;
        private bool _disposed;

        /// <summary>
        /// Creates a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <exception cref="COMException">BarTender is not installed.</exception>
        public Application()
        {
            var btAppType = Type.GetTypeFromProgID(BARTENDER_PROGID)
                ?? throw new COMException(Properties.Resources.BarTenderNotInstalled);
            try
            {
                _btApp = Activator.CreateInstance(btAppType)
               ?? throw new COMException(Properties.Resources.CreateInstanceFail);
            }
            catch (Exception ex)
            {
                throw new COMException("您的电脑未安装Bartender（"+ex.Message+"）");
            }
           
        }

        /// <summary>
        /// Returns the edition of BarTender being used to process the print job.
        /// </summary>
        public string Edition => (string)GetProperty(nameof(Edition));

        /// <summary>
        /// Returns the version of BarTender application.
        /// </summary>
        public string Version => (string)GetProperty(nameof(Version));

        /// <summary>
        /// Returns the version, including any applicable service release numbers, of BarTender.
        /// </summary>
        public string FullVersion => (string)GetProperty(nameof(FullVersion));

        /// <summary>
        /// Returns the build number of the BarTender application.
        /// </summary>
        public int BuildNumber => (int)GetProperty(nameof(BuildNumber));

        /// <summary>
        /// Returns the unique identifier of the BarTender process.
        /// </summary>
        public int ProcessId => (int)GetProperty(nameof(ProcessId));

        /// <summary>
        /// Returns or sets whether the BarTender application will be visible when it runs.
        /// </summary>
        /// <value>true if the BarTender application will be visible; false, it will run in the background.</value>
        public bool Visible
        {
            get => (bool)GetProperty(nameof(Visible));
            set => SetProperty(nameof(Visible), value);
        }

        /// <summary>
        /// Returns the state of command line processing.
        /// </summary>
        /// <value>true if BarTender is currently processing command lines; otherwise, false.</value>
        public bool IsProcessingCommandLines => (bool)GetProperty(nameof(IsProcessingCommandLines));

        /// <summary>
        /// Returns whether BarTender is currently printing a label format.
        /// </summary>
        /// <value>true if BarTender is currently printing; otherwise, false.</value>
        public bool IsPrinting => (bool)GetProperty(nameof(IsPrinting));

        /// <summary>
        /// Puts a command line into a command processing queue internal to BarTender and immediately returns.
        /// </summary>
        /// <param name="commandLineText">A string contains the command line for the application to process.</param>
        public void CommandLine(string commandLineText) => InvokeMethod(nameof(CommandLine), commandLineText);

        /// <summary>
        /// Saves all opened label formats.
        /// </summary>
        public void Save() => InvokeMethod(nameof(Save), true);

        /// <summary>
        /// Quits BarTender application.
        /// </summary>
        /// <param name="saveOption">Determines whether the open label formats will be saved.</param>
        public void Quit(SaveOptionConstants saveOption = SaveOptionConstants.PromptSave)
        {
            if (_disposed) return;

            InvokeMethod(nameof(Quit), saveOption);

            Dispose();
        }

        /// <summary>
        /// Returns a reference to the <see cref="LicenseServer"/> object.
        /// </summary>
        public LicenseServer LicenseServer
        {
            get
            {
                if (_licenseServer is null)
                {
                    object btLicenseServer = GetProperty(nameof(LicenseServer));
                    _licenseServer = new LicenseServer(btLicenseServer);
                }

                return _licenseServer;
            }
        }

        /// <summary>
        /// Returns a reference to the <see cref="FormatCollection"/> object,
        /// which manages all of the label formats currently open in BarTender.
        /// </summary>
        public FormatCollection Formats
        {
            get
            {
                if (_formats is null)
                {
                    object btFormats = GetProperty(nameof(Formats));
                    _formats = new FormatCollection(btFormats);
                }

                return _formats;
            }
        }

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Application() => Dispose(false);

        private object GetProperty(string name)
        {
            ThrowIfDisposed();
            return _btApp.GetType().InvokeMember(name, BindingFlags.GetProperty, null, _btApp, null)!;
        }

        private void SetProperty(string name, object value)
        {
            ThrowIfDisposed();
            _btApp.GetType().InvokeMember(name, BindingFlags.SetProperty, null, _btApp, new object[] { value });
        }

        private object? InvokeMethod(string name, params object?[] args)
        {
            ThrowIfDisposed();
            return _btApp.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, _btApp, args);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed) return;

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources...

                if (_licenseServer is not null)
                {
                    _licenseServer.Dispose();
                    _licenseServer = null;
                }

                if (_formats is not null)
                {
                    _formats.Dispose();
                    _formats = null;
                }
            }

            // Dispose unmanaged resources...
            if (_btApp is not null)
            {
                Marshal.ReleaseComObject(_btApp);
            }

            _disposed = true;
        }
    }
}
