namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Represents an object on the label format.
    /// </summary>
    public sealed class DesignObject : ComObjectWrapper
    {
        private readonly string _name;

        internal DesignObject(object? obj)
            : base(obj)
        {
            _name = (string)GetProperty(nameof(Name));
        }

        /// <summary>
        /// Returns the name of the object.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Returns or sets whether to print the object.
        /// </summary>
        public bool DoNotPrint
        {
            get => (bool)GetProperty(nameof(DoNotPrint));
            set => SetProperty(nameof(DoNotPrint), value);
        }
    }
}
