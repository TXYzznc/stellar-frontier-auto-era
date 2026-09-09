namespace AutoEra.UI
{
    public enum AutoEraHubPage
    {
        Overview,
        Tasks,
        Objects,
        Rules,
        Statistics
    }

    /// <summary>Small, deterministic selection state shared by the hub navigation and its page roots.</summary>
    public sealed class AutoEraHubPageSelection
    {
        public AutoEraHubPageSelection(AutoEraHubPage initialPage)
        {
            ActivePage = initialPage;
        }

        public AutoEraHubPage ActivePage { get; private set; }

        public bool Select(AutoEraHubPage page)
        {
            if (page == ActivePage)
            {
                return false;
            }

            ActivePage = page;
            return true;
        }

        public AutoEraHubPage Move(int delta)
        {
            const int pageCount = 5;
            int value = ((int)ActivePage + delta) % pageCount;
            if (value < 0)
            {
                value += pageCount;
            }

            ActivePage = (AutoEraHubPage)value;
            return ActivePage;
        }
    }
}
