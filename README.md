# Mirana
Mirana is an extension to lua. It compiles to lua before running. Full grammar is in grammar/mirana.g4.

# Features

- Easy lambda expression:
  Lambda expressions composed by a parameter list (can be embraced by a pair of parenthesis or not), an arrow, and an expression block, embraced by braces. Lambdas can also be moved out of the parenthesis when used as the last parameter of a function call. For example:
  ``` 
  a { (v1, v2, ...) -> 15 }
  a(3, 7) { -> 8 }
  ```
  Normal lua function declarations can also use an expression together with an arrow as the return value of the function.
  ```
  function(t) -> t:GetUnitName()
  ```
  compiles to
  ```lua
  function(t) return t:GetUnitName() end
  ```
- Operator lambda:
  Most operators (+,-,*,/,%,^,==,~=,..,and,or,not,&&,||,++,~~) can be used as lambda expression when surrounded by braces.
  ``` 
  local a,b = {>},{not}
  ```
  compiles to
  ```lua
  local a,b = function(opv_1, opv_2) return opv_1 > opv_2 end, function(opv_1) return not opv_1 end
  ```
- If integrated variable declaration: a local variable declaration can be inserted before the predicate of an if statement and separated by a comma. The predicate in this way can be omitted if the variable itself is to be checked (semantically, neither `nil` nor `false` in lua). Throughout Mirana files, you can use `elif` instead of `elseif`.
  ```
  if local target = a:GetTarget() then
    ShootAt(target)
  elif local health = f:GetHealth(); health > 1000 then -- elif equals to elseif
    ...
  end
  ```
  compiles to
  ```lua
  do
    local target = a:GetTarget()
    if target then
      ShootAt(target)
    else
      local health = f:GetHealth()
      if health > 1000 then
        print(health)
      end
    end
  end
  ```
- If expression: use `if`, `elif`, `else` and braces as keywords to write if expressions. The else branch can be omitted, and in this case the else branch is evaluated to `nil`. Variables can be integrated into `if` and `elif` predicates. For example:
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
<!-- - Macros: they're very like C++ macros.
  ``` 
  #define Abc(a,b,c) local a = function(d) return c+b-(c*c) end
  #define Match(f,d,...) f#d + f k##__VA_ARGS__
  Match(1,teach{}, {{{}}}, "*#")
  ```
  preprocesses to:
  ```lua
  local mf = function(d) return 3-3+10-(3-3*3-3) end 
  "1""teach{}" + 1 k {{{}}},  "*#"
  ``` -->
  
# Usage

Require .NET 5.0 to build and run.
Call from command-line with MiranaCompiler. Parameters are either path to .mira file or path to folder that contains .mira files.

Alternatively, you can call the Mirana compiler from C# or any other .NET languages. For example:

```C#
using MiranaCompiler;

Compiler compiler = new();
await compiler.CompileAsync(new [] {
  "path_to_mira_files",
  "path_to_mira_file",
});
```
