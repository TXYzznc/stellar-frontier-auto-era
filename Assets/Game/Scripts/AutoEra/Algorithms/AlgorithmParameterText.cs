using System;
using System.Globalization;

namespace AutoEra.Algorithms
{
    /// <summary>Typed public parameter entry; parsing never replaces an enumeration or position with a number.</summary>
    public static class AlgorithmParameterText
    {
        public static bool TryParse(AlgorithmType type,string text,out AlgorithmValue value)
        {
            value=null; if(type==null || text==null) return false;
            var parsed=new AlgorithmValue { Type=type.Copy() };
            if(type.Kind==AlgorithmValueKind.Number)
            { if(!Number(text,out parsed.Number)) return false; }
            else if(type.Kind==AlgorithmValueKind.Enumeration)
            { if(!int.TryParse(text,NumberStyles.Integer,CultureInfo.InvariantCulture,out parsed.EnumValue)) return false; }
            else if(type.Kind==AlgorithmValueKind.Position)
            {
                var parts=text.Split(','); if(parts.Length!=3 || !Number(parts[0],out parsed.X) || !Number(parts[1],out parsed.Y) || !Number(parts[2],out parsed.Z)) return false;
            }
            else return false;
            value=parsed; return true;
        }
        private static bool Number(string text,out double value) => double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out value) && !double.IsNaN(value) && !double.IsInfinity(value);
        public static string Format(AlgorithmValue value)
        {
            if(value?.Type==null) return "—";
            switch(value.Type.Kind)
            {
                case AlgorithmValueKind.Boolean:return value.Boolean?"真":"假";
                case AlgorithmValueKind.Enumeration:return value.EnumValue.ToString(CultureInfo.InvariantCulture);
                case AlgorithmValueKind.Position:return value.X.ToString("G",CultureInfo.InvariantCulture)+", "+value.Y.ToString("G",CultureInfo.InvariantCulture)+", "+value.Z.ToString("G",CultureInfo.InvariantCulture);
                default:return value.Number.ToString("G",CultureInfo.InvariantCulture);
            }
        }
    }
}
