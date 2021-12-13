using System;
using System.Collections.Generic;
using UnityEngine;

namespace foo.ui.performance
{
	public class UiElementContainerCache<TC, TE> where TC : MonoBehaviour, IReleaseable
	{
		public Dictionary<string, TC> allContainers { get; } = new Dictionary<string, TC>();
		private readonly GameObject containerPrefab;

		public UiElementContainerCache(GameObject containerPrefab)
		{
			this.containerPrefab = containerPrefab;
		}

		public TC GetContainer(string id, Transform parent, int siblingIndex)
		{
			if (allContainers.TryGetValue(id, out TC container))
			{
				return SetupExistingContainer(parent, siblingIndex, container);
			}

			return CreateNewContainer(id, parent);
		}

		private TC CreateNewContainer(string id, Transform parent)
		{
			TC container = RenderUtil.InstantiatePrefab<TC>(containerPrefab, parent);
			container.name = $"{typeof(TC).DeclaringType}Container-{id}";

		  allContainers.Add(id, container);
			return container;
		}

		private static TC SetupExistingContainer(Transform parent, int siblingIndex, TC container)
		{
			if (container.transform.parent != parent)
			{
				container.transform.SetParent(parent, worldPositionStays: false);
				ScrollRectCullableElement cullable = container.GetComponent<ScrollRectCullableElement>();
				if (cullable)
				{
					cullable.Reset();
				}
			}

			container.transform.SetSiblingIndex(siblingIndex);
			return container;
		}

		public TC GetContainer(string id)
		{
			allContainers.TryGetValue(id, out TC container);
			return container;
		}

		public void Release()
		{
			foreach (TC container in allContainers.Values)
			{
				container.Release();
			}

			allContainers.Clear();
		}

		public void CallOnAllVisibleElements(Action<TE> callBack)
		{
			foreach (KeyValuePair<string, TC> elementContainer in allContainers)
			{
				if (elementContainer.Value.IsVisible())
				{
					callBack(elementContainer.Value.GetElement());
				}
			}
		}
	}
}
