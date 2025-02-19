using System;
using System.Device.Gpio;

public class GpioService : IDisposable
{
    private readonly GpioController _controller;
    private readonly int _pin;

    public GpioService(int pin)
    {
        _controller = new GpioController();
        _pin = pin;
        _controller.OpenPin(_pin, PinMode.Output);
    }

    public void TurnOn()
    {
        _controller.Write(_pin, PinValue.High);
    }

    public void TurnOff()
    {
        _controller.Write(_pin, PinValue.Low);
    }

    public void Dispose()
    {
        _controller.ClosePin(_pin);
        _controller.Dispose();
    }
}