# UI list performance

## Overview
The classes provided in this coding sample are part of a UI performance system. 

This UI performace system disables and enables (culls) UI elements in a scrolling list based on whether they are visible. When not visible, the UI elements are stored in a pool for later reuse. When made visible, an element is retrieved from the pool and set up with data. 

A different approach would be to use pagination to split the list into several pages, this solution catered to the need of keeping all the elements the list in one page to let the player scroll through the whole list without needing to switch pages. 

Both the pooling and the culling system are not provided in this repository since they were not developed by me. The existing system was used for a specfic screen. I made it generic to be able to use it for other UI screens that also showed performance issues. 

## The container - connection between culling and pooling
To reuse the culling and pooling system for a variety of different UI elements that appear in lists, I created the following two generic classes: 

* `UiElementContainer`: 
  A container holding the position and callbacks for an element in the list. 
* `UiElementContainerCache`: 
  A runtime lookup table for the instantiated gameObjects with the `UiElementContainer` component.

### `UiElementContainer<T>`
The container for an element holds its position while it is not visible. This has the effect that the list does not change size when elements are culled and pooled. The container is called by the culler to signal whether an element just went visible or invisible. When an element becomes invisible, it's container returns it to the pool. When made visible, the container calls the setup functions on the element with the data that needs to be shown.

### `UiElementContainerCache<TC, T>`
In the class that populates the list, a cache of containers connects the UI elements with their respective data elements. The container can be used to manipulate a specfic element.

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
The cached containers are stored with a string id which means that string comparison is used when retrieving a container. String comparison does create garbage that needs to be collected. This could become a performance concern when containers are retrieved frequently. In the situation of a player scrolling a list my team and I did not encounter any issues.

### Somewhat clunky setup
A new inheriting container component needs to be created for each new type of UI element that wants to profit from the pooling and culling. These container classes are empty besides inheriting from MonoBehavior and the typing the generic container.

### Decouple initialization and showing data
The init call could be extracted so that it is only called when the element is instantiated by the pool, not every time the UI element is made visible.
