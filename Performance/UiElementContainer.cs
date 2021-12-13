using System;
using UnityEngine;

/* This class is part of a UI performance system. This system disables and enables (culls) ui elements in a scrolling list
 * based on whether they are visible or not. When they are not visible, the elements are stored in a pool for later reuse.
 * When made visible, an element is retrieved from the pool.
 *
 * This UI element container class is the connector between the culling and pooling system.
 * When creating a list of culled and pooled elements, this component is instantiated as a placeholder for the element.
 * The container retains the size and position of the element, so that the scrolling list knows its overall scrolling size when the invisible
 * elements are pooled.
 *
 * All containers for the list are stored in a UiElementContainerCache<TC, TE>
 *
 * This component as well as the pool are generic so that it can be resused for different UI elements that need to be displayed in a list.
 */

namespace foo.ui.performance
{
	[RequireComponent(typeof(ScrollRectCullableElement))] // Scroll Rect extension that hides elements that are out of the visible scroll area
	[RequireComponent(typeof(RectTransform))] // Make sure this is a Unity UI element
	[RequireComponent(typeof(CanvasGroup))] // CanvasGroup for scroll performance
	public class UiElementContainer<T> : MonoBehaviour, IReleaseable
    where T : MonoBehaviour, IClearable // IClearable is called through the pool
	{
		private T element;
		private bool visible;

		private GameObject prefab;
		private Action<T> onInit;
		private Action<T> onSetFullyVisible;
		private Action<T> onRelease;

		public void Init(Action<T> onInit, GameObject prefab, Action<T> onSetFullyVisible = null, Action<T> onRelease = null)
		{
			this.prefab = prefab;
			this.onInit = onInit;
			this.onSetFullyVisible = onSetFullyVisible;
			this.onRelease = onRelease;
			scrollRectCullableElement = GetComponent<ScrollRectCullableElement>();
		}

		public void OnSetVisible(bool visible) // This is a function that is connected to the culler via a Unity action
		{
			if (visible != this.visible)
			{
				this.visible = visible; // Lazy initialisation
				if (visible)
				{
					InitElement();
				}
				else
				{
					ReleaseElement();
				}
			}
		}

		public void OnSetFullyVisible(bool visible) // This is a function that is connected to the culler via a Unity action
		{
			if (visible && element != null)
			{
				onSetFullyVisible?.Invoke(element);
			}
		}

		private void InitElement()
		{
			if (element != null)
			{
				UiElementPrefabPool<T>.Release(element, prefab);
			}

			element = UiElementPrefabPool<T>.Get(prefab, transform);
			onInit(element);
			element.gameObject.SetActive(true);
		}

		private void ReleaseElement()
		{
			if (element == null)
			{
				return;
			}
			onRelease?.Invoke(element);
			UiElementPrefabPool<T>.Release(element, prefab);
			element = null;
		}

		public void Release()
		{
			visible = false;
			ReleaseElement();
		}
	}
}
