using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World.Identity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineHardwareUiEditModeTests
    {
        private static MachineRoster Roster()
        { var ids = new PersistentIdAllocator(); return new MachineRoster(ids, new PersistentObjectRegistry(ids)); }
        private static MachineDefinition Definition() => new MachineDefinition(1001,"Fixture",1,2,1,2,30,true,true,100);
        private static ComponentDefinition Cargo() => new ComponentDefinition(2201,HardwareKind.Effector,1,30,0,0,false);
        [Test]
        public void Picker_UsesInstanceIdentityAndRevalidatesOtherOwner()
        {
            using (var roster = Roster())
            {
                var first = roster.Create(Definition()); var second = roster.Create(Definition());
                var cargo = roster.CreateComponent(Cargo()); var same = roster.CreateComponent(Cargo());
                using (var view = new MachineHardwarePresenter(roster,first.Id,ManagementOrigin.Library))
                {
                    view.SelectSlot(HardwareKind.Effector,0); Assert.That(view.Candidates.Count,Is.EqualTo(2));
                    Assert.That(view.SelectCandidate(same.Id),Is.True); Assert.That(view.Preview(false),Is.True);
                    roster.Install(second.Id,ManagementOrigin.Library,same.Id,0);
                    Assert.That(view.Confirm(),Is.False); Assert.That(cargo.OwnerId.IsValid,Is.False);
                }
            }
        }
        [Test]
        public void Confirm_StopDrainThenInstallAndNeverResume()
        {
            using (var roster = Roster())
            {
                var machine = roster.Create(Definition()); roster.Deploy(machine.Id); machine.Activate(ManagementOrigin.Field);
                machine.UpdateEnvironment(true,true); machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                machine.UpdateBehaviorActivity(true); var cargo = roster.CreateComponent(Cargo());
                using(var view = new MachineHardwarePresenter(roster,machine.Id,ManagementOrigin.Field))
                {
                    view.SelectSlot(HardwareKind.Effector,0); view.SelectCandidate(cargo.Id); view.Preview(false);
                    Assert.That(view.Confirm(),Is.True); Assert.That(view.Layer,Is.EqualTo(MachinePanelLayer.Waiting));
                    Assert.That(cargo.OwnerId.IsValid,Is.False);
                    view.Back(); view.ToggleRun();
                    Assert.That(machine.RequestedRunState,Is.EqualTo(MachineRunState.Stopped),"Hiding waiting feedback must not restart accepted hardware work.");
                    machine.UpdateBehaviorActivity(false);
                    Assert.That(view.Layer,Is.EqualTo(MachinePanelLayer.Success)); Assert.That(cargo.OwnerId,Is.EqualTo(machine.Id));
                    Assert.That(machine.RequestedRunState,Is.EqualTo(MachineRunState.Stopped));
                }
            }
        }
        [Test]
        public void CancelAndDispose_PreventLateInstall()
        {
            using(var roster = Roster())
            {
                var machine = roster.Create(Definition()); roster.Deploy(machine.Id); machine.UpdateBehaviorActivity(true);
                var cargo = roster.CreateComponent(Cargo());
                var view = new MachineHardwarePresenter(roster,machine.Id,ManagementOrigin.Field);
                view.SelectSlot(HardwareKind.Effector,0); view.SelectCandidate(cargo.Id); view.Preview(false); view.Confirm();
                view.CancelWaiting(); view.Dispose(); machine.UpdateBehaviorActivity(false);
                Assert.That(cargo.OwnerId.IsValid,Is.False); Assert.That(machine.RequestedRunState,Is.EqualTo(MachineRunState.Stopped));
            }
        }
        [Test]
        public void CloseView_DoesNotCancelAuthoritativeWaitingOperation()
        {
            using(var roster = Roster())
            {
                var machine=roster.Create(Definition()); roster.Deploy(machine.Id); machine.UpdateBehaviorActivity(true);
                var cargo=roster.CreateComponent(Cargo());
                using(var view=new MachineHardwarePresenter(roster,machine.Id,ManagementOrigin.Field))
                { view.SelectSlot(HardwareKind.Effector,0); view.SelectCandidate(cargo.Id); view.Preview(false); view.Confirm(); }
                machine.UpdateBehaviorActivity(false);
                Assert.That(cargo.OwnerId,Is.EqualTo(machine.Id));
                Assert.That(roster.GetHardwareOperation(machine.Id).State,Is.EqualTo(HardwareOperationState.Completed));
            }
        }
        [Test]
        public void Remote_HardwareDeniedButAuthorizedRenameIsReal()
        {
            using(var roster = Roster())
            {
                var machine = roster.Create(Definition()); roster.Deploy(machine.Id); machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true,true);
                using(var view = new MachineHardwarePresenter(roster,machine.Id,ManagementOrigin.Hub))
                {
                    Assert.That(view.CanManage,Is.True); Assert.That(view.CanChangeHardware,Is.False);
                    view.SelectSlot(HardwareKind.Effector,0); Assert.That(view.Layer,Is.EqualTo(MachinePanelLayer.Overview));
                    Assert.That(view.Rename("Unique"),Is.EqualTo(MachineManagementResult.Completed)); Assert.That(machine.Name,Is.EqualTo("Unique"));
                    machine.UpdateEnvironment(true,false); Assert.That(view.Rename("Stale"),Is.EqualTo(MachineManagementResult.InvalidOrigin));
                }
            }
        }
        [Test]
        public void Removal_ProjectsRealCapacityAndRejectsOccupiedCapacity()
        {
            using(var roster = Roster())
            {
                var machine = roster.Create(Definition()); var cargo = roster.CreateComponent(Cargo());
                roster.Install(machine.Id,ManagementOrigin.Library,cargo.Id,0); machine.UpdateContainerUsage(45);
                using(var view = new MachineHardwarePresenter(roster,machine.Id,ManagementOrigin.Library))
                {
                    view.SelectSlot(HardwareKind.Effector,0); Assert.That(view.Layer,Is.EqualTo(MachinePanelLayer.Hardware));
                    view.Preview(true); Assert.That(view.TryGetImpact(out _,out _,out int capacity),Is.False); Assert.That(capacity,Is.EqualTo(30));
                    Assert.That(view.Confirm(),Is.False);
                }
            }
        }
    }
}
