﻿using System;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Experimental.UIElements;
using UnityEditor.VFX;
using System.Collections.Generic;
using UnityEditor;

namespace  UnityEditor.VFX.UI
{
    [Serializable]
    class VFXViewWindow : GraphViewEditorWindow
    {
        ShortcutHandler m_ShortcutHandler;

        protected void SetupFramingShortcutHandler(VFXView view)
        {
            m_ShortcutHandler = new ShortcutHandler(
                new Dictionary<Event, ShortcutDelegate>
                {
                    { Event.KeyboardEvent("a"), view.FrameAll },
                    { Event.KeyboardEvent("f"), view.FrameSelection },
                    { Event.KeyboardEvent("o"), view.FrameOrigin },
                    { Event.KeyboardEvent("delete"), view.DeleteSelection },
                    { Event.KeyboardEvent("#tab"), view.FramePrev },
                    { Event.KeyboardEvent("tab"), view.FrameNext },
                    {Event.KeyboardEvent("c"), view.CloneModels},     // TEST
                    {Event.KeyboardEvent("#r"), view.Resync},
                    {Event.KeyboardEvent("#d"), view.OutputToDot},
                    {Event.KeyboardEvent("^#d"), view.OutputToDotReduced},
                    {Event.KeyboardEvent("#c"), view.OutputToDotConstantFolding},
                });

            view.onEnter += OnEnterPanel;
            view.onLeave += OnLeavePanel;
        }

        [MenuItem("Window/VFXEditor")]
        public static void ShowWindow()
        {
            GetWindow<VFXViewWindow>();
        }

        protected override GraphView BuildView()
        {
            BuildPresenters();
            SetupFramingShortcutHandler(viewPresenter.View);
            return viewPresenter.View;
        }

        protected override GraphViewPresenter BuildPresenters()
        {
            if (!string.IsNullOrEmpty(m_DisplayedAssetPath))
            {
                VFXAsset asset = AssetDatabase.LoadAssetAtPath<VFXAsset>(m_DisplayedAssetPath);
                viewPresenter.SetVFXAsset(asset, true);
            }
            return viewPresenter;
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            var objs = Selection.objects;
            if (objs != null && objs.Length == 1 && objs[0] is VFXAsset)
            {
                viewPresenter.SetVFXAsset(objs[0] as VFXAsset, true);
            }
            else if (!string.IsNullOrEmpty(m_DisplayedAssetPath))
            {
                VFXAsset asset = AssetDatabase.LoadAssetAtPath<VFXAsset>(m_DisplayedAssetPath);

                viewPresenter.SetVFXAsset(asset, true);
            }
            else
                viewPresenter.SetVFXAsset(viewPresenter.GetVFXAsset(), true);
        }

        protected new void OnDisable()
        {
            viewPresenter.SetVFXAsset(null, false);
            base.OnDisable();
        }

        void OnSelectionChange()
        {
            var objs = Selection.objects;
            if (objs != null && objs.Length == 1 && objs[0] is VFXAsset)
            {
                m_DisplayedAssetPath = AssetDatabase.GetAssetPath(objs[0] as VFXAsset);
                viewPresenter.SetVFXAsset(objs[0] as VFXAsset, false);
            }
        }

        void OnEnterPanel()
        {
            //VisualElement rootVisualContainer = UIElementsEntryPoint.GetRootVisualContainer(this);
            rootVisualContainer.parent.AddManipulator(m_ShortcutHandler);
        }

        void OnLeavePanel()
        {
            //VisualElement rootVisualContainer = UIElementsEntryPoint.GetRootVisualContainer(this);
            rootVisualContainer.parent.RemoveManipulator(m_ShortcutHandler);
        }

        void Update()
        {
            var graph = viewPresenter.GetGraph();
            if (graph != null)
                graph.RecompileIfNeeded();
            viewPresenter.RecompileExpressionGraphIfNeeded();
        }

        [SerializeField]
        private string m_DisplayedAssetPath;

        static public VFXViewPresenter viewPresenter
        {
            get
            {
                if (s_ViewPresenter == null)
                    s_ViewPresenter = ScriptableObject.CreateInstance<VFXViewPresenter>();

                return s_ViewPresenter;
            }
        }

        static VFXViewPresenter s_ViewPresenter;
    }
}
