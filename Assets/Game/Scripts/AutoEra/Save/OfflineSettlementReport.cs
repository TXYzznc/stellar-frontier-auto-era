using System;
using System.Collections.Generic;
using AutoEra.World;
using AutoEra.World.Time;

namespace AutoEra.Save
{
    public sealed class OfflineReportSnapshot
    {
        public int Version=1;
        public string RunId;
        public long InitialWorld,CurrentWorld,TargetWorld;
        public bool Completed,Confirmed,ResourcesInitialized;
        public ResourceRow[] Resources;
        public EventRow[] Events;
        public SequenceRange[] Receipts;
        public sealed class ResourceRow {public string Item;public long Initial,Current;}
        public sealed class EventRow {public string Domain;public OfflineEventKind Kind;public ulong Count;}
        public sealed class SequenceRange {public ulong First,Last;}
    }
    /// <summary>Facts-only report. Updating or reading this object never produces, transfers, spends or awards anything.</summary>
    public sealed class OfflineSettlementReport
    {
        private readonly Dictionary<string,OfflineReportSnapshot.ResourceRow> _resources=new Dictionary<string,OfflineReportSnapshot.ResourceRow>(StringComparer.Ordinal);
        private readonly Dictionary<string,OfflineReportSnapshot.EventRow> _events=new Dictionary<string,OfflineReportSnapshot.EventRow>(StringComparer.Ordinal);
        private readonly List<OfflineReportSnapshot.SequenceRange> _receipts=new List<OfflineReportSnapshot.SequenceRange>();
        public string RunId {get;}
        public long InitialWorld {get;}
        public long CurrentWorld {get;private set;}
        public long TargetWorld {get;}
        public bool Completed {get;private set;}
        public bool Confirmed {get;private set;}
        public bool ResourcesInitialized {get;private set;}
        public OfflineSettlementReport(string runId,long initialWorld,long targetWorld)
        {
            if(string.IsNullOrWhiteSpace(runId) || initialWorld<0 || targetWorld<initialWorld)throw new ArgumentException("Invalid report settlement identity.");
            RunId=runId;InitialWorld=CurrentWorld=initialWorld;TargetWorld=targetWorld;
        }
        public bool TryRecordEvent(OfflineScheduledEvent item,out string reason)
        {
            reason="报告事件身份无效";if(Completed || item==null || item.Sequence==0 || item.Due<InitialWorld || item.Due>TargetWorld || string.IsNullOrWhiteSpace(item.Domain) ||
                !Enum.IsDefined(typeof(OfflineEventKind),item.Kind))return false;
            foreach(var range in _receipts)if(item.Sequence>=range.First && item.Sequence<=range.Last) {reason=null;return true;}
            string key=item.Domain+":"+(int)item.Kind;
            if(!_events.TryGetValue(key,out var row)) {_events.Add(key,row=new OfflineReportSnapshot.EventRow {Domain=item.Domain,Kind=item.Kind});}
            if(row.Count==ulong.MaxValue) {reason="报告计数已耗尽";return false;}
            row.Count++;InsertReceipt(item.Sequence);reason=null;return true;
        }
        private void InsertReceipt(ulong sequence)
        {
            int index=0;while(index<_receipts.Count && _receipts[index].Last<sequence)index++;
            bool previous=index>0 && _receipts[index-1].Last!=ulong.MaxValue && _receipts[index-1].Last+1==sequence;
            bool next=index<_receipts.Count && sequence!=ulong.MaxValue && sequence+1==_receipts[index].First;
            if(previous)
            {_receipts[index-1].Last=sequence;if(next) {_receipts[index-1].Last=_receipts[index].Last;_receipts.RemoveAt(index);} }
            else if(next)_receipts[index].First=sequence;
            else _receipts.Insert(index,new OfflineReportSnapshot.SequenceRange {First=sequence,Last=sequence});
        }
        public bool TryRefreshResources(AutoEraWorldSession world,bool initial,out string reason)
        {
            reason="实际货物事实尚未到达完整边界";
            if(world==null || !world.IsActive || !world.Resources.CatalogReady || Completed || !world.Resources.Authority.TryCapturePersistent(out var cargo) ||
                world.Clock.WorldMilliseconds<CurrentWorld || world.Clock.WorldMilliseconds>TargetWorld ||
                initial && (ResourcesInitialized || world.Clock.WorldMilliseconds!=InitialWorld) || !initial && !ResourcesInitialized)return false;
            var totals=new Dictionary<string,long>(StringComparer.Ordinal);
            try
            {
                foreach(var lot in cargo.Lots) {totals.TryGetValue(lot.Item,out long value);totals[lot.Item]=checked(value+lot.Units);}
                foreach(var balance in cargo.Balances) {totals.TryGetValue(balance.Item,out long value);totals[balance.Item]=checked(value+balance.Units);}
                foreach(var row in _resources.Values)row.Current=0;
                foreach(var pair in totals)
                {
                    if(!_resources.TryGetValue(pair.Key,out var row))_resources.Add(pair.Key,row=new OfflineReportSnapshot.ResourceRow {Item=pair.Key});
                    if(initial)row.Initial=pair.Value;row.Current=pair.Value;
                }
                ResourcesInitialized=true;CurrentWorld=world.Clock.WorldMilliseconds;reason=null;return true;
            }
            catch(OverflowException) {reason="资源报告超出整数表示范围";return false;}
        }
        public bool TryComplete(OfflineEventScheduler scheduler)
        {
            if(scheduler==null || !ResourcesInitialized || !scheduler.Completed || !scheduler.IsAtCheckpointBoundary || scheduler.RunId!=RunId || scheduler.TargetWorldMilliseconds!=TargetWorld ||
                scheduler.WorldMilliseconds!=CurrentWorld || CurrentWorld!=TargetWorld)return false;
            Completed=true;return true;
        }
        public bool TryConfirm() {if(!Completed)return false;Confirmed=true;return true;}
        public OfflineReportSnapshot CapturePersistent()
        {
            var resources=new List<OfflineReportSnapshot.ResourceRow>();foreach(var r in _resources.Values)resources.Add(new OfflineReportSnapshot.ResourceRow {Item=r.Item,Initial=r.Initial,Current=r.Current});resources.Sort((a,b)=>StringComparer.Ordinal.Compare(a.Item,b.Item));
            var events=new List<OfflineReportSnapshot.EventRow>();foreach(var r in _events.Values)events.Add(new OfflineReportSnapshot.EventRow {Domain=r.Domain,Kind=r.Kind,Count=r.Count});events.Sort((a,b)=> {int c=StringComparer.Ordinal.Compare(a.Domain,b.Domain);return c!=0 ? c : a.Kind.CompareTo(b.Kind);});
            var receipts=new OfflineReportSnapshot.SequenceRange[_receipts.Count];for(int i=0;i<receipts.Length;i++)receipts[i]=new OfflineReportSnapshot.SequenceRange {First=_receipts[i].First,Last=_receipts[i].Last};
            return new OfflineReportSnapshot {RunId=RunId,InitialWorld=InitialWorld,CurrentWorld=CurrentWorld,TargetWorld=TargetWorld,Completed=Completed,Confirmed=Confirmed,ResourcesInitialized=ResourcesInitialized,Resources=resources.ToArray(),Events=events.ToArray(),Receipts=receipts};
        }
        public static bool TryRestorePersistent(OfflineReportSnapshot saved,out OfflineSettlementReport report,out string reason)
        {
            report=null;reason="原报告状态无效";
            if(saved?.Version!=1 || saved.Resources==null || saved.Events==null || saved.Receipts==null || saved.CurrentWorld<saved.InitialWorld || saved.CurrentWorld>saved.TargetWorld ||
                saved.Completed && (saved.CurrentWorld!=saved.TargetWorld || !saved.ResourcesInitialized) || saved.Confirmed && !saved.Completed ||
                !saved.ResourcesInitialized && (saved.Resources.Length!=0 || saved.CurrentWorld!=saved.InitialWorld))return false;
            try
            {
                var candidate=new OfflineSettlementReport(saved.RunId,saved.InitialWorld,saved.TargetWorld) {CurrentWorld=saved.CurrentWorld,Completed=saved.Completed,Confirmed=saved.Confirmed,ResourcesInitialized=saved.ResourcesInitialized};
                foreach(var row in saved.Resources)
                    if(row==null || string.IsNullOrWhiteSpace(row.Item) || row.Initial<0 || row.Current<0 || !candidate._resources.TryAdd(row.Item,new OfflineReportSnapshot.ResourceRow {Item=row.Item,Initial=row.Initial,Current=row.Current}))return false;
                ulong count=0,receipts=0,previous=0;
                foreach(var row in saved.Events)
                {
                    if(row==null || string.IsNullOrWhiteSpace(row.Domain) || !Enum.IsDefined(typeof(OfflineEventKind),row.Kind) || row.Count==0 ||
                        !candidate._events.TryAdd(row.Domain+":"+(int)row.Kind,new OfflineReportSnapshot.EventRow {Domain=row.Domain,Kind=row.Kind,Count=row.Count}))return false;count=checked(count+row.Count);
                }
                foreach(var row in saved.Receipts)
                {
                    if(row==null || row.First==0 || row.Last<row.First || row.First<=previous || previous!=0 && previous!=ulong.MaxValue && row.First==previous+1)return false;
                    receipts=checked(receipts+row.Last-row.First+1);previous=row.Last;candidate._receipts.Add(new OfflineReportSnapshot.SequenceRange {First=row.First,Last=row.Last});
                }
                if(count!=receipts)return false;report=candidate;reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is OverflowException) {reason=error.Message;return false;}
        }
    }
}
