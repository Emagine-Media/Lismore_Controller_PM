#if PLAYMAKER
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    public enum EventType
    {
        AttachToPanel = 0,
        Blur = 10,

        //Change<T0>=30,
        ChangeFloat = 31,

        Click = 40,

        //CommandBase<T0>=50,
        ContextClick = 60,
        ContextualMenuPopulate = 70,
        CustomStyleResolved = 80,
        DetachFromPanel = 90,
        //DragAndDropBase<T0>=100,

        // Drag events are not (yet) supported at runtime.
        
        // DragEnter = 110,
        // DragExited = 120,
        // DragLeave = 130,
        // DragPerform = 140,
        // DragUpdated = 150,

        // ExecuteCommand is Editor only.
        // ExecuteCommand = 160,

        Focus = 170,
        //FocusBase<T0>=180,
        FocusIn = 190,
        FocusOut = 200,

        GeometryChanged = 210,

        IMGUI = 220,

        Input = 230,

        //KeyboardBase<T0>=240,
        KeyDown = 250,
        KeyUp = 260,
        MouseCapture = 270,

        //MouseCaptureBase<T0>=280,
        MouseCaptureOut = 290,
        MouseDown = 300,
        MouseEnter = 310,
        MouseEnterWindow = 320,
        //MouseBase<T0>=330,
        MouseLeave = 340,
        MouseLeaveWindow = 350,
        MouseMove = 360,
        MouseOut = 370,
        MouseOver = 380,
        MouseUp = 390,

        NavigationCancel = 400,
        //NavigationBase<T0>=410,
        NavigationMove = 420,
        NavigationSubmit = 430,

        //PanelChangedBase<T0>=440,
        PointerCancel = 450,
        PointerCapture = 460,

        //PointerCaptureBase<T0>=470,
        PointerCaptureOut = 480,
        PointerDown = 490,
        PointerEnter = 500,
        //PointerBase<T0>=510,
        PointerLeave = 520,
        PointerMove = 530,
        PointerOut = 540,
        PointerOver = 550,
        PointerStationary = 560,
        PointerUp = 570,

        Tooltip = 580,

        TransitionCancel = 590,
        TransitionEnd = 600,
        //TransitionBase<T0>=610,
        TransitionRun = 620,
        TransitionStart = 630,

        // ValidateCommand is Editor only.
        // ValidateCommand = 640,

        Wheel = 650
    }
}
#endif
