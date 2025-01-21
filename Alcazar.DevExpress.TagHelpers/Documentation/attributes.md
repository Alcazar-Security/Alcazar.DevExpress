---
uid: attributes
---

Attributes
==========


Event handlers
--------------


### OnItemClick
Item controls, such as the dropdown button, which hosts menu items in the dropdown menu, offer an event hendler when the item is clicked.
The tag helper attribute is declared as:

```C#
[HtmlAttributeName("item-click")]
public string OnItemClick { get; set; }
```

#### Capabilities
The `OnItemClick` supports these use cases:
* No action.
* Redirect to page using an HTTP GET request.
* Custom JS event handler.


#### Processing
The `ProcessAsync` override processes the value of the attribute:

```C#
// Event handlers
builder = ItemClick(builder);
```

#### OnItemClick content
The attribute can contain values:

+----------------------+------------------------------+-----------------------------------------------------------+
| Value                | Usage                        | Remark
+======================+==============================+===========================================================+
| (empty)              | No action                    | The event handler is not used. The item template will typically provide for event handling, such as an HTML anchor <a>.
+----------------------+------------------------------+-----------------------------------------------------------+
| href                 | Redirect to page             | The item includes a href. A default JS method is used as event handler, which redirects to the requested page.
+----------------------+------------------------------+-----------------------------------------------------------+
| rb                   | RazorBlock                   | TODO
+----------------------+------------------------------+-----------------------------------------------------------+
| (any other value)    | Custom JS                    | The attribute value contains the name of a custom JS method to be invoked.
+----------------------+------------------------------+-----------------------------------------------------------+
<hr/>


#### Default event handler

```JS
function dx_dropdown_itemclick(dx) {
	var href = dx.itemData.href;
	if (href !== null) {
		// Redirect to a url, if we have one
		window.location.href = href;
	}
}
```

#### Custom event handlers
Custom event handlers can be used for use cases which are not covered 

#### Examples
* DropdownButtonTagHelper
