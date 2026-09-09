namespace AutoEra.UI.Contracts
{
    /// <summary>
    /// Rejects late callbacks after a UIForm closes or after a newer request supersedes the old one.
    /// </summary>
    public sealed class AutoEraUiRequestVersionGate
    {
        private long _formVersion;
        private long _requestVersion;
        private bool _isOpen;

        public long FormVersion => _formVersion;
        public long RequestVersion => _requestVersion;
        public bool IsOpen => _isOpen;

        public void Open()
        {
            _formVersion++;
            _requestVersion = 0;
            _isOpen = true;
        }

        public long BeginRequest()
        {
            if (!_isOpen)
            {
                return -1;
            }

            return ++_requestVersion;
        }

        public bool Accepts(long formVersion, long requestVersion)
        {
            return _isOpen && formVersion == _formVersion && requestVersion == _requestVersion;
        }

        public void Close()
        {
            _isOpen = false;
            _formVersion++;
            _requestVersion = 0;
        }
    }
}
