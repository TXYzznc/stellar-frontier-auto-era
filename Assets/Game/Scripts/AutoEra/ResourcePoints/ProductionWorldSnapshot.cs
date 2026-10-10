using System;
using System.Collections.Generic;
using AutoEra.Logistics;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.ResourcePoints
{
    public sealed class ProductionWorldSnapshot
    {
        public int Version=1;
        public long CapturedAt;
        public ForestProductionSnapshot[] Forests;
        public MineralProductionSnapshot[] Minerals;
        public GroundRecord[] Grounds;
        public ResponsibilityRecord[] Responsibilities;
        public sealed class GroundRecord {public ulong Point;public Vector3 Position;}
        public sealed class ResponsibilityRecord {public ulong Producer,Machine,Component,Task,Behavior,Correlation;}
    }
    public sealed class ProductionRulesSnapshot
    {
        public int[] Counts;
        public Vector2[] Curve;
        public double Spacing,Sapling,Mature,Maximum,Recovery,Wood,Saw,Resistance,DrillMinimum,DrillMaximum;
        public ProductionRules Read() => new ProductionRules(Counts,Spacing,Sapling,Mature,Maximum,Curve,Recovery,Wood,Saw,Resistance,DrillMinimum,DrillMaximum);
        internal bool Matches(ProductionRules other)
        {
            if(other==null)return false;var b=other.CapturePersistent();
            if(Spacing!=b.Spacing || Sapling!=b.Sapling || Mature!=b.Mature || Maximum!=b.Maximum || Recovery!=b.Recovery || Wood!=b.Wood || Saw!=b.Saw ||
                Resistance!=b.Resistance || DrillMinimum!=b.DrillMinimum || DrillMaximum!=b.DrillMaximum || Counts==null || Curve==null || Counts.Length!=b.Counts.Length || Curve.Length!=b.Curve.Length)return false;
            for(int i=0;i<Counts.Length;i++)if(Counts[i]!=b.Counts[i])return false;
            for(int i=0;i<Curve.Length;i++)if(!Curve[i].Equals(b.Curve[i]))return false;return true;
        }
    }
    public sealed partial class ProductionRules
    {
        public ProductionRulesSnapshot CapturePersistent() => new ProductionRulesSnapshot {Counts=(int[])_counts.Clone(),Curve=(Vector2[])_curve.Clone(),
            Spacing=Spacing,Sapling=SaplingHeight,Mature=MatureHeight,Maximum=MaximumHeight,Recovery=StumpRecoverySeconds,Wood=WoodPerMeter,
            Saw=SawDamage,Resistance=MiningResistance,DrillMinimum=DrillMinimumDamage,DrillMaximum=DrillMaximumDamage};
    }
    public sealed class ForestProductionSnapshot
    {
        public ulong Point;
        public long Now,Revision;
        public double WoodRemainder;
        public ProductionRulesSnapshot Rules;
        public TreeRecord[] Trees;
        public sealed class TreeRecord
        {
            public ulong Id,FellingSequence,DamageSequence;
            public Vector3 Position,FallDirection;
            public double Height,HP,UpperLength,FractureHeight,LastDamage;
            public TreeStage Stage;
            public long FallingDeadline,RecoverAt,DamageTime;
            public bool DamageFelled;
        }
    }
    public sealed class MineralProductionSnapshot
    {
        public ulong Point;
        public long Now,Revision,DepletedAt,CleanupAt;
        public int Total,Produced,FullRockCount;
        public double Fraction;
        public bool Removed;
        public int[] RockOrder;
        public ProductionRulesSnapshot Rules;
        public DrillStep[] Steps;
        public sealed class DrillStep {public ulong Behavior,Sequence;public int Units;}
    }
    public sealed partial class ForestProduction
    {
        public ForestProductionSnapshot CapturePersistent()
        {
            if(_disposed || !_cargo.IsAtCommitBoundary)throw new InvalidOperationException("Forest is not at a capture boundary.");
            var trees=new ForestProductionSnapshot.TreeRecord[_trees.Count];
            for(int i=0;i<trees.Length;i++)
            {
                var t=_trees[i];trees[i]=new ForestProductionSnapshot.TreeRecord {Id=t.Id.Value,Position=t.Position,FallDirection=t.FallDirection,
                    Height=t.Height,HP=t.HP,UpperLength=t.UpperLength,FractureHeight=t.FractureHeight,Stage=t.Stage,FellingSequence=t.FellingSequence,
                    FallingDeadline=t.FallingDeadline,RecoverAt=t.RecoverAt,DamageSequence=t.LastDamageSequence,LastDamage=t.LastDamage,DamageTime=t.LastDamageTime,DamageFelled=t.LastDamageFelled};
            }
            return new ForestProductionSnapshot {Point=Id.Value,Now=_now,Revision=Revision,WoodRemainder=_woodRemainder,Rules=_rules.CapturePersistent(),Trees=trees};
        }
        public static ForestProduction RestorePersistent(ForestProductionSnapshot saved,PersistentObjectRegistry registry,CargoOwnershipAuthority cargo,long worldMilliseconds)
        {
            if(saved?.Rules==null || saved.Trees==null || saved.Trees.Length==0 || saved.Point==0 || registry==null || cargo==null || !cargo.IsAtCommitBoundary ||
                saved.Now<0 || saved.Now>worldMilliseconds || saved.Revision<0 || !ProductionRules.Finite(saved.WoodRemainder) || saved.WoodRemainder<0 || saved.WoodRemainder>=1)
                throw new ArgumentException("Invalid forest checkpoint.");
            var rules=saved.Rules.Read();var ids=new HashSet<ulong>();var positions=new HashSet<Vector3>();
            foreach(var t in saved.Trees)
            {
                if(t==null || t.Id==0 || t.Id==saved.Point || !ids.Add(t.Id) || !Finite(t.Position) || !positions.Add(t.Position) || !Finite(t.FallDirection) ||
                    !Enum.IsDefined(typeof(TreeStage),t.Stage) || !ProductionRules.Finite(t.Height) || t.Height<0 || t.Height>rules.MaximumHeight ||
                    !ProductionRules.Finite(t.HP) || t.HP<0 || t.HP>t.Height || !ProductionRules.Finite(t.UpperLength) || t.UpperLength<0 ||
                    !ProductionRules.Finite(t.FractureHeight) || t.FractureHeight<0 || t.FractureHeight>rules.MaximumHeight ||
                    !ProductionRules.Finite(t.LastDamage) || t.LastDamage<0 || t.DamageTime<0 || t.DamageTime>worldMilliseconds || t.FallingDeadline<0 || t.RecoverAt<0)
                    throw new ArgumentException("Invalid original tree identity or state.");
                if((t.Stage==TreeStage.Growing || t.Stage==TreeStage.Mature) && t.Height<rules.SaplingHeight ||
                    t.Stage==TreeStage.Growing && t.Height>=rules.MatureHeight || t.Stage==TreeStage.Mature && t.Height<rules.MatureHeight ||
                    t.Stage==TreeStage.Falling && (t.FellingSequence==0 || t.UpperLength<=0 || Math.Abs(t.UpperLength+t.FractureHeight-t.Height)>1e-8 ||
                        Math.Abs(t.FallDirection.sqrMagnitude-1)>1e-5 || Math.Abs(t.FallDirection.y)>1e-6) ||
                    t.Stage==TreeStage.Stump && (t.FellingSequence==0 || t.Height!=t.FractureHeight || t.HP!=t.Height) ||
                    t.DamageSequence>0 && t.LastDamage<=0)throw new ArgumentException("Incoherent original tree lifecycle.");
                bool settled=cargo.TryReadProduction(new PersistentId(t.Id),t.FellingSequence,out var receipt);
                if(t.Stage==TreeStage.Falling && settled || t.Stage!=TreeStage.Falling && t.FellingSequence>0 &&
                    (!settled || receipt.Owner.Id.Value!=saved.Point || receipt.Item!=ResourceItemCatalog.Wood))
                    throw new ArgumentException("Tree state disagrees with its original production receipt.");
            }
            var owner=new CargoOwner(CargoOwnerKind.WorldFree,new PersistentId(saved.Point));
            if(!cargo.TryReadContainer(owner,out var pile) || pile.Kind!=CargoContainerKind.WorldFree)throw new ArgumentException("Original forest pile is missing.");
            return new ForestProduction(saved,rules,registry,cargo);
        }
        private ForestProduction(ForestProductionSnapshot saved,ProductionRules rules,PersistentObjectRegistry registry,CargoOwnershipAuthority cargo)
        {
            Id=new PersistentId(saved.Point);GroundOwner=new CargoOwner(CargoOwnerKind.WorldFree,Id);_rules=rules;_registry=registry;_cargo=cargo;
            _now=saved.Now;Revision=saved.Revision;_woodRemainder=saved.WoodRemainder;
            try
            {
                foreach(var t in saved.Trees)
                {
                    var tree=new ForestTree {Id=new PersistentId(t.Id),Position=t.Position,FallDirection=t.FallDirection,Height=t.Height,HP=t.HP,
                        UpperLength=t.UpperLength,FractureHeight=t.FractureHeight,Stage=t.Stage,FellingSequence=t.FellingSequence,FallingDeadline=t.FallingDeadline,
                        RecoverAt=t.RecoverAt,LastDamageSequence=t.DamageSequence,LastDamage=t.LastDamage,LastDamageTime=t.DamageTime,LastDamageFelled=t.DamageFelled};
                    if(tree.Stage!=TreeStage.Removed && _registry.TryRegister(tree.Id,PersistentObjectKind.Tree,tree)!=PersistentRegistryResult.Success)
                        throw new ArgumentException("Recovered tree identity is occupied.");
                    _trees.Add(tree);_byId.Add(tree.Id,tree);
                }
            }
            catch {Dispose();throw;}
        }
    }
    public sealed partial class MineralProduction
    {
        public MineralProductionSnapshot CapturePersistent()
        {
            if(!_cargo.IsAtCommitBoundary)throw new InvalidOperationException("Mineral is not at a capture boundary.");
            var steps=new List<MineralProductionSnapshot.DrillStep>();
            foreach(var r in _lastSteps.Values)steps.Add(new MineralProductionSnapshot.DrillStep {Behavior=r.ProducerId.Value,Sequence=r.Sequence,Units=r.Units});
            steps.Sort((a,b)=>a.Behavior.CompareTo(b.Behavior));
            return new MineralProductionSnapshot {Point=Id.Value,Now=_now,Revision=Revision,Total=TotalUnits,Produced=ProducedUnits,Fraction=_fraction,
                FullRockCount=FullRockCount,RockOrder=(int[])_rockOrder.Clone(),DepletedAt=DepletedAt,CleanupAt=CleanupAt,Removed=IsRemoved,Rules=_rules.CapturePersistent(),Steps=steps.ToArray()};
        }
        public static MineralProduction RestorePersistent(MineralProductionSnapshot saved,CargoOwnershipAuthority cargo,long worldMilliseconds)
        {
            if(saved?.Rules==null || saved.Steps==null || saved.RockOrder==null || saved.Point==0 || cargo==null || !cargo.IsAtCommitBoundary || saved.Now<0 || saved.Now>worldMilliseconds ||
                saved.Revision<0 || saved.Produced<0 || saved.Produced>saved.Total || !ProductionRules.Finite(saved.Fraction) || saved.Fraction<0 || saved.Fraction>=1 ||
                saved.Total-saved.Produced-saved.Fraction<0 || !(saved.Total==100 && saved.FullRockCount==6 || saved.Total==250 && saved.FullRockCount==8 || saved.Total==500 && saved.FullRockCount==10) ||
                saved.RockOrder.Length!=saved.FullRockCount)throw new ArgumentException("Invalid mineral checkpoint.");
            var order=new HashSet<int>();foreach(int index in saved.RockOrder)if(index<0 || index>=saved.FullRockCount || !order.Add(index))throw new ArgumentException("Invalid original rock order.");
            var owner=new CargoOwner(CargoOwnerKind.WorldFree,new PersistentId(saved.Point));
            if(!cargo.TryReadContainer(owner,out var pile) || pile.Kind!=CargoContainerKind.WorldFree)throw new ArgumentException("Original mineral pile is missing.");
            bool depleted=saved.Produced==saved.Total;
            if(depleted ? saved.Fraction!=0 || saved.DepletedAt<0 || saved.DepletedAt>worldMilliseconds || saved.CleanupAt<=saved.DepletedAt :
                saved.DepletedAt!=-1 || saved.CleanupAt!=-1 || saved.Removed)throw new ArgumentException("Incoherent depletion checkpoint.");
            if(depleted)
            {
                long day=AutoEra.Energy.DaylightCycle.DayMilliseconds;
                if(saved.DepletedAt/day>=long.MaxValue/day-1 || saved.CleanupAt!=checked((saved.DepletedAt/day+1)*day))
                    throw new ArgumentException("Original mineral cleanup time changed.");
            }
            if(saved.Removed && worldMilliseconds<saved.CleanupAt)throw new ArgumentException("Mineral removed before its original cleanup boundary.");
            var behaviors=new HashSet<ulong>();
            foreach(var step in saved.Steps)
            {
                if(step==null || step.Behavior==0 || step.Sequence==0 || step.Units<0 || !behaviors.Add(step.Behavior))throw new ArgumentException("Invalid drill retry identity.");
                bool hasReceipt=cargo.TryReadProduction(new PersistentId(step.Behavior),step.Sequence,out var receipt);
                if(step.Units>0 && !hasReceipt || hasReceipt && (receipt.Units!=step.Units || !receipt.Owner.Equals(owner) || receipt.Item!=ResourceItemCatalog.Ore))
                    throw new ArgumentException("Drill step disagrees with its original receipt.");
            }
            long produced=0;
            if(!cargo.TryCapturePersistent(out var inventory))throw new ArgumentException("Production authority is not at a boundary.");
            foreach(var receipt in inventory.Production)if(receipt.Owner==saved.Point && receipt.Item==ResourceItemCatalog.Ore)produced=checked(produced+receipt.Units);
            if(produced!=saved.Produced)throw new ArgumentException("Mineral stock disagrees with committed production.");
            return new MineralProduction(saved,saved.Rules.Read(),cargo);
        }
        private MineralProduction(MineralProductionSnapshot saved,ProductionRules rules,CargoOwnershipAuthority cargo)
        {
            Id=new PersistentId(saved.Point);GroundOwner=new CargoOwner(CargoOwnerKind.WorldFree,Id);_rules=rules;_cargo=cargo;_rockOrder=(int[])saved.RockOrder.Clone();
            TotalUnits=saved.Total;ProducedUnits=saved.Produced;FullRockCount=saved.FullRockCount;_fraction=saved.Fraction;_now=saved.Now;
            DepletedAt=saved.DepletedAt;CleanupAt=saved.CleanupAt;IsRemoved=saved.Removed;Revision=saved.Revision;
            foreach(var step in saved.Steps)_lastSteps.Add(new PersistentId(step.Behavior),new ResourceProductionReceipt(new PersistentId(step.Behavior),step.Sequence,GroundOwner,ResourceItemCatalog.Ore,step.Units,default));
        }
    }

    public sealed partial class ResourceProductionWorldService
    {
        public bool TryCapturePersistent(out ProductionWorldSnapshot saved)
        {
            saved=null;if(_disposed || _advancing || !_world.IsActive || !_world.Resources.Authority.IsAtCommitBoundary)return false;
            long now=_world.Clock.WorldMilliseconds;
            var forests=new List<ForestProductionSnapshot>();var minerals=new List<MineralProductionSnapshot>();
            foreach(var forest in _orderedForests)
            {
                var data=forest.CapturePersistent();if(data.Now!=now)return false;
                foreach(var tree in data.Trees)if(tree.DamageTime>now)return false;
                forests.Add(data);
            }
            foreach(var mineral in _minerals.Values)
            {var data=mineral.CapturePersistent();if(data.Now>now)return false;minerals.Add(data);}
            minerals.Sort((a,b)=>a.Point.CompareTo(b.Point));
            var grounds=new List<ProductionWorldSnapshot.GroundRecord>();
            foreach(var row in _groundPositions)
            {
                if(!_forests.ContainsKey(row.Key) && !_minerals.ContainsKey(row.Key))return false;
                grounds.Add(new ProductionWorldSnapshot.GroundRecord {Point=row.Key.Value,Position=row.Value});
            }
            grounds.Sort((a,b)=>a.Point.CompareTo(b.Point));
            var responsibilities=new List<ProductionWorldSnapshot.ResponsibilityRecord>();
            foreach(var row in _responsibilities)
            {
                var r=row.Value;responsibilities.Add(new ProductionWorldSnapshot.ResponsibilityRecord {Producer=row.Key.Value,
                    Machine=r.Machine.Value,Component=r.Component.Value,Task=r.Task.Value,Behavior=r.Behavior.Value,Correlation=r.Correlation.Value});
            }
            responsibilities.Sort((a,b)=>a.Producer.CompareTo(b.Producer));
            saved=new ProductionWorldSnapshot {CapturedAt=now,Forests=forests.ToArray(),Minerals=minerals.ToArray(),Grounds=grounds.ToArray(),Responsibilities=responsibilities.ToArray()};return true;
        }
        public bool TryRestorePersistent(ProductionWorldSnapshot saved,out string reason)
        {
            reason="Incomplete production world checkpoint";
            if(_disposed || _advancing || !_world.IsActive || _forests.Count!=0 || _minerals.Count!=0 || _groundPositions.Count!=0 || _responsibilities.Count!=0 ||
                saved?.Version!=1 || saved.Forests==null || saved.Minerals==null || saved.Grounds==null || saved.Responsibilities==null || saved.CapturedAt!=_world.Clock.WorldMilliseconds)return false;
            var forests=new Dictionary<PersistentId,ForestProduction>();var minerals=new Dictionary<PersistentId,MineralProduction>();
            var grounds=new Dictionary<PersistentId,Vector3>();var responsibilities=new Dictionary<PersistentId,Responsibility>();bool committed=false;
            try
            {
                ulong through=_world.IdAllocator.IsExhausted ? ulong.MaxValue : _world.IdAllocator.NextId.Value-1;
                bool Id(ulong id)=>id!=0 && id<=through;
                var points=new HashSet<ulong>();var trees=new HashSet<ulong>();var falling=new HashSet<ulong>();
                foreach(var f in saved.Forests)
                {
                    if(f==null || !Id(f.Point) || !points.Add(f.Point) || f.Trees==null || f.Now!=saved.CapturedAt)throw new ArgumentException("Invalid production point identity or incomplete world step.");
                    foreach(var t in f.Trees)
                    {
                        if(t==null || !Id(t.Id) || !trees.Add(t.Id))throw new ArgumentException("Duplicate original tree identity.");
                        if(t.Stage==TreeStage.Falling)falling.Add(t.Id);
                    }
                }
                foreach(var m in saved.Minerals)
                {
                    if(m==null || !Id(m.Point) || !points.Add(m.Point) || m.Steps==null)throw new ArgumentException("Invalid mineral point identity.");
                    foreach(var step in m.Steps)if(step==null || !Id(step.Behavior))throw new ArgumentException("Invalid original drill behavior.");
                }
                foreach(var tree in trees)if(points.Contains(tree))throw new ArgumentException("Tree identity also owns a production point.");
                foreach(var row in saved.Grounds)
                {
                    if(row==null || !points.Contains(row.Point) || !ProductionRules.Finite(row.Position.x) || !ProductionRules.Finite(row.Position.y) ||
                        !ProductionRules.Finite(row.Position.z) || !grounds.TryAdd(new PersistentId(row.Point),row.Position))throw new ArgumentException("Invalid original ground pile location.");
                }
                ulong correlations=_world.Events.Capture().CorrelationsAllocatedThrough;
                foreach(var row in saved.Responsibilities)
                {
                    if(row==null || !Id(row.Producer) || !Id(row.Machine) || !Id(row.Component) || !Id(row.Task) || !Id(row.Behavior) || row.Correlation>correlations ||
                        !_world.Machines.TryGet(new PersistentId(row.Machine),out var machine) || !_world.Machines.TryGetComponent(new PersistentId(row.Component),out var component) ||
                        component.OwnerId!=machine.Id || component.Definition.Kind!=AutoEra.Machines.HardwareKind.Effector ||
                        !(falling.Contains(row.Producer) || row.Producer==row.Behavior) ||
                        !responsibilities.TryAdd(new PersistentId(row.Producer),new Responsibility(machine.Id,component.Id,new PersistentId(row.Task),new PersistentId(row.Behavior),new AutoEra.Events.CorrelationId(row.Correlation))))
                        throw new ArgumentException("Original production responsibility is invalid.");
                }
                foreach(var row in saved.Forests)
                {
                    var forest=ForestProduction.RestorePersistent(row,_world.ObjectRegistry,_world.Resources.Authority,saved.CapturedAt);
                    forests.Add(forest.Id,forest);
                }
                foreach(var row in saved.Minerals)
                {
                    var mineral=MineralProduction.RestorePersistent(row,_world.Resources.Authority,saved.CapturedAt);
                    minerals.Add(mineral.Id,mineral);
                }
                foreach(var pair in forests) {_forests.Add(pair.Key,pair.Value);_orderedForests.Add(pair.Value);}
                _orderedForests.Sort((a,b)=>a.Id.CompareTo(b.Id));
                foreach(var pair in minerals)_minerals.Add(pair.Key,pair.Value);
                foreach(var pair in grounds)_groundPositions.Add(pair.Key,pair.Value);
                foreach(var pair in responsibilities)_responsibilities.Add(pair.Key,pair.Value);
                committed=true;reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            {reason=error.Message;return false;}
            finally {if(!committed)foreach(var forest in forests.Values)forest.Dispose();}
        }
    }
}
