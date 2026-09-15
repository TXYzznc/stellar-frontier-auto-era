using System;
using AutoEra.Application;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace AutoEra.Procedures
{
    [Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName)]
    public sealed class AutoEraStartupProcedure : ProcedureBase, IFrameworkStartupProcedure
    {
        protected override void OnEnter(IFsm<IProcedureManager> owner)
        {
            base.OnEnter(owner);
            AutoEraRuntimeSettings.Load(key => GF.Config.GetString(key));
            AutoEraRuntimeSettings.ValidateLoadedStartupData();
            var context = new AutoEraApplicationCompositionRoot().Create();
            if (!AutoEraProcedureContextSlot.TrySet(owner, context))
            { context.Dispose(); throw new InvalidOperationException("Application context already exists."); }
            ChangeState<AutoEraMainMenuProcedure>(owner);
        }

        protected override void OnDestroy(IFsm<IProcedureManager> owner)
        {
            if (AutoEraProcedureContextSlot.TryTake(owner, out var context)) context.Dispose();
            base.OnDestroy(owner);
        }
    }
}
