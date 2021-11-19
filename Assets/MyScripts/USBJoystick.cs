using UnityEditor;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Controls;

namespace UnityEngine.InputSystem.LowLevel
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    struct USBJoystick : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('H', 'I', 'D');

        [FieldOffset(0)] public byte reportId;

        [InputControl(name = "stick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "stick/x", layout = "Axis", offset = 1, format = "BYTE", defaultState = 0, parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "stick/left", layout = "Button", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=1")]
        [InputControl(name = "stick/right", layout = "Button", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=1")]
        [InputControl(name = "stick/y", layout = "Axis", offset = 2, format = "BYTE", defaultState = 0, parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "stick/up", layout = "Button", offset = 2, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=1,invert")]
        [InputControl(name = "stick/down", layout = "Button", offset = 2, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=1,invert=false")]
        [InputControl(name = "stick/z", layout = "Axis", offset = 3, format = "BYTE", defaultState = 0, parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "stick/ccw", layout = "Button", offset = 3, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=1,invert")]
        [InputControl(name = "stick/cw", layout = "Button", offset = 3, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=1,invert=false")]
        [FieldOffset(0)] public byte joystickX;
        [FieldOffset(0)] public byte joystickY;
        [FieldOffset(0)] public byte joystickZ;

        [InputControl(name = "leftButton", format = "BIT", displayName = "Buttons", layout = "Button", bit = 2)]
        [FieldOffset(4)] public byte leftButton;
        [InputControl(name = "middleButton", format = "BIT", displayName = "Buttons", layout = "Button", bit = 1)]
        [FieldOffset(4)] public byte middleButton;
        [InputControl(name = "rightButton", format = "BIT", displayName = "Buttons", layout = "Button", bit = 0)]
        [FieldOffset(4)] public byte rightButton;
    }

    [InputControlLayout(stateType = typeof(USBJoystick))]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class CTIJoystick : InputDevice
    {
        public static CTIJoystick current { get; private set; }
        public AxisControl x { get; private set; }
        public AxisControl y { get; private set; }
        public AxisControl z { get; private set; }
        public ButtonControl leftButton { get; private set; }
        public ButtonControl middleButton { get; private set; }
        public ButtonControl rightButton { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            x = GetChildControl<AxisControl>("stick/x");
            y = GetChildControl<AxisControl>("stick/y");
            z = GetChildControl<AxisControl>("stick/z");
            leftButton = GetChildControl<ButtonControl>("leftButton");
            middleButton = GetChildControl<ButtonControl>("middleButton");
            rightButton = GetChildControl<ButtonControl>("rightButton");
        }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        static CTIJoystick()
        {
            InputSystem.RegisterLayout<CTIJoystick>(
                matches: new InputDeviceMatcher().WithInterface("HID")
                .WithVersion("256")
                .WithCapability("productId", "12")
                .WithCapability("vendorId", "2289")
                .WithCapability("usage", "4")
                .WithCapability("usagePage", "1"));
        }
        [RuntimeInitializeOnLoadMethod]
        static void Init() { }
    }
}