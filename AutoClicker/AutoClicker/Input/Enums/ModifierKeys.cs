using System;

namespace AutoClicker.Input.Enums;

[Flags]
public enum ModifierKeys : uint
{
    None   = 0b_0000,      // No modifiers
    Alt    = 0b_0001,      // MOD_ALT (bit 0)
    Ctrl   = 0b_0010,      // MOD_CONTROL (bit 1)
    Shift  = 0b_0100,      // MOD_SHIFT (bit 2)
    Win    = 0b_1000       // MOD_WIN (bit 3)
}
