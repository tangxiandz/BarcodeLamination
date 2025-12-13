namespace MistakeProofing.Printing.BarTender
{
    /// <summary>
    /// Represents the settings that enable BarTender to communicate with the Seagull License Server.
    /// </summary>
    public sealed class LicenseServer : ComObjectWrapper
    {
        internal LicenseServer(object? obj)
            : base(obj)
        { }

        /// <summary>
        /// Returns a value indicating whether BarTender is connected to the Seagull License Server.
        /// </summary>
        /// <value>true if BarTender is connected to the Seagull License Server; otherwise, false.</value>
        public bool IsConnected => (bool)GetProperty(nameof(IsConnected));
    }
}
