
# KJade

A parser for a subset of the Jade/PugJS template engine.
Built for .NET core, and intended for use with NancyFX.

KJade has no external dependencies; the lexer and parser were written
from scratch with regex. HTML/XML building is done at the end, and KJade
provides the `ICompiler` interface allowing anyone to implement a compiler
to a language other than XML from the KJade AST, a simple tree.

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
        title Test Page
    body
        div#intro(a1="val1",a2="val2")
            h1 This is a KJade test!
            p easy
            p.friendly easier!
            p.
                Free form
                paragraph!
            h1 Hello, #{model.Name}!
        .lol#infer-node-type
        .btn.btn-default.btn-cool.btn-amazing
        div#some-id.another-class
            h2 Some more information
        div.some-class.some-other-class
            h2 Finally
        div(some-attr="some value")
            p This div is really unique, using paren notation!
```

Additionally, block comments are not supported, only single line comments with `//` are supported.
Each comment must be on its own line. Indentation before the `//` is ignored.

### Experimental Features (Use at your own risk!)

#### Tips

- KJade is very indentation sensitive!
- KJade does NOT support single quotes (`'`) for enclosing attribute values.

#### Variable Replacement

KJade's variable replacement works differently than Jade's:
The `Compile` method of `JadeCompiler` has an optional argument specifying a model, which can be
any object.

For example, let our model be an object like this: `{ Name = "Bob" }`
Then, in the value of a KJade block, `Hello, #{model.Name}` would evaluate to `Hello, Bob`.

Additionally, putting a `!` before the `#` will tell KJade to automatically HTML-encode the value
of the property of the model.

### Conditionals

You can specify data that will only be shown if a property of the model evaluates to not null or true.
Otherwise, the data will be hidden. This is useful for error messages. The syntax is as follows:

```jade
@if model.InputHasError
Your input has an error.
@endif
```

### Enumerables

You can expand data with Enumerables. If you pass in a property that implements `IEnumerable`,
KJade will expand the code within, replacing `{$element}` with the string representation
of the current element. You can use this to render lists or other repeating structures easily.

```jade
@enumerable model.SomeEnumerable
li {$element}
@endenumerable
```

## KJadeViewEngine

KJadeViewEngine is a NancyFx plugin that allows you to use KJade in your
Nancy web applications. These features are only available when using the view engine.

### Imports

In the NancyFx View Engine plugin, you can import other views with `@import [view name]`.
For example, the following will import the header view:

`@import Header`

### Partials

Partials are very similar to imports, except that they specify an alternate model.
The syntax is as follows:

`@partial [view name];[model]`

For example:

`@partial Components/PartialTest;model.PTestModel`

This will import `Components/PartialTest` and evaluate it with the model `model.PTestModel`,
where model is the model of the current view.