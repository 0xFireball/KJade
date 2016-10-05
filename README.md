
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

## Syntax

KJade syntax is very similar to that of Jade; however, some of the
latest Jade language features may not yet be available:

A hierarchy of elements is built based on indentation level. Two nodes on the
same indentation level end up in the same nesting level in XML.

For example:

```jade
html
    head
        title Test KJade page
    body
        div.header#intro
            h1 This is a test KJade page.
            p Hello, World!
```

