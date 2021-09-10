Mirana versioning file

This file records the version log of Mirana.

## 1.0 25/08/21

Publish the first version of Mirana.

## 1.1 06/09/21

- Fix the issue that tailing fun lambdas are treated as separate function calls.
- Add operator `+=`, `-=`, `*=`.

## 1.2 06/09/21

- Fix versioning attribute.

## 1.3 06/09/21

- Stop compiling when the source has syntax errors.
- Fix the issue that after table constructors with only one field indents are printed.
- Improve the performance.

## 1.4 07/09/21

- Add if expressions.

## 1.5 07/09/21

- Add easy lambda expressions.

## 1.5.1 08/09/21

- Print the state of one compilation unit immediately when the task is done.

## 1.5.2 09/09/21

- Fix the bug that in easy lambda expression without parameter list the empty parameter list is not printed.

## 1.5.3 10/09/21

- Fix in `if` expressions before `elseif` clauses indents are not printed.

## 1.5.4 10/09/21

- Fix the bug that in easy lambda expression where the last statement is a function call the value is not returned
