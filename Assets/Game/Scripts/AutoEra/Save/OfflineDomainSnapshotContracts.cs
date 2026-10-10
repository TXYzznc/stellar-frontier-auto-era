using System;
using System.Collections.Generic;

namespace AutoEra.Save
{
    /// <summary>Versioned facts supplied by the future progress/economy authority. This contract never grants or spends anything.</summary>
    public sealed class OfflineSettlementMarks
    {
        public int Version=1;
        public ulong[] CommittedPurchases,CommittedRewards;
        public int LastClaimedLocalDate,LatestSeenLocalDate;
        public bool Validate(ulong allocatedThrough)
        {
            if(Version!=1 || CommittedPurchases==null || CommittedRewards==null || !ValidDate(LastClaimedLocalDate) || !ValidDate(LatestSeenLocalDate) || LastClaimedLocalDate>LatestSeenLocalDate)return false;
            var ids=new HashSet<ulong>();
            foreach(var batch in new[] {CommittedPurchases,CommittedRewards})foreach(ulong id in batch)if(id==0 || id>allocatedThrough || !ids.Add(id))return false;
            return true;
        }
        public bool IsSupplyEligible(int currentLocalDate)
            =>ValidDate(currentLocalDate) && currentLocalDate!=0 && currentLocalDate>=LatestSeenLocalDate && currentLocalDate>LastClaimedLocalDate;
        private static bool ValidDate(int value)
        {
            if(value==0)return true;
            int year=value/10000,month=value/100%100,day=value%100;
            return year>=1 && year<=9999 && month>=1 && month<=12 && day>=1 && day<=DateTime.DaysInMonth(year,month);
        }
    }
}
