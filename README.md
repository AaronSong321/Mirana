# Mirana
Mirana is an extension to lua. It compiles to lua before running. Full grammar is in grammar/mirana.g4.

# Features

- Arrow lambda expression:
  When there's only one parameter, the parenthesis can be omitted.
  ```
  local a = t -> t:GetName()
  local c = (a, b) -> a == b
  ```
  compiles to
  ```lua
  local a = function(t) return t:GetName() end
  local c = function(a, b) return a == b end
  ```
- Fun lambda expression:
  Use $1-$8 as parameters. If only one parameter is required, it's allowed to use `it`. $1-$8 and it cannot be used together in one lambda expression.
  ```
  local a = fun { $1 == $2 }
  local b = fun { it.count }
  ```
  compiles to
  ```lua
  local a = function(__mira_lpar_1, __mira_lpar_2)
    return __mira_lpar_1 == __mira_lpar_2
  end
  local b = function(__mira_lpar_it)
    return __mira_lpar_it.count
  end
  ```
  When a fun lambda is the last of an argument list, it can be taken out of the parenthesis. Further, if the fun lambda is the only argument, the parenthesis can be omitted.
  ```
  a(1, 2) fun { it * 2 }
  a.b fun { it + it }
  a:c fun { {it.match, it} }:o fun {
    it + 1
  }
  ```
- Easy lambda expression:
  The `fun` keyword is absurd between a function name and a pair of braces. Easy lambda expressions don't need the `fun` keyword. It's composed by a parameter list (can be embraced by a pair of parenthesis or not), an arrow, and an expression block, embraced by braces. Easy lambdas can also be moved out of the parenthesis when used as the last parameter of a function call. For example:
  ``` 
  a { (v1, v2, ...) -> 15 }
  a(3, 7) { -> 8 }
  ```
- Operator lambda:
  Most operators (+,-,*,/,%,^,==,~=,..,and,or,not,&&,||,++,~~) can be used as lambda expression when surrounded by braces.
  ``` 
  local a,b = {>},{not}
  ```
  compiles to
  ```lua
  local a,b = function(__mira_olpar_1, __mira_olpar_2) return __mira_olpar_1 > __mira_olpar_2 end, function(__mira_olpar_1) return not __mira_olpar_1 end
  ```
- If integrated variable declaration:
  ```
  if target = a:GetTarget() then
    ShootAt(target)
  elif health = f:GetHealth() then -- elif equals to elseif
    ...
  end
  ```
  compiles to
  ```lua
  do
    local target = a:GetTarget()
    local health = f:GetHealth()
    if target then
        ShootAt(target)
    elseif health then
        print(health)
    end
  end
  ```
- If expression: use `if`, `elif`, `else` and braces as keywords to write if expressions. The else branch can be omitted, and in this case the else branch is evaluated to `nil`. For example:
  ```
  local c = if t > 3 {
    15
  } elif local target = GetBot() {
    "else"
  } else {
    b
  }
  ```
- operator ++, ~~:
  They can only be used as statements and in prefix form at this time.
  ```
  ~~a
  ++a.b()[1]:c()[2]
  ```
  compiles to
  ```lua
  a = a - 1
  local __mira_locvar_1 = a.b()[1]:c()
  __mira_locvar_1[2] = __mira_locvar_1[2] + 1
  ```
  also operator +=, -=, *=.
- Macros: they're very like C++ macros.
  ``` 
  #define Abc(a,b,c) local a = function(d) return c+b-(c*c) end
  #define Match(f,d,...) f#d + f k##__VA_ARGS__
  Match(1,teach{}, {{{}}}, "*#")
  ```
  preprocesses to:
  ```lua
  local mf = function(d) return 3-3+10-(3-3*3-3) end 
  "1""teach{}" + 1 k {{{}}},  "*#"
  ```
# Usage

Require .NET 5.0 to build and run.
Call from command-line with MiranaCompiler. Parameters are either path to .mira file or path to folder that contains .mira files.
Alternatively, you can call it from C# or any other .NET languages. Use:

```C#
using MiranaCompiler;

Compiler compiler = new();
await compiler.CompileAsync(new [] {
  "path_to_mira_files",
  "path_to_mira_file",
});
```
