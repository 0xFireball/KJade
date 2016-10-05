
# KJade

A parser for a variant of the Jade/PugJS template engine.
Built for .NET core, and intended for use with NancyFX.

This parser is an experimental implementation of the Jade syntax.
It works by reading through the code, lexing it into tokens,
and building an AST from the tokens.

Finally, an extensible class is used to allow generation of HTML/XML
from the AST. KJade is completely written in C#, and is built for the
.NET core platform.

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

