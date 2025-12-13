namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Represents a substring that is assigned to an object on the label format.
    /// </summary>
    public sealed class SubString : ComObjectWrapper
    {
        private readonly string _name;

        internal SubString(object? obj)
            : base(obj)
        {
            _name = (string)GetProperty(nameof(Name));
        }

        /// <summary>
        /// Returns the name of the substring.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Returns the type of data source that the substring is using.
        /// </summary>
        public SubStringTypeConstants Type => (SubStringTypeConstants)GetProperty(nameof(Type));

        /// <summary>
        /// Returns or sets the value for the substring.
        /// </summary>
        public string Value
        {
            get => (string)GetProperty(nameof(Value));
            set => SetProperty(nameof(Value), value);
        }

        public override string ToString() => Name;
    }
}
