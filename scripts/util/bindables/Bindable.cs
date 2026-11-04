using System;

public class Bindable<T> : IDisposable
{
    public EventHandler<BindableEventArgs<T>> ValueChanged;

    public string Name { get; }

    public T Default { get; set; }

    public T Value
    {
        get;
        set
        {
            var old = field;
            field = value;
            ValueChanged?.Invoke(this, new BindableEventArgs<T>(value, old));
        }
    }

    public Bindable(T defaultValue)
    {
        Default = defaultValue;
        Value = defaultValue;
    }

    public Bindable(string name, T defaultValue)
    {
        Name = name;
        Default = defaultValue;
        Value = defaultValue;
    }

    public void Dispose() => ValueChanged = null;

    public override string ToString() => Value.ToString();
}
