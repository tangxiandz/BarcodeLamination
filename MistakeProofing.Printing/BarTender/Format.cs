namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Represents a label format.
    /// </summary>
    public sealed class Format : ComObjectWrapper
    {
        private SubStringCollection? _subStrings = null;
        private DesignObjectCollection? _objects = null;

        private bool _disposed;

        /// <summary>
        /// Occurs when the label format is closed.
        /// </summary>
        internal event EventHandler? Closed;

        internal Format(object? obj)
            : base(obj)
        { }

        /// <summary>
        /// Returns the file name of the label format.
        /// </summary>
        public string FileName => (string)GetProperty(nameof(FileName));

        /// <summary>
        /// Returns the title of the label format.
        /// </summary>
        /// <remarks>
        /// The <see cref="Title"/> property returns the format portion of the title bar for the label format.
        /// This value is not necessarily the same as the format file name because
        /// if the format has not yet been saved, no format file name will exist.
        /// </remarks>
        public string Title => (string)GetProperty(nameof(Title));

        /// <summary>
        /// Returns or sets the name of the printer.
        /// </summary>
        public string Printer
        {
            get => (string)GetProperty(nameof(Printer));
            set => SetProperty(nameof(Printer), value);
        }

        /// <summary>
        /// Returns or sets the number of copies that you want to print of each label, -or-
        /// when serializing labels, the number of copies of each label in a sequence.
        /// </summary>
        public int IdenticalCopiesOfLabel
        {
            get => (int)GetProperty(nameof(IdenticalCopiesOfLabel));
            set => SetProperty(nameof(IdenticalCopiesOfLabel), value);
        }

        /// <summary>
        /// Returns or sets the number of serialized labels to print.
        /// </summary>
        public int NumberSerializedLabels
        {
            get => (int)GetProperty(nameof(NumberSerializedLabels));
            set => SetProperty(nameof(NumberSerializedLabels), value);
        }

        /// <summary>
        /// Returns a reference to the <see cref="SubStringCollection" /> object,
        /// which contains a list of named substrings that are assigned to all objects on the label format.
        /// </summary>
        public SubStringCollection NamedSubStrings
        {
            get
            {
                if (_subStrings is null)
                {
                    object btSubStrings = GetProperty(nameof(NamedSubStrings));
                    _subStrings = new SubStringCollection(btSubStrings);
                }

                return _subStrings;
            }
        }

        /// <summary>
        /// Returns a reference to the <see cref="DesignObjectCollection" /> object.
        /// </summary>
        public DesignObjectCollection Objects
        {
            get
            {
                _objects ??= new DesignObjectCollection(GetProperty(nameof(Objects)));

                return _objects;
            }
        }

        /// <summary>
        /// Returns the data value of an object on the label format.
        /// </summary>
        /// <param name="subStringName">The name of the substring to retrieve.</param>
        /// <returns>The value of the specified substring.</returns>
        public string GetNamedSubStringValue(string subStringName)
            => (string)InvokeMethod(nameof(GetNamedSubStringValue), subStringName)!;

        /// <summary>
        /// Sets the value of a named substring on a label format.
        /// </summary>
        /// <param name="subStringName">The name of the substring.</param>
        /// <param name="value">The new value for the substring.</param>
        /// <remarks>This method can only modify a substring with a Screen Data data source.</remarks>
        public void SetNamedSubStringValue(string subStringName, string? value)
            => InvokeMethod(nameof(SetNamedSubStringValue), subStringName, value ?? string.Empty);

        /// <summary>
        /// Prints the label format.
        /// </summary>
        /// <param name="showStatusWindow">Determines whether a prompt showing the status of the print job will be displayed.</param>
        /// <param name="showPrintDialog">Determines whether the print dialog will be displayed.</param>
        public void PrintOut(bool showStatusWindow = false, bool showPrintDialog = false)
            => InvokeMethod(nameof(PrintOut), showStatusWindow, showPrintDialog);

        /// <summary>
        /// Creates an image of the label format and saves it to an image file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileType"></param>
        /// <param name="color"></param>
        /// <param name="resolution"></param>
        /// <param name="saveOption"></param>
        public void ExportToFile(
            string filename,
            string fileType,
            ColorConstants color,
            ResolutionConstants resolution,
            SaveOptionConstants saveOption = SaveOptionConstants.PromptSave)
            => InvokeMethod(nameof(ExportToFile), filename, fileType, color, resolution, saveOption);

        /// <summary>
        /// Saves the label format.
        /// </summary>
        public void Save() => InvokeMethod(nameof(Save));

        /// <summary>
        /// Saves the label format to a different label format file.
        /// </summary>
        /// <param name="filename">The new label format file name.</param>
        /// <param name="overwriteIfExists">
        /// Determines whether to overwrite an existing file.
        /// If true, then overwrite if file exists. If false, then an error will occur if file exists. The default is false.
        /// </param>
        /// <exception cref="IOException">The file already exists and overwrite is false.</exception>
        public void SaveAs(string filename, bool overwriteIfExists = false)
        {
            string path = Path.GetFullPath(filename);
            if (!overwriteIfExists && File.Exists(path))
            {
                throw new IOException(string.Format(Properties.Resources.FileAlreadyExists, path));
            }

            InvokeMethod(nameof(SaveAs), path, overwriteIfExists);
        }

        /// <summary>
        /// Closes an open label format.
        /// </summary>
        /// <param name="saveOption">Determines whether the label format will be saved.</param>
        public void Close(SaveOptionConstants saveOption = SaveOptionConstants.PromptSave)
        {
            if (_disposed) return;

            InvokeMethod(nameof(Close), saveOption);

            // Notifies the collection holding the format object
            // that the format is closed 
            OnClosed();

            // Dispose the object when it closed.
            Dispose();
        }

        private void OnClosed()
        {
            var handler = Closed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString() => Title;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_subStrings is not null)
                    {
                        _subStrings.Dispose();
                        _subStrings = null;
                    }

                    if (_objects is not null)
                    {
                        _objects.Dispose();
                        _objects = null;
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
