---
uid: attr-text
---

text
====

The `text` attribute sets the text content of the control. This attribute is used for many editors and controls.
Use `text` instead of `a-text` for all tag helpers. `a-text` is only used for its own tag helper, which injects text content into HTPM tags.

```C#
[HtmlAttributeName("text")]
public string Text { get; set; }
```

#### Capabilities
The `text` attribute may contain:
* Text to display
* #prop-name to translate using language texts.
* ##parent-prop-name to translate using language texts. Translation uses the parent page, and not the page within which the tag occurs.


#### Processing
The `ProcessAsync` override processes the value of the attribute:

```C#
string text = TranslateToProp(Text, ViewContext);
output.Content.AppendHtml(text);
```

#### Content
The attribute can contain values:

+----------------------+------------------------------+-----------------------------------------------------------+
| Value                | Usage                        | Remark
+======================+==============================+===========================================================+
| (empty)              | No text                      | No text to display.
+----------------------+------------------------------+-----------------------------------------------------------+
| #prop-name           | Page property                | The text gets translated to a current-view property.
+----------------------+------------------------------+-----------------------------------------------------------+
| ##parent-prop-name   | Parent page property         | The text gets translated to a parent-view property.
+----------------------+------------------------------+-----------------------------------------------------------+
| (any other value)    | Display text                 | The text does not get translated, and is displayed AS-IS.
+----------------------+------------------------------+-----------------------------------------------------------+
<hr/>

#### Examples
* AlertTagHelper

### Related attributes
* `title` is handled in the same way, but does not represent the text content of an HTML tag, but its title or tooltip.
