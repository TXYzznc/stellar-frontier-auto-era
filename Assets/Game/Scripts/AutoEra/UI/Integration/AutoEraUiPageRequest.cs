using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>
    /// 打开「多页界面」时指定初始页与操作目标的请求对象（经 <see cref="AutoEraUiParamKeys.Request"/> 传入）。
    ///
    /// 页索引是**规格页序**（Grp_PageHost 下内容页的顺序），不是业务枚举。调用方应使用目标
    /// Form 自己公开的页常量（如 <c>BaseCommandHubForm.PageTasks</c>），不要把数字硬编码到
    /// 调用点，也不要在目标 Form 里另立一套页编号。
    /// </summary>
    public sealed class AutoEraUiPageRequest
    {
        public AutoEraUiPageRequest(int page)
            : this(page, PersistentId.Invalid)
        {
        }

        public AutoEraUiPageRequest(int page, PersistentId target)
        {
            Page = page;
            Target = target;
        }

        /// <summary>目标界面初始显示的页索引。</summary>
        public int Page { get; }

        /// <summary>
        /// 本页要操作的**稳定对象身份**（机器、区域对象、槽位……）；没有对象语义时为
        /// <see cref="PersistentId.Invalid"/>。
        ///
        /// 规格要求「传稳定ID，失效或无权限时禁用写操作并说明，**不自动按名字替换对象**」，
        /// 所以这里只带身份、不带名字：目标界面拿不到对象时应当禁用并解释，
        /// 而不是回到列表里按名称猜一个。
        /// </summary>
        public PersistentId Target { get; }
    }
}
