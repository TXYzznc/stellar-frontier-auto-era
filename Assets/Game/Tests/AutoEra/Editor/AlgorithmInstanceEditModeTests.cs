using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmInstanceEditModeTests
    {
        private sealed class Sink : IAlgorithmCommandSink
        { public bool Safe=true; public bool IsSafe=>Safe; public ulong Submit(AlgorithmTrigger t,AlgorithmIntent i)=>t.TaskId; public void EndBatch(AlgorithmTrigger t){} public void Cancel(){} public bool TryReadCargo(string f,string i2,out AlgorithmValue v){v=null;return false;} public bool TryQueryTask(string n,out AlgorithmValue t){t=null;return false;} }
        private static AlgorithmRuntime Runtime(ulong id,MachineComputePool pool,Sink sink)
        {
            var g=AlgorithmExecutionEditModeTests.Graph(); g.DocumentId=id; g.Nodes[1].Kind=AlgorithmNodeKind.Parameter;
            Assert.That(AlgorithmValidator.TryCompile(g,100,out var p,out _),Is.True);
            return new AlgorithmRuntime(new PersistentId(id),p,pool,sink);
        }
        [Test]
        public void DraftAndPendingAreIsolated_StaleAndHardwareRevisionReject()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);var sink=new Sink();ulong hardware=1;
            using(var service=new AlgorithmInstanceService(ids,pool,()=>hardware,d=>true))
            {
                var r=Runtime(100,pool,sink);Assert.That(service.Add(r),Is.True);
                var draft=service.ReadDraft(100);draft.Nodes[1].Default.Number=20;
                Assert.That(r.CopyApplied().Nodes[1].Default.Number,Is.EqualTo(12));
                Assert.That(service.Edit(100,1,draft),Is.True);sink.Safe=false;
                Assert.That(service.Apply(100,2,1,1,out var request),Is.True);service.Pump(0);
                Assert.That(service.ReadRequest(100).State,Is.EqualTo(AlgorithmApplyState.WaitingSafePoint));
                Assert.That(service.Edit(100,2,service.ReadDraft(100)),Is.True);sink.Safe=true;service.Pump(1);
                Assert.That(service.ReadRequest(100).Reason,Is.EqualTo("StaleRevision"));Assert.That(r.Revision,Is.EqualTo(1));
                Assert.That(service.Apply(100,3,1,1,out request),Is.True);hardware=2;service.Pump(2);
                Assert.That(service.ReadRequest(100).Reason,Is.EqualTo("HardwareOrBindingChanged"));
            }
            Assert.That(pool.AppliedLogicCost,Is.Zero);
        }
        [Test]
        public void ParametersPreserveState_StructurePausesPeersAndResetsOnlyTarget()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var a=Runtime(100,pool,new Sink());var b=Runtime(200,pool,new Sink());service.Add(a);service.Add(b);
                a.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });b.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });service.Pump(0);
                var d=service.ReadDraft(100);d.Nodes[1].Default.Number=20;service.Edit(100,1,d);service.Apply(100,2,1,1,out _);
                service.Pump(1);Assert.That(b.Paused,Is.False);service.Pump(2);
                Assert.That(a.CopyState()["counter"].Number,Is.EqualTo(12));Assert.That(a.Revision,Is.EqualTo(2));
                // Consume the deliberate Startup event before requesting the next change.
                service.Pump(3);
                d=service.ReadDraft(100);d.Nodes[2].StateKey="newCounter";service.Edit(100,2,d);service.Apply(100,3,2,1,out _);
                service.Pump(4);Assert.That(a.Paused&&b.Paused,Is.True);service.Pump(5);
                Assert.That(a.CopyState(),Is.Empty);Assert.That(b.CopyState()["counter"].Number,Is.EqualTo(12));Assert.That(b.Paused,Is.False);
            }
        }
        [Test]
        public void CheckpointPreservesPendingRequestAndState_WithoutReplayingCompletedRoot()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });service.Pump(0);
                service.Edit(100,1,service.ReadDraft(100));service.SaveDraft(100,2);service.Apply(100,2,1,1,out var request);
                Assert.That(service.Capture(100,0,out var checkpoint),Is.True);
                service.CancelApply(100,request.RequestId);Assert.That(service.Restore(100,checkpoint,1000),Is.True);
                Assert.That(r.History().Length,Is.EqualTo(1));Assert.That(r.WaitingCount,Is.Zero);
                Assert.That(service.ReadRequest(100).RequestId,Is.EqualTo(request.RequestId));Assert.That(service.SavedDraftRevision(100),Is.EqualTo(2));
                Assert.That(r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 }),Is.False);
                service.Pump(1000);service.Pump(1001);Assert.That(r.Revision,Is.EqualTo(2));
            }
        }
        [Test]
        public void TemplatesStripBindings_AreIndependent_AndSystemTemplatesCannotBeDeleted()
        {
            var library=new AlgorithmTemplateLibrary(new PersistentIdAllocator());var g=AlgorithmExecutionEditModeTests.Graph();
            g.Bindings.Add(new AlgorithmBinding { Key="sensor",ComponentId=90,TargetId=91,Generation=2,Type=AlgorithmType.Of(AlgorithmValueKind.Number) });
            ulong id=library.Save("System",g,true);Assert.That(id,Is.Not.Zero);Assert.That(library.Delete(id,1),Is.False);Assert.That(library.Rename(id,1,"new"),Is.False);
            var a=library.Instantiate(id);var b=library.Instantiate(id);Assert.That(a.DocumentId,Is.Not.EqualTo(b.DocumentId));
            Assert.That(a.Nodes[0].Id,Is.Not.EqualTo(b.Nodes[0].Id));Assert.That(a.Bindings,Is.Empty);
            a.Nodes[1].Default.Number=999;Assert.That(b.Nodes[1].Default.Number,Is.EqualTo(12));
            ulong player=library.CopyAsPlayer(id,"Player");Assert.That(player,Is.Not.Zero);Assert.That(library.List().Length,Is.EqualTo(2));Assert.That(library.Delete(player,1),Is.True);
        }
        [Test]
        public void WarningsRequireExplicitConfirmation_AndDoNotInvalidateOldPlan()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);var d=service.ReadDraft(100);
                d.Nodes.Add(new AlgorithmNode { Id=9,Kind=AlgorithmNodeKind.Input,BindingKey="sensor" });
                d.Bindings.Add(new AlgorithmBinding { Key="sensor",ComponentId=80,TargetId=81,Generation=1,Available=false,Type=AlgorithmType.Of(AlgorithmValueKind.Number) });
                service.Edit(100,1,d);Assert.That(service.Apply(100,2,1,1,out var request),Is.True);
                Assert.That(request.State,Is.EqualTo(AlgorithmApplyState.AwaitingWarningConfirmation));service.Pump(0);Assert.That(r.Revision,Is.EqualTo(1));
                Assert.That(service.ConfirmWarnings(100,request.RequestId),Is.True);service.Pump(1);service.Pump(2);Assert.That(r.Revision,Is.EqualTo(2));
            }
        }

        [Test]
        public void DraftInstance_NoRuntime_ListsReadsAndGuardsWithoutThrow()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                // 模板实例化产物：未绑定、未编译。走 AddDraft，而非需要已编译运行时的 Add。
                var graph=AlgorithmExecutionEditModeTests.Graph();graph.DocumentId=300;
                Assert.That(service.AddDraft(graph),Is.True,"未绑定未编译的文档必须能作为草稿实例入库。");

                var list=service.ListInstances();
                Assert.That(list.Length,Is.EqualTo(1));
                Assert.That(list[0].Id,Is.EqualTo(300UL));
                Assert.That(list[0].AppliedRevision,Is.Zero,"草稿实例没有已应用版本。");
                Assert.That(list[0].LogicCost,Is.Zero);
                Assert.That(list[0].DraftRevision,Is.EqualTo(1UL));

                Assert.That(service.ReadDraft(300),Is.Not.Null);
                Assert.That(service.ReadHistory(300),Is.Empty,"草稿实例无运行历史。");
                Assert.That(service.SavedDraftRevision(300),Is.EqualTo(1UL));

                // 草稿实例不可应用/捕获（无运行时，语义上是中间态）。
                Assert.That(service.Apply(300,1,0,1,out _),Is.False);
                Assert.That(service.Capture(300,0,out _),Is.False);

                // 有草稿实例时 Pump/Dispose 不抛（空运行时守卫）。
                service.Pump(0);
            }
            Assert.That(pool.AppliedLogicCost,Is.Zero,"草稿实例不占逻辑算力。");
        }

        [Test]
        public void Rebind_UpdatesDraftBinding_AndIncrementsRevision()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var graph=AlgorithmExecutionEditModeTests.Graph();graph.DocumentId=300;
                graph.Nodes.Add(new AlgorithmNode { Id=9,Kind=AlgorithmNodeKind.Input,BindingKey="sensor" });
                Assert.That(service.AddDraft(graph),Is.True);

                // 新增绑定（无对应 Binding），Type 从节点 ValueType 派生。
                Assert.That(service.Rebind(300,1,"sensor",90,91,2),Is.True,"Rebind 应为新 BindingKey 新增绑定。");
                var draft=service.ReadDraft(300);
                Assert.That(draft.Revision,Is.EqualTo(2UL));
                var binding=draft.Bindings.Find(b=>b.Key=="sensor");
                Assert.That(binding,Is.Not.Null);
                Assert.That(binding.ComponentId,Is.EqualTo(90UL));
                Assert.That(binding.TargetId,Is.EqualTo(91UL));
                Assert.That(binding.Generation,Is.EqualTo(2UL));
                Assert.That(binding.Type,Is.Not.Null,"Type 应从节点 ValueType 派生。");

                // 更新已有绑定。
                Assert.That(service.Rebind(300,2,"sensor",80,81,3),Is.True);
                draft=service.ReadDraft(300);
                Assert.That(draft.Revision,Is.EqualTo(3UL));
                binding=draft.Bindings.Find(b=>b.Key=="sensor");
                Assert.That(binding.ComponentId,Is.EqualTo(80UL));

                // 无效：修订不匹配 / 空 Key / 实例不存在。
                Assert.That(service.Rebind(300,1,"sensor",70,71,4),Is.False,"修订不匹配必须拒绝。");
                Assert.That(service.Rebind(300,3,"",70,71,4),Is.False);
                Assert.That(service.Rebind(999,1,"sensor",70,71,4),Is.False);
            }
        }

        [Test]
        public void MoveNode_UpdatesLayoutAndIncrementsRevision()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var graph=AlgorithmExecutionEditModeTests.Graph();graph.DocumentId=300;
                Assert.That(service.AddDraft(graph),Is.True);

                Assert.That(service.MoveNode(300,1,1,12.5f,-3.25f),Is.True,"MoveNode 应更新画布坐标。");
                var draft=service.ReadDraft(300);
                Assert.That(draft.Revision,Is.EqualTo(2UL));
                var node=draft.Nodes.Find(n=>n.Id==1);
                Assert.That(node.LayoutX,Is.EqualTo(12.5f));
                Assert.That(node.LayoutY,Is.EqualTo(-3.25f));

                // 无效：修订不匹配 / 节点不存在 / 实例不存在。
                Assert.That(service.MoveNode(300,1,1,1,1),Is.False,"修订不匹配必须拒绝。");
                Assert.That(service.MoveNode(300,2,9999,1,1),Is.False,"节点不存在必须拒绝。");
                Assert.That(service.MoveNode(999,1,1,1,1),Is.False);
            }
        }

        [Test]
        public void CompileDraft_ActivatesRuntime_AndRejectsNonDraftOrMismatch()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);var sink=new Sink();
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var graph=AlgorithmExecutionEditModeTests.Graph();graph.DocumentId=300;graph.Nodes[1].Kind=AlgorithmNodeKind.Parameter;
                Assert.That(service.AddDraft(graph),Is.True);

                // 编译草稿 → 挂接运行时（Graph 无 Input/Effector 端点，编译必通过）。
                var compiled=AlgorithmExecutionEditModeTests.Graph();compiled.DocumentId=300;compiled.Nodes[1].Kind=AlgorithmNodeKind.Parameter;
                Assert.That(AlgorithmValidator.TryCompile(compiled,100,out var plan,out _),Is.True);
                var runtime=new AlgorithmRuntime(new PersistentId(300),plan,pool,sink);
                Assert.That(service.CompileDraft(300,runtime),Is.True,"草稿实例应能挂接运行时。");
                var info=service.ListInstances();
                Assert.That(info.Length,Is.EqualTo(1));
                Assert.That(info[0].AppliedRevision,Is.EqualTo(1UL),"激活后 AppliedRevision 应为运行时修订 1。");
                Assert.That(pool.AppliedLogicCost,Is.GreaterThan(0),"激活应占用算力。");

                // 已持有运行时：再次 CompileDraft 拒绝。
                Assert.That(service.CompileDraft(300,runtime),Is.False,"已持有运行时必须拒绝。");

                // Id 不一致：拒绝。
                var other=Runtime(400,pool,sink);
                Assert.That(service.CompileDraft(300,other),Is.False,"Id 不一致必须拒绝。");

                // 非安全运行时：拒绝（先加一个草稿 500，再用非安全 sink 编译）。
                sink.Safe=false;
                var g500=AlgorithmExecutionEditModeTests.Graph();g500.DocumentId=500;g500.Nodes[1].Kind=AlgorithmNodeKind.Parameter;
                Assert.That(service.AddDraft(g500),Is.True);
                Assert.That(AlgorithmValidator.TryCompile(g500,100,out var plan500,out _),Is.True);
                var unsafeRuntime=new AlgorithmRuntime(new PersistentId(500),plan500,pool,sink);
                Assert.That(service.CompileDraft(500,unsafeRuntime),Is.False,"非安全运行时必须拒绝。");
            }
        }

        [Test]
        public void ApplyOverload_UsesInternalHardwareRevision()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);var sink=new Sink();ulong hardware=7;
            using(var service=new AlgorithmInstanceService(ids,pool,()=>hardware,d=>true))
            {
                var r=Runtime(100,pool,sink);service.Add(r);
                var d=service.ReadDraft(100);d.Nodes[1].Default.Number=20;service.Edit(100,1,d);

                // 4 参数重载：显式传硬件修订。
                Assert.That(service.Apply(100,2,1,7,out var req4),Is.True);
                service.CancelApply(100,req4.RequestId);

                // 3 参数重载：内部取 _hardwareRevision() == 7，行为与 4 参数一致。
                Assert.That(service.Apply(100,2,1,out var req3),Is.True,"无 hardware 重载应内部取硬件修订 7。");
                Assert.That(req3.HardwareRevision,Is.EqualTo(7UL));

                // 硬件修订不匹配时 3 参数重载同样拒绝。
                service.CancelApply(100,req3.RequestId);
                hardware=8;
                Assert.That(service.Apply(100,2,1,out _),Is.True,"3 参数重载总是取当前硬件修订，故仍应成功。");
                hardware=7;
            }
        }

        [Test]
        public void CreateNode_AllocatesStableIdAndBumpsRevision_StaleRevisionRejects()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);
                var draft=service.ReadDraft(100);

                Assert.That(service.CreateNode(100,draft.Revision,AlgorithmNodeKind.Constant,12.5f,-8f,out var nodeId),Is.True,"匹配修订的创建必须成功。");
                Assert.That(nodeId,Is.GreaterThan(0UL),"服务必须分配非零稳定 Id。");
                var updated=service.ReadDraft(100);
                Assert.That(updated.Revision,Is.EqualTo(draft.Revision+1),"创建成功后草稿修订加一。");
                var created=updated.Nodes.Find(n=>n.Id==nodeId);
                Assert.That(created,Is.Not.Null);
                Assert.That(created.Kind,Is.EqualTo(AlgorithmNodeKind.Constant));
                Assert.That(created.LayoutX,Is.EqualTo(12.5f));
                Assert.That(created.LayoutY,Is.EqualTo(-8f));
                Assert.That(created.Default,Is.Not.Null,"值节点必须带有限默认值。");

                // 过期修订：拒绝且草稿保持不变。
                Assert.That(service.CreateNode(100,draft.Revision,AlgorithmNodeKind.Log,0,0,out _),Is.False,"过期修订必须拒绝。");
                Assert.That(service.ReadDraft(100).Revision,Is.EqualTo(draft.Revision+1),"拒绝后草稿修订不变。");

                // 未定义种类：拒绝。
                Assert.That(service.CreateNode(100,updated.Revision,(AlgorithmNodeKind)999,0,0,out _),Is.False,"未定义种类必须拒绝。");
            }
        }

        [Test]
        public void Connect_StrongTypedAndInputExclusive_DisconnectExact()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);
                var draft=service.ReadDraft(100);
                Assert.That(service.CreateNode(100,draft.Revision,AlgorithmNodeKind.Arithmetic,0,0,out var math),Is.True);
                Assert.That(service.CreateNode(100,service.ReadDraft(100).Revision,AlgorithmNodeKind.Boolean,0,0,out var boolean),Is.True);
                ulong rev=service.ReadDraft(100).Revision;

                // Constant(Number,2) "value" → Arithmetic "a"：兼容，连接成功。
                Assert.That(service.Connect(100,rev,2,"value",math,"a"),Is.True,"兼容端口连接必须成功。");
                // 同一目标输入第二次连接拒绝（输入唯一）。
                Assert.That(service.Connect(100,service.ReadDraft(100).Revision,2,"value",math,"a"),Is.False,"已占用输入必须拒绝。");
                rev=service.ReadDraft(100).Revision;
                // Number → Boolean "a"：类型不兼容拒绝。
                Assert.That(service.Connect(100,rev,2,"value",boolean,"a"),Is.False,"类型不兼容必须拒绝。");
                // 端口不存在拒绝（输出侧/输入侧各一）。
                Assert.That(service.Connect(100,rev,2,"nope",math,"a"),Is.False,"不存在的输出端口必须拒绝。");
                Assert.That(service.Connect(100,rev,2,"value",math,"nope"),Is.False,"不存在的输入端口必须拒绝。");
                // 不存在的节点拒绝。
                Assert.That(service.Connect(100,rev,999,"value",math,"a"),Is.False,"不存在的源节点必须拒绝。");
                Assert.That(service.ReadDraft(100).Revision,Is.EqualTo(rev),"全部拒绝后草稿修订不变。");

                // 精确断开：只移除目标边，不影响其余边。
                Assert.That(service.Disconnect(100,rev,2,"value",math,"a"),Is.True,"精确断开已存在边必须成功。");
                var after=service.ReadDraft(100);
                Assert.That(after.Edges.Find(e=>e.From==2&&e.To==math&&e.Input=="a"),Is.Null,"目标边应被移除。");
                Assert.That(after.Edges.Find(e=>e.From==2&&e.To==3&&e.Input=="value"),Is.Not.Null,"其余边不受影响。");
                // 重复断开同一条边拒绝。
                Assert.That(service.Disconnect(100,after.Revision,2,"value",math,"a"),Is.False,"边已不存在必须拒绝。");
            }
        }

        [Test]
        public void DeleteNode_CascadesEdges_BumpsRevision_StaleRevisionRejects()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);
                var draft=service.ReadDraft(100);
                Assert.That(service.CreateNode(100,draft.Revision,AlgorithmNodeKind.Arithmetic,0,0,out var math),Is.True);
                ulong rev=service.ReadDraft(100).Revision;
                // 2(Constant) → math 的 a 口；既有图 2 → 3(SetVariable value) 保持。
                Assert.That(service.Connect(100,rev,2,"value",math,"a"),Is.True);

                // 过期修订拒绝（Connect 已把修订推到 rev+1，rev 已过期）。
                Assert.That(service.DeleteNode(100,rev,math),Is.False,"过期修订必须拒绝。");
                Assert.That(service.ReadDraft(100).Revision,Is.EqualTo(rev+1),"拒绝后修订不变。");

                // 正常删除：节点软删 + 关联边级联移除，其余边不动。
                Assert.That(service.DeleteNode(100,rev+1,math),Is.True,"删除存在节点必须成功。");
                var after=service.ReadDraft(100);
                Assert.That(after.Revision,Is.EqualTo(rev+2),"删除成功后修订加一。");
                Assert.That(after.Nodes.Find(n=>n.Id==math).Deleted,Is.True,"节点应软删。");
                Assert.That(after.Edges.Find(e=>e.To==math),Is.Null,"指向被删节点的边必须级联移除。");
                Assert.That(after.Edges.Find(e=>e.From==2&&e.To==math),Is.Null,"从被删节点出发的边必须级联移除。");
                Assert.That(after.Edges.Find(e=>e.From==2&&e.To==3&&e.Input=="value"),Is.Not.Null,"与被删节点无关的边不受影响。");

                // 重复删除拒绝。
                Assert.That(service.DeleteNode(100,after.Revision,math),Is.False,"已删除节点再删必须拒绝。");
                // 不存在节点拒绝。
                Assert.That(service.DeleteNode(100,after.Revision,999),Is.False,"不存在的节点必须拒绝。");
            }
        }
    }
}
