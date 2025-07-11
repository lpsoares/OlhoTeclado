# Cython Decoders

Some of the decoders are implemented in Cython for performance reasons. The Cython code is located in the `cython` directory. To use these decoders, you need to build the Cython extension.

## Automatically generated files

The Cython code is generated from the `*.pyx` files and the `*.cpp` and `*.hpp` files with `camelCase` names. The generated files have `snake_case` names. For example, the eyeswipe decoder files are `eyeswipeDecoder.cpp`, `eyeswipeDecoder.hpp`, and `eyeswipe_decoder.pyx`. The generated files are `eyeswipe_decoder.cpp` and `eyeswipe_decoder.cpython-*`.
