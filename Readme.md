# UI list performance

## Overview
The classes provided in this coding sample are part of a UI performance system. This system disables and enables (culls) UI elements in a scrolling list based on whether they are visible or not. When they are not visible, the UI elements are stored in a pool for later reuse. When made visible, an element is retrieved from the pool. Both the pooling and the culling system are not provided in this repository. This repository is only showing how the connection between both is created with generic classes.

## Culling System
The culling system uses a Unity `ScrollRect` to figure out whether an element is in the visible scroll section or not. It is connected to the Unity action that is called when scrolling happens. Based on the `RectTransform` which knows the bounds of the element, the element will be called to set itself active or inactive.

## Pooling System
The pooling system is a classical pooling system that stores elements that are currently not in use.

This has the advantage that memory does not need to be allocated over and over again and thus avoids defragmentation of runtime memory. Other advantages are avoiding garbage collection and runtime memory allocation.

A number of elements can also be pre-warmed during the loading screen to avoid hiccups during play time. Or the size of the pool and thus the amount of elements existing at the same time can be controlled. This can be useful when catering optimal performance to devices with different amount of memory available.

## The container - connection between culling and pooling
To be able to reuse this culling and pooling system for a variety of different UI elements that appear in lists, I created the following two generic classes. `UiElementContainer`: a base class for a component that needs to be attached to a Unity gameObject. 
`UiElementContainerCache`: a runtime lookup table for the instantiated gameObjects with the `UiElementContainer` component.

### `UiElementContainer<T>`
In order for the scrollable list to maintain its content size, each cullable UI element is parented by an `UiElementContainer`. The container is called by the culler to signal whether an element just went visible or invisible. When an element becomes invisible, it's container returns it to the pool. When made visible, the container calls the setup functions on the element with the data that needs to be shown at this position of the scrolling list.

### `UiElementContainerCache<TC, T>`
In the class that populates the list, a cache of containers connects the UI elements with their respective data elements. Each container is initialised with a callback for setting up the UI element and showing the data at that index when it is made visible.

### Example Use - Start here
```
public Transform listRoot;
public InventoryUiElement inventoryUiElementPrefab;
public InventoryUiElementContainer<InventoryUiElement> inventoryUiElementContainerPrefab;
...
UiElementContainerCache<InventoryUiElementContainer<InventoryUiElement>, InventoryUiElement> cache = new UiElementContainerCache<InventoryUiElementContainer<InventoryUiElement>, InventoryUiElement> (inventoryUiElementContainerPrefab);
...
for (int i = 0; i < inventoryItems.Count; i++)
{
  InventoryItem inventoryItem = inventoryItems[i];
  InventoryUiElementContainer<InventoryUiElement> inventoryUiElementContainer = cache.GetContainer(inventoryItem.id, listRoot, i);
  inventoryUiElementContainer.Init(
  	inventoryUiElement =>
  	{
  		inventoryUiElement.Init();
  		inventoryUiElement.Show(fooData);
  	},
  	inventoryUiElementPrefab.gameObject
  );
}
```

## Known drawbacks
### String comparison
The cached containers are stored with a string id which means that string comparison is used when retrieving a container. String comparison does create garbage that needs to be collected. This could become a performance concern when containers are retrieved frequently. An example would be when an outside function needs to update some part of specific ui elements. Let's say that there is an animation function that invokes a change of background on every odd element's background every two seconds. If this function would poll for all odd elements ever so often, it would be recommended not to use the string id of the data element for identification.
### Setup
Since UiElementContainer is typed, a new prefab and inheriting component needs to be create for each new type of UI element that wants to profit from the pooling and culling.
### Ui element Init call
The init call could be extracted so that it is only called when the element is instantiated by the pool, not every time the UI element is made visible.
