using System;
using System.Collections.Generic;
using AutoEra.Events;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public sealed class ResourceWorldSnapshot
    {
        public CargoOwnershipSnapshot Cargo;
        public TransportLedgerSnapshot Transport;
        public bool WithEvents;
        public CorrelationRecord[] Correlations;
        public sealed class CorrelationRecord {public ulong Transaction,Correlation;}
    }
    public sealed partial class ResourceWorldService
    {
        public bool TryCapturePersistent(out ResourceWorldSnapshot saved)
        {
            saved=null;
            if(_disposed || _custodyChanged || !CatalogReady || !Authority.TryCapturePersistent(out var cargo))return false;
            var correlations=new List<ResourceWorldSnapshot.CorrelationRecord>();
            foreach(var pair in _correlations)correlations.Add(new ResourceWorldSnapshot.CorrelationRecord {Transaction=pair.Key.Value,Correlation=pair.Value.Value});
            correlations.Sort((a,b)=>a.Transaction.CompareTo(b.Transaction));
            saved=new ResourceWorldSnapshot {Cargo=cargo,Transport=Transport.CapturePersistent(),WithEvents=_events!=null,Correlations=correlations.ToArray()};return true;
        }
        /// <summary>Candidate-only: configure the real catalog and restore event allocation before calling this method.</summary>
        public bool TryRestorePersistent(ResourceWorldSnapshot saved,out string reason)
        {
            reason="Resource world checkpoint is incomplete";
            if(_disposed || !CatalogReady || _cargo.Count!=0 || _bound.Count!=0 || _correlations.Count!=0 || saved?.Cargo?.Transactions==null ||
                saved.Transport==null || saved.Correlations==null || saved.WithEvents!=(_events!=null))return false;
            var correlations=new Dictionary<PersistentId,CorrelationId>();var transactions=new HashSet<ulong>();var identities=new HashSet<ulong>();
            foreach(var tx in saved.Cargo.Transactions)if(tx==null || !transactions.Add(tx.Id))return false;
            ulong through=_events?.Capture().CorrelationsAllocatedThrough ?? 0;
            foreach(var row in saved.Correlations)
            {
                if(row==null || !transactions.Contains(row.Transaction) || row.Correlation==0 || row.Correlation>through || !identities.Add(row.Correlation) ||
                    !correlations.TryAdd(new PersistentId(row.Transaction),new CorrelationId(row.Correlation)))return false;
            }
            if(saved.WithEvents ? correlations.Count!=transactions.Count : correlations.Count!=0)return false;
            if(!Authority.TryRestorePersistent(saved.Cargo,out reason) || !Transport.TryRestorePersistent(saved.Transport,Authority,out reason))return false;
            foreach(var pair in correlations)_correlations.Add(pair.Key,pair.Value);
            _custodyChanged=false;reason=null;return true;
        }
    }
}
