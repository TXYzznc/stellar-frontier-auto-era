using System;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;
namespace AutoEra.Tests.Editor
{
    public sealed class SensorPublicDataEditModeTests
    {
        [Test] public void FormalProfileRows_KeepModelIdentityAndRejectChangedFrozenValues()
        {
            var row = new AutoEra.DataTable.SensorDefinitions();
            row.ParseDataRow("\t21022\t\t2102\t2\tSoil\t1000\t4\t10",null);
            var catalog = new AutoEra.Machines.Sensors.SensorCatalog(new[]{row});
            Assert.That(catalog.TryGet(21022,out var profile),Is.True);
            Assert.That(profile.ComponentDefinitionId,Is.EqualTo(2102));
            Assert.That(profile.Level,Is.EqualTo(2));
            row.ParseDataRow("\t21022\t\t2102\t2\tSoil\t500\t4\t10",null);
            Assert.Throws<FormatException>(()=>new AutoEra.Machines.Sensors.SensorCatalog(new[]{row}));
        }
        [Test] public void EmptyLandMoisture_IsImmutableAndAreaWeighted()
        {
            var soil=new[]{new SoilCellReadout(1,Vector3.zero,1,20,true),new SoilCellReadout(2,Vector3.right,3,60,true)};
            var data=new SensorSnapshot(1,soil:soil); soil[0]=new SoilCellReadout(3,Vector3.zero,1,99,true);
            Assert.That(data.Soil[0].Id,Is.EqualTo(1)); Assert.That(data.Crops,Is.Null);
            Assert.That(data.TryGetWeightedMoisture(out var moisture),Is.True); Assert.That(moisture,Is.EqualTo(50));
        }
        [Test] public void CropIdsAlignAndInvalidDataIsRejected()
        {
            var data=new SensorSnapshot(1,soil:new[]{new SoilCellReadout(7,Vector3.zero,1,30,true)},crops:new[]{new CropCellReadout(7,.5f,.2f,true)});
            Assert.That(data.Crops[0].Id,Is.EqualTo(data.Soil[0].Id));
            Assert.Throws<ArgumentException>(()=>new SoilCellReadout(1,Vector3.zero,1,float.NaN,true));
            Assert.Throws<ArgumentException>(()=>new SensorSnapshot(1,soil:new[]{data.Soil[0],data.Soil[0]}));
        }
    }
}
