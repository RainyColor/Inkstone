#if TOOLS
using Godot;
using System;

[Tool]
public partial class ChoiceButton : Button
{
    public delegate void ChoicePressedEventHandler(int idx);
    public event ChoicePressedEventHandler ChoicePressed;

	public int Idx = 0;

    public override void _Pressed()
    {
        base._Pressed();
        ChoicePressed(Idx);
    }
}
#endif
