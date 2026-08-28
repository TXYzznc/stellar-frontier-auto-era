using System;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace AutoEra.Application
{
    /// <summary>
    /// The only FSM data slot that carries the AutoEra application context between
    /// product procedures. It is intentionally typed and does not expose arbitrary lookup.
    /// </summary>
    public static class AutoEraProcedureContextSlot
    {
        private const string DataKey = "AutoEra.ApplicationContext";

        public static bool TrySet(IFsm<IProcedureManager> procedureOwner, AutoEraApplicationContext context)
        {
            if (procedureOwner == null)
            {
                throw new ArgumentNullException(nameof(procedureOwner));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.IsDisposed || procedureOwner.HasData(DataKey))
            {
                return false;
            }

            VarObject variable = ReferencePool.Acquire<VarObject>();
            variable.Value = context;
            procedureOwner.SetData(DataKey, variable);
            return true;
        }

        public static bool TryGet(IFsm<IProcedureManager> procedureOwner, out AutoEraApplicationContext context)
        {
            if (procedureOwner == null)
            {
                throw new ArgumentNullException(nameof(procedureOwner));
            }

            context = null;
            if (!procedureOwner.HasData(DataKey))
            {
                return false;
            }

            VarObject variable = procedureOwner.GetData<VarObject>(DataKey);
            context = variable?.Value as AutoEraApplicationContext;
            return context != null && !context.IsDisposed;
        }

        public static bool TryTake(IFsm<IProcedureManager> procedureOwner, out AutoEraApplicationContext context)
        {
            if (!TryGet(procedureOwner, out context))
            {
                return false;
            }

            procedureOwner.RemoveData(DataKey);
            return true;
        }

        public static bool Clear(IFsm<IProcedureManager> procedureOwner)
        {
            if (procedureOwner == null)
            {
                throw new ArgumentNullException(nameof(procedureOwner));
            }

            return procedureOwner.RemoveData(DataKey);
        }
    }
}
