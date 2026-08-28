using System;
using System.Collections.Generic;
using AutoEra.Application;
using AutoEra.World;
using AutoEra.World.Time;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using NUnit.Framework;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraProcedureContextSlotEditModeTests
    {
        [Test]
        public void CompositionRoot_CreatesInjectableIndependentContexts()
        {
            var root = new AutoEraApplicationCompositionRoot();
            var provider = new FixedUtcTimeProvider(DateTimeOffset.UtcNow);

            using (AutoEraApplicationContext first = root.Create(provider, new AutoEraWorldSessionFactory()))
            using (AutoEraApplicationContext second = root.Create(provider, new AutoEraWorldSessionFactory()))
            {
                Assert.That(first.UtcTimeProvider, Is.SameAs(provider));
                Assert.That(second.UtcTimeProvider, Is.SameAs(provider));
                Assert.That(first, Is.Not.SameAs(second));
                Assert.That(first.WorldSessionFactory, Is.Not.SameAs(second.WorldSessionFactory));
            }
        }

        [Test]
        public void ProcedureSlot_HoldsOnlyOneLiveContextAndCanTransferIt()
        {
            var fsm = new FakeProcedureFsm();
            var context = new AutoEraApplicationCompositionRoot().Create();
            var rejectedContext = new AutoEraApplicationCompositionRoot().Create();

            Assert.That(AutoEraProcedureContextSlot.TrySet(fsm, context), Is.True);
            Assert.That(AutoEraProcedureContextSlot.TrySet(fsm, rejectedContext), Is.False);
            rejectedContext.Dispose();
            Assert.That(AutoEraProcedureContextSlot.TryGet(fsm, out AutoEraApplicationContext visible), Is.True);
            Assert.That(visible, Is.SameAs(context));
            Assert.That(AutoEraProcedureContextSlot.TryTake(fsm, out AutoEraApplicationContext transferred), Is.True);
            Assert.That(transferred, Is.SameAs(context));
            Assert.That(AutoEraProcedureContextSlot.TryGet(fsm, out _), Is.False);

            context.Dispose();
        }

        [Test]
        public void ProcedureSlot_RejectsDisposedContextAndClearIsIdempotent()
        {
            var fsm = new FakeProcedureFsm();
            var context = new AutoEraApplicationCompositionRoot().Create();
            context.Dispose();

            Assert.That(AutoEraProcedureContextSlot.TrySet(fsm, context), Is.False);
            Assert.That(AutoEraProcedureContextSlot.Clear(fsm), Is.False);
            Assert.That(AutoEraProcedureContextSlot.TryGet(fsm, out _), Is.False);
        }

        private sealed class FixedUtcTimeProvider : IUtcTimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedUtcTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public DateTimeOffset GetUtcNow()
            {
                return _utcNow;
            }
        }

        private sealed class FakeProcedureFsm : IFsm<IProcedureManager>
        {
            private readonly Dictionary<string, Variable> _data = new Dictionary<string, Variable>(StringComparer.Ordinal);

            public string Name => "AutoEraProcedureContextSlotTests";
            public string FullName => Name;
            public IProcedureManager Owner => null;
            public int FsmStateCount => 0;
            public bool IsRunning => false;
            public bool IsDestroyed => false;
            public FsmState<IProcedureManager> CurrentState => null;
            public float CurrentStateTime => 0f;

            public void Start<TState>() where TState : FsmState<IProcedureManager> => throw new NotSupportedException();
            public void Start(Type stateType) => throw new NotSupportedException();
            public bool HasState<TState>() where TState : FsmState<IProcedureManager> => false;
            public bool HasState(Type stateType) => false;
            public TState GetState<TState>() where TState : FsmState<IProcedureManager> => null;
            public FsmState<IProcedureManager> GetState(Type stateType) => null;
            public FsmState<IProcedureManager>[] GetAllStates() => Array.Empty<FsmState<IProcedureManager>>();
            public void GetAllStates(List<FsmState<IProcedureManager>> results) { }
            public bool HasData(string name) => _data.ContainsKey(name);
            public TData GetData<TData>(string name) where TData : Variable => (TData)GetData(name);
            public Variable GetData(string name) => _data.TryGetValue(name, out Variable value) ? value : null;

            public void SetData<TData>(string name, TData data) where TData : Variable
            {
                SetData(name, (Variable)data);
            }

            public void SetData(string name, Variable data)
            {
                if (_data.TryGetValue(name, out Variable oldData))
                {
                    ReferencePool.Release(oldData);
                }

                _data[name] = data;
            }

            public bool RemoveData(string name)
            {
                if (!_data.TryGetValue(name, out Variable data))
                {
                    return false;
                }

                ReferencePool.Release(data);
                return _data.Remove(name);
            }
        }
    }
}
