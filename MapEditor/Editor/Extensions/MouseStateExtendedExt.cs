using MonoGame.Extended.Input;

namespace Editor.Extensions
{
    public static class MouseStateExtendedExt
    {
        public static int GetDeltaScrollWheel(this MouseStateExtended state) => state.DeltaScrollWheelValue / 120;
    }
}
