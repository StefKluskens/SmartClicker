//Tool created by Stef Kluskens

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SK.Tools.SmartClicker
{
    public class SmartClickerWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public List<GameObject> SelectedObjects = new();

        private int _selectedIndex = -1;

        private VisualTreeAsset _smartClickerItemTemplate;

        private bool _hovering = false;
        private GameObject _hoveredObject = null;
        private Bounds? _hoverObjectBounds = null;

        private const string SMART_CLICKER_TEMPLATE_NAME = "SmartClickerItemUI";
        private const string OBJECT_NAME_LABEL = "objectName";
        private const string OBJECT_ICON_IMAGE = "objectIcon";

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
        }

        public void CreateGUI()
        {
            ListView selectedObjectsView = new();
            rootVisualElement.Add(selectedObjectsView);

            _smartClickerItemTemplate = Resources.Load(SMART_CLICKER_TEMPLATE_NAME) as VisualTreeAsset;

            selectedObjectsView.makeItem = () => { return _smartClickerItemTemplate.Instantiate(); };
            selectedObjectsView.bindItem = (element, index) =>
            {
                CreateSelectedObjectUIItem(index, element);
            };

            selectedObjectsView.itemsSource = SelectedObjects;
            selectedObjectsView.selectionChanged += OnObjectSelectionChange;
            selectedObjectsView.selectedIndex = _selectedIndex;
            selectedObjectsView.selectionChanged += (items) => { _selectedIndex = selectedObjectsView.selectedIndex; };
        }

        private void CreateSelectedObjectUIItem(int index, VisualElement element)
        {
            GameObject obj = SelectedObjects[index];

            Label label = element.Q<Label>(OBJECT_NAME_LABEL);
            label.text = obj.name;

            Image image = element.Q<Image>(OBJECT_ICON_IMAGE);
            Texture icon = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image;
            image.image = icon;

            element.RegisterCallback<PointerEnterEvent>(evt =>
            {
                OnItemHoverEnter(obj);
            });

            element.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                OnItemHoverExit(obj);
            });
        }

        private void OnObjectSelectionChange(IEnumerable<object> selectedItems)
        {
            IEnumerator<object> enumerator = selectedItems.GetEnumerator();
            if (enumerator.MoveNext())
            {
                GameObject selectedObject = enumerator.Current as GameObject;
                if (selectedItems == null)
                {
                    return;
                }

                Selection.SetActiveObjectWithContext(selectedObject, null);
                Close();
            }
        }

        private void OnItemHoverEnter(Object obj)
        {
            _hoveredObject = obj as GameObject;
            _hovering = true;
        }

        private void OnItemHoverExit(Object obj)
        {
            _hoveredObject = null;
            _hoverObjectBounds = null;
            _hovering = false;
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (!_hovering)
            {
                return;
            }

            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Handles.color = Color.yellow;

            if (_hoverObjectBounds == null)
            {
                Renderer[] renderers = _hoveredObject.GetComponentsInChildren<Renderer>();
                _hoverObjectBounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    _hoverObjectBounds.Value.Encapsulate(renderer.bounds);
                }
            }

            Handles.DrawWireCube(_hoverObjectBounds.Value.center, _hoverObjectBounds.Value.size);
        }
    }
}