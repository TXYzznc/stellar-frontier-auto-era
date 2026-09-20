namespace AutoEra.UI
{
    /// <summary>
    /// 界面打开参数的键名。底层是 <c>UIParams</c> 继承自 <c>RefParams</c> 的键值通道
    /// （按 <c>RefParams.Id + key</c> 存于 <c>GF.VariablePool</c>）。
    ///
    /// 键名集中在这里，避免每个打开点各自拼字符串；写入用 <c>Set</c>，读取**必须**用
    /// <c>TryGet</c>——<c>RefParams.Get(string)</c> 在缺 key 时会对 <c>null</c> 取
    /// <c>.Value</c> 并抛 <c>NullReferenceException</c>。
    /// </summary>
    public static class AutoEraUiParamKeys
    {
        /// <summary><see cref="AutoEraUiSession"/>：本次打开能接触到的应用与世界服务。</summary>
        public const string Session = "AutoEra.UI.Session";

        /// <summary>页面打开请求：pageKey 与 objectId／slotId／algorithmId／recordId 等稳定身份。</summary>
        public const string Request = "AutoEra.UI.Request";

        /// <summary>来源上下文：返回时恢复分页、选择、筛选、滚动与焦点。</summary>
        public const string Source = "AutoEra.UI.Source";
    }
}
