
# KJade

A parser for a subset of the Jade/PugJS template engine.
Built for .NET core, and intended for use with NancyFX.

This parser is an experimental implementation of some of the Jade syntax.
It works by reading through the code, lexing it into tokens,
and building an AST from the tokens.

Finally, an extensible class is used to allow generation of HTML/XML
from the AST. KJade is completely written in C#, and is built for the
.NET core platform.

KJade is not intended to be able to handle all Jade syntax; its aim is instead
to provide a concise templating experience similar to that of Jade/PugJS, but with
C#/.NET instead of JS.

## A quick note on supported syntax

KJade syntax is a subset of Jade syntax. While Jade often has a number
of different syntaxes that will evaluate to the same result, KJade omits
such syntactic forms.

For example, Jade supports multiline values for elements with both `|` and a `.` after
the element name. KJade only supports the `.` method, and treats `|` as part of the element value.

A hierarchy of elements is built based on indentation level. Two nodes on the
same indentation level end up in the same nesting level in XML.

Here's a short document demonstrating some of the syntax:

```jade
html
    head
        title Test KJade page
    body
        div.header#intro
            h1 This is a test KJade page.
            p.
                Hello, World!
                I would write a longer paragraph
                But i'm too lazy...
            h3 Three cheers for KJade!
        //div is automatically inferred if not specified!
        .container#second
            h2 This is in a container div!
```

Additionally, block comments are not supported, only single line comments with `//` are supported.
Each comment must be on its own line. Indentation before the `//` is ignored.

