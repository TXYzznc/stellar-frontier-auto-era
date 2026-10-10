using UnityEngine;

namespace AutoEra.Art.PCG
{
    public sealed class PCGWindSource : MonoBehaviour
    {
        [InspectorName("所属风场")] public PCGWindField Field;
        [InspectorName("影响半径")] public float Radius = 18;
        [InspectorName("影响强度")] public float Strength = 2;
        [InspectorName("衰减指数")] public float Falloff = 2;
        [InspectorName("向外扩散")] public bool Outward;
        [InspectorName("局部风向")] public Vector2 Direction = new Vector2(1, .3f);
        private void OnEnable() { if (Field != null) Field.Register(this); }
        private void OnDisable() { if (Field != null) Field.Unregister(this); }
    }
}
