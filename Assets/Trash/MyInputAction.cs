//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.2.0
//     from Assets/Scripts/MyInputAction.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @MyInputAction : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @MyInputAction()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MyInputAction"",
    ""maps"": [
        {
            ""name"": ""MainMap"",
            ""id"": ""c7751b7f-6c10-4559-a904-f150b5050771"",
            ""actions"": [
                {
                    ""name"": ""a"",
                    ""type"": ""Value"",
                    ""id"": ""1ee36f4b-4e18-43e1-af48-2a691893feef"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""b"",
                    ""type"": ""Value"",
                    ""id"": ""63be9aa8-611e-4559-9067-4d0ab9c0a640"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""d1b9c6c9-e760-4a26-9462-d7a06c8d2b7a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""a"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f87fb630-4ea7-4448-8f9f-583541dcd803"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""b"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""528379d1-a1f9-4f93-9ca2-1dfbf1cd8e65"",
            ""actions"": [
                {
                    ""name"": ""Click"",
                    ""type"": ""PassThrough"",
                    ""id"": ""19e4964f-ca57-4ec8-b144-d089292ec504"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Point"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e62a63cd-7445-4c47-99f5-2e9023c4a464"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""0503e7c4-a84e-457f-82dc-157ea9f67aa5"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5bb38c5c-eb8d-42e0-9f28-b8932ce3eb0e"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MainMap
        m_MainMap = asset.FindActionMap("MainMap", throwIfNotFound: true);
        m_MainMap_a = m_MainMap.FindAction("a", throwIfNotFound: true);
        m_MainMap_b = m_MainMap.FindAction("b", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
        m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // MainMap
    private readonly InputActionMap m_MainMap;
    private IMainMapActions m_MainMapActionsCallbackInterface;
    private readonly InputAction m_MainMap_a;
    private readonly InputAction m_MainMap_b;
    public struct MainMapActions
    {
        private @MyInputAction m_Wrapper;
        public MainMapActions(@MyInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @a => m_Wrapper.m_MainMap_a;
        public InputAction @b => m_Wrapper.m_MainMap_b;
        public InputActionMap Get() { return m_Wrapper.m_MainMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MainMapActions set) { return set.Get(); }
        public void SetCallbacks(IMainMapActions instance)
        {
            if (m_Wrapper.m_MainMapActionsCallbackInterface != null)
            {
                @a.started -= m_Wrapper.m_MainMapActionsCallbackInterface.OnA;
                @a.performed -= m_Wrapper.m_MainMapActionsCallbackInterface.OnA;
                @a.canceled -= m_Wrapper.m_MainMapActionsCallbackInterface.OnA;
                @b.started -= m_Wrapper.m_MainMapActionsCallbackInterface.OnB;
                @b.performed -= m_Wrapper.m_MainMapActionsCallbackInterface.OnB;
                @b.canceled -= m_Wrapper.m_MainMapActionsCallbackInterface.OnB;
            }
            m_Wrapper.m_MainMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @a.started += instance.OnA;
                @a.performed += instance.OnA;
                @a.canceled += instance.OnA;
                @b.started += instance.OnB;
                @b.performed += instance.OnB;
                @b.canceled += instance.OnB;
            }
        }
    }
    public MainMapActions @MainMap => new MainMapActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private IUIActions m_UIActionsCallbackInterface;
    private readonly InputAction m_UI_Click;
    private readonly InputAction m_UI_Point;
    public struct UIActions
    {
        private @MyInputAction m_Wrapper;
        public UIActions(@MyInputAction wrapper) { m_Wrapper = wrapper; }
        public InputAction @Click => m_Wrapper.m_UI_Click;
        public InputAction @Point => m_Wrapper.m_UI_Point;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void SetCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterface != null)
            {
                @Click.started -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
                @Point.started -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Point.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
                @Point.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
            }
            m_Wrapper.m_UIActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Point.started += instance.OnPoint;
                @Point.performed += instance.OnPoint;
                @Point.canceled += instance.OnPoint;
            }
        }
    }
    public UIActions @UI => new UIActions(this);
    public interface IMainMapActions
    {
        void OnA(InputAction.CallbackContext context);
        void OnB(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnClick(InputAction.CallbackContext context);
        void OnPoint(InputAction.CallbackContext context);
    }
}
