using System;

namespace MetalShearsController.Models;

public class PositionUnits
{
    private int _value = 0;

    public int Units => _value;
    public double Value
    {
        get => _value / 10.0;
        set
        {
            _value = value >= 0.0 ? (int)double.Ceiling(value * 10.0) : 0;
        }
    }

    public PositionUnits(double mm)
    {
        Value = mm;
    }

    //public static PositionUnits operator +(PositionUnits a, PositionUnits b) => new() { Value = a.Value + b.Value };
    public static implicit operator double(PositionUnits a) => a.Value;
    public static implicit operator string(PositionUnits a) => a.ToString();

    public override string ToString()
    {
        return (_value / 10.0).ToString("0000.0") + " mm";
    }

}

