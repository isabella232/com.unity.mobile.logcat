using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Android.Logcat
{
    internal enum MessagesContextMenu
    {
        None,
        Copy,
        SelectAll,
        SaveSelection,
        AddTag,
        RemoveTag,
        FilterByProcessId
    }

    internal enum ToolsContextMenu
    {
        None,
        ScreenCapture,
        OpenTerminal,
        StacktraceUtility,
        MemoryBehaviorAuto,
        MemoryBehaviorManual,
        MemoryBehaviorHidden
    }

    class AndroidContextMenu<T>
    {
        internal class MenuItemData
        {
            public T Item { get; }
            public string Name { get; }
            public bool Selected { get; }

            public MenuItemData(T item, string name, bool selected)
            {
                Item = item;
                Name = name;
                Selected = selected;
            }
        }

        public object UserData { set; get; }

        private List<MenuItemData> m_Items = new List<MenuItemData>();

        public void Add(T item, string name, bool selected = false)
        {
            m_Items.Add(new MenuItemData(item, name, selected));
        }

        public void AddSplitter()
        {
            Add(default, string.Empty);
        }

        public string[] Names => m_Items.Select(i => i.Name).ToArray();

        private int[] Selected
        {
            get
            {
                var selected = new List<int>();
                for (int i = 0; i < m_Items.Count; i++)
                {
                    if (!m_Items[i].Selected)
                        continue;
                    selected.Add(i);
                }

                return selected.ToArray();
            }
        }

        public MenuItemData GetItemAt(int selected)
        {
            if (selected < 0 || selected > m_Items.Count - 1)
                return null;
            return m_Items[selected];
        }

        public void Show(Vector2 mousePosition, EditorUtility.SelectMenuItemFunction callback)
        {
            var enabled = Enumerable.Repeat(true, Names.Length).ToArray();
            var separator = new bool[Names.Length];
            EditorUtility.DisplayCustomMenuWithSeparators(new Rect(mousePosition.x, mousePosition.y, 0, 0),
                Names,
                enabled,
                separator,
                Selected,
                callback,
                this);
        }
    }
}
