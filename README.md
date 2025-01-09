Note: 
This solution is currently under development and is not (yet) intended to be used by third parties.
We are busy creating more tag helpers, unit test cases, and a sample application.
Please watch this space...

Alcazar.DevExpress
==================

The Alcazar.DevExpress solution provides extensions for DevExpress and DevExtreme (.Net edition) to build web applications.

Alcazar.DevExpress.TagHelpers
-----------------------------

Tag helpers are a concept used by Microsoft ASP.NET Core to encapsulate functionality into standard HTML. 
Tag helpers enable server-side code to participate in creating and rendering HTML elements in Razor files.
It is an elegant mechanism to produce simple and neat HTML for web pages with rich functionality.
See https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro for more information.

On the contrary, the standard and default way to use .Net DevExpress controls is using their FluentAPI, embedded in Razor pages.
Mixing HTML and code-like structures like Fluent API allows rich functionality, but also creates messy code, which often is not easily readable.

Some time ago, DevExpress have started a repository https://github.com/DevExpress/DevExtreme.AspNet.TagHelpers to provide tag helpers for their controls.
This repository is rather incomplete, and has been made obsolete by its owner in 2017 (!).

Our solution is NOT based on this obsolete repository, but instead developed completely from scratch.
We will endeavor to provide tag helpers for most, if not all .Net DevExpress controls, supporting most options and attributes.
We are being realistic, that it might by hard, if not impossible, to create a tag helper equivalent for all features available in the DevExpress FluentAPI,
but we will certainly strive for enabling a large number of common use cases.

License note
------------

Alcazar Security does not represent DevExpress, not are we a reseller of their products.
We are a user of their products and this solution makes extensions to DevExpress available to other interested parties.
To run this solution, a valid and current license issued by https://www.devexpress.com/ is required.
No components licensed by DevExpress are published by this repository.