namespace AutoEra.UI
{
    /// <summary>
    /// GF UIDialog 语义的统一基类：确认、输入和危险操作都是短生命周期模态窗，
    /// 由调用方通过参数提供上下文，关闭时不保留自己的领域状态。
    /// </summary>
    public abstract class AutoEraUIDialogFormBase : AutoEraShellFormBase
    {
        public override bool BlocksWorldInput => true;

        protected bool HasDialogRequest<TRequest>() where TRequest : class =>
            TryGetRequest(out TRequest request) && request != null;
    }
}
