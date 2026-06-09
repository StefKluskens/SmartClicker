//Tool created by Stef Kluskens

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SK.Tools.SmartClicker
{
    [InitializeOnLoad]
    public static class SmartClicker
    {
        private static Vector2 _mouseDownPos;
        private static bool _altClickStarted;

        static SmartClicker()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private static void OnSceneGui(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.button != 0 || !e.alt)
            {
                return;
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    _mouseDownPos = e.mousePosition;
                    _altClickStarted = true;
                    break;
                case EventType.MouseUp:
                    if (!_altClickStarted)
                    {
                        return;
                    }

                    _altClickStarted = false;

                    if ((e.mousePosition - _mouseDownPos).sqrMagnitude > 25.0f)
                    {
                        return;
                    }

                    ShowSmartClicker(e);
                    break;
                default:
                    break;
            }

            
        }

        private static void ShowSmartClicker(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            if (hits.Length == 0)
            {
                return;
            }

            SmartClickerWindow window = ScriptableObject.CreateInstance<SmartClickerWindow>();
            window.SelectedObjects = hits.Select(h => h.collider.gameObject).ToList();

            Rect dropdownRect = new(GUIUtility.GUIToScreenPoint(e.mousePosition), Vector3.zero);
            window.ShowAsDropDown(dropdownRect, new Vector2(300, 200));
        }
    }
}
