namespace AutoEra.UI
{
    /// <summary>
    /// 打开「多页界面」时指定初始页的请求对象（经 <see cref="AutoEraUiParamKeys.Request"/> 传入）。
    ///
    /// 页索引是**规格页序**（Grp_PageHost 下内容页的顺序），不是业务枚举。调用方应使用目标
    /// Form 自己公开的页常量（如 <c>BaseCommandHubForm.PageTasks</c>），不要把数字硬编码到
    /// 调用点，也不要在目标 Form 里另立一套页编号。
    /// </summary>
    public sealed class AutoEraUiPageRequest
    {
        public AutoEraUiPageRequest(int page)
        {
            Page = page;
        }

        /// <summary>目标界面初始显示的页索引。</summary>
        public int Page { get; }
    }
}
